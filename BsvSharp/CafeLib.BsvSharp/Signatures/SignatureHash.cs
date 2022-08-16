using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Util.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeLib.BsvSharp.Signatures
{
    internal class SignatureHash
    {
        public static UInt256 SighashSingleBug = UInt256.One;

        public static UInt256 ComputeSignatureHash(Transaction tx,
            int inputNumber,
            SignatureHashType sighashType,
            Script subscript,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            // Obtain a copy of the transaction.
            var txCopy = Transaction.FromBytes(tx.Serialize());

            // Check for replay protection.
            if ((flags & ScriptFlags.ENABLE_REPLAY_PROTECTION) != 0)
            {
                // Legacy chain's value for fork id must be of the form 0xffxxxx.
                // By xoring with 0xdead, we ensure that the value will be different
                // from the original one, even if it already starts with 0xff.
                var forkValue = sighashType.RawSigHashType >> 8;
                var newForkValue = 0xff0000 | (forkValue ^ 0xdead);
                sighashType = new SignatureHashType((newForkValue << 8) | (sighashType.RawSigHashType & 0xff));
            }

            // Check for fork id.
            if (sighashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            {
                return ComputeSighashFromForkId(tx, inputNumber, sighashType, subscript, amount, flags);
            }

            // For no ForkId sighash, separators need to be removed.
            var scriptCopy = RemoveCodeSeparators(subscript);

            // Erase the txn input scripts.
            txCopy.Inputs.ForEach(x => x.UtxoScript = new Script());

            // Setup the input we wish to sign
            var tmpInput = txCopy.Inputs[inputNumber];
            txCopy.Inputs[inputNumber] = new TxIn(tmpInput.TxHash, tmpInput.Index, tmpInput.Amount, subscript, tmpInput.SequenceNumber);

            // Check signature hash type.
            if (sighashType.IsBaseNone || sighashType.IsBaseSingle)
            {
                // Clear sequence numbers
                txCopy.Inputs.ForEach((x, i) =>
                {
                    if (i != inputNumber)
                        x.SequenceNumber = 0;
                });
            }

            if (sighashType.IsBaseNone)
            {
                // Remove all outputs if signature hash type is none.
                txCopy.Outputs.Clear();
            }
            else if (sighashType.IsBaseSingle)
            {
                // The SIGHASH_SINGLE bug.
                // https://bitcointalk.org/index.php?topic=260595.0
                if (inputNumber >= txCopy.Outputs.Length)
                {
                    return SighashSingleBug;
                }

                var txCopyOut = txCopy.Outputs[inputNumber];
                var txOut = new TxOut(txCopyOut.TxHash, txCopyOut.Index, txCopyOut.Script, txCopyOut.IsChangeOutput);

                // Resize outputs to current size of inputIndex + 1
                txCopy.Outputs.Clear();
                for (var ii = 0; ii < inputNumber + 1; ++ii)
                {
                    txCopy.Outputs.Add(new TxOut(UInt256.Zero, 0, new Amount(-1L), new()));
                }

                // Add back the saved output in the corresponding position of inputIndex
                txCopy.Outputs[inputNumber] = txOut;
            }

            if (sighashType.HasAnyoneCanPay)
            {
                var txKeep = tx.Inputs[inputNumber];
                txCopy.Inputs.Clear();
                txCopy.Inputs.Add(txKeep);
            }

            // Finish up...
            using var writer = new HashWriter();
            writer
                .Write(txCopy.Serialize())
                .Write(sighashType.RawSigHashType)
                ;
            return writer.GetHashFinal();
        }

        #region Helpers

        private static UInt256 ComputePreImage(Transaction tx,
            int inputNumber,
            SignatureHashType sighashType,
            Script subscript,
            Amount amount,
            ScriptFlags flags)
        {
            // Obtain a copy of the transaction.
            var txCopy = Transaction.FromBytes(tx.Serialize());

            // Check for replay protection.
            if ((flags & ScriptFlags.ENABLE_REPLAY_PROTECTION) != 0)
            {
                // Legacy chain's value for fork id must be of the form 0xffxxxx.
                // By xoring with 0xdead, we ensure that the value will be different
                // from the original one, even if it already starts with 0xff.
                var forkValue = sighashType.RawSigHashType >> 8;
                var newForkValue = 0xff0000 | (forkValue ^ 0xdead);
                sighashType = new SignatureHashType((newForkValue << 8) | (sighashType.RawSigHashType & 0xff));
            }

            // Check for fork id.
            if (sighashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            {
                return ComputeSighashFromForkId(tx, inputNumber, sighashType, subscript, amount, flags);
            }

            // For no ForkId sighash, separators need to be removed.
            var scriptCopy = RemoveCodeSeparators(subscript);

            // Erase the txn input scripts.
            txCopy.Inputs.ForEach(x => x.UtxoScript = new Script());

            // Setup the input we wish to sign
            var tmpInput = txCopy.Inputs[inputNumber];
            txCopy.Inputs[inputNumber] = new TxIn(tmpInput.TxHash, tmpInput.Index, tmpInput.Amount, subscript, tmpInput.SequenceNumber);

            // Check signature hash type.
            if (sighashType.IsBaseNone || sighashType.IsBaseSingle)
            {
                // Clear sequence numbers
                txCopy.Inputs.ForEach((x, i) =>
                {
                    if (i != inputNumber)
                        x.SequenceNumber = 0;
                });
            }

            if (sighashType.IsBaseNone)
            {
                // Remove all outputs if signature hash type is none.
                txCopy.Outputs.Clear();
            }
            else if (sighashType.IsBaseSingle)
            {
                // The SIGHASH_SINGLE bug.
                // https://bitcointalk.org/index.php?topic=260595.0
                if (inputNumber >= txCopy.Outputs.Length)
                {
                    return SighashSingleBug;
                }

                var txCopyOut = txCopy.Outputs[inputNumber];
                var txOut = new TxOut(txCopyOut.TxHash, txCopyOut.Index, txCopyOut.Script, txCopyOut.IsChangeOutput);

                // Resize outputs to current size of inputIndex + 1
                txCopy.Outputs.Clear();
                for (var ii = 0; ii < inputNumber + 1; ++ii)
                {
                    txCopy.Outputs.Add(new TxOut(UInt256.Zero, 0, new Amount(-1L), new()));
                }

                // Add back the saved output in the corresponding position of inputIndex
                txCopy.Outputs[inputNumber] = txOut;
            }

            if (sighashType.HasAnyoneCanPay)
            {
                var txKeep = tx.Inputs[inputNumber];
                txCopy.Inputs.Clear();
                txCopy.Inputs.Add(txKeep);
            }

            // Finish up...
            using var writer = new HashWriter();
            writer
                .Write(txCopy.Serialize())
                .Write(sighashType.RawSigHashType)
                ;
            return writer.GetHashFinal();
        }

        private static UInt256 ComputeSighashFromForkId(Transaction tx,
            int inputIndex,
            SignatureHashType sighashType,
            Script scriptCode,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            return UInt256.Zero;
        }

        private static Script RemoveCodeSeparators(Script script)
        {
            return new Script();
        }

        #endregion
    }
}
