using System;

namespace BaldurToolkit.Entities.Timers
{
    public class UpdateErrorEventArgs : EventArgs
    {
        public UpdateErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; }
    }
}
