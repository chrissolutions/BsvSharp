using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class BlockException : Exception
    {
        public BlockException(string message)
            : base(message)
        {
        }
    }
}