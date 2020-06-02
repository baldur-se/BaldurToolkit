using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseDecorator : BaseTask, IDecoratorTask
    {
        public BaseDecorator(ITask baseTask)
        {
            this.BaseTask = baseTask;
        }

        public BaseDecorator(string name, ITask baseTask)
            : base(name)
        {
            this.BaseTask = baseTask;
        }

        public ITask BaseTask { get; }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (this.BaseTask.Status == TaskStatus.Running)
            {
                this.BaseTask.Interrupt();
            }
        }
    }
}
