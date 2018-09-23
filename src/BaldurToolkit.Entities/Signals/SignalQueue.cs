using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BaldurToolkit.Entities.Signals
{
    public class SignalQueue<TSignal> : ISignalReceiver<TSignal>
    {
        private const int DefaultDelayQueueCapacity = 16;

        private static readonly TargetDeltaTimeComparer TargetDeltaTimeComparerValue = new TargetDeltaTimeComparer();

        private readonly object syncRoot = new object();
        private readonly ConcurrentQueue<SignalInfo> basicQueue;
        private readonly Internal.PriorityQueue<DelayedSignalInfo> delayedQueue;

        public SignalQueue()
        {
            this.basicQueue = new ConcurrentQueue<SignalInfo>();
            this.delayedQueue = new Internal.PriorityQueue<DelayedSignalInfo>(DefaultDelayQueueCapacity, TargetDeltaTimeComparerValue);
        }

        public int Count
        {
            get
            {
                var count = this.basicQueue.Count;
                lock (this.syncRoot)
                {
                    count += this.delayedQueue.Count;
                }

                return count;
            }
        }

        public void Enqueue(TSignal signal)
        {
            this.basicQueue.Enqueue(new SignalInfo(signal));
        }

        public void Enqueue(TSignal signal, TimeSpan delay)
        {
            this.basicQueue.Enqueue(new SignalInfo(signal, delay));
        }

        void ISignalReceiver<TSignal>.AddSignal(TSignal signal)
        {
            this.Enqueue(signal);
        }

        void ISignalReceiver<TSignal>.AddSignal(TSignal signal, TimeSpan delay)
        {
            this.Enqueue(signal, delay);
        }

        protected bool TryGetNext(out TSignal signal, DeltaTime deltaTime)
        {
            while (true)
            {
                if (this.basicQueue.TryDequeue(out var info))
                {
                    if (info.Delay != TimeSpan.Zero)
                    {
                        lock (this.syncRoot)
                        {
                            this.delayedQueue.Push(new DelayedSignalInfo(info.Signal, deltaTime.TotalTime.Add(info.Delay)));
                        }

                        continue;
                    }

                    signal = info.Signal;
                    return true;
                }

                lock (this.syncRoot)
                {
                    if (this.delayedQueue.Count > 0)
                    {
                        var item = this.delayedQueue.Top;
                        if (item.TargetDeltaTime <= deltaTime.TotalTime)
                        {
                            this.delayedQueue.Pop();
                            signal = item.Signal;
                            return true;
                        }
                    }
                }

                signal = default(TSignal);
                return false;
            }
        }

        private struct SignalInfo
        {
            public readonly TSignal Signal;
            public readonly TimeSpan Delay;

            public SignalInfo(TSignal signal)
            {
                this.Signal = signal;
                this.Delay = TimeSpan.Zero;
            }

            public SignalInfo(TSignal signal, TimeSpan delay)
            {
                this.Signal = signal;
                this.Delay = delay;
            }
        }

        private struct DelayedSignalInfo
        {
            public readonly TSignal Signal;
            public readonly TimeSpan TargetDeltaTime;

            public DelayedSignalInfo(TSignal signal, TimeSpan targetTotalTime)
            {
                this.Signal = signal;
                this.TargetDeltaTime = targetTotalTime;
            }
        }

        private sealed class TargetDeltaTimeComparer : IComparer<DelayedSignalInfo>
        {
            public int Compare(DelayedSignalInfo x, DelayedSignalInfo y)
            {
                return x.TargetDeltaTime.CompareTo(y.TargetDeltaTime);
            }
        }
    }
}
