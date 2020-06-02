using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseContextComposite<TContext> : BaseComposite, IContextTask<TContext>
    {
        private TContext context;

        public BaseContextComposite(params ITask[] children)
            : base(children)
        {
        }

        public BaseContextComposite(TContext context, params ITask[] children)
            : base(children)
        {
            this.context = context;
        }

        public BaseContextComposite(string name, params ITask[] children)
            : base(name, children)
        {
        }

        public BaseContextComposite(string name, TContext context, params ITask[] children)
            : base(name, children)
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
