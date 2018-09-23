using System;
using System.Collections.Generic;

namespace BaldurToolkit.Network.Libuv
{
    public class LibuvWorkerPool
    {
        private readonly object syncRoot = new object();

        private readonly List<LibuvWorker> workers = new List<LibuvWorker>();

        private int index = 0;

        public IList<LibuvWorker> Workers
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.workers.ToArray();
                }
            }
        }

        public void Register(LibuvWorker worker)
        {
            lock (this.syncRoot)
            {
                this.workers.Add(worker);
            }
        }

        public LibuvWorker Request()
        {
            lock (this.syncRoot)
            {
                var count = this.workers.Count;
                if (count == 0)
                {
                    throw new Exception("Libuv worker pool is empty.");
                }

                return this.workers[this.index++ % count];
            }
        }
    }
}
