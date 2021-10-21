using System.Collections.Generic;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain
{
    public static class MerkleExtensions
    {
        public static UInt256 ComputeMerkleRoot(this IEnumerable<Transaction> txs)
        {
            var mt = new MerkleTree();
            foreach (var tx in txs)
            {
                mt.AddHash(tx.TxHash);
            }
            return mt.GetMerkleRoot();
        }
    }
}
