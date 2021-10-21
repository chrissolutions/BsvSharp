using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class AddressFormatException : Exception
    {
        public AddressFormatException(string message)
            : base(message)
        {
        }
    }
}