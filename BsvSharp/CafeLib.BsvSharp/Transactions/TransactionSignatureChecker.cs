#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public struct TransactionSignatureChecker : ISignatureChecker
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
            return TransactionInput.SequenceFinal != _tx.Inputs[_txInIndex].SequenceNumber;
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
            if ((txToSequence & TransactionInput.SequenceLocktimeDisableFlag) != 0)
            {
                return false;
            }

            // Mask off any bits that do not have consensus-enforced meaning
            // before doing the integer comparisons
            const uint nLockTimeMask = TransactionInput.SequenceLocktimeTypeFlag | TransactionInput.SequenceLocktimeMask;
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
                    txToSequenceMasked < TransactionInput.SequenceLocktimeTypeFlag &&
                    nSequenceMasked < TransactionInput.SequenceLocktimeTypeFlag ||
                    txToSequenceMasked >= TransactionInput.SequenceLocktimeTypeFlag &&
                    nSequenceMasked >= TransactionInput.SequenceLocktimeTypeFlag
                )
            )
            {
                return false;
            }

            // Now that we know we're comparing apples-to-apples, the
            // comparison is a simple numeric one.
            return nSequenceMasked <= txToSequenceMasked;
        }

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
            return SignatureHash.ComputeSignatureHash(txTo, nIn, sigHashType, scriptCode, amount, flags);
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

        #endregion
    }
}
