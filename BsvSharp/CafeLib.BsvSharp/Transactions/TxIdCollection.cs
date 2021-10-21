using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    ///	Collection of vertices.
    /// </summary>
    public class TxIdCollection<T> : List<T> where T : ITxId
    {
        public int Length => Count;

        protected TxIdCollection()
        {
        }

        protected TxIdCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }
    }

    public class TxInCollection : TxIdCollection<TxIn>
    {
        public TxInCollection()
        {
        }

        public TxInCollection(IEnumerable<TxIn> collection)
            : base(collection)
        {
        }
    }

    public class TxOutCollection : TxIdCollection<TxOut>
    {
        public TxOutCollection()
        {
        }

        public TxOutCollection(IEnumerable<TxOut> collection)
            : base(collection)
        {
        }
    }

    public class TxCollection : TxIdCollection<Transaction>
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