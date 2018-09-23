using System;
using NLibuv;

namespace BaldurToolkit.Network.Libuv.Connections
{
    public class LibuvPipeConnectionListener<TConnection> : LibuvConnectionListener<TConnection>
        where TConnection : LibuvConnection
    {
        private string pipeName;
        private byte[] pipeAcceptBuffer = new byte[16];

        public LibuvPipeConnectionListener(LibuvWorker worker, string pipeName, Func<UvTcp, LibuvWorker, TConnection> connectionFactory)
            : base(worker, connectionFactory)
        {
            this.pipeName = pipeName;
        }

        protected LibuvPipeConnectionListener(LibuvWorker worker, string pipeName)
            : base(worker)
        {
            this.pipeName = pipeName;
        }

        protected override void OnListeningStarted(ListenState state)
        {
            // Execute on the loop thread
            this.Worker.EnqueueJob(this.StartListeningCallback, state);
        }

        private void StartListeningCallback(object stateObject)
        {
            var state = (ListenState)stateObject;
            try
            {
                var pipe = new UvPipe(this.Worker.Loop, true);
                state.OpenedHandles.Add(pipe);

                pipe.Connect(this.pipeName, this.PipeConnectCallback, state);
            }
            catch (Exception exception)
            {
                lock (this.SyncRoot)
                {
                    this.CloseBaseSocket();
                }

                this.OnConnectionAcceptError(exception);
                state.StartTcs?.TrySetException(exception);
            }
        }

        private void PipeConnectCallback(UvPipeConnectRequest request, Exception error, object stateObject)
        {
            var state = (ListenState)stateObject;
            try
            {
                if (error != null)
                {
                    throw error;
                }

                request.BaseHandle.ReadStart(this.AllocCallback, this.ReadCallback, stateObject);
                state.StartTcs?.TrySetResult(null);
            }
            catch (Exception exception)
            {
                lock (this.SyncRoot)
                {
                    this.CloseBaseSocket();
                }

                this.OnConnectionAcceptError(exception);
                state.StartTcs?.TrySetException(exception);
            }
        }

        private ArraySegment<byte> AllocCallback(UvStream stream, int suggestedSize, object stateObject)
        {
            return new ArraySegment<byte>(this.pipeAcceptBuffer);
        }

        private void ReadCallback(UvStream stream, int nread, Exception error, object stateObject)
        {
            UvTcp client = null;
            try
            {
                if (error != null)
                {
                    throw error;
                }

                // When receiving a TCP handle over an IPC pipe the accept logic is slightly different:
                // First we receive some data over the pipe (usually some dummy message)
                // and then we check if we got some handle as well by ckecking the UvPipe.PendingCount.
                // If we do, we can simply accept the pending handle as usual.
                var pipe = (UvPipe)stream;
                if (pipe.PendingCount() > 0)
                {
                    client = new UvTcp(this.Worker.Loop);
                    pipe.Accept(client);

                    this.OnNewConnectionAccepted(client);
                }
            }
            catch (Exception exception)
            {
                lock (this.SyncRoot)
                {
                    this.CloseBaseSocket();
                }

                client?.Close();

                if (exception is UvErrorException errorException && errorException.ErrorCode == UvErrorCode.EOF)
                {
                    // Ignore EOF errors
                }
                else
                {
                    this.OnConnectionAcceptError(exception);
                }
            }
        }
    }
}
