using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionRequestedEventArgs<TConnection> : EventArgs
        where TConnection : class, IConnection
    {
        public ConnectionRequestedEventArgs(ConnectionRequest<TConnection> connectionRequest)
        {
            this.ConnectionRequest = connectionRequest;
        }

        public ConnectionRequest<TConnection> ConnectionRequest { get; }
    }
}
