using System;

namespace BaldurToolkit.Network.Buffers
{
    public class BufferSegment : IDisposable
    {
        public BufferSegment(byte[] array)
            : this(array, 0, array.Length, null)
        {
        }

        public BufferSegment(byte[] array, int offset, int count)
            : this(array, offset, count, null)
        {
        }

        public BufferSegment(byte[] buffer, int offset, int length, BufferPool pool)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (buffer.Length - offset < length)
            {
                throw new ArgumentException("Offset and length do not specify a valid range in array.");
            }

            this.Pool = pool;
            this.Buffer = buffer;
            this.Offset = offset;
            this.Length = length;
        }

        ~BufferSegment()
        {
            this.Dispose(false);
        }

        public BufferPool Pool { get; private set; }

        public byte[] Buffer { get; }

        public int Offset { get; }

        public int Length { get; }

        public void Free()
        {
            this.Pool = null;
        }

        public override int GetHashCode()
        {
            return this.Buffer == null
                        ? 0
                        : this.Buffer.GetHashCode() ^ this.Offset ^ this.Length;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Object may be resurrected by the pool
            this.Pool?.Release(this);
        }
    }
}
