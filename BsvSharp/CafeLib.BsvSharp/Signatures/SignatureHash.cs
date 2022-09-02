#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Signatures
{
    internal class SignatureHash
    {
        public static UInt256 SighashSingleBug = UInt256.One;

        /// <summary>
        /// Compute signature hash.
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <param name="inputNumber">input number</param>
        /// <param name="sigHashType">sighash type</param>
        /// <param name="subscript">scriptSig</param>
        /// <param name="amount">spend amount</param>
        /// <param name="flags">script flags</param>
        /// <returns>signature hash</returns>
        public static UInt256 ComputeSignatureHash
        (
            Transaction tx,
            int inputNumber,
            SignatureHashType sigHashType,
            Script subscript,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            if ((flags & ScriptFlags.ENABLE_REPLAY_PROTECTION) != 0)
            {
                // Legacy chain's value for fork id must be of the form 0xffxxxx.
                // By xoring with 0xdead, we ensure that the value will be different
                // from the original one, even if it already starts with 0xff.
                var forkValue = sigHashType.RawSigHashType >> 8;
                var newForkValue = 0xff0000 | (forkValue ^ 0xdead);
                sigHashType = new SignatureHashType((newForkValue << 8) | (sigHashType.RawSigHashType & 0xff));
            }

            return inputNumber switch
            {
                _ when sigHashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0 =>
                    ComputeSighashForForkId(tx, inputNumber, sigHashType, subscript, amount),

                _ when inputNumber >= tx.Inputs.Count => SighashSingleBug,

                _ when sigHashType.IsBaseSingle && inputNumber >= tx.Outputs.Count => SighashSingleBug,

                _ => ComputeSighashForNonForkId(tx, inputNumber, sigHashType, subscript)
            };
        }

        #region Helpers

        /// <summary>
        /// Compute the sighash for fork id.
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <param name="inputNumber">input number</param>
        /// <param name="sigHashType">sighash type</param>
        /// <param name="subscript">scriptSig</param>
        /// <param name="amount">spend amount</param>
        /// <returns>signature hash</returns>
        private static UInt256 ComputeSighashForForkId(
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

        /// <summary>
        /// Compute the sighash for non-fork id.
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <param name="inputNumber">input number</param>
        /// <param name="sigHashType">sighash type</param>
        /// <param name="subscript">scriptSig</param>
        /// <returns>signature hash</returns>
        /// <returns></returns>
        private static UInt256 ComputeSighashForNonForkId(
            Transaction tx,
            int inputNumber,
            SignatureHashType sigHashType,
            Script subscript)
        {
            using var writer = new HashWriter();

            // Original digest algorithm...
            var scriptCopy = RemoveCodeSeparators(subscript);

            // Start with the version...
            writer.Write(tx.Version);

            // Add Input(s)...
            if (sigHashType.HasAnyoneCanPay)
            {
                // AnyoneCanPay serializes only the input being signed.
                var i = tx.Inputs[inputNumber];
                writer
                    .Write((byte)1)
                    .Write(i.PrevOut)
                    .Write(scriptCopy)
                    .Write(i.SequenceNumber);
            }
            else
            {
                // Non-AnyoneCanPay case. Process all inputs but handle input being signed in its own way.
                writer.Write(tx.Inputs.Count.AsVarIntBytes());
                for (var nInput = 0; nInput < tx.Inputs.Count; nInput++)
                {
                    var i = tx.Inputs[nInput];
                    writer.Write(i.PrevOut);
                    if (nInput != inputNumber)
                        writer.Write(Script.None);
                    else
                        writer.Write(scriptCopy);
                    if (nInput != inputNumber && (sigHashType.IsBaseSingle || sigHashType.IsBaseNone))
                        writer.Write(0);
                    else
                        writer.Write(i.SequenceNumber);
                }
            }

            // Add Output(s)...
            var nOutputs = sigHashType.IsBaseNone ? 0 : sigHashType.IsBaseSingle ? inputNumber + 1 : tx.Outputs.Count;
            writer.Write(nOutputs.AsVarIntBytes());
            for (var nOutput = 0; nOutput < nOutputs; nOutput++)
            {
                if (sigHashType.IsBaseSingle && nOutput != inputNumber)
                    writer.Write(TransactionOutput.Empty);
                else
                    writer.Write(tx.Outputs[nOutput]);
            }

            // Finish up...
            writer
                .Write(tx.LockTime)
                .Write(sigHashType.RawSigHashType)
                ;
            return writer.GetHashFinal();
        }

        /// <summary>
        /// Get hash of inputs prevouts
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <returns>hash of inputs prevouts</returns>
        private static UInt256 GetPrevOutHash(Transaction tx)
        {
            using var hw = new HashWriter();
            foreach (var i in tx.Inputs)
            {
                hw.Write(i.PrevOut);
            }

            return hw.GetHashFinal();
        }

        /// <summary>
        /// Get hash of sequence numbers
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <returns>hash of inputs sequence numbers</returns>
        private static UInt256 GetSequenceHash(Transaction tx)
        {
            using var hw = new HashWriter();
            foreach (var i in tx.Inputs)
            {
                hw.Write(i.SequenceNumber);
            }

            return hw.GetHashFinal();
        }

        /// <summary>
        /// Get hash of outputs
        /// </summary>
        /// <param name="tx">transaction</param>
        /// <param name="inputNumber">optional input number</param>
        /// <returns>hash of outputs</returns>
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
            var ops = sb.Operands.RemoveAll(o => o.Opcode == Opcode.OP_CODESEPARATOR);
            return sb;
        }

        #endregion
    }
}
