using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionClosedEventArgs : EventArgs
    {
        public static new readonly ConnectionClosedEventArgs Empty = new ConnectionClosedEventArgs();

        public ConnectionClosedEventArgs()
        {
        }

        public ConnectionClosedEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
