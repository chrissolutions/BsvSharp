using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class BadChecksumException : Exception
    {
        public BadChecksumException(string message)
            : base(message)
        {
        }
    }
}