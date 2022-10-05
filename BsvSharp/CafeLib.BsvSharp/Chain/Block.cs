#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain
{
    /// A block is the largest of the blockchain's building blocks.
    ///
    /// This is the data structure that miners assemble from transactions,
    /// and over which they calculate a sha256 hash
    /// as part of their proof-of-work to win the right to extend the blockchain.
    ///
    public class Block : BlockHeader
    {
        public TransactionList Transactions { get; private set; }

        public Block()
        {
            Transactions = new TransactionList();
        }

        /// <summary>
        /// Create block from bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <exception cref="BlockException"></exception>
        public new static Block FromBytes(byte[] bytes)
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
            if (!TryDeserialzeBlock(ref reader)) return false;
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
        /// Compute the merkle tree root.
        /// </summary>
        /// <returns></returns>
        private UInt256 ComputeMerkleRoot() => Transactions.ComputeMerkleRoot();

        /// <summary>
        /// Verify merkle tree root.
        /// </summary>
        /// <returns></returns>
        private bool VerifyMerkleRoot() => ComputeMerkleRoot() == MerkleRoot;

        /// <summary>
        /// Read data from the byte sequence into the block.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        private bool TryDeserialzeBlock(ref ByteSequenceReader reader)
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
        /// 
        /// </summary>
        /// <returns></returns>
        private bool TrySerializeBlock(IDataWriter writer)
        {
            if (!TrySerializeHeader(writer)) return false;

            foreach (var tx in Transactions)
            {
                tx.WriteTo(writer);
            }

            return true;
        }

        #endregion
    }
}
