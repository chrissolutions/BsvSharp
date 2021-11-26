#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    /// Closely mirrors the data and layout of a Bitcoin transaction input's previous output reference as stored in each block.
    /// Focus is on performance when processing large numbers of transactions, including blocks of transactions.
    /// </summary>
    public struct OutPoint
    {
        public UInt256 TxId { get; private set; }
        public UInt256 TxHash => TxId;
        public int Index { get; private set; }

        public OutPoint(UInt256 txId, int index)
        {
            TxId = txId; 
            Index = index;
        }

        public bool TryReadOutPoint(ref ByteSequenceReader r)
        {
            var txHash = TxId;

            if (!r.TryReadUInt256(ref txHash) || !r.TryReadLittleEndian(out int index)) return false;

            TxId = txHash;
            Index = index;
            return true;
        }

        public IDataWriter WriteTo(IDataWriter writer)
        {
            writer
                .Write(TxId)
                .Write(Index);
            return writer;
        }
    }
}
