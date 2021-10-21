using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class BadParameterException : Exception
    {
        public BadParameterException(string message)
            : base(message)
        {
        }
    }
}