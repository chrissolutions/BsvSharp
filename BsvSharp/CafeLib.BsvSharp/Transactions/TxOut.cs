#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public class TxOut : ITxId, IDataSerializer, IEquatable<TxOut>
    {
        private ScriptBuilder _scriptBuilder;

        /// <summary>
        /// Empty transaction output
        /// </summary>
        public static readonly TxOut Empty = new TxOut();

        /// <summary>
        /// Owner Transaction Hash.
        /// </summary>
        public UInt256 TxHash { get; }

        /// <summary>
        /// Owner Transaction Index.
        /// </summary>
        public int Index { get; }

        public Amount Amount { get; private set; }
        public bool IsChangeOutput { get; }

        public Script Script => _scriptBuilder ?? Script.None;

        public UInt256 Hash => TxHash;

        public TxOut()
        {
            TxHash = UInt256.Zero;
            Index = -1;
            Amount = Amount.Null;
        }

        /// <summary>
        /// Convenience property to check if this output has been made unspendable
        /// using either an OP_RETURN or "OP_FALSE OP_RETURN" in first positions of
        /// the script.
        /// </summary>
        /// <returns></returns>
        public bool IsDataOut => _scriptBuilder.Ops.Any() && _scriptBuilder.Ops[0].Operand.Code == Opcode.OP_FALSE
                   || _scriptBuilder.Ops.Count >= 2 && _scriptBuilder.Ops[0].Operand.Code == Opcode.OP_RETURN;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txHash"></param>
        /// <param name="index"></param>
        /// <param name="scriptBuilder"></param>
        /// <param name="isChangeOutput"></param>
        public TxOut(UInt256 txHash, int index, ScriptBuilder scriptBuilder, bool isChangeOutput = false)
            : this (txHash, index, Amount.Zero, scriptBuilder, isChangeOutput)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txHash"></param>
        /// <param name="index"></param>
        /// <param name="amount"></param>
        /// <param name="scriptBuilder"></param>
        /// <param name="isChangeOutput"></param>
        public TxOut(UInt256 txHash, int index, Amount amount, ScriptBuilder scriptBuilder, bool isChangeOutput = false)
        {
            TxHash = txHash;
            Index = index;
            Amount = amount;
            _scriptBuilder = scriptBuilder;
            IsChangeOutput = isChangeOutput;
        }

        public bool TryReadTxOut(ref ByteSequenceReader reader)
        {
            if (!reader.TryReadLittleEndian(out long amount)) return false;
            Amount = amount;

            var script = new Script();
            if (!script.TryReadScript(ref reader)) return false;
            _scriptBuilder = new ScriptBuilder(script);
            return true;
        }

        //public void Write(BinaryWriter s)
        //{
        //    s.Write(_amount);
        //    _script.Write(s);
        //}

        //public void Read(BinaryReader s)
        //{
        //    _amount = s.ReadInt64();
        //    _script.Read(s);
        //}

        /// <summary>
        /// Returns true is satoshi amount is within valid range
        /// </summary>
        public bool ValidAmount => Amount >= Amount.Zero && Amount <= Amount.MaxValue;

        /// <summary>
        /// Returns string representation of TxOut.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{new Amount(Amount)} {_scriptBuilder.ToScript()}";

        /// <summary>
        /// Write TxOut to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <param name="parameters">parameters</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer, object parameters) => WriteTo(writer);
        
        /// <summary>
        /// Write TxOut to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer)
        {
            writer
                .Write(Amount)
                .Write(Script)
                ;
            return writer;
        }

        public override int GetHashCode() => HashCode.Combine(_scriptBuilder, TxHash, Index, IsChangeOutput);

        public bool Equals(TxOut other)
        {
            return !(other is null) && Equals(_scriptBuilder, other._scriptBuilder) && TxHash.Equals(other.TxHash) && Index == other.Index && Amount.Equals(other.Amount) && IsChangeOutput == other.IsChangeOutput;
        }

        public override bool Equals(object obj)
        {
            return obj is TxOut other && Equals(other);
        }

        public static bool operator ==(TxOut x, TxOut y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(TxOut x, TxOut y) => !(x == y);
    }
}
