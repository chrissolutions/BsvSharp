#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TransactionOutputList : TransactionIdList<TransactionOutput>
    {
        public TransactionOutputList()
        {
        }

        public TransactionOutputList(IEnumerable<TransactionOutput> list)
            : base(list)
        {
        }
    }
}