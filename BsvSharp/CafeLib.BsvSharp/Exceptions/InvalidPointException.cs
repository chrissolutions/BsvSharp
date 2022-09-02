#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class InvalidPointException : Exception
    {
        public InvalidPointException(string message)
            : base(message)
        {
        }
    }
}