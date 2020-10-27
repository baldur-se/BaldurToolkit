using System;

namespace BaldurToolkit.Entities.Timers
{
    public interface IUpdateWorker : IUpdateTimer
    {
        event EventHandler<UpdateErrorEventArgs> UpdateError;

        void Start();

        void Stop();
    }
}
