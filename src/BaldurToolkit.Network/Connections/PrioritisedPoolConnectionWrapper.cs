using System;
using System.Net;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public class PrioritisedPoolConnectionWrapper<TConnection> : IConnection
        where TConnection : class, IConnection
    {
        public PrioritisedPoolConnectionWrapper(TConnection connection, ConnectionPool<TConnection> fallbackPool)
        {
            this.Connection = connection;
            this.Pool = fallbackPool;
        }

        public PrioritisedPoolConnectionWrapper(ConnectionPool<TConnection> pool)
            : this(pool.Request(), pool)
        {
        }

        public event EventHandler<ConnectionClosedEventArgs> Closed
        {
            add => this.Pool.Closed += value;
            remove => this.Pool.Closed -= value;
        }

        public bool IsOpen => this.Pool.IsOpen;

        public EndPoint RemoteEndPoint => this.Pool.RemoteEndPoint;

        protected TConnection Connection { get; set; }

        protected ConnectionPool<TConnection> Pool { get; }

        public void Close()
        {
            this.Pool.Close();
        }

        public void Send(IPacket packet)
        {
            var connection = this.Connection;
            if (!connection.IsOpen)
            {
                connection = this.Connection = this.Pool.Request();
            }

            connection.Send(packet);
        }
    }
}
