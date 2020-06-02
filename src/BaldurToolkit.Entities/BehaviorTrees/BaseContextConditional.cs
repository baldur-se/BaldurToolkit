using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseContextConditional<TContext> : BaseConditional, IContextTask<TContext>
    {
        private TContext context;

        public BaseContextConditional()
        {
        }

        public BaseContextConditional(TContext context)
        {
            this.context = context;
        }

        public BaseContextConditional(string name)
            : base(name)
        {
        }

        public BaseContextConditional(string name, TContext context)
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
