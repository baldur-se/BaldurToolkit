using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaldurToolkit.Network.Connections
{
    /// <summary>
    /// Provides basic functionality for connection listeners.
    /// </summary>
    /// <typeparam name="TConnection">The type of the connection.</typeparam>
    public abstract class ConnectionListener<TConnection> : IConnectionListener
        where TConnection : class, IConnection
    {
        private readonly HashSet<TConnection> openedConnections = new HashSet<TConnection>();

        /// <summary>
        /// Occurs when new connection accepted.
        /// </summary>
        public event EventHandler<ConnectionEventArgs<TConnection>> ConnectionAccepted;

        /// <summary>
        /// Occurs when accepted connection was closed.
        /// </summary>
        public event EventHandler<ConnectionEventArgs<TConnection>> ConnectionClosed;

        /// <summary>
        /// Occurs on connection error.
        /// </summary>
        public event EventHandler<ConnectionErrorEventArgs> ConnectionError;

        /// <inheritdoc />
        public int ConnectionCount
        {
            get
            {
                lock (this.openedConnections)
                {
                    return this.openedConnections.Count;
                }
            }
        }

        /// <summary>
        /// Gets list of all working connections.
        /// </summary>
        public IEnumerable<TConnection> OpenedConnections
        {
            get
            {
                lock (this.openedConnections)
                {
                    return this.openedConnections.ToArray();
                }
            }
        }

        /// <inheritdoc />
        IEnumerable<IConnection> IConnectionListener.OpenedConnections
        {
            get
            {
                lock (this.openedConnections)
                {
                    foreach (var connection in this.openedConnections)
                    {
                        yield return connection;
                    }
                }
            }
        }

        /// <inheritdoc />
        public abstract bool IsListening { get; }

        /// <inheritdoc />
        public abstract Task StartListening();

        /// <inheritdoc />
        public abstract Task StopListening();

        /// <inheritdoc />
        public virtual Task DisconnectAll()
        {
            var tcs = new TaskCompletionSource<object>();

            HashSet<TConnection> connectionsToClose;
            lock (this.openedConnections)
            {
                connectionsToClose = new HashSet<TConnection>(this.openedConnections);
            }

            if (connectionsToClose.Count == 0)
            {
                tcs.SetResult(null);
            }
            else
            {
                this.ConnectionClosed += (sender, args) =>
                {
                    lock (connectionsToClose)
                    {
                        connectionsToClose.Remove(args.Connection);
                        if (connectionsToClose.Count > 0)
                        {
                            return;
                        }
                    }

                    tcs.TrySetResult(null);
                };
                foreach (var connection in connectionsToClose.ToArray())
                {
                    connection.Close();
                }
            }

            return tcs.Task;
        }

        /// <summary>
        /// Must be called in derived class when a new connection accepted and a new connection object is created.
        /// </summary>
        /// <param name="connection">The connection.</param>
        protected virtual void OnConnectionCreated(TConnection connection)
        {
            lock (this.openedConnections)
            {
                this.openedConnections.Add(connection);
            }

            connection.Closed += this.OnConnectionClosed;

            this.ConnectionAccepted?.Invoke(this, new ConnectionEventArgs<TConnection>(connection));
        }

        /// <summary>
        /// Must be called in derived clas when a connection accept error occurs.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected virtual void OnConnectionAcceptError(Exception exception)
        {
            this.ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(exception));
        }

        /// <summary>
        /// Invokes when a connection is closed.
        /// By default, removes connection from opened connection list and invokes a <see cref="ConnectionClosed"/> event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event args.</param>
        protected virtual void OnConnectionClosed(object sender, EventArgs args)
        {
            var connection = (TConnection)sender;
            lock (this.openedConnections)
            {
                this.openedConnections.Remove(connection);
            }

            this.ConnectionClosed?.Invoke(this, new ConnectionEventArgs<TConnection>(connection));
        }
    }
}
