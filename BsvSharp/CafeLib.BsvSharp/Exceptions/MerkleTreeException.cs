using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class MerkleTreeException : Exception
    {
        public MerkleTreeException(string message)
            : base(message)
        {
        }
    }
}