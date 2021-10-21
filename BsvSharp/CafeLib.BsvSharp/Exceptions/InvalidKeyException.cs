using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException(string message)
            : base(message)
        {
        }
    }
}