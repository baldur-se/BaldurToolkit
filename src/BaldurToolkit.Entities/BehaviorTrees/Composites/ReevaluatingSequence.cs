using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes children one by one until any of them return <see cref="TaskStatus.Failure"/> or all of them finish with <see cref="TaskStatus.Success"/>.
    /// It will immediately return <see cref="TaskStatus.Running"/> if any child returns <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will start from the first child again even if it was finished.
    /// When a task returns <see cref="TaskStatus.Running"/> or <see cref="TaskStatus.Failure"/>, any running
    /// subsequent tasks will be interrupted.
    /// </summary>
    public class ReevaluatingSequence : Sequence
    {
        public ReevaluatingSequence(params ITask[] children)
            : this(null, children)
        {
        }

        public ReevaluatingSequence(string name, params ITask[] children)
            : base(name, children)
        {
            this.ReevaluateFinishedTasks = true;
        }
    }
}
