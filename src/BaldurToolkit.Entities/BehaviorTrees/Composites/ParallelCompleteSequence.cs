using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes each child until it finishes.
    /// At the end of iteration, it will return <see cref="TaskStatus.Running"/> if any child is still <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ParallelCompleteSequence.Update"/> call will update all children that are still <see cref="TaskStatus.Running"/>.
    /// When each child finishes, <see cref="ParallelCompleteSequence"/> will return <see cref="TaskStatus.Success"/> if
    /// all children return <see cref="TaskStatus.Success"/>, otherwise it will return <see cref="TaskStatus.Failure"/>.
    /// </summary>
    public class ParallelCompleteSequence : ParallelSequence
    {
        public ParallelCompleteSequence(params ITask[] children)
            : this(null, children)
        {
        }

        public ParallelCompleteSequence(string name, params ITask[] children)
            : base(name, children)
        {
            this.TerminateOnFirstConditionMet = false;
        }
    }
}
