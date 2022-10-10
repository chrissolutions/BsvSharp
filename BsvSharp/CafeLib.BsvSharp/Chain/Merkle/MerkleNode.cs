﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain.Merkle
{
    public class MerkleNode
    {
        public static MerkleNode GetRoot(int leafCount)
        {
            return GetRoot(Enumerable.Range(0, leafCount).Select(_ => UInt256.Zero));
        }

        public static MerkleNode GetRoot(IEnumerable<UInt256> leafs)
        {
            var row = leafs.Select(l => new MerkleNode(l)).ToList();
            if (row.Count == 0)
                return new MerkleNode(new UInt256(0));


            while (row.Count != 1)
            {
                var parentRow = new List<MerkleNode>();
                for (int i = 0; i < row.Count; i += 2)
                {
                    var left = row[i];
                    var right = i + 1 < row.Count ? row[i + 1] : null;
                    var parent = new MerkleNode(left, right);
                    parentRow.Add(parent);
                }
                row = parentRow;
            }
            return row[0];
        }
        public MerkleNode(UInt256 hash)
        {
            Hash = hash;
            IsLeaf = true;
        }

        public MerkleNode(MerkleNode left, MerkleNode right)
        {
            Left = left;
            Right = right;

            if (left != null)
                left.Parent = this;

            if (right != null)
                right.Parent = this;

            UpdateHash();
        }

        public UInt256 Hash { get; set; }

        public void UpdateHash()
        {
            var right = Right ?? Left;
            if (Left != null)
                Hash = Hashes.Hash256(ByteSpan.Concat(Left.Hash.Span, right.Hash.Span).ToArray());
        }

        public bool IsLeaf
        {
            get;
            private set;
        }

        public MerkleNode Parent
        {
            get;
            private set;
        }
        public MerkleNode Left
        {
            get;
            private set;
        }
        public MerkleNode Right
        {
            get;
            private set;
        }

        public IEnumerable<MerkleNode> EnumerateDescendants()
        {
            IEnumerable<MerkleNode> result = new[] { this };

            if (Right != null)
                result = Right.EnumerateDescendants().Concat(result);

            if (Left != null)
                result = Left.EnumerateDescendants().Concat(result);

            return result;
        }

        public MerkleNode GetLeaf(int i)
        {
            return GetLeafs().Skip(i).FirstOrDefault();
        }
        public IEnumerable<MerkleNode> GetLeafs()
        {
            return EnumerateDescendants().Where(l => l.IsLeaf);
        }


        internal bool IsMarked
        {
            get;
            set;
        }

        public IEnumerable<MerkleNode> Ancestors()
        {
            var n = Parent;
            while (n != null)
            {
                yield return n;
                n = n.Parent;
            }
        }

        public override string ToString()
        {
            return Hash.ToString();
        }

        public string ToString(bool hierarchy)
        {
            if (!hierarchy)
                return ToString();
            var builder = new StringBuilder();
            ToString(builder, 0);
            return builder.ToString();
        }

        private void ToString(StringBuilder builder, int indent)
        {
            var tabs = new string(Enumerable.Range(0, indent).Select(_ => '\t').ToArray());
            builder.Append(tabs);
            builder.AppendLine(ToString());
            Left?.ToString(builder, indent + 1);
            Right?.ToString(builder, indent + 1);
        }
    }
}
