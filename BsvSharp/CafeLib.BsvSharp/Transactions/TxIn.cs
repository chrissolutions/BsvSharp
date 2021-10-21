#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    /// <summary>
    /// Closely mirrors the data and layout of a Bitcoin transaction input as stored in each block.
    /// Focus is on performance when processing large numbers of transactions, including blocks of transactions.
    /// Not used for making dynamic changes (building scripts).
    /// See <see cref="Transaction"/> when dynamically building a transaction input.
    /// </summary>
    public class TxIn : ITxId, IDataSerializer
    {
        /// <summary>
        /// This is the ScriptPub of the referenced Prevout.
        /// Used to sign and verify this input.
        /// </summary>
        private readonly ScriptBuilder _scriptBuilder;

        /// <summary>
        /// Setting nSequence to this value for every input in a transaction disables nLockTime.
        /// </summary>
        public const uint SequenceFinal = uint.MaxValue;

        /// <summary>
        /// Below flags apply in the context of Bip 68.
        /// If this flag set, txIn.nSequence is NOT interpreted as a relative lock-time.
        /// </summary>
        public const uint SequenceLocktimeDisableFlag = 1U << 31;

        /// <summary>
        /// If txIn.nSequence encodes a relative lock-time and this flag is set, the relative lock-time
        /// has units of 512 seconds, otherwise it specifies blocks with a granularity of 1.
        /// </summary>
        public const int SequenceLocktimeTypeFlag = 1 << 22;

        /// <summary>
        /// If txIn.nSequence encodes a relative lock-time, this mask is applied to extract that lock-time
        /// from the sequence field.
        /// </summary>
        public const uint SequenceLocktimeMask = 0x0000ffff;

        /* In order to use the same number of bits to encode roughly the same
           * wall-clock duration, and because blocks are naturally limited to occur
           * every 600s on average, the minimum granularity for time-based relative
           * lock-time is fixed at 512 seconds.  Converting from CTxIn::nSequence to
           * seconds is performed by multiplying by 512 = 2^9, or equivalently
           * shifting up by 9 bits. */
        public const uint SequenceLocktimeGranularity = 9;

        public UInt256 TxHash => PrevOut.TxId;

        public string TxId => Encoders.HexReverse.Encode(TxHash);
        public int Index => PrevOut.Index;

        public OutPoint PrevOut { get; private set; }

        public Script UtxoScript { get; private set; }

        public uint SequenceNumber { get; set; }

        public Amount Amount { get; private set; }

        public Script ScriptSig => _scriptBuilder;

        /// <summary>
        /// This is used by the Transaction during serialization checks.
        /// It is only used in the context of P2PKH transaction types and
        /// will likely be deprecated in future.
        /// 
        /// FIXME: Perform stronger check than this. We should be able to
        /// validate the _scriptBuilder Signatures. At the moment this is more
        /// of a check on where a signature is required.
        /// </summary>
        public bool IsFullySigned { get; private set; }

        /// <summary>
        /// TxIn default constructor.
        /// </summary>
        public TxIn()
        {
        }

        /// <summary>
        /// Transaction input constructor.
        /// </summary>
        /// <param name="prevOutPoint"></param>
        /// <param name="amount"></param>
        /// <param name="utxoScript"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="scriptBuilder"></param>
        public TxIn(OutPoint prevOutPoint, Amount amount, Script utxoScript, uint sequenceNumber, ScriptBuilder scriptBuilder = null)
        {
            PrevOut = prevOutPoint;
            Amount = amount;
            UtxoScript = utxoScript;
            _scriptBuilder = scriptBuilder ?? new DefaultSignedUnlockBuilder();
            SequenceNumber = sequenceNumber;
        }

        /// <summary>
        /// Transaction input constructor.
        /// </summary>
        /// <param name="prevTxId"></param>
        /// <param name="outIndex"></param>
        /// <param name="amount"></param>
        /// <param name="utxoScript"></param>
        /// <param name="scriptBuilder"></param>
        public TxIn(UInt256 prevTxId, int outIndex, Amount amount, Script utxoScript, ScriptBuilder scriptBuilder)
            : this(prevTxId, outIndex, amount, utxoScript, SequenceFinal -1, scriptBuilder)
        {
        }

        /// <summary>
        /// Transaction input constructor.
        /// </summary>
        /// <param name="prevTxId"></param>
        /// <param name="outIndex"></param>
        /// <param name="amount"></param>
        /// <param name="utxoScript"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="scriptBuilder"></param>
        public TxIn(UInt256 prevTxId, int outIndex, Amount amount, Script utxoScript = new Script(), uint sequenceNumber = SequenceFinal, ScriptBuilder scriptBuilder = null)
            : this(new OutPoint(prevTxId, outIndex), amount, utxoScript, sequenceNumber, scriptBuilder)
        {
        }

        public IDataWriter WriteTo(IDataWriter writer, object parameters) => WriteTo(writer);
        public IDataWriter WriteTo(IDataWriter writer)
        {
            writer.Write(Encoders.HexReverse.Decode(TxId));
            writer.Write(Index);
            writer.Write(UtxoScript);
            writer.Write(SequenceNumber);
            return writer;
        }

       internal bool Sign(Transaction tx, PrivateKey privateKey, SignatureHashEnum sighashType = SignatureHashEnum.All | SignatureHashEnum.ForkId)
        {
            var sigHash = new SignatureHashType(sighashType);
            var signatureHash = TransactionSignatureChecker.ComputeSignatureHash(UtxoScript, tx, tx.Inputs.IndexOf(this), sigHash, Amount);
            var signature = privateKey.SignTxSignature(signatureHash, sigHash);

            if (_scriptBuilder is SignedUnlockBuilder builder)
            {
                //culminate in injecting the derived signature into the ScriptBuilder instance
                builder.AddSignature(signature);
                IsFullySigned = true;
                return true;
            }

            IsFullySigned = false;
            return false;
        }

        public bool TryReadTxIn(ref ByteSequenceReader r)
        {
            var prevOut = new OutPoint();
            if (!prevOut.TryReadOutPoint(ref r)) return false;
            PrevOut = prevOut;

            var script = new Script();
            if (!script.TryReadScript(ref r)) return false;
            UtxoScript = script;

            if (!r.TryReadLittleEndian(out uint sequenceNumber)) return false;
            SequenceNumber = sequenceNumber;
            return true;
        }
    }
}
