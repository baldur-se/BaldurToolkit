using System;
using System.Net;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public interface IConnection
    {
        /// <summary>
        /// Occurs when connection was closed.
        /// </summary>
        event EventHandler<ConnectionClosedEventArgs> Closed;

        /// <summary>
        /// Gets a value indicating whether the connection is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets the remote EndPoint.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Asynchronously sends a packet to the remote host.
        /// </summary>
        /// <param name="packet">Packet to send.</param>
        void Send(IPacket packet);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();
    }
}
