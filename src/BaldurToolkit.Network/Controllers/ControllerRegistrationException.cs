using System;

namespace BaldurToolkit.Network.Controllers
{
    public class ControllerRegistrationException : Exception
    {
        public ControllerRegistrationException()
        {
        }

        public ControllerRegistrationException(string message)
            : base(message)
        {
        }

        public ControllerRegistrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
