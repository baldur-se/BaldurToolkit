using System;

namespace BaldurToolkit.Entities.Timers
{
    public interface IUpdateTimeTracker
    {
        TimeSpan Average { get; }

        TimeSpan Peak { get; }

        void AddValue(TimeSpan value);
    }
}
