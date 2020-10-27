using System;
using System.Diagnostics;
using System.Threading;

namespace BaldurToolkit.Entities.Timers
{
    public abstract class BaseUpdateWorker : IUpdateWorker
    {
        private readonly object syncRoot = new object();
        private readonly TimeSpan updateInterval;
        private Thread workerThread;
        private ManualResetEvent workerThreadExitResetEvent;
        private bool running;

        public BaseUpdateWorker(TimeSpan updateInterval)
        {
            this.updateInterval = updateInterval;
        }

        public event EventHandler<UpdateErrorEventArgs> UpdateError;

        public string Name { get; set; }

        public IUpdateTimeTracker Tracker { get; set; }

        public void Start()
        {
            lock (this.syncRoot)
            {
                if (this.running)
                {
                    throw new Exception("The worker is already started.");
                }

                this.running = true;
                this.workerThreadExitResetEvent = new ManualResetEvent(false);
            }

            this.workerThread = new Thread(this.Worker)
            {
                IsBackground = false,
            };

            if (this.Name != null)
            {
                this.workerThread.Name = this.Name;
            }

            this.workerThread.Start();
        }

        public void Stop()
        {
            lock (this.syncRoot)
            {
                this.running = false;
                this.workerThreadExitResetEvent?.Set();
                this.workerThreadExitResetEvent = null;
            }
        }

        public abstract bool Register(IUpdateable updateable);

        public abstract bool Remove(IUpdateable updateable);

        protected abstract void DoUpdate(DeltaTime deltaTime);

        protected void OnUpdateError(Exception exception)
        {
            this.UpdateError?.Invoke(this, new UpdateErrorEventArgs(exception));
        }

        private void Worker()
        {
            try
            {
                var stopwatch = new Stopwatch();
                var deltaTime = new DeltaTime();

                while (this.running)
                {
                    var elapsed = stopwatch.Elapsed;
                    stopwatch.Reset();
                    stopwatch.Start();

                    deltaTime.ElapsedTime = elapsed;
                    deltaTime.TotalTime += elapsed;

                    try
                    {
                        this.DoUpdate(deltaTime);
                    }
                    catch (Exception exception)
                    {
                        this.OnUpdateError(exception);
                    }

                    var updateTime = stopwatch.Elapsed;
                    var timeToSleep = (int)(this.updateInterval - updateTime).TotalMilliseconds;
                    if (timeToSleep < 1)
                    {
                        timeToSleep = 1;
                    }

                    this.Tracker?.AddValue(updateTime);

                    if (this.running)
                    {
                        this.workerThreadExitResetEvent?.WaitOne(timeToSleep);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}
