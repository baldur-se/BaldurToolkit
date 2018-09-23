using System;

namespace BaldurToolkit.Cron
{
    public class CronTask : ITask
    {
        public CronTask(string name, CronExpression cronExpression, Action callback = null)
        {
            this.Name = name;
            this.Expression = cronExpression;
            this.Callback = callback;
        }

        public CronTask(string name, string cronExpressionText, Action callback = null)
            : this(name, new CronExpression(cronExpressionText), callback)
        {
        }

        public string Name { get; }

        public CronExpression Expression { get; }

        protected Action Callback { get; set; }

        public bool IsActiveAt(DateTime time)
        {
            return this.Expression.Check(time);
        }

        public virtual void Execute()
        {
            this.Callback?.Invoke();
        }
    }
}
