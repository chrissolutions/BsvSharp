using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidPathException : Exception
    {
        public InvalidPathException(string message)
            : base(message)
        {
        }
    }
}