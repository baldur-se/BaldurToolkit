using System;

namespace BaldurToolkit.Entities.Signals
{
    public interface ISignalHandler<in TSignal>
    {
        void HandleSignal(TSignal signal);
    }
}
