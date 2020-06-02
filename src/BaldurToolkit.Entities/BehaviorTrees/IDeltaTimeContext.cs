using System;
using BaldurToolkit.Entities;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public interface IDeltaTimeContext
    {
        DeltaTime DeltaTime { get; }
    }
}
