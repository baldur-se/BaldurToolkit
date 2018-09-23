using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.App
{
    public abstract class EmptyApp : IApp
    {
        private readonly object locker = new object();

        private readonly IList<EventHandler> stoppingEventHandlers = new List<EventHandler>();
        private readonly IList<EventHandler> stoppedEventHandlers = new List<EventHandler>();

        protected EmptyApp(string name, string instanceName)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.InstanceName = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
        }

        protected EmptyApp(AppIdentity appIdentity)
            : this(appIdentity.Name, appIdentity.InstanceName)
        {
        }

        /// <inheritdoc />
        public event EventHandler Started;

        /// <inheritdoc />
        public event EventHandler Stopping
        {
            add => this.stoppingEventHandlers.Add(value);
            remove => this.stoppingEventHandlers.Remove(value);
        }

        /// <inheritdoc />
        public event EventHandler Stopped
        {
            add => this.stoppedEventHandlers.Add(value);
            remove => this.stoppedEventHandlers.Remove(value);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string InstanceName { get; }

        /// <inheritdoc />
        public Guid InstanceGuid { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public DateTime StartTime { get; private set; }

        /// <inheritdoc />
        public TimeSpan Uptime => DateTime.Now - this.StartTime;

        /// <inheritdoc />
        public void Start()
        {
            lock (this.locker)
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("App is already running.");
                }

                this.IsRunning = true;
                this.StartTime = DateTime.Now;
            }

            this.OnStarting();
            var startedEvent = this.Started;
            startedEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Stop()
        {
            lock (this.locker)
            {
                if (!this.IsRunning)
                {
                    return;
                }

                this.IsRunning = false;
            }

            this.OnStopping();
            try
            {
                var handlers = this.stoppingEventHandlers.ToArray().Reverse();
                this.SafeExecuteEvent(handlers, this, EventArgs.Empty);
            }
            catch (AggregateException exception)
            {
                throw new Exception("Errors occurred during the application Stopping event.", exception);
            }

            this.OnStopped();
            try
            {
                var handlers = this.stoppedEventHandlers.ToArray().Reverse();
                this.SafeExecuteEvent(handlers, this, EventArgs.Empty);
            }
            catch (AggregateException exception)
            {
                throw new Exception("Errors occurred during the application Stopped event.", exception);
            }
        }

        public override string ToString()
        {
            return $"{this.Name} ({this.InstanceName})";
        }

        /// <summary>
        /// Executes when application begins the starting operation.
        /// This method will be invoked right before the <see cref="Started"/> event.
        /// </summary>
        protected virtual void OnStarting()
        {
        }

        /// <summary>
        /// Executes when application begins the stopping operation.
        /// This method will be invoked right before the <see cref="Stopping"/> event.
        /// </summary>
        protected virtual void OnStopping()
        {
        }

        /// <summary>
        /// Executes when application finishes the stopping operation.
        /// This method will be invoked right before the <see cref="Stopped"/> event.
        /// </summary>
        protected virtual void OnStopped()
        {
        }

        protected void SafeExecuteEvent(IEnumerable<EventHandler> eventHandlers, object sender, EventArgs args, bool aggregateExceptions = true)
        {
            if (eventHandlers == null)
            {
                return;
            }

            List<Exception> exceptions = null;

            foreach (var eventDelegate in eventHandlers)
            {
                try
                {
                    eventDelegate.Invoke(sender, args);
                }
                catch (Exception exception)
                {
                    if (!aggregateExceptions)
                    {
                        throw;
                    }

                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(exception);
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
