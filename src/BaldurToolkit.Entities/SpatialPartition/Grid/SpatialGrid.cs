using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.Entities.SpatialPartition.Grid
{
    public abstract class SpatialGrid<TEntityId, TEntity>
        where TEntity : IEntity<TEntityId>
        where TEntityId : struct, IEquatable<TEntityId>
    {
        private readonly ConcurrentDictionary<int, SpatialGridCell> cells
            = new ConcurrentDictionary<int, SpatialGridCell>();

        private readonly ConcurrentDictionary<TEntityId, EntityRegistrationInfo> entities
            = new ConcurrentDictionary<TEntityId, EntityRegistrationInfo>();

        public event EventHandler<EntityEventArgs<TEntityId, TEntity>> EntityRegistered;

        public event EventHandler<EntityEventArgs<TEntityId, TEntity>> EntityRemoved;

        public int NeighborBlockDistance { get; set; } = 1;

        public IEnumerable<TEntity> RegisteredEntities => this.entities.Select(regInfo => regInfo.Value.Entity);

        public abstract int GetHash(TEntity entity);

        public bool Contains(TEntity entity)
        {
            return this.entities.ContainsKey(entity.Id);
        }

        public bool TryAddOrUpdate(TEntity entity, out IEnumerable<TEntity> entitiesToAdd, out IEnumerable<TEntity> entitiesToRemove)
        {
            var newCellHash = this.GetHash(entity);

            SpatialGridCell oldCell = null;
            entitiesToAdd = Enumerable.Empty<TEntity>();
            entitiesToRemove = Enumerable.Empty<TEntity>();

            var regInfo = this.entities.AddOrUpdate(
                entity.Id,
                (id) => new EntityRegistrationInfo(entity, this.GetCell(newCellHash)),
                (id, oldRegInfo) =>
                {
                    oldCell = oldRegInfo.Cell;

                    if (newCellHash != oldCell.Hash)
                    {
                        oldRegInfo.Cell = this.GetCell(newCellHash);
                    }

                    return oldRegInfo;
                });

            // At this point we have three possible states:
            // 1. Entity was registered on the grid for the first time
            // 2. Entity remains on the same cell
            // 3. Entity moved to the other cell
            if (oldCell != null && oldCell.Hash == newCellHash)
            {
                // Entity stays on the same cell, no need to update
                return false;
            }

            IEnumerable<SpatialGridCell> newCellBlock = this.GetCellWithNeighbors(newCellHash).ToList();

            if (oldCell != null)
            {
                // Entity moved to the other cell
                oldCell.Remove(entity);
                var oldCellBlock = this.GetCellWithNeighbors(oldCell.Hash).ToList();

                entitiesToRemove = oldCellBlock.Except(newCellBlock).SelectMany(cell => cell.Entities).ToList();

                newCellBlock = newCellBlock.Except(oldCellBlock);
            }

            entitiesToAdd = newCellBlock.SelectMany(cell => cell.Entities).ToList();
            regInfo.Cell.Register(entity);

            if (oldCell == null)
            {
                // New entity registered on the grid
                this.EntityRegistered?.Invoke(this, new EntityEventArgs<TEntityId, TEntity>(entity));
            }

            return true;
        }

        public bool TryRemove(TEntity entity, out IEnumerable<TEntity> entitiesToRemove)
        {
            if (this.entities.TryRemove(entity.Id, out var regInfo))
            {
                regInfo.Cell.Remove(entity);

                var oldCellBlock = this.GetCellWithNeighbors(regInfo.Cell.Hash);
                entitiesToRemove = oldCellBlock.SelectMany(cell => cell.Entities).ToList();

                this.EntityRemoved?.Invoke(this, new EntityEventArgs<TEntityId, TEntity>(entity));
                return true;
            }

            entitiesToRemove = Enumerable.Empty<TEntity>();
            return false;
        }

        protected abstract IEnumerable<int> GetNeighborHashes(int centerHash, int distance);

        protected SpatialGridCell GetCell(int hash)
        {
            return this.cells.GetOrAdd(hash, _ => this.CreateCell(hash));
        }

        protected SpatialGridCell CreateCell(int hash)
        {
            var neighborHashes = this.GetNeighborHashes(hash, this.NeighborBlockDistance);
            return new SpatialGridCell(hash, neighborHashes);
        }

        protected IEnumerable<SpatialGridCell> GetCellWithNeighbors(int hash)
        {
            var cell = this.GetCell(hash);
            yield return cell;

            foreach (var neighborHash in cell.NeighborHashes)
            {
                yield return this.GetCell(neighborHash);
            }
        }

        protected struct EntityRegistrationInfo
        {
            public readonly TEntity Entity;
            public SpatialGridCell Cell;

            public EntityRegistrationInfo(TEntity entity, SpatialGridCell cell)
            {
                this.Entity = entity;
                this.Cell = cell;
            }
        }

        protected class SpatialGridCell
        {
            private readonly ConcurrentDictionary<TEntityId, TEntity> entities = new ConcurrentDictionary<TEntityId, TEntity>();

            public SpatialGridCell(int hash, IEnumerable<int> neighborHashes)
            {
                this.Hash = hash;
                this.NeighborHashes = neighborHashes;
            }

            public int Hash { get; }

            public IEnumerable<int> NeighborHashes { get; }

            public IEnumerable<TEntity> Entities => this.entities.Select(kvp => kvp.Value);

            public void Register(TEntity entity)
            {
                this.entities.TryAdd(entity.Id, entity);
            }

            public void Remove(TEntity entity)
            {
                this.entities.TryRemove(entity.Id, out _);
            }
        }
    }
}
