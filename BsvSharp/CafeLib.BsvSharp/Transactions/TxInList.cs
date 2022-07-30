using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TxInList : TxIdList<TxIn>
    {
        public TxInList()
        {
        }

        public TxInList(IEnumerable<TxIn> list)
            : base(list)
        {
        }
    }
}