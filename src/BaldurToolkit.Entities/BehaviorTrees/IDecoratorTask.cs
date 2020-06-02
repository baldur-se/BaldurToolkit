using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public interface IDecoratorTask : ITask
    {
        ITask BaseTask { get; }
    }
}
