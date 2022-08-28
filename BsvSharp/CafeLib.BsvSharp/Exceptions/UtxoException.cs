#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class UtxoException : Exception
    {
        public UtxoException(string message)
            : base(message)
        {
        }
    }
}