using System;

namespace BaldurToolkit.Network.Libuv
{
    public class LoopErrorEventArgs : EventArgs
    {
        public LoopErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
