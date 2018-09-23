using System;
using System.Collections.Generic;
using System.Net;
using NLibuv;

namespace BaldurToolkit.Network.Libuv.Connections
{
    public class LibuvTcpConnectionBroker<TConnection> : LibuvTcpConnectionListener<TConnection>
        where TConnection : LibuvConnection
    {
        private readonly List<UvPipe> workerPipes = new List<UvPipe>();
        private int index = 0;

        protected LibuvTcpConnectionBroker(LibuvWorker worker, IPEndPoint endPoint, string pipeName)
            : base(worker, endPoint)
        {
            this.PipeName = pipeName;
        }

        public string PipeName { get; }

        protected override void OnListeningStarted(ListenState state)
        {
            // Execute on the loop thread
            this.Worker.EnqueueJob(this.StartListeningCallback, state);
        }

        protected override void OnNewConnectionAccepted(UvTcp socket)
        {
            var targetPipe = this.GetNextWorkerPipe();
            targetPipe.WriteHandle(socket, this.WriteHandleCallback, socket);
        }

        private void StartListeningCallback(object stateObject)
        {
            var state = (ListenState)stateObject;
            try
            {
                // First we open a master pipe for incoming worker IPC connections.
                // Actual socket listening starts only when the first worker connection is accepted.
                var pipe = new UvPipe(this.Worker.Loop);
                state.OpenedHandles.Add(pipe);

                pipe.Bind(this.PipeName);
                pipe.Listen(128, this.AcceptPipeConnectionCallback, state);
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

        private void AcceptPipeConnectionCallback(UvNetworkStream server, Exception error, object stateObject)
        {
            var state = (ListenState)stateObject;
            try
            {
                var workerPipe = new UvPipe(this.Worker.Loop, true);
                state.OpenedHandles.Add(workerPipe);

                server.Accept(workerPipe);

                this.workerPipes.Add(workerPipe);
                if (this.workerPipes.Count == 1)
                {
                    // Start listening the TCP socket when we got our first worker
                    base.OnListeningStarted(state);
                }
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

        private UvPipe GetNextWorkerPipe()
        {
            var count = this.workerPipes.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("Can not find any worker pipes.");
            }

            return this.workerPipes[this.index++ % count];
        }

        private void WriteHandleCallback(UvWriteRequest request, Exception error, object stateObject)
        {
            try
            {
                if (error != null)
                {
                    throw error;
                }
            }
            catch (Exception exception)
            {
                this.OnConnectionAcceptError(exception);
            }
            finally
            {
                ((UvTcp)stateObject).Close();
            }
        }
    }
}
