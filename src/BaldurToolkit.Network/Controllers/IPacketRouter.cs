using System;
using BaldurToolkit.Network.Connections;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Controllers
{
    public interface IPacketRouter
    {
        /// <summary>
        /// Registers controller in the router.
        /// </summary>
        /// <param name="controller">Controller instance to register.</param>
        void Register(IController controller);

        /// <summary>
        /// Registers a controller in the router under specified type.
        /// Given controller must be an instance of given controller type.
        /// </summary>
        /// <param name="controller">Controller instance to register.</param>
        /// <param name="registerAs">The type by whitch controller will be registered.</param>
        void Register(IController controller, Type registerAs);

        /// <summary>
        /// Removes controller instance from the router.
        /// </summary>
        /// <param name="controllerType">Controller type to remove.</param>
        /// <returns>Success status.</returns>
        bool Remove(Type controllerType);

        /// <summary>
        /// Gets the controller instance from the router by controller type.
        /// </summary>
        /// <typeparam name="TController">Controller type.</typeparam>
        /// <returns>Controller instance.</returns>
        TController Get<TController>()
            where TController : class, IController;

        /// <summary>
        /// Tries to get the controller instance from the router by controller type.
        /// </summary>
        /// <typeparam name="TController">Controller type.</typeparam>
        /// <param name="controller">Found controller (if any).</param>
        /// <returns>True if the controller has been found.</returns>
        bool TryGet<TController>(out TController controller)
            where TController : class, IController;

        /// <summary>
        /// Handle packet for connection using handlers in registered controllers.
        /// </summary>
        /// <param name="connection">The connection which received the given packet.</param>
        /// <param name="packet">The packet.</param>
        void Handle(IConnection connection, IPacket packet);
    }
}
