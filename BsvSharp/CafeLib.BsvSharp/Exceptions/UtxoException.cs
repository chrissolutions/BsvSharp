using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class UtxoException : Exception
    {
        public UtxoException(string message)
            : base(message)
        {
        }
    }
}