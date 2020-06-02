using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes each child until it finishes or any task returns <see cref="TaskStatus.Failure"/>.
    /// At the end of iteration, it will return <see cref="TaskStatus.Running"/> if any child is still <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will update all children that are still <see cref="TaskStatus.Running"/>.
    /// When each child successfully finishes, <see cref="ParallelSequence"/> will return will return <see cref="TaskStatus.Success"/>,
    /// otherwise it will return <see cref="TaskStatus.Failure"/>.
    /// If any child was in the state of <see cref="TaskStatus.Running"/> when another child returned <see cref="TaskStatus.Failure"/>,
    /// <see cref="ParallelSequence"/> will interrupt it.
    /// </summary>
    public class ParallelSequence : Parallel
    {
        public ParallelSequence(params ITask[] children)
            : this(null, children)
        {
        }

        public ParallelSequence(string name, params ITask[] children)
            : base(name, 1, children.Length, children)
        {
            this.TerminateOnFirstConditionMet = true;
            this.DefaultStatus = TaskStatus.Success;
        }
    }
}
