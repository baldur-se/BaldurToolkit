using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Actions
{
    public class CustomAction : BaseAction
    {
        private readonly Func<TaskStatus> onUpdate;
        private readonly Action onStart;
        private readonly Action<bool> onStop;

        public CustomAction(Func<TaskStatus> onUpdate)
            : this(null, onUpdate)
        {
        }

        public CustomAction(string name, Func<TaskStatus> onUpdate, Action onStart = null, Action<bool> onStop = null)
            : base(name)
        {
            this.onUpdate = onUpdate ?? throw new ArgumentNullException(nameof(onUpdate));
            this.onStart = onStart;
            this.onStop = onStop;
        }

        protected override void OnStart()
        {
            base.OnStart();

            this.onStart?.Invoke();
        }

        protected override TaskStatus OnUpdate()
        {
            return this.onUpdate();
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            this.onStop?.Invoke(interrupted);
        }
    }
}
