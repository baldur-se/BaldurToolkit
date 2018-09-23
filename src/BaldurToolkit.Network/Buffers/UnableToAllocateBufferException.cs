using System;

namespace BaldurToolkit.Network.Buffers
{
    public class UnableToAllocateBufferException : Exception
    {
        public UnableToAllocateBufferException()
        {
        }

        public UnableToAllocateBufferException(string message)
            : base(message)
        {
        }

        public UnableToAllocateBufferException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
