#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TransactionList : TransactionIdList<Transaction>
    {
        public TransactionList()
        {
        }

        public TransactionList(IEnumerable<Transaction> collection)
            : base(collection)
        {
        }
    }
}