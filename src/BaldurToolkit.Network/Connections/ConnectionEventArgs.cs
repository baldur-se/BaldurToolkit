using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionEventArgs<T> : EventArgs
        where T : IConnection
    {
        public ConnectionEventArgs(T connection)
        {
            this.Connection = connection;
        }

        public T Connection { get; }
    }
}
