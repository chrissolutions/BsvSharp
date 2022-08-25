using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TransactionInputList : TransactionIdList<TransactionInput>
    {
        public TransactionInputList()
        {
        }

        public TransactionInputList(IEnumerable<TransactionInput> list)
            : base(list)
        {
        }
    }
}