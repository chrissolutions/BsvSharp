#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class SignatureException : Exception
    {
        public SignatureException(string message)
            : base(message)
        {
        }
    }
}