using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InputScriptException : Exception
    {
        public InputScriptException(string message)
            : base(message)
        {
        }
    }
}