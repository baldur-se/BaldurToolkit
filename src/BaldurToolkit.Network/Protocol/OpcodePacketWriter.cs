using System;
using System.IO;
using System.Text;
using BaldurToolkit.Network.Buffers;

namespace BaldurToolkit.Network.Protocol
{
    public class OpcodePacketWriter : PacketWriter, IOpcodePacket
    {
        public OpcodePacketWriter(int opcode, byte[] buffer)
            : base(buffer)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, byte[] buffer, Encoding encoding)
            : base(buffer, encoding)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, byte[] buffer, int offset, int length)
            : base(buffer, offset, length)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, byte[] buffer, int offset, int length, Encoding encoding)
            : base(buffer, offset, length, encoding)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, BufferSegment segment)
            : base(segment)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, BufferSegment segment, Encoding encoding)
            : base(segment, encoding)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, Stream output)
            : base(output)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, Stream output, Encoding encoding)
            : base(output, encoding)
        {
            this.Opcode = opcode;
        }

        public OpcodePacketWriter(int opcode, Stream output, Encoding encoding, bool leaveOpen)
            : base(output, encoding, leaveOpen)
        {
            this.Opcode = opcode;
        }

        public int Opcode { get; }
    }
}
