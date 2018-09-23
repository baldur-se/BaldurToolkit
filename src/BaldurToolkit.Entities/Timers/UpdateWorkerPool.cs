using System;
using System.Collections.Generic;
using System.Threading;

namespace BaldurToolkit.Entities.Timers
{
    public class UpdateWorkerPool : IUpdateWorker
    {
        private readonly List<IUpdateWorker> workers = new List<IUpdateWorker>();

        private bool isStarted;
        private int balanceCounter;

        public void Add(IUpdateWorker worker)
        {
            if (this.isStarted)
            {
                throw new Exception("Unable to add worker to the pool: pool is already started.");
            }

            this.workers.Add(worker);
        }

        public bool Register(IUpdateable updateable)
        {
            if (this.workers.Count == 0)
            {
                throw new Exception("Can not add entity to the worker pool: pool is empty.");
            }

            var index = Interlocked.Increment(ref this.balanceCounter) % this.workers.Count;
            return this.workers[index].Register(updateable);
        }

        public bool Remove(IUpdateable updateable)
        {
            if (this.workers.Count == 0)
            {
                throw new Exception("Can not remove entity from the worker pool: pool is empty.");
            }

            var removed = false;
            foreach (var worker in this.workers)
            {
                removed |= worker.Remove(updateable);
            }

            return removed;
        }

        public void Start()
        {
            if (this.isStarted)
            {
                return;
            }

            this.isStarted = true;

            foreach (var worker in this.workers)
            {
                worker.Start();
            }
        }

        public void Stop()
        {
            if (!this.isStarted)
            {
                return;
            }

            this.isStarted = false;

            foreach (var worker in this.workers)
            {
                worker.Stop();
            }
        }
    }
}
