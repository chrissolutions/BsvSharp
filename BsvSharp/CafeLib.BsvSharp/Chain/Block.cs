#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;
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

        public new static Block FromBytes(byte[] bytes)
        {
            var block = new Block();
            var ros = new ReadOnlyByteSequence(bytes);
            var ok = block.TryReadBlock(ref ros);
            return ok ? block : throw new BlockException(nameof(bytes));
        }

        public bool TryReadBlock(ref ReadOnlyByteSequence sequence)
        {
            var reader = new ByteSequenceReader(sequence);
            if (!TryReadBlock(ref reader)) return false;
            sequence = sequence.Data.Slice(reader.Data.Consumed);
            return true;
        }

        public ReadOnlyByteSequence Serialize()
        {
            var buffer = new ByteDataWriter();
            var ros = new ReadOnlyByteSequence(buffer.Span);
            return ros;
        }

        #region Helpers

        /// <summary>
        /// Read data from the byte sequence into the block.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        private bool TryReadBlock(ref ByteSequenceReader reader)
        {
            if (!TryReadBlockHeader(ref reader)) return false;
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

        private UInt256 ComputeMerkleRoot() => Transactions.ComputeMerkleRoot();

        private bool VerifyMerkleRoot() => ComputeMerkleRoot() == MerkleRoot;

        //private IEnumerable<(Transaction tx, TransactionOutput o, int i)> GetOutputsSendingToAddresses(UInt160[] addresses)
        //{
        //    var v = new UInt160();
        //    foreach (var tx in Transactions)
        //    {
        //        foreach (var output in tx.Outputs)
        //        {
        //            foreach (var op in output.Script.Decode())
        //            {
        //                if (op.Code == Opcode.OP_PUSH20) 
        //                {
        //                    op.Data.CopyTo(v.Span);
        //                    var i = Array.BinarySearch(addresses, v);
        //                    if (i >= 0) 
        //                    {
        //                        yield return (tx, output, i);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
