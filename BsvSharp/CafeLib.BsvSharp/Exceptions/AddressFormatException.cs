#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

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