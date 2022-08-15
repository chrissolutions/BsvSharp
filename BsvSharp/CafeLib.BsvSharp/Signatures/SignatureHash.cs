using CafeLib.BsvSharp.Builders;
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
            SignatureHashType sigHashType,
            Script subscript,
            Amount amount,
            ScriptFlags flags = ScriptFlags.ENABLE_SIGHASH_FORKID
        )
        {
            var preImage = ComputePreImage(tx, inputNumber, sigHashType, subscript, amount, flags);
            return (preImage == SighashSingleBug) ? SighashSingleBug : Hashes.Sha256(preImage.Span);


            //// Obtain a copy of the transaction.
            //var txCopy = Transaction.FromBytes(tx.Serialize());

            //// Check for replay protection.
            //if ((flags & ScriptFlags.ENABLE_REPLAY_PROTECTION) != 0)
            //{
            //    // Legacy chain's value for fork id must be of the form 0xffxxxx.
            //    // By xoring with 0xdead, we ensure that the value will be different
            //    // from the original one, even if it already starts with 0xff.
            //    var forkValue = sigHashType.RawSigHashType >> 8;
            //    var newForkValue = 0xff0000 | (forkValue ^ 0xdead);
            //    sigHashType = new SignatureHashType((newForkValue << 8) | (sigHashType.RawSigHashType & 0xff));
            //}

            //// Check for fork id.
            //if (sigHashType.HasForkId && (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            //{
            //    return GetSigHashFromForkId(tx, inputNumber, sigHashType, subScript, amount, flags);
            //}

            //// For no ForkId sighash, separators need to be removed.
            //var script = RemoveCodeSeparators(subScript);

            //// Erase the txn input scripts.
            //txCopy.Inputs.ForEach(x => x.UtxoScript = new Script());

            //// Setup the input we wish to sign
            //var tmpInput = txCopy.Inputs[inputNumber];
            //txCopy.Inputs[inputNumber] = new TxIn(tmpInput.TxHash, tmpInput.Index, tmpInput.Amount, subScript, tmpInput.SequenceNumber);

            //// Check signature hash type.
            //if (sigHashType.GetBaseType() == BaseSignatureHashEnum.None || sigHashType.GetBaseType() == BaseSignatureHashEnum.Single)
            //{
            //    // clear all sequenceNumbers
            //    txCopy.Inputs.ForEach((x, i) =>
            //    {
            //        if (i != inputNumber)
            //            x.SequenceNumber = 0;
            //    });
            //}

            //// Remove all outputs if signature hash type is none.
            //if (sigHashType.GetBaseType() == BaseSignatureHashEnum.None)
            //{
            //    txCopy.Outputs.Clear();
            //}
            //else if (sigHashType.GetBaseType() == BaseSignatureHashEnum.Single)
            //{
            //    // The SIGHASH_SINGLE bug.
            //    // https://bitcointalk.org/index.php?topic=260595.0
            //    if (inputNumber >= txCopy.Outputs.Length)
            //    {
            //        return UInt256.One;
            //    }

            //    var txCopyOut = txCopy.Outputs[inputNumber];
            //    var txout = new TxOut(txCopyOut.TxHash, txCopyOut.Index, txCopyOut.Script, txCopyOut.IsChangeOutput);

            //    // Resize outputs to current size of inputIndex + 1

            //    //    var outputCount = inputNumber + 1;
            //    //    txnCopy.outputs.removeWhere((elem) => true); //remove all the outputs
            //    //                                                 //create new outputs up to inputnumer + 1
            //    //    for (var ndx = 0; ndx < inputNumber + 1; ndx++)
            //    //    {
            //    //        var tx = new TransactionOutput();
            //    //        tx.script = SVScript.fromString(""); //FIXME: What happens if there are no outputs !?
            //    //        tx.satoshis = BigInt.parse(_BITS_64_ON, radix: 16);
            //    //        txnCopy.outputs.add(tx);
            //    //    }

            //    //    //add back the saved output in the corresponding position of inputIndex
            //    //    txnCopy.outputs[inputNumber] = txout; //FIXME : ??? Is this the correct way ?

            //}


            //if (this._sighashType & SighashType.SIGHASH_ANYONECANPAY > 0)
            //{
            //    var keepTxn = this._txn.inputs[inputNumber];
            //    txnCopy.inputs.removeWhere((elem) => true); //delete all inputs
            //    txnCopy.inputs.add(keepTxn);
            //}

            //return this.toString();
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
            if (sighashType.GetBaseType() == BaseSignatureHashEnum.None || sighashType.GetBaseType() == BaseSignatureHashEnum.Single)
            {
                // Clear sequence numbers
                txCopy.Inputs.ForEach((x, i) =>
                {
                    if (i != inputNumber)
                        x.SequenceNumber = 0;
                });
            }

            // Remove all outputs if signature hash type is none.
            if (sighashType.IsBaseNone)
            {
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
                //    var keepTxn = this._txn!.inputs[inputNumber];
                //    txnCopy.inputs.removeWhere((elem) => true); //delete all inputs
                //    txnCopy.inputs.add(keepTxn);
            }

            return UInt256.Zero;
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
