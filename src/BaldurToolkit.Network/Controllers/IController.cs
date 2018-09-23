using System;
using System.Collections.Generic;

namespace BaldurToolkit.Network.Controllers
{
    /// <summary>
    /// Represents a logically grouped set of methods to send and/or handle received data for connection.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Returns all packet handlers from current controller.
        /// </summary>
        /// <returns>The list of packet handlers.</returns>
        IEnumerable<IPacketHandler> GetHandlers();
    }
}
