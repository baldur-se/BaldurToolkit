using System;

namespace BaldurToolkit.Network.Buffers
{
    public interface IBufferPool
    {
        /// <summary>
        /// Acquires the largest possible buffer segment.
        /// </summary>
        /// <returns>The buffer segment.</returns>
        BufferSegment Acquire();

        /// <summary>
        /// Acquires a buffer segment with at least specified size.
        /// </summary>
        /// <param name="minimumSize">Minimum buffer segment size.</param>
        /// <returns>The buffer segment.</returns>
        BufferSegment Acquire(int minimumSize);
    }
}
