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

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public record MerkleBlock : BlockHeader 
    {
        internal PartialMerkleTree PartialMerkleTree { get; private set; }

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

        /// <summary>
        /// 
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

            PartialMerkleTree = new PartialMerkleTree(vHashes.ToArray(), vMatch.ToArray());
        }

        public MerkleBlock(string json)
        {
            var block = JsonConvert.DeserializeObject<dynamic>(json);
            Console.WriteLine();
        }

        public static MerkleBlock FromJson(string json)
        {
            var block = JsonConvert.DeserializeObject<dynamic>(json);
            if (block == null) return null;

            var version = Convert.ToInt32(block.header.version.ToString());
            var prevHash = UInt256.FromHex(block.header.prevHash.ToString());

            //var t = Convert.ToInt32(block.numTransactions.ToString());
            //var u = ((JArray)block.hashes).Select(x => UInt256.FromHex(x.ToString()));

            var header = new BlockHeader(
                Convert.ToInt32(block.header.version.ToString()),
                UInt256.FromHex(block.header.hash.ToString()),
                UInt256.FromHex(block.header.prevHash.ToString()),
                UInt256.FromHex(block.header.merkleRoot.ToString()),
                Convert.ToUInt32(block.header.time.ToString()),
                Convert.ToUInt32(block.header.bits.ToString()),
                Convert.ToUInt32(block.header.nonce.ToString()));


            var v = block.flags is JArray jarr
                ? jarr.Select(x => Convert.ToByte(x.ToString())).ToArray()
                : new byte[] { Convert.ToByte(block.flags.ToString()) }.ToArray();

            var vBytes = new BitArray(v.ToArray());
            var flags = new bool[vBytes.Length];

            for (var i = 0; i < vBytes.Length; i++)
            {
                flags[i] = ((vBytes[i / 8] ? 1 : 0) & (1 << i % 8)) != 0;
            }

            var merkleBlock = new MerkleBlock
            {
                _version = header.Version,
                _hash = header.Hash,
                _prevHash = header.PrevHash,
                _merkleRoot = header.MerkleRoot,
                _timestamp = header.Timestamp,
                _bits = header.Bits,
                _nonce = header.Nonce,

                PartialMerkleTree = new PartialMerkleTree
                    (
                        ((JArray)block.hashes).Select(x => UInt256.FromHex(x.Value<string>())).ToArray(),
                        flags
                    )
            };

            return merkleBlock;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<UInt256> FilteredTransactionHashes()
        {
            return PartialMerkleTree.FilteredHashes();
        }

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