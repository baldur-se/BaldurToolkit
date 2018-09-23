using System;
using System.IO;
using System.Text;
using BaldurToolkit.Network.Buffers;

namespace BaldurToolkit.Network.Protocol
{
    public class PacketWriter : BinaryWriter, IPacket
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public PacketWriter(byte[] buffer)
            : this(buffer, DefaultEncoding)
        {
        }

        public PacketWriter(byte[] buffer, Encoding encoding)
            : this(new BufferSegment(buffer), encoding)
        {
        }

        public PacketWriter(byte[] buffer, int offset, int length)
            : this(buffer, offset, length, DefaultEncoding)
        {
        }

        public PacketWriter(byte[] buffer, int offset, int length, Encoding encoding)
            : this(new BufferSegment(buffer, offset, length), encoding)
        {
        }

        public PacketWriter(BufferSegment segment)
            : this(segment, DefaultEncoding)
        {
        }

        public PacketWriter(BufferSegment segment, Encoding encoding)
            : this(new BufferStream(segment, 0), encoding)
        {
        }

        public PacketWriter(Stream output)
            : this(output, DefaultEncoding)
        {
        }

        protected PacketWriter(Stream output, Encoding encoding)
            : this(output, encoding, false)
        {
        }

        protected PacketWriter(Stream output, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            this.Encoding = encoding;
        }

        /// <summary>
        /// Gets current encoding.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Gets or sets current position of reader in the stream.
        /// </summary>
        public long Position
        {
            get => this.BaseStream.Position;
            set => this.BaseStream.Position = value;
        }

        /// <summary>
        /// Gets or sets the length of the stream.
        /// </summary>
        public long Length
        {
            get => this.BaseStream.Length;
            set => this.BaseStream.SetLength(value);
        }

        /// <summary>
        /// Writes specified amount of bytes with 0.
        /// </summary>
        /// <param name="num">The number of bytes to write.</param>
        public virtual void FillZero(int num)
        {
            this.Write(new byte[num]);
        }

        public Stream GetDataStream()
        {
            return this.BaseStream;
        }
    }
}
