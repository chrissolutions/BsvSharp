using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionException : Exception
    {
        public TransactionException(string message)
            : base(message)
        {
        }
    }
}