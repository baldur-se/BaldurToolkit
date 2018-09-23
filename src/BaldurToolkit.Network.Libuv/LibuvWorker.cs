using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NLibuv;

namespace BaldurToolkit.Network.Libuv
{
    public class LibuvWorker
    {
        private const int MaxLoops = 8;

        private readonly object syncRoot = new object();

        private readonly Thread thread;
        private readonly TaskCompletionSource<object> threadTcs = new TaskCompletionSource<object>();

        private readonly ConcurrentQueue<Job> jobQueue = new ConcurrentQueue<Job>();
        private readonly ConcurrentQueue<Job> finalJobQueue = new ConcurrentQueue<Job>();
        private readonly ConcurrentQueue<WriteRequestJob> writeRequestQueue = new ConcurrentQueue<WriteRequestJob>();

        private bool isStarted;
        private UvLoop uvLoop;
        private UvAsync jobNotificationAsync;

        public LibuvWorker()
        {
            this.thread = new Thread(this.ThreadStart)
            {
                Name = "BaldurToolkit.LibuvWorker",
            };
        }

        public event EventHandler<LoopErrorEventArgs> LoopErrorOccured;

        public UvLoop Loop => this.uvLoop;

        public Task StartAsync()
        {
            lock (this.syncRoot)
            {
                if (this.isStarted)
                {
                    throw new InvalidOperationException("Worker is already started.");
                }

                this.isStarted = true;
            }

            var tcs = new TaskCompletionSource<int>();
            this.thread.Start(tcs);
            return tcs.Task;
        }

        public async Task StopAsync(TimeSpan timeout)
        {
            lock (this.syncRoot)
            {
                if (!this.isStarted)
                {
                    return;
                }
            }

            if (!this.threadTcs.Task.IsCompleted)
            {
                this.EnqueueFinalJob(_ => this.jobNotificationAsync.Close());
                if (!await WaitAsync(this.threadTcs.Task, TimeSpan.FromTicks((long)(timeout.Ticks * 0.8))).ConfigureAwait(false))
                {
                    this.EnqueueFinalJob(_ => this.uvLoop.Stop());
                    if (!await WaitAsync(this.threadTcs.Task, TimeSpan.FromTicks((long)(timeout.Ticks * 0.2))).ConfigureAwait(false))
                    {
                        throw new Exception("Failed to stop the libuv loop.");
                    }
                }
            }
        }

        public void EnqueueJob(Action<object> callback, object stateObject = null)
        {
            var job = new Job(
                callback ?? throw new ArgumentNullException(nameof(callback)),
                stateObject);

            this.jobQueue.Enqueue(job);
            this.jobNotificationAsync.Send();
        }

        public Task EnqueueJobAsync(Action<object> callback, object stateObject = null)
        {
            var job = new Job(
                callback ?? throw new ArgumentNullException(nameof(callback)),
                stateObject,
                new TaskCompletionSource<object>());

            this.jobQueue.Enqueue(job);
            this.jobNotificationAsync.Send();

            return job.CompletionSource.Task;
        }

        public void EnqueueFinalJob(Action<object> callback, object stateObject = null)
        {
            var job = new Job(
                callback ?? throw new ArgumentNullException(nameof(callback)),
                stateObject);

            this.finalJobQueue.Enqueue(job);
            this.jobNotificationAsync.Send();
        }

        public Task EnqueueFinalJobAsync(Action<object> callback, object stateObject = null)
        {
            var job = new Job(
                callback ?? throw new ArgumentNullException(nameof(callback)),
                stateObject,
                new TaskCompletionSource<object>());

            this.finalJobQueue.Enqueue(job);
            this.jobNotificationAsync.Send();

            return job.CompletionSource.Task;
        }

        public void EnqueueWriteRequest(UvWriteRequest writeRequest)
        {
            var job = new WriteRequestJob(writeRequest ?? throw new ArgumentNullException(nameof(writeRequest)));

            this.writeRequestQueue.Enqueue(job);
            this.jobNotificationAsync.Send();
        }

        protected virtual void OnLoopError(Exception exception)
        {
            this.LoopErrorOccured?.Invoke(this, new LoopErrorEventArgs(exception));
        }

        protected virtual void OnJobNotificationReceived(UvAsync async)
        {
            var loopsRemaining = MaxLoops;
            bool wasWork;
            do
            {
                wasWork = this.DoWork();
                loopsRemaining--;
            }
            while (wasWork && loopsRemaining > 0);
        }

        protected virtual bool DoWork()
        {
            var wasWork = false;
            {
                while (this.jobQueue.TryDequeue(out var job))
                {
                    try
                    {
                        job.Callback.Invoke(job.State);
                        if (job.CompletionSource != null)
                        {
                            job.CompletionSource.TrySetResult(null);
                        }

                        wasWork = true;
                    }
                    catch (Exception exception)
                    {
                        if (job.CompletionSource != null)
                        {
                            job.CompletionSource.TrySetException(exception);
                        }

                        this.OnLoopError(exception);
                    }
                }
            }

            {
                while (this.writeRequestQueue.TryDequeue(out var job))
                {
                    try
                    {
                        job.WriteRequest.Write();
                        wasWork = true;
                    }
                    catch (Exception exception)
                    {
                        this.OnLoopError(exception);
                    }
                }
            }

            {
                while (this.finalJobQueue.TryDequeue(out var job))
                {
                    try
                    {
                        job.Callback.Invoke(job.State);
                        if (job.CompletionSource != null)
                        {
                            job.CompletionSource.TrySetResult(null);
                        }

                        wasWork = true;
                    }
                    catch (Exception exception)
                    {
                        if (job.CompletionSource != null)
                        {
                            job.CompletionSource.TrySetException(exception);
                        }

                        this.OnLoopError(exception);
                    }
                }
            }

            return wasWork;
        }

        private static async Task<bool> WaitAsync(Task task, TimeSpan timeout)
        {
            return await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) == task;
        }

        private void ThreadStart(object state)
        {
            var tcs = (TaskCompletionSource<int>)state;
            try
            {
                this.uvLoop = new UvLoop();
                this.jobNotificationAsync = new UvAsync(this.uvLoop, this.OnJobNotificationReceived);
                this.uvLoop.Run(UvLoopRunMode.RunNowait); // Test run
                tcs.SetResult(0);
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
                return;
            }

            try
            {
                try
                {
                    this.uvLoop.Run();
                }
                catch (Exception exception)
                {
                    this.OnLoopError(exception);
                }

                try
                {
                    this.jobNotificationAsync.Close();
                    this.uvLoop.Run(UvLoopRunMode.RunNowait); // Run one more time to close all handles left

                    this.uvLoop.Close();
                }
                catch (Exception exception)
                {
                    this.OnLoopError(exception);
                }
            }
            finally
            {
                this.threadTcs.SetResult(null);
            }
        }

        private struct Job
        {
            public Action<object> Callback;
            public object State;
            public TaskCompletionSource<object> CompletionSource;

            public Job(Action<object> callback, object state, TaskCompletionSource<object> completionSource = null)
            {
                this.Callback = callback;
                this.State = state;
                this.CompletionSource = completionSource;
            }
        }

        private struct WriteRequestJob
        {
            public UvWriteRequest WriteRequest;

            public WriteRequestJob(UvWriteRequest writeRequest)
            {
                this.WriteRequest = writeRequest;
            }
        }
    }
}
