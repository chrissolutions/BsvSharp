#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionException : Exception
    {
        public TransactionException(string message)
            : base(message)
        {
        }
    }
}