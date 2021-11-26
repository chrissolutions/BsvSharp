using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InterpreterException : Exception
    {
        public InterpreterException(string message)
            : base(message)
        {
        }
    }
}