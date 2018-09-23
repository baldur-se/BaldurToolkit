using System;
using System.IO;
using System.Text;

namespace BaldurToolkit.Network.Protocol
{
    public class OpcodePacketReader : PacketReader, IOpcodePacket
    {
        public OpcodePacketReader(int opcode, Stream input)
            : base(input)
        {
            this.Opcode = opcode;
        }

        protected OpcodePacketReader(int opcode, Stream input, Encoding encoding)
            : base(input, encoding)
        {
            this.Opcode = opcode;
        }

        protected OpcodePacketReader(int opcode, Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
            this.Opcode = opcode;
        }

        public int Opcode { get; }
    }
}
