using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    public class TxOutList : TxIdList<TxOut>
    {
        public TxOutList()
        {
        }

        public TxOutList(IEnumerable<TxOut> list)
            : base(list)
        {
        }
    }
}