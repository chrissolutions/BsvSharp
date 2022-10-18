using System;
using System.Collections.Generic;
using System.Linq;
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

        protected List<UInt256> TransactionHashes { get; } = new();

        protected List<bool> Flags { get; set; } = new();

        protected bool IsBad { get; private set; }

        public PartialMerkleTree()
        {
            TransactionCount = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vTxId"></param>
        /// <param name="vMatch"></param>
        /// <exception cref="ArgumentException"></exception>
        public PartialMerkleTree(IEnumerable<UInt256> vTxId, IEnumerable<bool> vMatch)
            : this()
        {
            var ids = vTxId as UInt256[] ?? vTxId.ToArray();
            TransactionCount = (uint)ids.Length;

            // calculate height of tree
            var height = 0;
            while (CalcTreeWidth(height) > 1)
                ++height;

            // traverse the partial tree
            TraverseAndBuild(height, 0, ids, vMatch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vMatch"></param>
        /// <param name="vnIndex"></param>
        /// <returns></returns>
        public UInt256 ExtractMatches(List<UInt256> vMatch, List<uint> vnIndex)
        {
            vMatch.Clear();

            // An empty set will not work
            if (TransactionCount == 0)
            {
                return UInt256.Zero;
            }

            // Check for excessively high numbers of transactions.
            // FIXME: Track the maximum block size we've seen and use it here.

            // There can never be more hashes provided than one for every txid.
            if (TransactionHashes.Count > TransactionCount)
            {
                return UInt256.Zero;
            }

            // There must be at least one bit per node in the partial tree, and at least
            // one node per hash.
            if (Flags.Count < TransactionHashes.Count)
            {
                return UInt256.Zero;
            }

            // calculate height of tree.
            var nHeight = CalcTreeHeight();

            // traverse the partial tree.
            uint bitsUsed = 0, hashUsed = 0;
            var hashMerkleRoot = TraverseAndExtract(nHeight, 0, ref bitsUsed, ref hashUsed, vMatch, vnIndex);

            // verify that no problems occurred during the tree traversal.
            if (IsBad)
            {
                return UInt256.Zero;
            }

            // verify that all bits were consumed (except for the padding caused by
            // serializing it as a byte sequence)
            if ((bitsUsed + 7) / 8 != (Flags.Count + 7) / 8)
            {
                return UInt256.Zero;
            }

            // verify that all hashes were consumed.
            return hashUsed != TransactionHashes.Count ? UInt256.Zero : hashMerkleRoot;
        }

        public List<UInt256> FilteredHashes()
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

            var hashes = new List<UInt256>();
            var indexes = new List<uint>();
            var result = ExtractMatches(hashes, indexes);

            if (result == UInt256.Zero)
            {
                throw new MerkleTreeException("Invalid merkle tree");
            }

            return hashes;
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
            reader.TryCopyTo(vBytes);
            Flags = new List<bool>();
            for (var i = 0; i < count * 8; i++)
            {
                Flags.Add((vBytes[i / 8] & 1 << i % 8) != 0);
            }

            IsBad = false;
            return !IsBad;
        }

        /// <summary>
        /// Serialize PartialMerkleTree.
        /// </summary>
        /// <returns></returns>
        public bool Serialize(IDataWriter writer)
        {
            writer.Write(new VarInt(TransactionCount));
            foreach (var t in TransactionHashes)
            {
                writer.Write(t);
            }

            var vBytes = new byte[(Flags.Count + 7) / 8];
            for (var p = 0; p < Flags.Count; p++)
                vBytes[p / 8] |= (byte)(Convert.ToByte(Flags[p]) << p % 8);
            writer.Write(new VarInt(vBytes.Length));
            writer.Write(new VarType(vBytes));
            return true;
        }

        #region Helpers

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
        /// Helper function to efficiently calculate the number of nodes at given height in the merkle tree.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        private uint CalcTreeWidth(int height)
        {
            return (uint)(TransactionCount + (1 << height) - 1) >> height;
        }

        /// <summary>
        /// Helper function to efficiently calculate the number of nodes at given height in the merkle tree.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="vTxId"></param>
        /// <returns></returns>
        private UInt256 CalcHash(int height, uint pos, IEnumerable<UInt256> vTxId)
        {
            if (height == 0)
            {
                // hash at height 0 is the transaction themselves.
                return vTxId.ElementAt((int)pos);
            }

            // Calculate left hash.
            var left = CalcHash(height - 1, pos * 2, vTxId);

            // Calculate right hash if not beyond the end of the array - copy left hash
            // otherwise1.
            var right = pos * 2 + 1 < CalcTreeWidth(height - 1)
                ? CalcHash(height - 1, pos * 2 + 1, vTxId)
                : left;

            // Combine sub hashes.
            return Hashes.Hash256(new ByteSpan(left.Span) + right.Span);
        }

        /// <summary>
        /// Recursive function that traverses tree nodes, storing the data as bits and hashes.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="vTxid"></param>
        /// <param name="vMatch"></param>
        private void TraverseAndBuild(int height, uint pos, IEnumerable<UInt256> vTxid, IEnumerable<bool> vMatch)
        {
            while (true)
            {
                // Determine whether this node is the parent of at least one matched txid.
                var fParentOfMatch = false;
                for (var p = pos << height; p < pos + 1 << height && p < TransactionCount; p++)
                {
                    fParentOfMatch |= vMatch.ElementAt((int)p);
                }

                // Store as flag bit.
                Flags.Add(fParentOfMatch);

                if (height == 0 || !fParentOfMatch)
                {
                    // If at height 0, or nothing interesting below, store hash and stop.
                    TransactionHashes.Add(CalcHash(height, pos, vTxid));
                }
                else
                {
                    // Otherwise, don't store any hash, but descend into the subtrees.
                    TraverseAndBuild(height - 1, pos * 2, vTxid, vMatch);
                    if (pos * 2 + 1 < CalcTreeWidth(height - 1))
                    {
                        height--;
                        pos = pos * 2 + 1;
                        continue;
                    }
                }

                break;
            }
        }

        /// <summary>
        /// Recursive function that traverses tree nodes, consuming the bits and hashes produced by TraverseAndBuild.
        /// It returns the hash of the respective node and its respective index.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <param name="nBitsUsed"></param>
        /// <param name="nHashUsed"></param>
        /// <param name="vMatch"></param>
        /// <param name="vnIndex"></param>
        /// <returns></returns>
        private UInt256 TraverseAndExtract
        (
            int height,
            uint pos,
            ref uint nBitsUsed,
            ref uint nHashUsed,
            List<UInt256> vMatch,
            List<uint> vnIndex
        )
        {
            if (nBitsUsed >= Flags.Count)
            {
                // Overflowed the bits array - failure
                IsBad = true;
                return UInt256.Zero;
            }

            bool fParentOfMatch = Flags[(int)nBitsUsed++];
            if (height == 0 || !fParentOfMatch)
            {
                // If at height 0, or nothing interesting below, use stored hash and do
                // not descend.
                if (nHashUsed >= TransactionHashes.Count)
                {
                    // Overflowed the hash array - failure
                    IsBad = true;
                    return UInt256.Zero;
                }

                var hash = TransactionHashes[(int)nHashUsed++];
                // In case of height 0, we have a matched txid.
                if (height == 0 && fParentOfMatch)
                {
                    vMatch.Add(hash);
                    vnIndex.Add(pos);
                }
                return hash;
            }

            // Otherwise, descend into the subtrees to extract matched txids and hashes.
            var left = TraverseAndExtract(height - 1, pos * 2, ref nBitsUsed, ref nHashUsed, vMatch, vnIndex);
            UInt256 right;

            if (pos * 2 + 1 < CalcTreeWidth(height - 1))
            {
                right = TraverseAndExtract(height - 1, pos * 2 + 1, ref nBitsUsed, ref nHashUsed, vMatch, vnIndex);
                if (right == left)
                {
                    // The left and right branches should never be identical, as the
                    // transaction hashes covered by them must each be unique.
                    IsBad = true;
                }
            }
            else
            {
                right = left;
            }

            // and combine them before returning.
            return Hashes.Hash256(new ByteSpan(left.Span) + right.Span);
        }

        #endregion
    }
}
