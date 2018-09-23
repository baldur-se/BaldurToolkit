using System;
using System.Net;
using NLibuv;

namespace BaldurToolkit.Network.Libuv.Connections
{
    public class LibuvTcpConnectionListener<TConnection> : LibuvConnectionListener<TConnection>
        where TConnection : LibuvConnection
    {
        private IPEndPoint endPoint;
        private int maxPendingConnections = 128;

        public LibuvTcpConnectionListener(LibuvWorker worker, IPEndPoint endPoint, Func<UvTcp, LibuvWorker, TConnection> connectionFactory)
            : base(worker, connectionFactory)
        {
            this.EndPoint = endPoint;
        }

        protected LibuvTcpConnectionListener(LibuvWorker worker, IPEndPoint endPoint)
            : base(worker)
        {
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// Gets or sets the maximum number of pending connections queue (backlog).
        /// Default value is 128.
        /// </summary>
        public int MaxPendingConnections
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return this.maxPendingConnections;
                }
            }

            set
            {
                lock (this.SyncRoot)
                {
                    if (this.State != null)
                    {
                        throw new Exception("Can not change backlog configuration while listening. Please stop listening first.");
                    }

                    this.maxPendingConnections = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets EndPoint for listening socket.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get => this.endPoint;
            set
            {
                lock (this.SyncRoot)
                {
                    if (this.State != null)
                    {
                        throw new Exception("Can not change end point while listening. Please stop listening first.");
                    }

                    this.endPoint = value ?? throw new ArgumentNullException(nameof(value));
                }
            }
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
                var tcpSocket = new UvTcp(this.Worker.Loop);
                state.OpenedHandles.Add(tcpSocket);

                tcpSocket.Bind(this.endPoint);
                tcpSocket.Listen(this.maxPendingConnections, this.AcceptConnectionCallback);
                state.StartTcs?.TrySetResult(null);
            }
            catch (Exception exception)
            {
                lock (this.SyncRoot)
                {
                    this.CloseBaseSocket();
                }

                state.StartTcs?.TrySetException(exception);
                this.OnConnectionAcceptError(exception);
            }
        }

        private void AcceptConnectionCallback(UvNetworkStream server, Exception error, object stateObject)
        {
            UvTcp client = null;
            try
            {
                if (error != null)
                {
                    throw error;
                }

                client = new UvTcp(this.Worker.Loop);
                server.Accept(client);

                this.OnNewConnectionAccepted(client);
            }
            catch (Exception exception)
            {
                lock (this.SyncRoot)
                {
                    this.CloseBaseSocket();
                }

                client?.Close();
                this.OnConnectionAcceptError(exception);
            }
        }
    }
}
