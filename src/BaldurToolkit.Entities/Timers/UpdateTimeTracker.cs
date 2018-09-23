using System;

namespace BaldurToolkit.Entities.Timers
{
    public class UpdateTimeTracker : IUpdateTimeTracker
    {
        private readonly TimeSpan[] items;
        private int count = 0;
        private int currentIndex = 0;
        private TimeSpan cachedSum = TimeSpan.Zero;
        private TimeSpan cachedPeak = TimeSpan.Zero;

        public UpdateTimeTracker(int historySize = 100)
        {
            this.items = new TimeSpan[historySize];
        }

        public TimeSpan Average
        {
            get
            {
                lock (this.items)
                    return this.count != 0
                        ? new TimeSpan(this.cachedSum.Ticks / this.count)
                        : TimeSpan.Zero;
            }
        }

        public TimeSpan Peak
        {
            get
            {
                lock (this.items)
                {
                    return this.cachedPeak;
                }
            }
        }

        public void AddValue(TimeSpan value)
        {
            lock (this.items)
            {
                if (this.count == this.items.Length)
                {
                    this.cachedSum -= this.items[this.currentIndex];
                }
                else
                {
                    this.count++;
                }

                this.items[this.currentIndex] = value;
                this.cachedSum += value;

                if (++this.currentIndex == this.items.Length)
                {
                    this.currentIndex = 0;
                }

                if (value > this.cachedPeak)
                {
                    this.cachedPeak = value;
                }
            }
        }
    }
}
