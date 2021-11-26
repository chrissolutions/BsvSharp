#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public class TransactionSignatureChecker : ISignatureChecker
    {
        private readonly Transaction _tx;
        private readonly int _txInIndex;
        private readonly Amount _amount;

        private const int LocktimeThreshold = 500000000; // Tue Nov  5 00:53:20 1985 UTC

        /// <summary>
        /// Transaction signature checker constructor.
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="txInIndex"></param>
        /// <param name="amount"></param>
        public TransactionSignatureChecker(Transaction tx, int txInIndex, Amount amount)
        {
            _tx = tx;
            _txInIndex = txInIndex;
            _amount = amount;
        }

        public bool CheckSignature(VarType scriptSig, VarType vchPubKey, Script script, ScriptFlags flags)
        {
            if (scriptSig.IsEmpty) return false;
            if (vchPubKey.IsEmpty) return false;
            var publicKey = new PublicKey(vchPubKey);
            return publicKey.IsValid && VerifyTransaction(publicKey, scriptSig, script, _amount);
        }

        public bool CheckLockTime(uint lockTime)
        {
            // There are two kinds of nLockTime: lock-by-blockheight
            // and lock-by-blocktime, distinguished by whether
            // nLockTime < LOCKTIME_THRESHOLD.
            //
            // We want to compare apples to apples, so fail the script
            // unless the type of nLockTime being tested is the same as
            // the nLockTime in the transaction.
            if (
                !(
                    _tx.LockTime < LocktimeThreshold && lockTime < LocktimeThreshold ||
                    _tx.LockTime >= LocktimeThreshold && lockTime >= LocktimeThreshold
                )
            )
            {
                return false;
            }

            // Now that we know we're comparing apples-to-apples, the
            // comparison is a simple numeric one.
            if (lockTime > _tx.LockTime)
            {
                return false;
            }

            // Finally the nLockTime feature can be disabled and thus
            // CHECKLOCKTIMEVERIFY bypassed if every txIn has been
            // finalized by setting nSequence to max int. The
            // transaction would be allowed into the blockchain, making
            // the opCode ineffective.
            //
            // Testing if this vin is not final is sufficient to
            // prevent this condition. Alternatively we could test all
            // inputs, but testing just this input minimizes the data
            // required to prove correct CHECKLOCKTIMEVERIFY execution.
            return TxIn.SequenceFinal != _tx.Inputs[_txInIndex].SequenceNumber;
        }

        /// <summary>
        /// Translated from bitcoin core's CheckSequence.
        /// </summary>
        /// <param name="sequenceNumber"></param>
        /// <returns></returns>
        public bool CheckSequence(uint sequenceNumber)
        {
            // Relative lock times are supported by comparing the passed
            // in operand to the sequence number of the input.
            var txToSequence = _tx.Inputs[_txInIndex].SequenceNumber;

            // Fail if the transaction's version number is not set high
            // enough to trigger Bip 68 rules.
            if (_tx.Version < 2)
            {
                return false;
            }

            // Sequence numbers with their most significant bit set are not
            // consensus constrained. Testing that the transaction's sequence
            // number do not have this bit set prevents using this property
            // to get around a CHECKSEQUENCEVERIFY check.
            if ((txToSequence & TxIn.SequenceLocktimeDisableFlag) != 0)
            {
                return false;
            }

            // Mask off any bits that do not have consensus-enforced meaning
            // before doing the integer comparisons
            const uint nLockTimeMask = TxIn.SequenceLocktimeTypeFlag | TxIn.SequenceLocktimeMask;
            var txToSequenceMasked = txToSequence & nLockTimeMask;
            var nSequenceMasked = sequenceNumber & nLockTimeMask;

            // There are two kinds of nSequence: lock-by-blockheight
            // and lock-by-blocktime, distinguished by whether
            // nSequenceMasked < CTxIn::SEQUENCE_LOCKTIME_TYPE_FLAG.
            //
            // We want to compare apples to apples, so fail the script
            // unless the type of nSequenceMasked being tested is the same as
            // the nSequenceMasked in the transaction.
            if (
                !(
                    txToSequenceMasked < TxIn.SequenceLocktimeTypeFlag &&
                    nSequenceMasked < TxIn.SequenceLocktimeTypeFlag ||
                    txToSequenceMasked >= TxIn.SequenceLocktimeTypeFlag &&
                    nSequenceMasked >= TxIn.SequenceLocktimeTypeFlag
                )
            )
            {
                return false;
            }

            // Now that we know we're comparing apples-to-apples, the
            // comparison is a simple numeric one.
            return nSequenceMasked <= txToSequenceMasked;
        }

        /// <summary>
        /// Compute signature hash.
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <param name="txTo"></param>
        /// <param name="nIn"></param>
        /// <param name="sigHashType"></param>
        /// <param name="amount"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static UInt256 ComputeSignatureHash
        (
            Script scriptCode,
            Transaction txTo,
            int nIn,
            SignatureHashType sigHashType,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            if (sigHashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            {
                var hashPrevOuts = new UInt256();
                var hashSequence = new UInt256();
                var hashOutputs = new UInt256();

                if (!sigHashType.HasAnyoneCanPay)
                {
                    hashPrevOuts = GetPrevOutHash(txTo);
                }

                var baseNotSingleOrNone =
                    (sigHashType.GetBaseType() != BaseSignatureHashEnum.Single) &&
                    (sigHashType.GetBaseType() != BaseSignatureHashEnum.None);

                if (!sigHashType.HasAnyoneCanPay && baseNotSingleOrNone)
                {
                    hashSequence = GetSequenceHash(txTo);
                }

                if (baseNotSingleOrNone)
                {
                    hashOutputs = GetOutputsHash(txTo);
                }
                else if (sigHashType.GetBaseType() == BaseSignatureHashEnum.Single && nIn < txTo.Outputs.Count)
                {
                    using var hw = new HashWriter();
                    hw.Write(txTo.Outputs[nIn]);
                    hashOutputs = hw.GetHashFinal();
                }

                using var writer = new HashWriter();
                writer
                    // Version
                    .Write(txTo.Version)
                    // Input prevouts/nSequence (none/all, depending on flags)
                    .Write(hashPrevOuts)
                    .Write(hashSequence)
                    // The input being signed (replacing the scriptSig with scriptCode +
                    // amount). The prevout may already be contained in hashPrevout, and the
                    // nSequence may already be contain in hashSequence.
                    .Write(txTo.Inputs[nIn].PrevOut)
                    .Write(scriptCode)
                    .Write(amount)
                    .Write(txTo.Inputs[nIn].SequenceNumber)
                    // Outputs (none/one/all, depending on flags)
                    .Write(hashOutputs)
                    // Locktime
                    .Write(txTo.LockTime)
                    // Sighash type
                    .Write(sigHashType.RawSigHashType);

                return writer.GetHashFinal();
            }

            if (nIn >= txTo.Inputs.Count)
            {
                //  nIn out of range
                return UInt256.One;
            }

            // Check for invalid use of SIGHASH_SINGLE
            if (sigHashType.GetBaseType() == BaseSignatureHashEnum.Single && nIn >= txTo.Outputs.Count)
            {
                //  nOut out of range
                return UInt256.One;
            }

            {
                // Original digest algorithm...
                var hasAnyoneCanPay = sigHashType.HasAnyoneCanPay;
                // ReSharper disable once UnusedVariable
                var numberOfInputs = hasAnyoneCanPay ? 1 : txTo.Inputs.Count;
                using var writer = new HashWriter();
                // Start with the version...
                writer.Write(txTo.Version);
                // Add Input(s)...
                if (hasAnyoneCanPay)
                {
                    // AnyoneCanPay serializes only the input being signed.
                    var i = txTo.Inputs[nIn];
                    writer
                        .Write((byte)1)
                        .Write(i.PrevOut)
                        .Write(scriptCode, true)
                        .Write(i.SequenceNumber);
                }
                else
                {
                    // Non-AnyoneCanPay case. Process all inputs but handle input being signed in its own way.
                    var isSingleOrNone = sigHashType.IsBaseSingle || sigHashType.IsBaseNone;
                    writer.Write(txTo.Inputs.Count.AsVarIntBytes());
                    for (var nInput = 0; nInput < txTo.Inputs.Count; nInput++)
                    {
                        var i = txTo.Inputs[nInput];
                        writer.Write(i.PrevOut);
                        if (nInput != nIn)
                            writer.Write(Script.None);
                        else
                            writer.Write(scriptCode, true);
                        if (nInput != nIn && isSingleOrNone)
                            writer.Write(0);
                        else
                            writer.Write(i.SequenceNumber);
                    }
                }
                // Add Output(s)...
                var nOutputs = sigHashType.IsBaseNone ? 0 : sigHashType.IsBaseSingle ? nIn + 1 : txTo.Outputs.Count;
                writer.Write(nOutputs.AsVarIntBytes());
                for (var nOutput = 0; nOutput < nOutputs; nOutput++)
                {
                    if (sigHashType.IsBaseSingle && nOutput != nIn)
                        writer.Write(TxOut.Empty);
                    else
                        writer.Write(txTo.Outputs[nOutput]);
                }
                // Finish up...
                writer
                    .Write(txTo.LockTime)
                    .Write(sigHashType.RawSigHashType)
                    ;
                return writer.GetHashFinal();
            }
        }

        #region Helpers

        private bool VerifyTransaction
        (
            PublicKey publicKey,
            VarType signature,
            Script subScript,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            var hashType = new SignatureHashType(signature.LastByte);
            var sigHash = ComputeSignatureHash(subScript, _tx, _txInIndex, hashType, amount, flags);
            return publicKey.Verify(sigHash, signature);
        }

        private static UInt256 GetPrevOutHash(Transaction txTo)
        {
            using var hw = new HashWriter();
            foreach (var i in txTo.Inputs)
            {
                hw.Write(i.PrevOut);
            }

            return hw.GetHashFinal();
        }

        private static UInt256 GetSequenceHash(Transaction txTo)
        {
            using var hw = new HashWriter();
            foreach (var i in txTo.Inputs)
            {
                hw.Write(i.SequenceNumber);
            }

            return hw.GetHashFinal();
        }

        private static UInt256 GetOutputsHash(Transaction txTo)
        {
            using var hw = new HashWriter();
            foreach (var o in txTo.Outputs)
            {
                hw.Write(o);
            }

            return hw.GetHashFinal();
        }

        #endregion
    }
}
