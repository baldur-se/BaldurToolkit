using System;
using System.Collections.Concurrent;

namespace BaldurToolkit.Entities.Timers
{
    public class PrioritizedEntityUpdateWorker<TEntityId, TEntity> : BaseUpdateWorker
        where TEntity : class, IEntity<TEntityId>, IPrioritizedUpdateable
        where TEntityId : struct, IEquatable<TEntityId>
    {
        private readonly TimeSpan[] intervals;

        private readonly ConcurrentDictionary<TEntityId, EntityDeltaTime> entities
            = new ConcurrentDictionary<TEntityId, EntityDeltaTime>();

        public PrioritizedEntityUpdateWorker(TimeSpan updateInterval)
            : base(updateInterval)
        {
            this.intervals = new TimeSpan[Enum.GetNames(typeof(UpdatePriority)).Length];
        }

        public TimeSpan GetInterval(UpdatePriority priority)
        {
            return this.intervals[(int)priority];
        }

        public void SetInterval(UpdatePriority priority, TimeSpan interval)
        {
            this.intervals[(int)priority] = interval;
        }

        public bool Register(TEntity updateable)
        {
            return this.entities.TryAdd(updateable.Id, new EntityDeltaTime(updateable));
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
            foreach (var regInfo in this.entities)
            {
                var entityDeltaTime = regInfo.Value;

                entityDeltaTime.ElapsedTime += deltaTime.ElapsedTime;
                var interval = this.GetInterval(entityDeltaTime.Entity.UpdatePriority);

                if (entityDeltaTime.ElapsedTime >= interval)
                {
                    // Synchronize entity's personal total time with current total time
                    entityDeltaTime.TotalTime = deltaTime.TotalTime;

                    try
                    {
                        entityDeltaTime.Entity.Update(entityDeltaTime);
                    }
                    catch (Exception exception)
                    {
                        this.OnUpdateError(exception);
                    }

                    // Reset entity's personal elapsed time and wait till it reaches desired interval
                    entityDeltaTime.ElapsedTime = TimeSpan.Zero;
                }
            }
        }

        protected class EntityDeltaTime : DeltaTime
        {
            public EntityDeltaTime(IPrioritizedUpdateable entity)
            {
                this.Entity = entity;
            }

            public IPrioritizedUpdateable Entity { get; }
        }
    }
}
