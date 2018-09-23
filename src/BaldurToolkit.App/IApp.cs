using System;

namespace BaldurToolkit.App
{
    public interface IApp
    {
        /// <summary>
        /// Occurs when app finishes the starting operation.
        /// </summary>
        event EventHandler Started;

        /// <summary>
        /// Occurs when app begins the stopping operation.
        /// The app required to invoke registered event handlers in the LIFO order.
        /// </summary>
        event EventHandler Stopping;

        /// <summary>
        /// Occurs when app finishes the stopping operation.
        /// The app required to invoke registered event handlers in the LIFO order.
        /// </summary>
        event EventHandler Stopped;

        /// <summary>
        /// Gets the app name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the name of the app instance.
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        /// Gets the GUID of current app instance.
        /// </summary>
        Guid InstanceGuid { get; }

        /// <summary>
        /// Gets a value indicating whether the app is in a running state.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the app start time.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Gets the app uptime (elapsed time since last start).
        /// </summary>
        TimeSpan Uptime { get; }

        /// <summary>
        /// Starts current app instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops current app instance.
        /// </summary>
        void Stop();
    }
}
