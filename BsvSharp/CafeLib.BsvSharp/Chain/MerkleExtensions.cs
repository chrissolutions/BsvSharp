#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain
{
    public static class MerkleExtensions
    {
        public static UInt256 ComputeMerkleRoot(this IEnumerable<Transaction> txs)
        {
            var mt = new MerkleTree();
            txs.ForEach(x => mt.AddHash(x.TxHash));
            return mt.GetMerkleRoot();
        }
    }
}
