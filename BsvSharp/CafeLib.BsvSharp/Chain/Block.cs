#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Chain
{
    /// <summary>
    /// Closely mirrors the data and layout of a serialized Bitcoin block.
    /// Focus is on efficiency when processing large blocks.
    /// In particular, script data is stored as <see cref="ReadOnlySequence{Byte}"/> allowing large scripts to
    /// remain in whatever buffers were originally used. No script parsing data is maintained. 
    /// Not intended for making dynamic changes to a block (mining).
    /// </summary>
    public class Block : BlockHeader
    {
        public TxCollection Txs { get; private set; }

        public Block()
        {
            Txs = new TxCollection();
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
            Txs = new TxCollection(txs);
        }

        public bool TryReadBlock(ref ReadOnlyByteSequence ros)
        {
            var r = new ByteSequenceReader(ros);
            if (!TryReadBlock(ref r)) return false;
            ros = ros.Data.Slice(r.Data.Consumed);
            return true;
        }

        private bool TryReadBlock(ref ByteSequenceReader r)
        {
            if (!TryReadBlockHeader(ref r)) return false;
            if (!r.TryReadVariant(out var count)) return false;

            Txs = new TxCollection();
            for (var i = 0; i < count; i++)
            {
                var t = new Transaction();
                if (!t.TryReadTransaction(ref r)) return false;
                Txs.Add(t);
            }

            return VerifyMerkleRoot();
        }

        private UInt256 ComputeMerkleRoot() => Txs.ComputeMerkleRoot();

        private bool VerifyMerkleRoot() => ComputeMerkleRoot() == MerkleRoot;

        public IEnumerable<(Transaction tx, TxOut o, int i)> GetOutputsSendingToAddresses(UInt160[] addresses)
        {
            var v = new UInt160();
            foreach (var tx in Txs)
            {
                foreach (var o in tx.Outputs)
                {
                    foreach (var op in o.Script.Decode())
                    {
                        if (op.Code == Opcode.OP_PUSH20) 
                        {
                            op.Data.CopyTo(v.Span);
                            var i = Array.BinarySearch(addresses, v);
                            if (i >= 0) 
                            {
                                yield return (tx, o, i);
                            }
                        }
                    }
                }
            }
        }
    }
}
