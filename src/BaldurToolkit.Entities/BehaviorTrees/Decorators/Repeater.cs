using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Decorators
{
    public class Repeater : BaseDecorator
    {
        public Repeater(ITask baseTask, int maxFailures = 0, int maxSuccesses = 0, int maxRepeats = 0)
            : this(null, baseTask, maxFailures, maxSuccesses, maxRepeats)
        {
        }

        public Repeater(string name, ITask baseTask, int maxFailures = 0, int maxSuccesses = 0, int maxRepeats = 0)
            : base(name, baseTask)
        {
            this.MaxFailures = maxFailures;
            this.MaxSuccesses = maxSuccesses;
            this.MaxRepeats = maxRepeats;
        }

        public int MaxFailures { get; }

        public int MaxSuccesses { get; }

        public int MaxRepeats { get; }

        public int CurrentFailures { get; protected set; }

        public int CurrentSuccesses { get; protected set; }

        public int CurrentRepeats { get; protected set; }

        public TaskStatus LastBaseTaskStatus { get; protected set; }

        protected override TaskStatus OnUpdate()
        {
            this.LastBaseTaskStatus = this.BaseTask.Update();

            if (this.LastBaseTaskStatus != TaskStatus.Running && (this.MaxRepeats <= 0 || ++this.CurrentRepeats < this.MaxRepeats))
            {
                return TaskStatus.Running;
            }

            if (this.LastBaseTaskStatus == TaskStatus.Failure && (this.MaxFailures <= 0 || ++this.CurrentFailures < this.MaxFailures))
            {
                return TaskStatus.Running;
            }

            if (this.LastBaseTaskStatus == TaskStatus.Success && (this.MaxSuccesses <= 0 || ++this.CurrentSuccesses < this.MaxSuccesses))
            {
                return TaskStatus.Running;
            }

            return this.LastBaseTaskStatus;
        }
    }
}
