using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Actions
{
    public class Wait<TContext> : BaseContextAction<TContext>
        where TContext : IDeltaTimeContext
    {
        private TimeSpan targetTick;

        public Wait(TimeSpan waitDuration)
        {
            this.WaitDuration = waitDuration;
        }

        public Wait(TimeSpan waitDuration, TContext context)
            : base(context)
        {
            this.WaitDuration = waitDuration;
        }

        public Wait(string name, TimeSpan waitDuration)
            : base(name)
        {
            this.WaitDuration = waitDuration;
        }

        public Wait(string name, TimeSpan waitDuration, TContext context)
            : base(name, context)
        {
            this.WaitDuration = waitDuration;
        }

        public TimeSpan WaitDuration { get; }

        protected override void OnStart()
        {
            base.OnStart();
            this.targetTick = this.Context.DeltaTime.TotalTime + this.WaitDuration;
        }

        protected override TaskStatus OnUpdate()
        {
            if (this.targetTick <= this.Context.DeltaTime.TotalTime)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}
