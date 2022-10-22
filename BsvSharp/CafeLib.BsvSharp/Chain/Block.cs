#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain
{
    /// A block is the largest of the blockchain's building blocks.
    ///
    /// This is the data structure that miners assemble from transactions,
    /// and over which they calculate a sha256 hash
    /// as part of their proof-of-work to win the right to extend the blockchain.
    ///
    public record Block : BlockHeader
    {
        public TransactionList Transactions { get; private set; }

        /// <summary>
        /// Block default constructor.
        /// </summary>
        public Block()
        {
            Transactions = new TransactionList();
        }

        /// <summary>
        /// Block constructor
        /// </summary>
        /// <param name="txs"></param>
        /// <param name="version"></param>
        /// <param name="hashPrevBlock"></param>
        /// <param name="hashMerkleRoot"></param>
        /// <param name="time"></param>
        /// <param name="bits"></param>
        /// <param name="nonce"></param>
        public Block
        (
            IEnumerable<Transaction> txs,
            int version,
            UInt256 hashPrevBlock,
            UInt256 hashMerkleRoot,
            uint time,
            uint bits,
            uint nonce
        )
            : base(version, hashPrevBlock, hashMerkleRoot, time, bits, nonce)
        {
            Transactions = new TransactionList(txs);
        }

        /// <summary>
        /// Create block from bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="BlockException"></exception>
        public new static Block FromBytes(ReadOnlyByteSpan bytes)
        {
            var block = new Block();
            var ros = new ReadOnlyByteSequence(bytes);
            var ok = block.Deserialize(ref ros);
            return ok ? block : throw new BlockException(nameof(bytes));
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
        /// Verify merkle tree root.
        /// </summary>
        /// <returns></returns>
        private bool VerifyMerkleRoot()
        {
            var tree = GetMerkleTree();
            var merkleRoot = tree[^1];
            return merkleRoot == MerkleRoot;
        }

        /// <summary>
        /// Read data from the byte sequence into the block.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        private bool TryDeserializeBlock(ref ByteSequenceReader reader)
        {
            if (!TryDeserializeHeader(ref reader)) return false;
            if (!reader.TryReadVariant(out var count)) return false;

            Transactions = new TransactionList();
            for (var i = 0; i < count; i++)
            {
                var tx = new Transaction();
                if (!tx.TryReadTransaction(ref reader)) return false;
                Transactions.Add(tx);
            }

            return VerifyMerkleRoot();
        }

        /// <summary>
        /// Write data from the block.
        /// </summary>
        /// <returns></returns>
        private bool TrySerializeBlock(IDataWriter writer)
        {
            if (!TrySerializeHeader(writer)) return false;

            writer.Write(new VarInt(Transactions.Length));
            foreach (var tx in Transactions)
            {
                tx.WriteTo(writer);
            }

            return true;
        }

        private List<UInt256> GetMerkleTree()
        {
            var tree = Transactions.Select(x => x.TxHash).ToList();
            if (!tree.Any()) return new List<UInt256>();

            var j = 0;
            for (var size = Transactions.Length; size > 1; size = (int)Math.Floor((decimal)(size + 1) / 2))
            {
                for (var i = 0; i < size; i += 2)
                {
                    var i2 = Math.Min(i + 1, size - 1);
                    var buf = new ByteSpan(tree[j + i].Span) + tree[j + i2].Span;
                    tree.Add(Hashes.Hash256(buf));
                }

                j += size;
            }

            return tree;
        }

        #endregion
    }
}
