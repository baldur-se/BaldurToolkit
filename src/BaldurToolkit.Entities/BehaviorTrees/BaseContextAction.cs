using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseContextAction<TContext> : BaseAction, IContextTask<TContext>
    {
        private TContext context;

        public BaseContextAction()
        {
        }

        public BaseContextAction(TContext context)
        {
            this.context = context;
        }

        public BaseContextAction(string name)
            : base(name)
        {
        }

        public BaseContextAction(string name, TContext context)
            : base(name)
        {
            this.context = context;
        }

        public TContext Context
        {
            get => this.context ?? throw new NullReferenceException("Context was not set.");
            set => this.context = value;
        }
    }
}
