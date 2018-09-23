using System;

namespace BaldurToolkit.Entities
{
    /// <summary>
    /// An entity object, identified by a unique ID.
    /// </summary>
    /// <remarks>
    /// We don't want to limit our API users to use some concrete implementation of the ID type,
    /// but please keep in mind that the TId shoud implement the IEquatable interface, be immutable and
    /// have a correct implementation of the GetHashCode method.
    /// </remarks>
    /// <typeparam name="TId">The type of entity ID.</typeparam>
    public interface IEntity<out TId>
        where TId : IEquatable<TId>
    {
        TId Id { get; }
    }
}
