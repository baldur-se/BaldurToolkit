using System;
using System.Collections.Generic;
using System.Threading;
using BaldurToolkit.Network.Buffers;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Controllers;
using BaldurToolkit.Network.Protocol;
using NLibuv;

namespace BaldurToolkit.Network.Libuv.Connections
{
    public class LibuvConnection : Connection
    {
        private ReceiveState receiveState;

        public LibuvConnection(UvTcp socket, IProtocol protocol, IPacketRouter router, IBufferPool bufferPool)
            : base(router, protocol)
        {
            this.BaseSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.RemoteEndPoint = socket.GetPeerEndPoint();

            this.BufferPool = bufferPool;

            this.SilentSend = true;
        }

        public LibuvWorker Worker { get; set; }

        public IBufferPool BufferPool { get; set; }

        public bool SilentSend { get; set; }

        protected UvTcp BaseSocket { get; }

        public virtual void HandlePacket(IPacket packet)
        {
            this.Router.Handle(this, packet);
        }

        public override void Close()
        {
            this.Close(null);
        }

        public virtual void BeginReceive()
        {
            lock (this.SyncRoot)
            {
                if (this.receiveState != null)
                {
                    throw new Exception("Receiving is already started on this connection.");
                }

                this.receiveState = new ReceiveState();
                this.receiveState.Buffer = this.BufferPool.Acquire(this.Protocol.RecommendedBufferSize);
            }

            this.BaseSocket.ReadStart(this.AllocCallback, this.ReadCallback, this.receiveState);
        }

        public override void Send(IPacket packet)
        {
            try
            {
                var buffer = this.BufferPool.Acquire(this.Protocol.CalculatePacketSize(packet));
                var length = this.Protocol.Write(packet, buffer.Buffer, buffer.Offset, buffer.Length);

                var writeRequest = new UvWriteRequest(this.BaseSocket, new ArraySegment<byte>(buffer.Buffer, buffer.Offset, length), this.WriteCallback, buffer);

                if (this.Worker != null && Thread.CurrentThread.ManagedThreadId != this.BaseSocket.ThreadId)
                {
                    this.Worker.EnqueueWriteRequest(writeRequest);
                }
                else
                {
                    writeRequest.Write();
                }
            }
            catch (UvErrorException exception)
            {
                if (exception.ErrorCode == UvErrorCode.ECONNRESET)
                {
                    this.Close();
                }
                else
                {
                    this.Close(exception);
                }
            }
            catch (Exception exception)
            {
                this.Close(exception);
                if (!this.SilentSend)
                {
                    throw;
                }
            }
        }

        public override string ToString()
        {
            if (this.Worker != null)
            {
                return $"{this.GetType().Name}({this.RemoteEndPoint};t{this.Worker.Loop.ThreadId})";
            }

            return base.ToString();
        }

        protected void Close(Exception exception)
        {
            if (!this.TryClose())
            {
                return;
            }

            var closingState = new ClosingState();
            if (exception != null)
            {
                closingState.RegisterException(exception);
            }

            // Closing logic should be executed on the loop thread
            if (this.Worker != null && Thread.CurrentThread.ManagedThreadId != this.BaseSocket.ThreadId)
            {
                this.Worker.EnqueueFinalJob(this.CloseCallback, closingState);
            }
            else
            {
                this.CloseCallback(closingState);
            }
        }

        private void CloseCallback(object closingStateObject)
        {
            var closingState = (ClosingState)closingStateObject;

            // Stop reading process (if any)
            try
            {
                this.BaseSocket.ReadStop();
            }
            catch (Exception exception)
            {
                closingState.RegisterException(exception);
            }

            // Clear receive state and free the receive buffer
            var state = this.receiveState;
            this.receiveState = null;

            // Return acquired buffer to the pool
            state?.Buffer?.Dispose();

            // Shut down any writing operations
            // Handle will be closed after the shutdown process
            var shutdownRequest = new UvShutdownRequest(this.BaseSocket, this.ShutdownCallback, closingState);
            try
            {
                shutdownRequest.Shutdown();
            }
            catch (Exception exception)
            {
                this.ShutdownCallback(shutdownRequest, exception, closingState);
            }
        }

        private void ShutdownCallback(UvShutdownRequest request, Exception error, object closingStateObject)
        {
            var closingState = (ClosingState)closingStateObject;

            try
            {
                if (error != null)
                {
                    throw error;
                }
            }
            catch (UvErrorException e)
            {
                if (e.ErrorCode != UvErrorCode.ECONNRESET)
                {
                    closingState.RegisterException(e);
                }
            }
            catch (Exception e)
            {
                closingState.RegisterException(e);
            }

            try
            {
                this.BaseSocket.Close();
            }
            catch (Exception e)
            {
                closingState.RegisterException(e);
            }

            this.InvokeClosedEvent(closingState.GetException());
        }

        private ArraySegment<byte> AllocCallback(UvStream stream, int suggestedSize, object stateObject)
        {
            var state = (ReceiveState)stateObject;
            return new ArraySegment<byte>(state.Buffer.Buffer, state.Buffer.Offset + state.ReceivedBytes, state.Buffer.Length - state.ReceivedBytes);
        }

        private void ReadCallback(UvStream stream, int receivedBytes, Exception error, object stateObject)
        {
            try
            {
                if (error != null)
                {
                    throw error;
                }

                var state = (ReceiveState)stateObject;
                state.ReceivedBytes += receivedBytes;

                int remainingBytes;
                int nextPacketOffset = 0;

                do
                {
                    remainingBytes = this.Protocol.TryRead(
                        state.Buffer.Buffer,
                        state.Buffer.Offset + nextPacketOffset,
                        state.ReceivedBytes - nextPacketOffset,
                        out var packet);

                    if (packet != null)
                    {
                        // Synchronously handle packet before reading the next packet
                        this.HandlePacket(packet);
                    }

                    if (remainingBytes == 0)
                    {
                        // Packet has been received completely, no other data currently available
                        state.ReceivedBytes = 0;

                        // Return to a buffer of recommended size
                        if (state.Buffer.Length > this.Protocol.RecommendedBufferSize)
                        {
                            BufferSegment bufferToReturn = null;
                            try
                            {
                                var oldBuffer = state.Buffer;
                                state.Buffer = this.BufferPool.Acquire(this.Protocol.RecommendedBufferSize);
                                bufferToReturn = oldBuffer;
                            }
                            finally
                            {
                                bufferToReturn?.Dispose();
                            }
                        }
                    }
                    else if (remainingBytes < 0)
                    {
                        // There is complete another packet in our buffer, calculate it's offset
                        nextPacketOffset = state.ReceivedBytes + remainingBytes;
                    }
                    else if (state.ReceivedBytes + remainingBytes > state.Buffer.Length)
                    {
                        // There's next packet's incomplete body left and it does not fit into the rest of the buffer.
                        // We need to move it at the beginning of the buffer.
                        var nextPacketSize = state.ReceivedBytes - nextPacketOffset + remainingBytes;
                        var sourceBuffer = state.Buffer;
                        BufferSegment bufferToReturn = null;
                        try
                        {
                            if (nextPacketSize > state.Buffer.Length)
                            {
                                // The packet is bigger than our current buffer. We should try to acquire a bigger one from the buffer pool.
                                state.Buffer = this.BufferPool.Acquire(nextPacketSize);
                                bufferToReturn = sourceBuffer;
                            }

                            // Basically cutting the used part of received butes and storing the rest into the state's buffer
                            state.ReceivedBytes -= nextPacketOffset;
                            Buffer.BlockCopy(
                                sourceBuffer.Buffer,
                                sourceBuffer.Offset + nextPacketOffset,
                                state.Buffer.Buffer,
                                state.Buffer.Offset,
                                state.ReceivedBytes);
                        }
                        finally
                        {
                            bufferToReturn?.Dispose();
                        }
                    }
                }
                while (remainingBytes < 0);

                // Wait for another read callback
            }
            catch (UvErrorException exception)
            {
                if (exception.ErrorCode == UvErrorCode.EOF || exception.ErrorCode == UvErrorCode.ECONNRESET)
                {
                    this.Close();
                }
                else
                {
                    this.Close(exception);
                }
            }
            catch (Exception exception)
            {
                this.Close(exception);
            }
        }

        private void WriteCallback(UvWriteRequest request, Exception error, object stateObject)
        {
            try
            {
                if (stateObject is BufferSegment buffer)
                {
                    buffer.Dispose();
                }

                if (error != null)
                {
                    throw error;
                }
            }
            catch (UvErrorException exception)
            {
                if (exception.ErrorCode == UvErrorCode.ECONNRESET)
                {
                    this.Close();
                }
                else
                {
                    this.Close(exception);
                }
            }
            catch (Exception exception)
            {
                this.Close(exception);
            }
        }

        protected class ReceiveState
        {
            public BufferSegment Buffer { get; set; }

            public int ReceivedBytes { get; set; }
        }

        protected class ClosingState
        {
            private List<Exception> exceptions;

            public void RegisterException(Exception exception)
            {
                if (this.exceptions == null)
                {
                    this.exceptions = new List<Exception>();
                }

                this.exceptions.Add(exception);
            }

            public Exception GetException()
            {
                if (this.exceptions == null)
                {
                    return null;
                }

                if (this.exceptions.Count == 1)
                {
                    return this.exceptions[0];
                }

                return new AggregateException(this.exceptions);
            }
        }
    }
}
