using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseConditional : BaseTask, IConditionalTask
    {
        public BaseConditional()
        {
        }

        public BaseConditional(string name)
            : base(name)
        {
        }
    }
}
