using System;

namespace BaldurToolkit.Cron
{
    public class TaskExecutedEventArgs : EventArgs
    {
        public TaskExecutedEventArgs(ITask task)
        {
            this.Task = task;
        }

        public ITask Task { get; }
    }
}
