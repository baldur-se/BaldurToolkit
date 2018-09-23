using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionCreatedEventArgs : EventArgs
    {
        public ConnectionCreatedEventArgs(IConnection connection)
        {
            this.Connection = connection;
        }

        public IConnection Connection { get; }
    }
}
