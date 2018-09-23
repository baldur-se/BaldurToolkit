using System;

namespace BaldurToolkit.Network.Protocol
{
    public interface IOpcodePacket : IPacket
    {
        int Opcode { get; }
    }
}
