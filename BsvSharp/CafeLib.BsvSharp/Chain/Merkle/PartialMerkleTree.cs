﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public class PartialMerkleTree
    {
        public PartialMerkleTree()
        {

        }

        public uint TransactionCount { get; set; }

        public List<UInt256> Hashes { get; } = new();

        public BitArray Flags { get; set; } = new(0);

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

        //private byte ToByte(bool v)
        //{
        //    return (byte)(v ? 1 : 0);
        //}

        //#endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vTxid"></param>
        /// <param name="vMatch"></param>
        /// <exception cref="ArgumentException"></exception>
        public PartialMerkleTree(UInt256[] vTxid, bool[] vMatch)
        {
            if (vMatch.Length != vTxid.Count)
                throw new ArgumentException("The size of the array of txid and matches is different");

            TransactionCount = (uint)vTxid.Count;

            MerkleNode root = MerkleNode.GetRoot(vTxid);
            BitWriter flags = new BitWriter();

            MarkNodes(root, vMatch);
            BuildCore(root, flags);

            Flags = flags.ToBitArray();
        }

        private static void MarkNodes(MerkleNode root, bool[] vMatch)
        {
            BitReader matches = new BitReader(new BitArray(vMatch));
            foreach (var leaf in root.GetLeafs())
            {
                if (matches.Read())
                {
                    MarkToTop(leaf, true);
                }
            }
        }

        private static void MarkToTop(MerkleNode leaf, bool value)
        {
            leaf.IsMarked = value;
            foreach (var ancestor in leaf.Ancestors())
            {
                ancestor.IsMarked = value;
            }
        }

        public MerkleNode GetMerkleRoot()
        {
            MerkleNode node = MerkleNode.GetRoot((int)TransactionCount);
            BitReader flags = new BitReader(Flags);
            var hashes = Hashes.GetEnumerator();
            GetMatchedTransactionsCore(node, flags, hashes, true).AsEnumerable();
            return node;
        }
        //public bool Check(UInt256 expectedMerkleRootHash = null)
        //{
        //    try
        //    {
        //        var hash = GetMerkleRoot().Hash;
        //        return expectedMerkleRootHash == null || hash == expectedMerkleRootHash;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        private void BuildCore(MerkleNode node, BitWriter flags)
        {
            while (true)
            {
                if (node == null) return;

                flags.Write(node.IsMarked);

                if (node.IsLeaf || !node.IsMarked) Hashes.Add(node.Hash);

                if (node.IsMarked)
                {
                    BuildCore(node.Left, flags);
                    node = node.Right;
                    continue;
                }

                break;
            }
        }

        public IEnumerable<UInt256> GetMatchedTransactions()
        {
            BitReader flags = new BitReader(Flags);
            MerkleNode root = MerkleNode.GetRoot((int)TransactionCount);
            var hashes = Hashes.GetEnumerator();
            return GetMatchedTransactionsCore(root, flags, hashes, false);
        }

        private IEnumerable<UInt256> GetMatchedTransactionsCore(MerkleNode node, BitReader flags, IEnumerator<UInt256> hashes, bool calculateHash)
        {
            if (node == null)
                return Array.Empty<UInt256>();

            node.IsMarked = flags.Read();

            if (node.IsLeaf || !node.IsMarked)
            {
                hashes.MoveNext();
                node.Hash = hashes.Current;
            }
            if (!node.IsMarked)
                return Array.Empty<UInt256>();

            if (node.IsLeaf)
                return new[] { node.Hash };

            var left = GetMatchedTransactionsCore(node.Left, flags, hashes, calculateHash);
            var right = GetMatchedTransactionsCore(node.Right, flags, hashes, calculateHash);

            if (calculateHash)
                node.UpdateHash();

            return left.Concat(right);
        }

        public MerkleNode TryGetMerkleRoot()
        {
            try
            {
                return GetMerkleRoot();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Remove superflous branches
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public PartialMerkleTree Trim(params UInt256[] matchedTransactions)
        {
            PartialMerkleTree trimmed = new PartialMerkleTree();
            trimmed.TransactionCount = TransactionCount;
            var root = GetMerkleRoot();
            foreach (var leaf in root.GetLeafs())
            {
                MarkToTop(leaf, false);
            }
            BitWriter flags = new BitWriter();
            foreach (var leaf in root.GetLeafs().Where(l => matchedTransactions.Contains(l.Hash)))
            {
                MarkToTop(leaf, true);
            }
            trimmed.BuildCore(root, flags);
            trimmed.Flags = flags.ToBitArray();
            return trimmed;
        }
    }
}