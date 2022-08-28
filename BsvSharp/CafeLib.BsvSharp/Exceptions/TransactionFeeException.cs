#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Exceptions
{
    public class TransactionFeeException : TransactionException
    {
        public TransactionFeeException(string message)
            : base(message)
        {
        }
    }
}