using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class DerivationException : Exception
    {
        public DerivationException(string message)
            : base(message)
        {
        }
    }
}