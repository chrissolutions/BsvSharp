#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class DerivationException : Exception
    {
        public DerivationException(string message)
            : base(message)
        {
        }
    }
}