using System;
using System.Collections.Generic;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public interface ICompositeTask : ITask
    {
        int ChildrenCount { get; }

        IReadOnlyCollection<ITask> GetChildren();
    }
}
