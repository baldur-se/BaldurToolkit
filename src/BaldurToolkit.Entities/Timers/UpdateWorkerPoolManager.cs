using System;
using System.Collections.Generic;

namespace BaldurToolkit.Entities.Timers
{
    public class UpdateWorkerPoolManager : IUpdateWorker
    {
        private readonly List<WorkerInfo> workers = new List<WorkerInfo>();

        public event EventHandler<UpdateErrorEventArgs> UpdateError;

        public void Add(Func<IUpdateable, bool> predicate, IUpdateWorker worker)
        {
            worker.UpdateError += this.OnWorkerUpdateError;

            this.workers.Add(new WorkerInfo(predicate, worker));
        }

        public bool Register(IUpdateable updateable)
        {
            foreach (var workerInfo in this.workers)
            {
                if (workerInfo.Predicate(updateable))
                {
                    return workerInfo.Worker.Register(updateable);
                }
            }

            throw new ArgumentException($"Unable to find suitable worker for {updateable}.");
        }

        public bool Remove(IUpdateable updateable)
        {
            foreach (var workerInfo in this.workers)
            {
                if (workerInfo.Predicate(updateable))
                {
                    return workerInfo.Worker.Remove(updateable);
                }
            }

            throw new ArgumentException($"Unable to find suitable worker for {updateable}.");
        }

        public void Start()
        {
            foreach (var workerInfo in this.workers)
            {
                workerInfo.Worker.Start();
            }
        }

        public void Stop()
        {
            foreach (var workerInfo in this.workers)
            {
                workerInfo.Worker.Stop();
            }
        }

        private void OnWorkerUpdateError(object sender, UpdateErrorEventArgs args)
        {
            this.UpdateError?.Invoke(this, args);
        }

        protected struct WorkerInfo
        {
            public readonly Func<IUpdateable, bool> Predicate;
            public readonly IUpdateWorker Worker;

            public WorkerInfo(Func<IUpdateable, bool> predicate, IUpdateWorker worker)
            {
                this.Predicate = predicate;
                this.Worker = worker;
            }
        }
    }
}
