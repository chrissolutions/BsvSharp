using System;
using System.Collections.Generic;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    internal class PartialMerkleTree
    {
        protected uint TransactionCount { get; set; }

        protected List<UInt256> TransactionHashes { get; } = new();

        protected List<bool> Flags { get; set; } = new();

        protected bool IsBad { get; private set; }

        public PartialMerkleTree()
        {
            TransactionCount = 0;
            IsBad = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vTxid"></param>
        /// <param name="vMatch"></param>
        /// <exception cref="ArgumentException"></exception>
        public PartialMerkleTree(ReadOnlySpan<UInt256> vTxid, ReadOnlySpan<bool> vMatch)
            : this()
        {
            TransactionCount = (uint)vTxid.Length;

            // calculate height of tree
            var height = 0;
            while(CalcTreeWidth(height) > 1)
                ++height;

            // traverse the partial tree
            TraverseAndBuild(height, 0, vTxid, vMatch);
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
            var nHeight = 0;
            while (CalcTreeWidth(nHeight) > 1)
            {
                nHeight++;
            }

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

        /// <summary>
        /// Serialize block.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyByteSequence Serialize()
        {
            var writer = new ByteDataWriter();
            // if (!TrySerializeBlock(writer)) return null;
            var ros = new ReadOnlyByteSequence(writer.Span);
            return ros;
        }


        // serialization implementation
        //#region IBitcoinSerializable Members

        //public void ReadWrite(BitcoinStream stream)
        //{
        //    stream.ReadWrite(ref _TransactionCount);
        //    stream.ReadWrite(ref _Hashes);
        //    byte[] vBytes = null;
        //    if (!stream.Serializing)
        //    {
        //        stream.ReadWriteAsVarString(ref vBytes);
        //        BitWriter writer = new BitWriter();
        //        for (int p = 0; p < vBytes.Length * 8; p++)
        //            writer.Write((vBytes[p / 8] & (1 << (p % 8))) != 0);
        //        _Flags = writer.ToBitArray();
        //    }
        //    else
        //    {
        //        vBytes = new byte[(_Flags.Length + 7) / 8];
        //        for (int p = 0; p < _Flags.Length; p++)
        //            vBytes[p / 8] |= (byte)(ToByte(_Flags.Get(p)) << (p % 8));
        //        stream.ReadWriteAsVarString(ref vBytes);
        //    }
        //}

        //#endregion

        #region Helpers

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
        private UInt256 CalcHash(int height, uint pos, ReadOnlySpan<UInt256> vTxId)
        {
            if (height == 0)
            {
                // hash at height 0 is the transaction themselves.
                return vTxId[(int)pos];
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
        private void TraverseAndBuild(int height, uint pos, ReadOnlySpan<UInt256> vTxid, ReadOnlySpan<bool> vMatch)
        {
            while (true)
            {
                // Determine whether this node is the parent of at least one matched txid.
                var fParentOfMatch = false;
                for (var p = pos << height; p < (pos + 1) << height && p < TransactionCount; p++)
                {
                    fParentOfMatch |= vMatch[(int)p];
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
            if (nBitsUsed >= vMatch.Count)
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
