using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseTask : ITask
    {
        public BaseTask()
        {
        }

        public BaseTask(string name)
            : this()
        {
            this.Name = name;
        }

        public string Name { get; set; }

        public TaskStatus Status { get; private set; } = TaskStatus.None;

        public long ExecutionCounter { get; private set; }

        public TaskStatus Update()
        {
            if (this.Status != TaskStatus.Running)
            {
                this.OnStart();
            }

            this.Status = this.OnUpdate();

            if (this.Status != TaskStatus.Running)
            {
                this.OnStop(false);
            }

            this.ExecutionCounter++;

            return this.Status;
        }

        public void Interrupt()
        {
            if (this.Status == TaskStatus.Running)
            {
                this.OnStop(true);
                this.Status = TaskStatus.Failure;
            }
        }

        public override string ToString()
        {
            var name = this.Name != null ? $"({this.Name})" : string.Empty;
            return $"{this.GetType().Name}{name}";
        }

        protected virtual void OnStart()
        {
        }

        protected abstract TaskStatus OnUpdate();

        protected virtual void OnStop(bool interrupted)
        {
        }
    }
}
