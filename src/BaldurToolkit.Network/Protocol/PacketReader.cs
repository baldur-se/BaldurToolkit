using System;
using System.IO;
using System.Text;
using BaldurToolkit.Network.Buffers;

namespace BaldurToolkit.Network.Protocol
{
    public class PacketReader : BinaryReader, IPacket
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public PacketReader(byte[] buffer)
            : this(new BufferSegment(buffer))
        {
        }

        public PacketReader(byte[] buffer, int offset, int length)
            : this(new BufferSegment(buffer, offset, length))
        {
        }

        public PacketReader(BufferSegment segment)
            : this(new BufferStream(segment, false))
        {
        }

        public PacketReader(Stream input)
            : this(input, DefaultEncoding)
        {
        }

        protected PacketReader(Stream input, Encoding encoding)
            : this(input, encoding, false)
        {
        }

        protected PacketReader(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
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
        /// Moves current position by specified number of bytes.
        /// </summary>
        /// <param name="num">The number of bytes to skip.</param>
        public void Skip(long num)
        {
            this.BaseStream.Seek(num, SeekOrigin.Current);
        }

        public Stream GetDataStream()
        {
            return this.BaseStream;
        }
    }
}
