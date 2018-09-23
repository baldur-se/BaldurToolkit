using System;

namespace BaldurToolkit.App.FileSystem
{
    public class RecursivePathException : Exception
    {
        public RecursivePathException()
        {
        }

        public RecursivePathException(string message)
            : base(message)
        {
        }

        public RecursivePathException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
