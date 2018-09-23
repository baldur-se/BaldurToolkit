using System;
using System.Collections.Generic;
using System.Reflection;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public abstract class OpcodePacketController<TConnection, TPacket> : BaseController<HandleOpcodePacketAttribute>
        where TConnection : IConnection
        where TPacket : IPacket
    {
        private readonly List<ValidatorAppenderDelegate> validatorAppenderList = new List<ValidatorAppenderDelegate>();

        protected delegate void ValidatorAppenderDelegate(MethodInfo method, OpcodePacketHandler<TConnection, TPacket> handler);

        protected override IPacketHandler CreatePacketHandlerInstance(MethodInfo method, HandleOpcodePacketAttribute attribute)
        {
            var handler = new OpcodePacketHandler<TConnection, TPacket>(attribute.Opcode, this, method);

            foreach (var validatorAppender in this.validatorAppenderList)
            {
                validatorAppender(method, handler);
            }

            return handler;
        }

        protected void AddValidatorAppender(ValidatorAppenderDelegate appender)
        {
            this.validatorAppenderList.Add(appender);
        }

        /// <summary>
        /// Adds a validator created by validator factory to each handler which has specified attribute.
        /// </summary>
        /// <typeparam name="TAttributeType">The attribute type.</typeparam>
        /// <param name="validatorFactory">The validator factory.</param>
        protected void SetValidatorForAttribute<TAttributeType>(Func<TAttributeType, IPacketValidator<TConnection, TPacket>> validatorFactory)
            where TAttributeType : Attribute
        {
            this.validatorAppenderList.Add((method, handler) =>
            {
                var validatorAttributes = method.GetCustomAttributes<TAttributeType>();
                foreach (var validatorAttribute in validatorAttributes)
                {
                    var validator = validatorFactory(validatorAttribute);
                    if (validator != null)
                    {
                        handler.Validators.Add(validator);
                    }
                }
            });
        }
    }
}
