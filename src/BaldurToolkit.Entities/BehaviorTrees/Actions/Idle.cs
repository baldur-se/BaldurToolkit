using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Actions
{
    public class Idle : BaseAction
    {
        public Idle()
            : this(null)
        {
        }

        public Idle(string name)
            : base(name)
        {
        }

        protected override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
