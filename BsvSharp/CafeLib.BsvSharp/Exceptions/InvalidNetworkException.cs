#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidNetworkException : Exception
    {
        public InvalidNetworkException(string message)
            : base(message)
        {
        }
    }
}