using System;
using System.Collections.Generic;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public class OpcodePacketRouter : IPacketRouter
    {
        private readonly Dictionary<int, IOpcodePacketHandler> allHandlers = new Dictionary<int, IOpcodePacketHandler>();
        private readonly Dictionary<Type, IController> controllers = new Dictionary<Type, IController>();

        public void Register(IController controller)
        {
            this.Register(controller, controller.GetType());
        }

        public void Register(IController controller, Type registerAs)
        {
            if (!registerAs.IsInstanceOfType(controller))
            {
                throw new ControllerRegistrationException("Can not register controller: incorrect registerAs type..");
            }

            if (this.controllers.ContainsKey(registerAs))
            {
                throw new ControllerRegistrationException("Can not register controller: this controller is already registered.");
            }

            this.controllers.Add(registerAs, controller);

            foreach (var handler in controller.GetHandlers())
            {
                if (!(handler is IOpcodePacketHandler opcodeHandler))
                {
                    throw new ControllerRegistrationException("Invalid handler. Only instances of IOpcodePacketHandler are available");
                }

                if (this.allHandlers.ContainsKey(opcodeHandler.Opcode))
                {
                    throw new ControllerRegistrationException($"Can not register controller: opcode handler duplicate. Opcode: 0x{opcodeHandler.Opcode:X8}.");
                }

                this.allHandlers.Add(opcodeHandler.Opcode, opcodeHandler);
            }
        }

        public bool Remove(Type controllerType)
        {
            var result = this.controllers.Remove(controllerType);

            this.allHandlers.Clear();
            foreach (var item in this.controllers.Values)
            {
                foreach (var handler in item.GetHandlers())
                {
                    var opcodeHandler = (IOpcodePacketHandler)handler;
                    this.allHandlers.Add(opcodeHandler.Opcode, opcodeHandler);
                }
            }

            return result;
        }

        public TController Get<TController>()
            where TController : class, IController
        {
            if (!this.TryGet<TController>(out var controller))
            {
                throw new Exception($"Controller '{typeof(TController)}' was not found in the router.");
            }

            return controller;
        }

        public bool TryGet<TController>(out TController controller)
            where TController : class, IController
        {
            if (this.controllers.TryGetValue(typeof(TController), out var tmp))
            {
                controller = (TController)tmp;
                return true;
            }

            controller = null;
            return false;
        }

        public virtual void Handle(IConnection connection, IPacket packet)
        {
            if (!(packet is IOpcodePacket opcodePacket))
            {
                throw new Exception("Unsupported packet type. Only opcode packets supported.");
            }

            if (!this.allHandlers.TryGetValue(opcodePacket.Opcode, out var handler))
            {
                this.HandleUnknownPacket(connection, opcodePacket);
                return;
            }

            handler.Invoke(connection, opcodePacket);
        }

        /// <summary>
        /// Handles a packet that has no handlers in any registered controller.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="packet">The packet.</param>
        protected virtual void HandleUnknownPacket(IConnection connection, IOpcodePacket packet)
        {
            throw new UnknownPacketOpcodeException(packet.Opcode, "Unknown packet opcode.");
        }
    }
}
