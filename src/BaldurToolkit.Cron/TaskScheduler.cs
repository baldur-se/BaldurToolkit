using System;
using System.Collections.Generic;
using System.Threading;

namespace BaldurToolkit.Cron
{
    public class TaskScheduler
    {
        private readonly object syncRoot = new object();
        private readonly Dictionary<string, ITask> scheduledTasks = new Dictionary<string, ITask>();

        private Thread timerThread;
        private bool isRunning = false;
        private ManualResetEvent timerThreadExitResetEvent;

        /// <summary>
        /// Occurs when a task has been successfully executed.
        /// </summary>
        public event EventHandler<TaskExecutedEventArgs> TaskExecuted;

        /// <summary>
        /// Occurs when a task has been failed during it's execution.
        /// </summary>
        public event EventHandler<TaskExecutionFailEventArgs> TaskExecutionFail;

        /// <summary>
        /// Gets the amount of scheduled tasks in the current cron manager.
        /// </summary>
        public int ActiveTaskCount
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.scheduledTasks.Count;
                }
            }
        }

        /// <summary>
        /// Forcibly stops current cron manager.
        /// </summary>
        public void Stop()
        {
            lock (this.syncRoot)
            {
                this.scheduledTasks.Clear();
                this.StopThread();
            }
        }

        /// <summary>
        /// Schedules a task.
        /// </summary>
        /// <param name="task">The sask to schedule.</param>
        public void ScheduleTask(ITask task)
        {
            lock (this.syncRoot)
            {
                this.scheduledTasks.Add(task.Name, task);
                if (this.scheduledTasks.Count == 1)
                {
                    this.StartThread();
                }
            }
        }

        /// <summary>
        /// Schedules a new cron task.
        /// </summary>
        /// <remarks>
        /// Cron expression format:
        /// <code>
        /// * * * * *
        /// - - - - -
        /// | | | | |
        /// | | | | ----- Day of week (0 - 7 or Sun - Sat) (Sunday is 0 or 7)
        /// | | | ------- Month (1 - 12 or Jan - Dec)
        /// | | --------- Day of month (1 - 31)
        /// | ----------- Hour (0 - 23)
        /// ------------- Minute (0 - 59)
        /// </code>
        /// Each field can be in one of two formats: <code>*[/interval]</code> or <code>start[-end[/interval]]</code>.
        /// In one field you can specify multiple comma-separated entries (eg <code>12-15,19,22</code>).
        /// </remarks>
        /// <param name="name">Task name.</param>
        /// <param name="cronExpressionText">Cron-like expression.</param>
        /// <param name="callback">Delegate to be executed at specified time.</param>
        public void ScheduleCronTask(string name, string cronExpressionText, Action callback)
        {
            this.ScheduleTask(new CronTask(name, cronExpressionText, callback));
        }

        /// <summary>
        /// Removes scheduled task with specified name;
        /// </summary>
        /// <param name="name">Task name.</param>
        /// <returns>True if removal was successful.</returns>
        public bool RemoveTask(string name)
        {
            lock (this.syncRoot)
            {
                var removed = this.scheduledTasks.Remove(name);
                if (removed && this.scheduledTasks.Count == 0)
                {
                    this.StopThread();
                }

                return removed;
            }
        }

        private static int SynchronizeIntervalWith(DateTime time)
        {
            return (60 * 1000) - ((time.Second * 1000) + time.Millisecond);
        }

        private void StartThread()
        {
            if (this.timerThread == null || this.isRunning == false)
            {
                this.isRunning = true;
                this.timerThreadExitResetEvent = new ManualResetEvent(false);
                this.timerThread = new Thread(this.Worker)
                {
                    IsBackground = true,
                };
                this.timerThread.Start();
            }
        }

        private void StopThread()
        {
            this.isRunning = false;
            this.timerThreadExitResetEvent?.Set();
            this.timerThreadExitResetEvent = null;
        }

        private void Worker()
        {
            while (true)
            {
                var now = DateTime.Now;
                var toExecute = new List<ITask>();

                lock (this.syncRoot)
                {
                    if (!this.isRunning)
                    {
                        return;
                    }

                    foreach (var item in this.scheduledTasks)
                    {
                        if (item.Value.IsActiveAt(now))
                        {
                            toExecute.Add(item.Value);
                        }
                    }
                }

                // Execute active tasks in the thread pool
                foreach (var item in toExecute)
                {
                    var task = item;
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        try
                        {
                            task.Execute();
                            this.TaskExecuted?.Invoke(this, new TaskExecutedEventArgs(task));
                        }
                        catch (Exception exception)
                        {
                            this.TaskExecutionFail?.Invoke(this, new TaskExecutionFailEventArgs(task, exception));
                        }
                    });
                }

                this.timerThreadExitResetEvent?.WaitOne(SynchronizeIntervalWith(DateTime.Now));
            }
        }
    }
}
