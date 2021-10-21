using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain
{
    /// <summary>
    /// As transactions are added to a merkle tree, the path of left and right branches from the
    /// root of the tree to each transaction cycle through permutations just as the digits of a
    /// binary counter.
    /// The digits of the binary number given by the node count minus one is the path to the next
    /// node to be added.
    /// 0 - First transactions. All branches to the left.
    /// 1 - Second transaction, level 1 branch to the right.
    /// 10 - Third transaction, level 2 branch to the right, level 1 to the left.
    /// </summary>
    internal class MerkleTreeNode
    {
        /// <summary>
        /// Reference to node for the tree level above this one or null.
        /// </summary>
        public MerkleTreeNode Parent;

        /// <summary>
        /// Hash of left sub-tree (or transaction hash of leaf node, level 0).
        /// </summary>
        public UInt256 LeftHash;

        /// <summary>
        /// Hash of right sub-tree (or transaction hash of leaf node, level 0).
        /// THIS PROPERTY MUST BE IMMEDIATELY AFTER LeftHash
        /// </summary>
        public UInt256 RightHash;

        /// <summary>
        /// 
        /// </summary>
        public bool RightOfParent;

        /// <summary>
        /// HasLeft, HasRight, HasBoth indicate whether valid subtree hashes have been copied to this node's
        /// LeftHash, RightHash, and Hashes properties.
        /// </summary>
        public bool HasLeft;

        /// <summary>
        /// 
        /// </summary>
        public bool HasRight;

        /// <summary>
        /// 
        /// </summary>
        public bool HasBoth => HasLeft && HasRight;

        /// <summary>
        /// New nodes are always created on the left branch from their parent node (when they get one).
        /// They always start with a valid left hash (either Tx hash or left subtree hash.
        /// They always start without a valid right hash.
        /// </summary>
        public MerkleTreeNode(UInt256 newLeftHash, MerkleTreeNode child)
        {
            SetLeftHash(newLeftHash);
            if (child != null)
                child.Parent = this;
        }

        /// <summary>
        /// LeftOfParent and RightOfParent indicate whether this node is currently tracking a subtree
        /// to the left or right of its parent node.
        /// </summary>
        public bool IsLeftOfParent
        {
            get => !RightOfParent;
            set => RightOfParent = !value;
        }

        public void SetLeftHash(UInt256 hash)
        {
            LeftHash = hash;
            HasLeft = true;
        }

        public void SetRightHash(UInt256 hash)
        {
            RightHash = hash;
            HasRight = true;
        }

        public UInt256 ComputeHash()
        {
            return Hashes.Hash256(LeftHash);
        }

    }
}