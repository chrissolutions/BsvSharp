using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain
{
    internal class PartialMerkleTree
    {
        protected uint TransactionCount { get; private set; }

        protected IList<UInt256> TransactionHashes { get; }

        protected IList<byte> Flags { get; set; }

        public bool IsBad { get; private set; }

        public PartialMerkleTree()
        {
            TransactionCount = 0;
        }

        /// <summary>
        /// PartialMerkleTree constructor.
        /// </summary>
        /// <param name="transactionCount"></param>
        /// <param name="hashes"></param>
        /// <param name="flags"></param>
        /// <exception cref="ArgumentException"></exception>
        public PartialMerkleTree(int transactionCount, IList<UInt256> hashes, IList<byte> flags)
            : this()
        {
            TransactionCount = (uint)transactionCount;
            TransactionHashes = hashes ?? new List<UInt256>();
            Flags = flags ?? new List<byte>();
        }

        public IList<UInt256> FilteredHashes()
        {
            // Can't have more hashes than numTransactions
            if (TransactionHashes.Count > TransactionCount)
            {
                throw new MerkleTreeException("Invalid merkle tree - more hashes than transactions");
            }

            // Can't have more flag bits than num hashes
            if (Flags.Count * 8 < TransactionHashes.Count)
            {
                throw new MerkleTreeException("Invalid merkle tree - more flag bits than hashes");
            }

            // If there is only one hash the filter do not match any txs in the block
            if (TransactionHashes.Count == 1)
            {
                return Array.Empty<UInt256>().ToList();
            }

            var height = CalcTreeHeight();
            uint hashesUsed = 0, flagBitsUsed = 0;
            var result = TraverseMerkleTree(height, 0, ref hashesUsed, ref flagBitsUsed, null, true);
            if (hashesUsed != TransactionHashes.Count)
            {
                throw new MerkleTreeException("Invalid merkle tree");
            }

            return result;
        }

        /// <summary>
        /// Returns the Merkle root hash.
        /// </summary>
        /// <returns></returns>
        public UInt256 GetMerkleRoot()
        {
            // Can't have more hashes than numTransactions
            if (TransactionHashes.Count > TransactionCount)
            {
                return UInt256.Zero;
            }

            // Can't have more flag bits than num hashes
            if (Flags.Count * 8 < TransactionHashes.Count)
            {
                return UInt256.Zero;
            }

            var height = CalcTreeHeight();

            uint flagBitsUsed = 0, hashesUsed = 0;
            var results = TraverseMerkleTree(height, 0, ref hashesUsed, ref flagBitsUsed, null);
            return hashesUsed != TransactionHashes.Count ? UInt256.Zero : results.First().Reverse();
        }

        /// <summary>
        /// Deserialize block.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        ///
        public bool Deserialize(ref ByteSequenceReader reader)
        {
            if (!reader.TryReadVariant(out var count)) return false;
            TransactionCount = (uint)count;

            for (var i = 0; i < TransactionCount; i++)
            {
                var hash = UInt256.Zero;
                if (reader.TryReadUInt256(ref hash)) return false;
                TransactionHashes.Add(hash);
            }

            if (!reader.TryReadVariant(out count)) return false;
            var vBytes = new byte[count];
            IsBad = !reader.TryCopyTo(vBytes);
            Flags = vBytes;
            return !IsBad;
        }

        /// <summary>
        /// Serialize PartialMerkleTree.
        /// </summary>
        /// <returns></returns>
        public bool Serialize(IDataWriter writer)
        {
            writer.Write(TransactionCount);
            writer.Write(new VarInt(TransactionHashes.Count));
            foreach (var hash in TransactionHashes)
            {
                writer.Write(hash.Reverse());
            }

            writer.Write(new VarInt(Flags.Count));
            foreach (var flag in Flags)
            {
                writer.Write(flag);
            }

            return true;
        }

        #region Helpers

        /// <summary>
        /// Calculate tree height.
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
        /// Helper function to efficiently calculate the number of nodes at given height in the merkle tree.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private uint CalcTreeWidth(int height)
        {
            return (uint)(TransactionCount + (1 << height) - 1) >> height;
        }

        private IList<UInt256> TraverseMerkleTree(int depth, int pos, ref uint hashesUsed, ref uint flagBitsUsed, IList<UInt256> hashes, bool checkForTxs = false)
        {
            hashes ??= new List<UInt256>();
            if (flagBitsUsed > Flags.Count * 8)
            {
                return null;
            }

            var isParentOfMatch = ((Flags[(int)(flagBitsUsed >> 3)] >> (int)(flagBitsUsed++ & 7)) & 1) != 0;
            if (depth == 0 || !isParentOfMatch)
            {
                // If at height 0, or nothing interesting below, use stored hash and do not descend.
                if (hashesUsed >= TransactionHashes.Count)
                {
                    return null;
                }

                var hash = TransactionHashes[(int)hashesUsed++];

                // In case of height 0, we have a matched txid.
                if (depth == 0 && isParentOfMatch)
                {
                    hashes.Add(hash);
                }

                return new[] { hash };
            }

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

        #endregion
    }
}
