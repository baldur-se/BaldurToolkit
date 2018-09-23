using System;

namespace BaldurToolkit.Network.Controllers
{
    public interface IOpcodePacketHandler : IPacketHandler
    {
        /// <summary>
        /// Gets opcode with whitch this handler is associated.
        /// </summary>
        int Opcode { get; }
    }
}
