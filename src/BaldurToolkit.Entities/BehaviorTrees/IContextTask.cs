using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public interface IContextTask<TContext>
    {
        TContext Context { get; set; }
    }
}
