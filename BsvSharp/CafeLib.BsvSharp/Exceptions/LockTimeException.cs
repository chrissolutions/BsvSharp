using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class LockTimeException : Exception
    {
        public LockTimeException(string message)
            : base(message)
        {
        }
    }
}