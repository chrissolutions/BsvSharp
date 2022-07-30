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
}