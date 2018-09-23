using System;
using System.IO;

namespace BaldurToolkit.Network.Protocol
{
    public interface IPacket
    {
        /// <summary>
        /// Gets current packet's data as a stream.
        /// </summary>
        /// <returns>The data stream.</returns>
        Stream GetDataStream();
    }
}
