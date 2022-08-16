using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Signatures
{
    internal class SignatureHash
    {
        public static UInt256 SighashSingleBug = UInt256.One;

        public static UInt256 ComputeSignatureHash(
            Transaction tx,
            int inputNumber,
            SignatureHashType sigHashType,
            Script subscript,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID)
        {
            // Obtain a copy of the transaction.
            var txCopy = Transaction.FromBytes(tx.Serialize());

            // Check for replay protection.
            if ((flags & ScriptFlags.ENABLE_REPLAY_PROTECTION) != 0)
            {
                // Legacy chain's value for fork id must be of the form 0xffxxxx.
                // By xoring with 0xdead, we ensure that the value will be different
                // from the original one, even if it already starts with 0xff.
                var forkValue = sigHashType.RawSigHashType >> 8;
                var newForkValue = 0xff0000 | (forkValue ^ 0xdead);
                sigHashType = new SignatureHashType((newForkValue << 8) | (sigHashType.RawSigHashType & 0xff));
            }

            // Check for fork id.
            if (sigHashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            {
                return ComputeSighashFromForkId(tx, inputNumber, sigHashType, subscript, amount);
            }

            // For no ForkId sighash, separators need to be removed.
            var scriptCopy = RemoveCodeSeparators(subscript);

            // Erase the transaction inputs script.
            txCopy.Inputs.ForEach(x => x.UtxoScript = new Script());

            // Setup the input we wish to sign
            var tmpInput = txCopy.Inputs[inputNumber];
            txCopy.Inputs[inputNumber] = new TxIn(tmpInput.TxHash, tmpInput.Index, tmpInput.Amount, scriptCopy, tmpInput.SequenceNumber);

            // Check signature hash type.
            if (sigHashType.IsBaseNone || sigHashType.IsBaseSingle)
            {
                // Clear sequence numbers
                txCopy.Inputs.ForEach((x, i) =>
                {
                    if (i != inputNumber)
                        x.SequenceNumber = 0;
                });
            }

            if (sigHashType.IsBaseNone)
            {
                // Remove all outputs if signature hash type is none.
                txCopy.Outputs.Clear();
            }
            else if (sigHashType.IsBaseSingle)
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

            if (sigHashType.HasAnyoneCanPay)
            {
                var txKeep = tx.Inputs[inputNumber];
                txCopy.Inputs.Clear();
                txCopy.Inputs.Add(txKeep);
            }

            // Finish up...
            using var writer = new HashWriter();
            writer
                .Write(txCopy.Serialize())
                .Write(sigHashType.RawSigHashType)
                ;
            return writer.GetHashFinal();
        }

        #region Helpers

        private static UInt256 ComputeSighashFromForkId(
            Transaction tx,
            int inputNumber,
            SignatureHashType sigHashType,
            Script subscript,
            Amount amount)
        {
            var hashPrevouts = new UInt256();
            var hashSequence = new UInt256();
            var hashOutputs = new UInt256();

            if (!sigHashType.HasAnyoneCanPay)
            {
                hashPrevouts = GetPrevOutHash(tx);
            }

            if (!sigHashType.HasAnyoneCanPay && !sigHashType.IsBaseSingle && !sigHashType.IsBaseNone)
            {
                hashSequence = GetSequenceHash(tx);
            }

            if (!sigHashType.IsBaseSingle && !sigHashType.IsBaseNone)
            {
                hashOutputs = GetOutputsHash(tx);
            }
            else if (sigHashType.IsBaseSingle && inputNumber < tx.Outputs.Length)
            {
                hashOutputs = GetOutputsHash(tx, inputNumber);
            }

            // Finish up.
            using var writer = new HashWriter();

            // Start with the version...
            writer.Write(tx.Version);

            // Input prevouts/nSequence (none/all, depending on flags)
            writer.Write(hashPrevouts);
            writer.Write(hashSequence);

            // Outpoint (32-byte hash + 4-byte little endian)
            writer.Write(tx.Inputs[inputNumber].PrevOut);

            // ScriptCode of the input.
            writer.Write(subscript);

            // Amount of the output spent by this input.
            writer.Write(amount);

            // nSequence of the input
            writer.Write(tx.Inputs[inputNumber].SequenceNumber);

            // Outputs (none/one/all, depending on flags)
            writer.Write(hashOutputs);

            // Locktime
            writer.Write(tx.LockTime);

            // sighashType
            writer.Write(sigHashType.RawSigHashType);

            // return hash.
            return writer.GetHashFinal();
        }

        private static UInt256 GetPrevOutHash(Transaction tx)
        {
            using var hw = new HashWriter();
            foreach (var i in tx.Inputs)
            {
                hw.Write(i.PrevOut);
            }

            return hw.GetHashFinal();
        }

        private static UInt256 GetSequenceHash(Transaction tx)
        {
            using var hw = new HashWriter();
            foreach (var i in tx.Inputs)
            {
                hw.Write(i.SequenceNumber);
            }

            return hw.GetHashFinal();
        }

        private static UInt256 GetOutputsHash(Transaction tx, int? inputNumber = null)
        {
            using var hw = new HashWriter();

            if (inputNumber == null)
            {
                foreach (var txout in tx.Outputs)
                {
                    hw.Write(txout);
                }
            }
            else
            {
                hw.Write(tx.Outputs[inputNumber.Value]);
            }

            return hw.GetHashFinal();
        }

        /// <summary>
        /// Strips all OP_CODESEPARATOR instructions from the script.
        /// </summary>
        /// <param name="script">script</param>
        /// <returns>return script with code separators removed</returns>
        private static Script RemoveCodeSeparators(Script script)
        {
            var sb = new ScriptBuilder(script);
            var ops = sb.Ops.RemoveAll(o => o.Opcode != Opcode.OP_CODESEPARATOR);
            return sb;
        }

        #endregion
    }
}
