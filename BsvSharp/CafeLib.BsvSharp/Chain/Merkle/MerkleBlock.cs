using System.Collections.Generic;
using System.Linq;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public class MerkleBlock // : IBitcoinSerializable
    {
        public BlockHeader Header { get; private set; }

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
        {
            Header = block;

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
            Header = block;

            List<bool> vMatch = new List<bool>();
            List<UInt256> vHashes = new List<UInt256>();
            for (int i = 0; i < block.Transactions.Count; i++)
            {
                var hash = block.Transactions[i].TxHash;
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