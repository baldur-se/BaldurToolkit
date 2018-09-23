using System;
using BaldurToolkit.Network.Controllers;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public class VirtualConnection : Connection
    {
        public VirtualConnection(IConnection baseConnection, IPacketRouter router, IProtocol protocol)
            : base(router, protocol)
        {
            this.BaseConnection = baseConnection;
        }

        public IConnection BaseConnection { get; }

        public override void Send(IPacket packet)
        {
            // You can wrap packet here
            this.BaseConnection.Send(packet);
        }

        public virtual void HandlePacket(IPacket packet)
        {
            this.Router.Handle(this, packet);
        }
    }
}
