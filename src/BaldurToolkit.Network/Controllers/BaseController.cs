using System;
using System.Collections.Generic;
using System.Reflection;

namespace BaldurToolkit.Network.Controllers
{
    public abstract class BaseController<THandleAttribute> : IController
        where THandleAttribute : Attribute
    {
        public virtual IEnumerable<IPacketHandler> GetHandlers()
        {
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var attributes = (THandleAttribute[])method.GetCustomAttributes(typeof(THandleAttribute));
                foreach (var attribute in attributes)
                {
                    yield return this.CreatePacketHandlerInstance(method, attribute);
                }
            }
        }

        protected abstract IPacketHandler CreatePacketHandlerInstance(MethodInfo method, THandleAttribute attribute);
    }
}
