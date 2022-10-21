using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafeLib.BsvSharp.Chain
{
    public record MerkleBlock : BlockHeader
    {
        private PartialMerkleTree PartialMerkleTree { get; init; }

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

            var bitArray = new BitArray(vMatch.ToArray());
            var flags = new byte[bitArray.Length];
            for (var i = 0; i < bitArray.Length; i++)
            {
                flags[i] = (byte)((bitArray[i / 8] ? 1 : 0) & 1 << i % 8);
            }

            PartialMerkleTree = new PartialMerkleTree(vHashes.Count, vHashes.ToArray(), flags);
        }

        /// <summary>
        /// MerkleBlock constructor.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="txIds"></param>
        public MerkleBlock(Block block, UInt256[] txIds)
            : base(block)
        {
            var vMatch = new List<bool>();
            var vHashes = new List<UInt256>();
            foreach (var hash in block.Transactions.Select(tx => tx.TxHash))
            {
                vHashes.Add(hash);
                vMatch.Add(txIds.Contains(hash));
            }

            var bitArray = new BitArray(vMatch.ToArray());
            var flags = new byte[bitArray.Length];
            for (var i = 0; i < bitArray.Length; i++)
            {
                flags[i] = (byte)((bitArray[i / 8] ? 1 : 0) & 1 << i % 8);
            }

            PartialMerkleTree = new PartialMerkleTree(vHashes.Count, vHashes.ToArray(), flags);
        }

        /// <summary>
        /// MerkleBlock constructor.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="transactionCount"></param>
        /// <param name="hashes"></param>
        /// <param name="flags"></param>
        public MerkleBlock
        (
            BlockHeader header,
            int transactionCount,
            IEnumerable<UInt256> hashes,
            IEnumerable<byte> flags
        )
            : base(header)
        {
            PartialMerkleTree = new PartialMerkleTree(transactionCount, hashes.ToArray(), flags.ToArray());
        }

        /// <summary>
        /// Create MerkleBlock from JSON layout.
        /// </summary>
        /// <param name="json">json layout</param>
        /// <returns>merkle block</returns>
        public static MerkleBlock FromJson(string json)
        {
            var block = JsonConvert.DeserializeObject<dynamic>(json);
            if (block == null) return null;

            var header = new BlockHeader(
                Convert.ToInt32(block.header.version.ToString()),
                UInt256.FromHex(block.header.hash.ToString()),
                UInt256.FromHex(block.header.prevHash.ToString()),
                UInt256.FromHex(block.header.merkleRoot.ToString()),
                Convert.ToUInt32(block.header.time.ToString()),
                Convert.ToUInt32(block.header.bits.ToString()),
                Convert.ToUInt32(block.header.nonce.ToString()));

            var hashes = ((JArray)block.hashes).Select(x => UInt256.FromHex(x.Value<string>())).ToArray();
            int transactionCount = Convert.ToInt32(block.numTransactions);

            var flags = block.flags is JArray jarray
                ? jarray.Select(x => Convert.ToByte(x.ToString())).ToArray()
                : new byte[] { Convert.ToByte(block.flags.ToString()) }.ToArray();

            return new MerkleBlock(header, transactionCount, hashes, flags);
        }

        /// <summary>
        /// Retrieve a collection of filtered transaction hashes.
        /// </summary>
        /// <returns>collection of hashes</returns>
        public IEnumerable<UInt256> FilteredTransactionHashes() => PartialMerkleTree.FilteredHashes();

        /// <summary>
        /// Returns *true* if the Merkle tree remains consistent in spite of missing transactions.
        /// </summary>
        /// <returns></returns>
        public bool ValidMerkleTree() => PartialMerkleTree.GetMerkleRoot() == MerkleRoot;

        /// <summary>
        /// Deserialize block.
        /// </summary>
        /// <param name="sequence">byte sequence</param>
        /// <returns>true if successful; false otherwise</returns>
        public bool Deserialize(ref ReadOnlyByteSequence sequence)
        {
            var reader = new ByteSequenceReader(sequence);
            if (!TryDeserializeBlock(ref reader)) return false;
            sequence = sequence.Data.Slice(reader.Data.Consumed);
            return true;
        }

        /// <summary>
        /// Serialize block.
        /// </summary>
        /// <returns></returns>
        public new ReadOnlyByteSequence Serialize()
        {
            var writer = new ByteDataWriter();
            if (!TrySerializeBlock(writer)) return null;
            var ros = new ReadOnlyByteSequence(writer.Span);
            return ros;
        }

        #region Helpers

        /// <summary>
        /// Read data from the byte sequence into the block.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        private bool TryDeserializeBlock(ref ByteSequenceReader reader)
        {
            return TryDeserializeHeader(ref reader) && PartialMerkleTree.Deserialize(ref reader);
        }

        /// <summary>
        /// Write data from the block.
        /// </summary>
        /// <returns></returns>
        private bool TrySerializeBlock(IDataWriter writer)
        {
            return TrySerializeHeader(writer) && PartialMerkleTree.Serialize(writer);
        }

        #endregion
    }
}