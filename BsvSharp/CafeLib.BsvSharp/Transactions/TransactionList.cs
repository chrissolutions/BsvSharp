using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TransactionList : TxIdList<Transaction>
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