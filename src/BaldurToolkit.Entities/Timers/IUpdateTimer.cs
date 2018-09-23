using System;

namespace BaldurToolkit.Entities.Timers
{
    public interface IUpdateTimer
    {
        bool Register(IUpdateable updateable);

        bool Remove(IUpdateable updateable);
    }
}
