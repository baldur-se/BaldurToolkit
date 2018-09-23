using System;

namespace BaldurToolkit.Entities.Timers
{
    public interface IUpdateWorker : IUpdateTimer
    {
        void Start();

        void Stop();
    }
}
