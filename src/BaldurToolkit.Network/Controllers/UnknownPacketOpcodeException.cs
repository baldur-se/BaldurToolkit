using System;

namespace BaldurToolkit.Network.Controllers
{
    public class UnknownPacketOpcodeException : Exception
    {
        public UnknownPacketOpcodeException(int opcode, string message)
            : base($"{message} for opcode 0x{opcode:X8}")
        {
            this.Opcode = opcode;
        }

        public int Opcode { get; }
    }
}
