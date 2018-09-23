using System;

namespace BaldurToolkit.Network.Protocol
{
    /// <summary>
    /// Protocol instance must be created for each connection and
    /// can store internal data (e.g. encryption keys/IVs, session IDs, etc.)
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// Gets minimum possible packet size.
        /// </summary>
        int MinPacketSize { get; }

        /// <summary>
        /// Gets maximum possible packet size.
        /// </summary>
        int MaxPacketSize { get; }

        /// <summary>
        /// Gets recommended size of the default buffer used to receive a packet.
        /// </summary>
        int RecommendedBufferSize { get; }

        /// <summary>
        /// Tries to read packet from specified buffer.
        /// </summary>
        /// <param name="buffer">Receive buffer.</param>
        /// <param name="offset">Data offset in the received buffer.</param>
        /// <param name="length">Number of bytes starting from offset to read.</param>
        /// <param name="packet">Created packet or null.</param>
        /// <returns>A value that indicates how many more bytes connection must read to parse the packet successfully.</returns>
        /// <remarks>
        ///     Return value can be negative, witch indicates next packet position starting from the end of segment, passed to this method.
        /// </remarks>
        int TryRead(byte[] buffer, int offset, int length, out IPacket packet);

        /// <summary>
        /// Writes specified packet to the buffer.
        /// </summary>
        /// <param name="packet">Packet to serialize.</param>
        /// <param name="buffer">Output buffer array.</param>
        /// <param name="offset">Starting position in the buffer.</param>
        /// <param name="length">Maximum allowed length.</param>
        /// <returns>Amount of written bytes.</returns>
        int Write(IPacket packet, byte[] buffer, int offset, int length);

        /// <summary>
        /// Calculates future total packet size.
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Size of the packet.</returns>
        int CalculatePacketSize(IPacket packet);
    }
}
