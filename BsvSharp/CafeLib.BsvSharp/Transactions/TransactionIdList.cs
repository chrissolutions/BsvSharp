#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    ///	Collection of vertices.
    /// </summary>
    public class TransactionIdList<T> : List<T> where T : ITransactionId
    {
        public int Length => Count;

        protected TransactionIdList()
        {
        }

        protected TransactionIdList(IEnumerable<T> list)
            : base(list)
        {
        }
    }
}