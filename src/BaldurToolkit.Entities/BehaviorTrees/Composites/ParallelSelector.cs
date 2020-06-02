using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes each child until it finishes or any task returns <see cref="TaskStatus.Success"/>.
    /// At the end of iteration, it will return <see cref="TaskStatus.Running"/> if any child is still <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will update all children that are still <see cref="TaskStatus.Running"/>.
    /// When at least one child successfully finishes, <see cref="ParallelSelector"/> will return will return <see cref="TaskStatus.Success"/>,
    /// otherwise it will return <see cref="TaskStatus.Failure"/>.
    /// If any child was in the state of <see cref="TaskStatus.Running"/> when another child returned <see cref="TaskStatus.Success"/>,
    /// <see cref="ParallelSelector"/> will interrupt it.
    /// </summary>
    public class ParallelSelector : Parallel
    {
        public ParallelSelector(params ITask[] children)
            : this(null, children)
        {
        }

        public ParallelSelector(string name, params ITask[] children)
            : base(name, children.Length, 1, children)
        {
            this.TerminateOnFirstConditionMet = true;
            this.DefaultStatus = TaskStatus.Failure;
        }
    }
}
