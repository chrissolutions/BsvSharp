using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class ScriptException : Exception
    {
        public ScriptException(string message)
            : base(message)
        {
        }
    }
}