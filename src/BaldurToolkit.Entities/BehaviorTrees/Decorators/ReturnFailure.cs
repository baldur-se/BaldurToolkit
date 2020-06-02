using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Decorators
{
    public class ReturnFailure : BaseDecorator
    {
        public ReturnFailure(ITask baseTask)
            : base(baseTask)
        {
        }

        public ReturnFailure(string name, ITask baseTask)
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

            return TaskStatus.Failure;
        }
    }
}
