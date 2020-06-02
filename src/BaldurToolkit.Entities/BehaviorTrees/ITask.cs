using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public interface ITask
    {
        string Name { get; }

        TaskStatus Status { get; }

        TaskStatus Update();

        void Interrupt();
    }
}
