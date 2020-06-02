using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Decorators
{
    public class Inverter : BaseDecorator
    {
        public Inverter(ITask baseTask)
            : base(baseTask)
        {
        }

        public Inverter(string name, ITask baseTask)
            : base(name, baseTask)
        {
        }

        protected override TaskStatus OnUpdate()
        {
            var status = this.BaseTask.Update();
            switch (status)
            {
            case TaskStatus.Success:
                return TaskStatus.Failure;
            case TaskStatus.Failure:
                return TaskStatus.Success;
            default:
                return status;
            }
        }
    }
}
