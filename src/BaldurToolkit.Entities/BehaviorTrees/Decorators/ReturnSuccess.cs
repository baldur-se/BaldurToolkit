using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Decorators
{
    public class ReturnSuccess : BaseDecorator
    {
        public ReturnSuccess(ITask baseTask)
            : base(baseTask)
        {
        }

        public ReturnSuccess(string name, ITask baseTask)
            : base(name, baseTask)
        {
        }

        protected override TaskStatus OnUpdate()
        {
            var status = this.BaseTask.Update();
            if (status == TaskStatus.Running)
            {
                return status;
            }

            return TaskStatus.Success;
        }
    }
}
