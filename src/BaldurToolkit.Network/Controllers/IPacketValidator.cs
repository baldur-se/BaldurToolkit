using System;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public interface IPacketValidator<in TConnection, in TPacket>
        where TConnection : IConnection
        where TPacket : IPacket
    {
        /// <summary>
        /// Validates that given packet can be handled for given connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="packet">The packet to validate.</param>
        /// <exception cref="ValidationException">If the packet is not valid.</exception>
        void Validate(TConnection connection, TPacket packet);
    }
}
