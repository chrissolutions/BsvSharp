using System.Collections.Generic;
using System.Linq;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public record MerkleBlock : BlockHeader // : IBitcoinSerializable
    {
        public PartialMerkleTree PartialMerkleTree { get; private set; }

        /// <summary>
        /// MerkleBlock default constructor.
        /// </summary>
        public MerkleBlock()
        {
        }

        /// <summary>
        /// Create from a Block, filtering transactions according to bloom filter
        /// Note that this will call IsRelevantAndUpdate on the filter for each transaction,
        /// thus the filter will likely be modified.
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="filter">bloom filter</param>
        public MerkleBlock(Block block, BloomFilter filter)
            : base(block)
        {
            var vMatch = new List<bool>();
            var vHashes = new List<UInt256>();

            block.Transactions.ForEach((x, i) =>
            {
                vMatch.Add(filter.IsRelevantAndUpdate(block.Transactions[i]));
                vHashes.Add(x.TxHash);
            });

            PartialMerkleTree = new PartialMerkleTree(vHashes.ToArray(), vMatch.ToArray());
        }

        public MerkleBlock(Block block, UInt256[] txIds)
        {
            var vMatch = new List<bool>();
            var vHashes = new List<UInt256>();
            foreach (var hash in block.Transactions.Select(tx => tx.TxHash))
            {
                vHashes.Add(hash);
                vMatch.Add(txIds.Contains(hash));
            }

            PartialMerkleTree = new PartialMerkleTree(vHashes.ToArray(), vMatch.ToArray());
        }

        //#region IBitcoinSerializable Members

        //public void ReadWrite(BitcoinStream stream)
        //{
        //    stream.ReadWrite(ref header);
        //    stream.ReadWrite(ref _PartialMerkleTree);
        //}

        //#endregion
    }
}