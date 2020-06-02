using System;

namespace BaldurToolkit.Entities.BehaviorTrees
{
    public abstract class BaseContextDecorator<TContext> : BaseDecorator, IContextTask<TContext>
    {
        private TContext context;

        protected BaseContextDecorator(ITask baseTask)
            : base(baseTask)
        {
        }

        protected BaseContextDecorator(ITask baseTask, TContext context)
            : base(baseTask)
        {
            this.context = context;
        }

        protected BaseContextDecorator(string name, ITask baseTask)
            : base(name, baseTask)
        {
        }

        protected BaseContextDecorator(string name, ITask baseTask, TContext context)
            : base(name, baseTask)
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
