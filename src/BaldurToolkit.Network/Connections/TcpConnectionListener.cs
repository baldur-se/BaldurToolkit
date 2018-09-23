using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BaldurToolkit.Network.Connections
{
    public class TcpConnectionListener<TConnection> : ConnectionListener<TConnection>, IDisposable
        where TConnection : TcpConnection
    {
        private readonly object syncRoot = new object();

        private IPEndPoint endPoint;
        private int maxConnections = -1;
        private int maxPendingConnections = 128;

        public TcpConnectionListener(IPEndPoint endPoint, Func<Socket, TConnection> connectionFactory)
            : this(endPoint)
        {
            this.ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public TcpConnectionListener(IPAddress host, int port, Func<Socket, TConnection> connectionFactory)
            : this(host, port)
        {
            this.ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public TcpConnectionListener(string host, int port, Func<Socket, TConnection> connectionFactory)
            : this(host, port)
        {
            this.ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected TcpConnectionListener(string host, int port)
            : this(IPAddress.Parse(host), port)
        {
        }

        protected TcpConnectionListener(IPAddress host, int port)
            : this(new IPEndPoint(host, port))
        {
        }

        protected TcpConnectionListener(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
        }

        ~TcpConnectionListener()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets maximum number of working connections.
        /// Set to -1 to disable limit.
        /// Default value is -1.
        /// </summary>
        public int MaxConnections
        {
            get => this.maxConnections;
            set
            {
                if (value < -1 || value == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.maxConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of pending connections queue (backlog).
        /// Default value is 128.
        /// </summary>
        public int MaxPendingConnections
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.maxPendingConnections;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    if (this.BaseSocket != null)
                    {
                        throw new Exception("Can not change end point while listening. Please stop listening first.");
                    }

                    this.maxPendingConnections = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets IPEndPoint for listening socket.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get => this.endPoint;
            set
            {
                lock (this.syncRoot)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    if (this.BaseSocket != null)
                    {
                        throw new Exception("Can not change end point while listening. Please stop listening first.");
                    }

                    this.endPoint = value;
                }
            }
        }

        public override bool IsListening
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.BaseSocket != null;
                }
            }
        }

        protected Func<Socket, TConnection> ConnectionFactory { get; set; }

        protected Socket BaseSocket { get; set; }

        public override Task StartListening()
        {
            lock (this.syncRoot)
            {
                if (this.BaseSocket != null)
                {
                    throw new Exception("Already listening a socket. Please stop listening first.");
                }

                this.BaseSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.BaseSocket.Bind(this.endPoint);
                this.BaseSocket.Listen(this.maxPendingConnections);
            }

            this.BeginAcceptConnection();

            return Task.CompletedTask;
        }

        public override Task StopListening()
        {
            lock (this.syncRoot)
            {
                this.CloseBaseSocket();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnNewConnectionAccepted(Socket socket)
        {
            if (this.MaxConnections != -1 && this.ConnectionCount >= this.MaxConnections)
            {
                socket.Dispose();
                return;
            }

            var connection = this.CreateConnection(socket);

            this.OnConnectionCreated(connection);

            connection.BeginReceive();
        }

        protected virtual TConnection CreateConnection(Socket socket)
        {
            return this.ConnectionFactory(socket);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            lock (this.syncRoot)
            {
                this.CloseBaseSocket();
            }
        }

        private void BeginAcceptConnection()
        {
            var baseSocket = this.BaseSocket;
            baseSocket?.BeginAccept(this.AcceptConnectionCallback, null);
        }

        private void AcceptConnectionCallback(IAsyncResult asyncResult)
        {
            var baseSocket = this.BaseSocket;
            if (baseSocket != null)
            {
                try
                {
                    var socket = baseSocket.EndAccept(asyncResult);

                    this.OnNewConnectionAccepted(socket);
                    this.BeginAcceptConnection();
                }
                catch (Exception exception)
                {
                    lock (this.syncRoot)
                    {
                        this.CloseBaseSocket();
                    }

                    this.OnConnectionAcceptError(exception);
                }
            }
        }

        private void CloseBaseSocket()
        {
            var baseSocket = this.BaseSocket;
            this.BaseSocket = null;

            baseSocket?.Dispose();
        }
    }
}
