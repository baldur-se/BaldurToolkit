using System;
using System.Collections.Generic;
using System.Net;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionPool<TConnection> : IConnection
        where TConnection : class, IConnection
    {
        private int nextIndex;

        /// <summary>
        /// Occurs when connection count decreased to 0.
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> Closed;

        /// <summary>
        /// Occurs when connection count is increased to 1.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Gets a value indicating whether the connection is open.
        /// </summary>
        public bool IsOpen => this.ConnectionsCount > 0;

        /// <summary>
        /// Gets the remote EndPoint.
        /// </summary>
        public virtual EndPoint RemoteEndPoint => null;

        /// <summary>
        /// Gets connections count.
        /// </summary>
        public int ConnectionsCount
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return this.Connections.Count;
                }
            }
        }

        protected object SyncRoot { get; } = new object();

        protected List<TConnection> Connections { get; } = new List<TConnection>();

        /// <summary>
        /// Requests a connection from the pool and asynchronously sends a packet over requested connection.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        public virtual void Send(IPacket packet)
        {
            this.Request().Send(packet);
        }

        /// <summary>
        /// Request next connection from the pool.
        /// </summary>
        /// <returns>Connection</returns>
        public virtual TConnection Request()
        {
            if (!this.TryRequest(out var connection))
            {
                throw new EmptyConnectionPoolException("Can not request connection: pool is empty.");
            }

            return connection;
        }

        /// <summary>
        /// Tries to request next connection from the pool (if any).
        /// </summary>
        /// <param name="connection">Requested connection.</param>
        /// <returns>True if successfully requested a connection, false if connection pool is empty.</returns>
        public virtual bool TryRequest(out TConnection connection)
        {
            connection = default(TConnection);
            lock (this.SyncRoot)
            {
                if (this.Connections.Count == 0)
                {
                    return false;
                }

                if (this.nextIndex >= this.Connections.Count)
                {
                    this.nextIndex = 0;
                }

                connection = this.Connections[this.nextIndex++];
                return true;
            }
        }

        /// <summary>
        /// Add connection to the pool.
        /// The connection will be automatically removed from the pool when it closes.
        /// </summary>
        /// <param name="connection">Connection</param>
        public virtual void AddConnection(TConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            connection.Closed += this.OnConnectionClosed;
            int connectionCount;
            lock (this.SyncRoot)
            {
                this.Connections.Add(connection);
                connectionCount = this.Connections.Count;
            }

            if (connectionCount == 1)
            {
                this.Opened?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Remove connection from pool.
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <returns>True if item is successfully removed; otherwise, False.</returns>
        public virtual bool RemoveConnection(TConnection connection)
        {
            bool success;
            int connectionCount;
            lock (this.SyncRoot)
            {
                success = this.Connections.Remove(connection);
                connectionCount = this.Connections.Count;
            }

            if (success)
            {
                connection.Closed -= this.OnConnectionClosed;
                if (connectionCount == 0)
                {
                    this.Closed?.Invoke(this, ConnectionClosedEventArgs.Empty);
                }
            }

            return success;
        }

        /// <summary>
        /// Closes all connections in the pool.
        /// </summary>
        public virtual void Close()
        {
            IEnumerable<TConnection> connections;
            lock (this.SyncRoot)
            {
                connections = this.Connections.ToArray();
            }

            foreach (var connection in connections)
            {
                connection.Close();
            }
        }

        public override string ToString()
        {
            if (this.RemoteEndPoint != null)
            {
                return $"{base.ToString()}({this.RemoteEndPoint})";
            }

            return base.ToString();
        }

        private void OnConnectionClosed(object sender, EventArgs eventArgs)
        {
            this.RemoveConnection((TConnection)sender);
        }
    }
}
