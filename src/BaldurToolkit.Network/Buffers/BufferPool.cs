using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BaldurToolkit.Network.Buffers
{
    public class BufferPool : IBufferPool, IDisposable
    {
        public const int MinBlockSize = 85 * 1024;

        private readonly List<byte[]> blocks = new List<byte[]>();
        private readonly ConcurrentStack<BufferSegment> segments = new ConcurrentStack<BufferSegment>();

        private bool closed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        /// <param name="segmentCount">Amount of created segments.</param>
        /// <param name="segmentSize">Size of each segment in bytes.</param>
        /// <param name="allowExpand">Indicates whenever buffer pool is allowed to create a new memory block if it exseeds all segments.</param>
        public BufferPool(int segmentCount, int segmentSize, bool allowExpand = true)
        {
            if (segmentCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentCount));
            }

            if (segmentSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentSize));
            }

            if ((segmentCount * segmentSize) < MinBlockSize)
            {
                segmentCount = (MinBlockSize / segmentSize) + 1;
            }

            this.SegmentSize = segmentSize;
            this.InitialSegmentCount = segmentCount;
            this.AllowExpand = allowExpand;
            this.MaxTryCount = 100;

            this.CreateBlock();
        }

        ~BufferPool()
        {
            this.Dispose(false);
        }

        public int InitialSegmentCount { get; protected set; }

        public int SegmentSize { get; protected set; }

        public bool AllowExpand { get; protected set; }

        public int MaxTryCount { get; set; }

        public int AvailableSegmentCount => this.segments.Count;

        public int TotalSegmentCount => this.blocks.Count * this.InitialSegmentCount;

        public long TotalAllocatedMemory => this.TotalSegmentCount * this.SegmentSize;

        public BufferSegment Acquire()
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null);
            }

            BufferSegment segment;
            var tryCount = 0;
            do
            {
                if (this.segments.TryPop(out segment))
                {
                    return segment;
                }

                if (this.AllowExpand)
                {
                    lock (this.blocks)
                    {
                        if (this.segments.TryPop(out segment))
                        {
                            return segment;
                        }

                        this.CreateBlock();
                    }
                }

                tryCount++;
            }
            while (tryCount <= this.MaxTryCount);

            throw new UnableToAllocateBufferException();
        }

        public BufferSegment Acquire(int minimumSize)
        {
            if (minimumSize > this.SegmentSize)
            {
                throw new UnableToAllocateBufferException("Requested buffer size is larger than current buffer pool's segment size.");
            }

            return this.Acquire();
        }

        public void Release(BufferSegment segment)
        {
            if (this.closed)
            {
                return;
            }

            if (segment.Pool != this)
            {
                throw new Exception("Attempt to checking in an invalid buffer segment.");
            }

            this.segments.Push(segment);
            GC.ReRegisterForFinalize(segment); // Resurrect object
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.closed = true;
        }

        private void CreateBlock()
        {
            var blockSize = this.InitialSegmentCount * this.SegmentSize;
            var block = new byte[blockSize];

            this.blocks.Add(block);
            for (int i = 0; i < this.InitialSegmentCount; i++)
            {
                var segment = new BufferSegment(block, i * this.SegmentSize, this.SegmentSize, this);
                this.segments.Push(segment);
            }
        }
    }
}
