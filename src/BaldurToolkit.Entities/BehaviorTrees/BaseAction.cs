using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseAction : BaseTask
    {
        public BaseAction()
        {
        }

        public BaseAction(string name)
            : base(name)
        {
        }
    }
}
