using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaldurToolkit.Network.Connections
{
    public interface IConnectionListener
    {
        /// <summary>
        /// Gets count of working connections.
        /// </summary>
        int ConnectionCount { get; }

        /// <summary>
        /// Gets list of all working connections.
        /// </summary>
        IEnumerable<IConnection> OpenedConnections { get; }

        /// <summary>
        /// Gets a value indicating whether the listener is in the listening state.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Starts listening TCP socket on given EndPoint.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartListening();

        /// <summary>
        /// Closes socket for listening.
        /// All connected clients will stay alive.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StopListening();

        /// <summary>
        /// Disconnects all accepted connections.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisconnectAll();
    }
}
