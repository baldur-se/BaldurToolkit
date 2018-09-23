using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BaldurToolkit.App;

namespace BaldurToolkit.AppRunner
{
    public class AppHost : IAppHost
    {
        protected const int ResetWaitTimeout = 5000;

        private readonly object syncRoot = new object();
        private readonly List<IApp> runningApps = new List<IApp>();
        private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
        private bool isRunning;

        ~AppHost()
        {
            this.Dispose(false);
        }

        public IReadOnlyList<IApp> RunningApps
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.runningApps.AsReadOnly();
                }
            }
        }

        public void Run(IEnumerable<IApp> apps)
        {
            lock (this.syncRoot)
            {
                if (this.isRunning)
                {
                    throw new Exception("Can not run an app: some app is already running.");
                }

                this.isRunning = true;
            }

            try
            {
                foreach (var app in apps)
                {
                    app.Stopped += this.OnAppStopped;
                    app.Start();

                    lock (this.syncRoot)
                    {
                        this.runningApps.Add(app);
                    }
                }

                this.resetEvent.Reset();

                while (true)
                {
                    lock (this.syncRoot)
                    {
                        if (!this.runningApps.Any())
                        {
                            break;
                        }
                    }

                    this.resetEvent.WaitOne(ResetWaitTimeout);
                }
            }
            finally
            {
                this.Stop();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Stop()
        {
            IEnumerable<IApp> appsToStop;
            lock (this.syncRoot)
            {
                this.isRunning = false;
                appsToStop = this.runningApps.ToArray();
            }

            foreach (var app in appsToStop)
            {
                app.Stop();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Stop();
        }

        protected virtual void OnAppStopped(object sender, EventArgs args)
        {
            var app = (IApp)sender;
            lock (this.syncRoot)
            {
                this.runningApps.Remove(app);
            }

            this.resetEvent.Set();
        }
    }
}
