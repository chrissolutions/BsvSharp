﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using CafeLib.BsvSharp.Exceptions;
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
        private int _numTransactions;
        private IList<UInt256> _hashes;
        private IList<byte> _flags;

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
            _numTransactions = transactionCount;
            _hashes = hashes.ToArray();
            _flags = flags.ToArray();

            PartialMerkleTree = new PartialMerkleTree(transactionCount, _hashes, _flags);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int CalcTreeHeight()
        {
            var height = 0;
            while (CalcTreeWidth(height) > 1)
            {
                height++;
            }
            return height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private int CalcTreeWidth(int height)
        {
            return (_numTransactions + (1 << height) - 1) >> height;
        }

        /// <summary>
        /// Returns *true* if the Merkle tree remains consistent in spite of missing transactions.
        /// </summary>
        /// <returns></returns>
        public bool ValidMerkleTree()
        {
            // Can't have more hashes than numTransactions
            if (_hashes.Count > _numTransactions)
            {
                return false;
            }

            // Can't have more flag bits than num hashes
            if (_flags.Count * 8 < _hashes.Count)
            {
                return false;
            }

            var height = CalcTreeHeight();

            uint flagBitsUsed = 0, hashesUsed = 0;
            var results = TraverseMerkleTree(height, 0, ref hashesUsed, ref flagBitsUsed, null);
            if (hashesUsed != _hashes.Count)
            {
                return false;
            }

            return results.First().Reverse() == _merkleRoot;
        }

        private IList<UInt256> TraverseMerkleTree(int depth, int pos, ref uint hashesUsed, ref uint flagBitsUsed, IList<UInt256> hashes, bool checkForTxs = false)
        {
            hashes ??= new List<UInt256>();
            if (flagBitsUsed > _flags.Count * 8)
            {
                return null;
            }

            var isParentOfMatch = ((_flags[(int)(flagBitsUsed >> 3)] >> (int)(flagBitsUsed++ & 7)) & 1) != 0;
            if (depth == 0 || !isParentOfMatch)
            {
                // If at height 0, or nothing interesting below, use stored hash and do not descend.
                if (hashesUsed >= _hashes.Count)
                {
                    return null;
                }

                var hash = _hashes[(int)hashesUsed++];

                // In case of height 0, we have a matched txid.
                if (depth == 0 && isParentOfMatch)
                {
                    hashes.Add(hash);
                }

                return new[] { hash };
            }
            else
            {
                var results = TraverseMerkleTree(depth - 1, pos * 2, ref hashesUsed, ref flagBitsUsed, hashes, checkForTxs);
                var left = results.First();
                var right = left;

                if (pos * 2 + 1 < CalcTreeWidth(depth - 1))
                {
                    results = TraverseMerkleTree(depth - 1, pos * 2 + 1, ref hashesUsed, ref flagBitsUsed, hashes, checkForTxs);
                    right = results.First();
                }

                return checkForTxs ? hashes : new[] { Hashes.Hash256(new ByteSpan(left.Reverse().Span) + right.Reverse().Span).Reverse() };
            }
        }

        #endregion
    }
}