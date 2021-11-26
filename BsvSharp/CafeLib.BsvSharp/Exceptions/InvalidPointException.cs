using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidPointException : Exception
    {
        public InvalidPointException(string message)
            : base(message)
        {
        }
    }
}