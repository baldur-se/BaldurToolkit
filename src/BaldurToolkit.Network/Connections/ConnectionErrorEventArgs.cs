using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionErrorEventArgs : EventArgs
    {
        public static readonly new ConnectionErrorEventArgs Empty = new ConnectionErrorEventArgs();

        public ConnectionErrorEventArgs()
        {
        }

        public ConnectionErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
