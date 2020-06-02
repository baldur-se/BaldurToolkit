using System;

namespace BaldurToolkit.Entities.BehaviorTrees.Composites
{
    public class Parallel : BaseComposite
    {
        public Parallel(int minFails = 0, int minSuccesses = 0, params ITask[] children)
            : this(null, minFails, minSuccesses, children)
        {
        }

        public Parallel(string name, int minFails = 0, int minSuccesses = 0, params ITask[] children)
            : base(name, children)
        {
            this.MinFails = minFails;
            this.MinSuccesses = minSuccesses;
            this.TerminateOnFirstConditionMet = false;
            this.TerminateLowerPriority = false;
            this.TerminateOnStatuses = new TaskStatus[0];
            this.DefaultStatus = TaskStatus.Success;
            this.ReevaluateFinishedTasks = false;
        }
    }
}
