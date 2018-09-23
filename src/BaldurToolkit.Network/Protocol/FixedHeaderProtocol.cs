using System;
using System.IO;

namespace BaldurToolkit.Network.Protocol
{
    /// <summary>
    /// Example binary length-prefixed opcode-based protocol with fixed header size.
    /// </summary>
    public class FixedHeaderProtocol : IProtocol
    {
        /// <summary>
        /// Header size whitch includes: [int packetLength][int opcode]
        /// </summary>
        public const int HeaderSize = 8;

        public virtual int MinPacketSize => HeaderSize;

        public virtual int MaxPacketSize => 64 * 1024;

        public virtual int RecommendedBufferSize => this.MaxPacketSize;

        public virtual int TryRead(byte[] buffer, int offset, int length, out IPacket packet)
        {
            packet = null;
            if (length < this.MinPacketSize)
            {
                return this.MinPacketSize - length;
            }

            var packetSize = BitConverter.ToInt32(buffer, offset);
            if (packetSize < this.MinPacketSize || packetSize > this.MaxPacketSize)
            {
                throw new Exception("Invalid packet size value.");
            }

            var remainingBytes = packetSize - length;
            if (remainingBytes <= 0)
            {
                // Full packet body is available
                var opcode = BitConverter.ToInt32(buffer, offset + 4);

                packet = this.CreatePacket(opcode, new MemoryStream(buffer, offset + HeaderSize, packetSize - HeaderSize, false));
            }

            return remainingBytes;
        }

        public virtual int Write(IPacket packet, byte[] buffer, int offset, int length)
        {
            if (!(packet is IOpcodePacket opcodePacket))
            {
                throw new Exception("Unsupported packet type.");
            }

            var packetSize = this.CalculatePacketSize(packet);
            if (packetSize > this.MaxPacketSize || packetSize > length)
            {
                throw new Exception("Packet data is too large.");
            }

            // Write header
            using (var writer = new BinaryWriter(new MemoryStream(buffer, offset, HeaderSize)))
            {
                writer.Write(packetSize);
                writer.Write(opcodePacket.Opcode);
            }

            // Write data
            var dataStream = opcodePacket.GetDataStream();
            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.Read(buffer, offset + HeaderSize, (int)dataStream.Length);

            return packetSize;
        }

        public virtual int CalculatePacketSize(IPacket packet)
        {
            return (int)packet.GetDataStream().Length + HeaderSize;
        }

        protected virtual IOpcodePacket CreatePacket(int opcode, Stream inputStream)
        {
            return new OpcodePacketReader(opcode, inputStream);
        }
    }
}
