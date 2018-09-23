using System;

namespace BaldurToolkit.Entities
{
    public class DeltaTime
    {
        public DeltaTime()
        {
            this.TotalTime = TimeSpan.Zero;
            this.ElapsedTime = TimeSpan.Zero;
        }

        public DeltaTime(TimeSpan totalTime, TimeSpan elapsedTime)
        {
            this.TotalTime = totalTime;
            this.ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Gets or sets total time since the world system start.
        /// </summary>
        public TimeSpan TotalTime { get; set; }

        /// <summary>
        /// Gets or sets time elapsed from the previous update tick.
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
    }
}
