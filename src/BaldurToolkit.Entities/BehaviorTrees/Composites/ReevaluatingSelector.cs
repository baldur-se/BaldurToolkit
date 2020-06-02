using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    /// <summary>
    /// Executes children one by one until at least one returns a <see cref="TaskStatus.Success"/>.
    /// It will immediately return <see cref="TaskStatus.Running"/> if any child returns <see cref="TaskStatus.Running"/>.
    /// Next <see cref="ITask.Update"/> call will start from the first child again even if it was finished.
    /// When a task returns <see cref="TaskStatus.Running"/> or <see cref="TaskStatus.Success"/>, any running
    /// subsequent tasks will be interrupted.
    /// </summary>
    public class ReevaluatingSelector : Selector
    {
        public ReevaluatingSelector(params ITask[] children)
            : this(null, children)
        {
        }

        public ReevaluatingSelector(string name, params ITask[] children)
            : base(name, children)
        {
            this.ReevaluateFinishedTasks = true;
        }
    }
}
