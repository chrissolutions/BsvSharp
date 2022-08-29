#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

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