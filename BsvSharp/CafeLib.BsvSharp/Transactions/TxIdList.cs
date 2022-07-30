using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    ///	Collection of vertices.
    /// </summary>
    public class TxIdList<T> : List<T> where T : ITxId
    {
        public int Length => Count;

        protected TxIdList()
        {
        }

        protected TxIdList(IEnumerable<T> list)
            : base(list)
        {
        }
    }

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

    public class TxCollection : TxIdList<Transaction>
    {
        public TxCollection()
        {
        }

        public TxCollection(IEnumerable<Transaction> collection)
            : base(collection)
        {
        }
    }
}