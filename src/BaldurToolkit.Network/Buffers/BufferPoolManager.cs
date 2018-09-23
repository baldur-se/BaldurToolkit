using System;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.Network.Buffers
{
    public class BufferPoolManager : IBufferPool
    {
        private readonly List<BufferPool> pools = new List<BufferPool>();
        private BufferPool[] orderedPools = new BufferPool[0];

        public BufferPoolManager(IEnumerable<BufferPool> bufferPools = null)
        {
            if (bufferPools != null)
            {
                foreach (var bufferPool in bufferPools)
                {
                    this.Add(bufferPool);
                }
            }
        }

        public void Add(BufferPool pool)
        {
            this.pools.Add(pool);

            this.orderedPools = this.pools
                .OrderBy(bufferPool => bufferPool.SegmentSize)
                .ToArray();
        }

        public BufferSegment Acquire()
        {
            if (this.orderedPools.Length == 0)
            {
                throw new UnableToAllocateBufferException("Buffer pool manager is empty.");
            }

            return this.orderedPools[this.orderedPools.Length - 1].Acquire();
        }

        public BufferSegment Acquire(int minimumSize)
        {
            foreach (var pool in this.orderedPools)
            {
                if (pool.SegmentSize >= minimumSize)
                {
                    return pool.Acquire();
                }
            }

            throw new UnableToAllocateBufferException("Can not find a buffer pool with appropriate segment size.");
        }
    }
}
