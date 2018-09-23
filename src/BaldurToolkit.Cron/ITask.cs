using System;

namespace BaldurToolkit.Cron
{
    public interface ITask
    {
        /// <summary>
        /// Gets the name of a task.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Check if this task should be executed at given time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>True if the task should be executed.</returns>
        bool IsActiveAt(DateTime time);

        /// <summary>
        /// Execute the task.
        /// </summary>
        void Execute();
    }
}
