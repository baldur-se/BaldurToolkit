using System;
using System.Threading;

namespace BaldurToolkit.Network.Connections
{
    /// <summary>
    /// A connection pool that tries to keep specified amount of connections open.
    /// You should subscribe to the <see cref="ConnectionRequested"/> event to either complete or fail the connection request.
    /// </summary>
    /// <typeparam name="TConnection">The type of the connection.</typeparam>
    public class ActiveConnectionPool<TConnection> : ConnectionPool<TConnection>
        where TConnection : class, IConnection
    {
        private Timer timer;

        private int minConnections = Environment.ProcessorCount;

        /// <summary>
        /// Occurs when a new connection is being created.
        /// </summary>
        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;

        /// <summary>
        /// Occurs when connection creation fails.
        /// </summary>
        public event EventHandler<ConnectionErrorEventArgs> ConnectionError;

        /// <summary>
        /// Occurs when a new connection is being requested by the pool.
        /// </summary>
        public event EventHandler<ConnectionRequestedEventArgs<TConnection>> ConnectionRequested;

        /// <summary>
        /// Gets or sets the number of connections to create.
        /// Set this value to -1 to make it equal to the amount of processors available.
        /// </summary>
        public int MinConnections
        {
            get => this.minConnections;
            set
            {
                if (value == -1)
                {
                    value = Environment.ProcessorCount;
                }

                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.minConnections = value;
            }
        }

        /// <summary>
        /// Gets or sets the period to check connections (in milliseconds). Default is 10 seconds.
        /// </summary>
        public int CheckPeriod { get; set; } = 10000;

        /// <summary>
        /// Opens the connection pool.
        /// </summary>
        public void Open()
        {
            lock (this.SyncRoot)
            {
                if (this.timer != null)
                {
                    return;
                }

                this.timer = new Timer(this.CheckConnectionsCallback, null, 0, this.CheckPeriod);
            }
        }

        /// <summary>
        /// Closes connection pool and closes all existing connection.
        /// </summary>
        public override void Close()
        {
            lock (this.SyncRoot)
            {
                if (this.timer == null)
                {
                    // The pool is already closed.
                    return;
                }

                this.timer.Dispose();
                this.timer = null;
            }

            base.Close();
        }

        protected virtual void OnConnectionRequested(ConnectionRequest<TConnection> request)
        {
            this.ConnectionRequested?.Invoke(this, new ConnectionRequestedEventArgs<TConnection>(request));
        }

        private void CheckConnectionsCallback(object state)
        {
            try
            {
                int connectionsToCreate = 0;
                lock (this.SyncRoot)
                {
                    if (this.timer != null)
                    {
                        connectionsToCreate = this.MinConnections - this.Connections.Count;
                    }
                }

                for (int i = 0; i < connectionsToCreate; i++)
                {
                    var request = new ConnectionRequest<TConnection>();
                    request.ConnectionCreated += this.OnConnectionCreated;
                    request.ConnectionError += this.OnConnectionCreationError;

                    try
                    {
                        this.OnConnectionRequested(request);
                    }
                    catch (Exception exception)
                    {
                        request.Fail(exception);
                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                this.ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(exception));
            }
        }

        private void OnConnectionCreationError(object sender, ConnectionErrorEventArgs args)
        {
            this.ConnectionError?.Invoke(this, args);
        }

        private void OnConnectionCreated(object sender, ConnectionCreatedEventArgs args)
        {
            this.AddConnection((TConnection)args.Connection);
            this.ConnectionCreated?.Invoke(this, args);
        }
    }
}
