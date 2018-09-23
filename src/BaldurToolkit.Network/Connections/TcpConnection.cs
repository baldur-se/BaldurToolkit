using System;
using System.Net.Sockets;
using BaldurToolkit.Network.Buffers;
using BaldurToolkit.Network.Controllers;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public class TcpConnection : Connection
    {
        private ReceiveState receiveState;

        public TcpConnection(Socket socket, IProtocol protocol, IPacketRouter router, IBufferPool bufferPool)
            : base(router, protocol)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (socket.ProtocolType != ProtocolType.Tcp)
            {
                throw new ArgumentException("Invalid socket protocol type. TCP required.", nameof(socket));
            }

            if (!socket.Connected)
            {
                throw new ArgumentException("Socket is not connected.", nameof(socket));
            }

            this.BaseSocket = socket;
            this.RemoteEndPoint = socket.RemoteEndPoint;

            this.BufferPool = bufferPool;

            this.SilentSend = true;
        }

        public IBufferPool BufferPool { get; set; }

        public bool SilentSend { get; set; }

        protected Socket BaseSocket { get; }

        public override void Close()
        {
            this.Close(null);
        }

        /// <summary>
        /// Begin asynchronous receive operation and incoming packet handling.
        /// </summary>
        public virtual void BeginReceive()
        {
            ReceiveState state;
            lock (this.SyncRoot)
            {
                if (this.receiveState != null)
                {
                    throw new Exception("Receiving is already started on this connection.");
                }

                this.receiveState = new ReceiveState(this.BufferPool.Acquire(this.Protocol.MaxPacketSize));
                state = this.receiveState;
            }

            this.BeginReceive(state);
        }

        public virtual void HandlePacket(IPacket packet)
        {
            this.Router.Handle(this, packet);
        }

        public override void Send(IPacket packet)
        {
            try
            {
                var buffer = this.BufferPool.Acquire(this.Protocol.CalculatePacketSize(packet));
                var length = this.Protocol.Write(packet, buffer.Buffer, buffer.Offset, buffer.Length);
                this.BaseSocket.BeginSend(buffer.Buffer, buffer.Offset, length, SocketFlags.None, this.BeginSendCallback, buffer);
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

        protected void Close(Exception exception)
        {
            if (!this.TryClose())
            {
                return;
            }

            ReceiveState state;
            lock (this.SyncRoot)
            {
                state = this.receiveState;
                this.receiveState = null;
            }

            if (this.BaseSocket.Connected)
            {
                this.BaseSocket.Shutdown(SocketShutdown.Both);
                this.BaseSocket.Dispose();
            }

            // Return acquired buffer to the pool
            state?.Buffer.Dispose();

            this.InvokeClosedEvent(exception);
        }

        private void BeginReceive(ReceiveState state)
        {
            this.BaseSocket.BeginReceive(
                state.Buffer.Buffer,
                state.Buffer.Offset + state.ReceivedBytes,
                state.Buffer.Length - state.ReceivedBytes,
                SocketFlags.None,
                this.BeginReceiveCallback,
                state);
        }

        private void BeginReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                var state = (ReceiveState)asyncResult.AsyncState;
                var receivedBytes = this.BaseSocket.EndReceive(asyncResult);
                if (receivedBytes == 0)
                {
                    // Connection closed
                    this.Close();
                    return;
                }

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

                    if (remainingBytes > state.Buffer.Length)
                    {
                        throw new Exception("Packet is too large. Please increase buffer size.");
                    }

                    if (remainingBytes == 0)
                    {
                        // Only one packet has been received and received completely
                        state.ReceivedBytes = 0;
                    }
                    else if (remainingBytes < 0)
                    {
                        // There is another packet's beginning in our buffer, calculate it's offset
                        nextPacketOffset = state.ReceivedBytes + remainingBytes;
                    }
                    else if (remainingBytes > 0 && nextPacketOffset > 0)
                    {
                        // We need to read the rest of the packet's body
                        // So we better move it to the beginning of the buffer
                        state.ReceivedBytes -= nextPacketOffset;
                        Buffer.BlockCopy(
                            state.Buffer.Buffer,
                            state.Buffer.Offset + nextPacketOffset,
                            state.Buffer.Buffer,
                            state.Buffer.Offset,
                            state.ReceivedBytes);
                    }
                }
                while (remainingBytes < 0);

                this.BeginReceive(state);
            }
            catch (Exception exception)
            {
                var socketException =
                    exception as SocketException ??
                    exception.InnerException as SocketException;

                if (socketException != null && socketException.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // Treat ConnectionReset exceptions as close request
                    this.Close();
                    return;
                }

                this.Close(exception);
            }
        }

        private void BeginSendCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult.AsyncState is BufferSegment buffer)
                {
                    buffer.Dispose();
                }

                this.BaseSocket.EndSend(asyncResult);
            }
            catch (Exception exception)
            {
                this.Close(exception);
            }
        }

        protected class ReceiveState
        {
            public ReceiveState(BufferSegment buffer)
            {
                this.Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            }

            public BufferSegment Buffer { get; }

            public int ReceivedBytes { get; set; }
        }
    }
}
