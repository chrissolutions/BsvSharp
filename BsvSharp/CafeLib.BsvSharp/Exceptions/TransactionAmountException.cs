#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionAmountException : TransactionException
    {
        public TransactionAmountException(string message)
            : base(message)
        {
        }
    }
}