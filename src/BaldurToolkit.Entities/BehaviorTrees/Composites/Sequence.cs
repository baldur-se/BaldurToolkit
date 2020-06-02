using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes children one by one until any of them return <see cref="TaskStatus.Failure"/> or all of them finish with <see cref="TaskStatus.Success"/>.
    /// It will immediately return <see cref="TaskStatus.Running"/> if any child returns <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will resume from that child.
    /// </summary>
    public class Sequence : BaseComposite
    {
        public Sequence(params ITask[] children)
            : this(null, children)
        {
        }

        public Sequence(string name, params ITask[] children)
            : base(name, children)
        {
            this.MinFails = 1;
            this.MinSuccesses = children.Length;
            this.TerminateOnFirstConditionMet = true;
            this.TerminateOnStatuses = new[] { TaskStatus.Running, TaskStatus.Failure };
            this.DefaultStatus = TaskStatus.Success;
        }
    }
}
