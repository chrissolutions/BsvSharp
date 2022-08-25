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