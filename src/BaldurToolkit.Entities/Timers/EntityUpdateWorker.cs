using System;
using System.Collections.Concurrent;

namespace BaldurToolkit.Entities.Timers
{
    public class EntityUpdateWorker<TEntityId, TEntity> : BaseUpdateWorker
        where TEntity : class, IEntity<TEntityId>, IUpdateable
        where TEntityId : struct, IEquatable<TEntityId>
    {
        private readonly ConcurrentDictionary<TEntityId, TEntity> entities
            = new ConcurrentDictionary<TEntityId, TEntity>();

        public EntityUpdateWorker(TimeSpan updateInterval)
            : base(updateInterval)
        {
        }

        public bool Register(TEntity updateable)
        {
            return this.entities.TryAdd(updateable.Id, updateable);
        }

        public bool Remove(TEntity updateable)
        {
            return this.entities.TryRemove(updateable.Id, out _);
        }

        public override bool Register(IUpdateable updateable)
        {
            if (!(updateable is TEntity entity))
            {
                throw new ArgumentException($"Invalid argument passed to {this.GetType().FullName}.{nameof(this.Register)}(): {updateable.GetType().FullName}");
            }

            return this.Register(entity);
        }

        public override bool Remove(IUpdateable updateable)
        {
            if (!(updateable is TEntity entity))
            {
                throw new ArgumentException($"Invalid argument passed to {this.GetType().FullName}.{nameof(this.Remove)}(): {updateable.GetType().FullName}");
            }

            return this.Remove(entity);
        }

        protected override void DoUpdate(DeltaTime deltaTime)
        {
            foreach (var entity in this.entities)
            {
                entity.Value.Update(deltaTime);
            }
        }
    }
}
