using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException(string message)
            : base(message)
        {
        }
    }
}