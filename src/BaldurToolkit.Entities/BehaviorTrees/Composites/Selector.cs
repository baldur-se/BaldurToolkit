using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes children one by one until at least one returns a <see cref="TaskStatus.Success"/>.
    /// It will immediately return <see cref="TaskStatus.Running"/> if any child returns <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will resume from that child.
    /// </summary>
    public class Selector : BaseComposite
    {
        public Selector(params ITask[] children)
            : this(null, children)
        {
        }

        public Selector(string name, params ITask[] children)
            : base(name, children)
        {
            this.MinFails = children.Length;
            this.MinSuccesses = 1;
            this.TerminateOnFirstConditionMet = true;
            this.TerminateOnStatuses = new[] { TaskStatus.Running, TaskStatus.Success };
            this.DefaultStatus = TaskStatus.Failure;
        }
    }
}
