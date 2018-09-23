using System;

namespace BaldurToolkit.Cron
{
    public class TaskExecutionFailEventArgs : EventArgs
    {
        public TaskExecutionFailEventArgs(ITask task, Exception exception)
        {
            this.Task = task;
            this.Exception = exception;
        }

        public ITask Task { get; }

        public Exception Exception { get; }
    }
}
