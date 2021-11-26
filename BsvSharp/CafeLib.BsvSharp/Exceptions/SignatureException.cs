using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class SignatureException : Exception
    {
        public SignatureException(string message)
            : base(message)
        {
        }
    }
}