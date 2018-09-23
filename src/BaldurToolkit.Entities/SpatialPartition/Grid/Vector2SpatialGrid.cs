using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BaldurToolkit.Entities.SpatialPartition.Grid
{
    public abstract class Vector2SpatialGrid<TEntityId, TEntity> : SpatialGrid<TEntityId, TEntity>
        where TEntity : IEntity<TEntityId>
        where TEntityId : struct, IEquatable<TEntityId>
    {
        public Vector2SpatialGrid(float cellSize)
        {
            this.CellSize = cellSize;
            this.MaxAxisValue = cellSize * short.MaxValue;
            this.MinAxisValue = cellSize * short.MinValue;
        }

        public float CellSize { get; }

        public float MaxAxisValue { get; }

        public float MinAxisValue { get; }

        public sealed override int GetHash(TEntity entity)
        {
            var vector = this.ExtractVector2(entity);

            if (vector.X > this.MaxAxisValue || vector.X < this.MinAxisValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (vector.Y > this.MaxAxisValue || vector.Y < this.MinAxisValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            var x = (short)Math.Floor(vector.X / this.CellSize);
            var y = (short)Math.Floor(vector.Y / this.CellSize);

            return ConstructKey(x, y);
        }

        protected sealed override IEnumerable<int> GetNeighborHashes(int centerHash, int distance)
        {
            var (x, y) = DeconstructKey(centerHash);

            for (int offsetY = -distance; offsetY <= distance; offsetY++)
            {
                for (int offsetX = -distance; offsetX <= distance; offsetX++)
                {
                    if (offsetY == 0 && offsetX == 0)
                    {
                        continue;
                    }

                    var targetX = x + offsetX;
                    var targetY = y + offsetY;

                    if (targetX > short.MaxValue || targetX < short.MinValue)
                    {
                        continue;
                    }

                    if (targetY > short.MaxValue || targetY < short.MinValue)
                    {
                        continue;
                    }

                    yield return ConstructKey((short)targetX, (short)targetY);
                }
            }
        }

        protected abstract Vector2 ExtractVector2(TEntity entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ConstructKey(short x, short y)
        {
            return (y << 16) | (x & 0xFFFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (short x, short y) DeconstructKey(int key)
        {
            return ((short)key, (short)(key >> 16));
        }
    }
}
