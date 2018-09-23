using System;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public interface IPacketHandler
    {
        /// <summary>
        /// Invokes current packet handler for specified state (connection) and request data (packet).
        /// </summary>
        /// <param name="connection">Invocation state.</param>
        /// <param name="packet">Invocation request.</param>
        /// <exception cref="ValidationException">When the packet appears invalid for the given handler.</exception>
        void Invoke(IConnection connection, IPacket packet);
    }
}
