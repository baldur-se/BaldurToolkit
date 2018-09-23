using System;

namespace BaldurToolkit.Entities
{
    public class PeriodicUpdateable : IUpdateable
    {
        private readonly DeltaTime ownDeltaTime = new DeltaTime();

        public PeriodicUpdateable(TimeSpan period, Action<DeltaTime> action)
        {
            this.Action = action;
            this.Period = period;
        }

        public TimeSpan Period { get; set; }

        protected Action<DeltaTime> Action { get; }

        public void Update(DeltaTime deltaTime)
        {
            this.ownDeltaTime.ElapsedTime += deltaTime.ElapsedTime;
            if (this.ownDeltaTime.ElapsedTime >= this.Period)
            {
                this.ownDeltaTime.TotalTime = deltaTime.TotalTime;
                this.Action.Invoke(this.ownDeltaTime);
                this.ownDeltaTime.ElapsedTime = TimeSpan.Zero;
            }
        }

        public void Reset()
        {
            this.ownDeltaTime.ElapsedTime = TimeSpan.Zero;
        }
    }
}
