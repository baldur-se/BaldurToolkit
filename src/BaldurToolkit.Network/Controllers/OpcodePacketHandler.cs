using System;
using System.Collections.Generic;
using System.Reflection;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public class OpcodePacketHandler<TConnection, TPacket> : IOpcodePacketHandler
        where TConnection : IConnection
        where TPacket : IPacket
    {
        private readonly HandlerDelegate handlerDelegate;

        public OpcodePacketHandler(int opcode, IController controllerInstance, MethodInfo method)
        {
            this.Opcode = opcode;
            this.handlerDelegate = (HandlerDelegate)method.CreateDelegate(typeof(HandlerDelegate), controllerInstance);
        }

        public delegate void HandlerDelegate(TConnection connection, TPacket packet);

        public int Opcode { get; set; }

        public IList<IPacketValidator<TConnection, TPacket>> Validators { get; } = new List<IPacketValidator<TConnection, TPacket>>();

        public virtual void Invoke(TConnection connection, TPacket packet)
        {
            foreach (var validator in this.Validators)
            {
                validator.Validate(connection, packet);
            }

            this.handlerDelegate(connection, packet);
        }

        public void Invoke(IConnection connection, IPacket packet)
        {
            this.Invoke((TConnection)connection, (TPacket)packet);
        }
    }
}
