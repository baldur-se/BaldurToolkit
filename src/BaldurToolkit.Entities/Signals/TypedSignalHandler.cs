using System;
using System.Collections.Generic;

namespace BaldurToolkit.Entities.Signals
{
    public class TypedSignalHandler<TSignal> : ISignalHandler<TSignal>
    {
        private readonly Dictionary<Type, Action<TSignal>> handlers = new Dictionary<Type, Action<TSignal>>();

        public virtual void HandleSignal(TSignal signal)
        {
            var signalType = signal.GetType();
            if (this.handlers.TryGetValue(signalType, out var handler))
            {
                handler(signal);
            }
        }

        protected void On<TConcreteSignal>(Action<TConcreteSignal> action)
            where TConcreteSignal : TSignal
        {
            var signalType = typeof(TConcreteSignal);
            this.handlers[signalType] = signal => action((TConcreteSignal)signal);
        }
    }
}
