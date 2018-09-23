using System;
using System.IO;

namespace BaldurToolkit.Network.Buffers
{
    /// <summary>
    /// Buffer-based stream that automatically increases it's length when writing up to the capacity value.
    /// </summary>
    public class BufferStream : Stream
    {
        private readonly BufferSegment segment;
        private int length;
        private int position;
        private bool writable;
        private bool closed;
        private bool disposeSegment;

        public BufferStream(BufferSegment segment, int initialLength, bool writable = true, bool disposeSegment = true)
        {
            if (initialLength > segment.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(initialLength));
            }

            this.segment = segment;
            this.length = initialLength;
            this.writable = writable;
            this.disposeSegment = disposeSegment;
        }

        public BufferStream(BufferSegment segment, bool writable = true)
            : this(segment, segment.Length, writable)
        {
        }

        public override bool CanRead => !this.closed;

        public override bool CanSeek => !this.closed;

        public override bool CanWrite => this.writable;

        public override long Length => this.length;

        public override long Position
        {
            get => this.position;
            set
            {
                if (this.closed)
                {
                    throw new ObjectDisposedException(null, "Stream is closed.");
                }

                if (value < 0 || value > this.length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.position = (int)value;
            }
        }

        public virtual long Capacity => this.segment.Length;

        public override void Flush()
        {
            // Do nothing
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;
                case SeekOrigin.Current:
                    this.Position += offset;
                    break;
                case SeekOrigin.End:
                    this.Position = this.length - offset;
                    break;
            }

            return this.position;
        }

        public override void SetLength(long value)
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            if (value > this.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Can not set stream length bigger than capacity.");
            }

            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            this.length = (int)value;
            if (this.position > this.length)
            {
                this.position = this.length;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("Offset and length were out of bounds for the array.");
            }

            count = Math.Min(count, this.length - this.position);

            Buffer.BlockCopy(this.segment.Buffer, this.segment.Offset + this.position, buffer, offset, count);
            this.position += count;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("Offset and length were out of bounds for the array.");
            }

            if (!this.CanWrite)
            {
                throw new InvalidOperationException("Can not write to read-only stream.");
            }

            var targetPosition = this.position + count;
            if (targetPosition < 0)
            {
                // overflow check
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (targetPosition > this.length)
            {
                this.SetLength(targetPosition);
            }

            Buffer.BlockCopy(buffer, offset, this.segment.Buffer, this.segment.Offset + this.position, count);
            this.position = targetPosition;
        }

        public override int ReadByte()
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            if (this.position >= this.length)
            {
                return -1;
            }

            return this.segment.Buffer[this.position++];
        }

        public override void WriteByte(byte value)
        {
            if (this.closed)
            {
                throw new ObjectDisposedException(null, "Stream is closed.");
            }

            if (!this.CanWrite)
            {
                throw new InvalidOperationException("Can not write to read-only stream.");
            }

            var targetPosition = this.position + 1;
            if (targetPosition > this.length)
            {
                this.SetLength(targetPosition);
            }

            this.segment.Buffer[this.segment.Offset + this.position] = value;
            this.position = targetPosition;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.closed = true;
                this.writable = false;
            }

            if (this.disposeSegment)
            {
                this.segment.Dispose();
            }
        }
    }
}
