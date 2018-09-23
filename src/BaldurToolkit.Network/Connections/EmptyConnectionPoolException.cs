using System;

namespace BaldurToolkit.Network.Connections
{
    public class EmptyConnectionPoolException : Exception
    {
        public EmptyConnectionPoolException()
        {
        }

        public EmptyConnectionPoolException(string message)
            : base(message)
        {
        }

        public EmptyConnectionPoolException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
