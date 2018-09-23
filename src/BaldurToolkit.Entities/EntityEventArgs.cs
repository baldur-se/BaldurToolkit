using System;

namespace BaldurToolkit.Entities
{
    public class EntityEventArgs<TEntityId, TEntity> : EventArgs
        where TEntity : IEntity<TEntityId>
        where TEntityId : IEquatable<TEntityId>
    {
        public EntityEventArgs(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; }
    }
}
