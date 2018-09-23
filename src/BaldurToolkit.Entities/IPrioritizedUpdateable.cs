using System;

namespace BaldurToolkit.Entities
{
    public interface IPrioritizedUpdateable : IUpdateable
    {
        UpdatePriority UpdatePriority { get; }
    }
}
