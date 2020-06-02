using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseComposite : BaseTask, ICompositeTask
    {
        private TaskStatus[] taskStatuses;

        public BaseComposite(params ITask[] children)
            : this(null, children)
        {
        }

        public BaseComposite(string name, params ITask[] children)
            : base(name)
        {
            this.Children = children;
        }

        public int MinFails { get; protected set; } = 1;

        public int MinSuccesses { get; protected set; } = 1;

        public bool ReevaluateFinishedTasks { get; set; } = false;

        public bool TerminateOnFirstConditionMet { get; set; } = true;

        public bool TerminateLowerPriority { get; set; } = true;

        public TaskStatus[] TerminateOnStatuses { get; set; } = { };

        public TaskStatus DefaultStatus { get; set; } = TaskStatus.Success;

        public int ChildrenCount => this.Children.Length;

        protected ITask[] Children { get; }

        public IReadOnlyCollection<ITask> GetChildren() => Array.AsReadOnly(this.Children);

        protected override void OnStart()
        {
            base.OnStart();
            this.taskStatuses = new TaskStatus[this.Children.Length];
        }

        protected override TaskStatus OnUpdate()
        {
            var fails = 0;
            var successes = 0;
            var running = false;
            var conditionMet = false;
            var status = this.DefaultStatus;
            var i = 0;
            for (; i < this.Children.Length; i++)
            {
                var child = this.Children[i];

                if (this.ReevaluateFinishedTasks || this.taskStatuses[i] == TaskStatus.None || this.taskStatuses[i] == TaskStatus.Running)
                {
                    this.taskStatuses[i] = child.Update();
                }

                if (this.TerminateOnStatuses.Contains(this.taskStatuses[i]))
                {
                    status = this.taskStatuses[i];
                    break;
                }

                if (this.taskStatuses[i] == TaskStatus.Running)
                {
                    running = true;
                }
                else
                {
                    if (this.MinSuccesses > 0 && this.taskStatuses[i] == TaskStatus.Success && ++successes >= this.MinSuccesses)
                    {
                        status = TaskStatus.Success;
                        conditionMet = true;
                        if (this.TerminateOnFirstConditionMet)
                        {
                            break;
                        }
                    }

                    if (this.MinFails > 0 && this.taskStatuses[i] == TaskStatus.Failure && ++fails >= this.MinFails)
                    {
                        status = TaskStatus.Failure;
                        conditionMet = true;
                        if (this.TerminateOnFirstConditionMet)
                        {
                            break;
                        }
                    }
                }
            }

            if (this.TerminateLowerPriority)
            {
                // Iterate starting from the next child and interrupt any running ones
                for (i++; i < this.Children.Length; i++)
                {
                    if (this.taskStatuses[i] == TaskStatus.Running)
                    {
                        this.Children[i].Interrupt();
                    }
                }
            }

            if (running && (!conditionMet || !this.TerminateOnFirstConditionMet))
            {
                return TaskStatus.Running;
            }

            return status;
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            for (int i = 0; i < this.Children.Length; i++)
            {
                if (this.taskStatuses[i] == TaskStatus.Running)
                {
                    this.Children[i].Interrupt();
                }
            }
        }
    }
}
