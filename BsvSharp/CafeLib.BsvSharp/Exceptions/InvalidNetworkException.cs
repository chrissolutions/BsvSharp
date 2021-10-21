using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidNetworkException : Exception
    {
        public InvalidNetworkException(string message)
            : base(message)
        {
        }
    }
}