using System;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Keys;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    public class PayToPubkeyTemplate : ScriptTemplate
    {
        public Script GenerateScriptPubKey(PublicKey publicKey)
        {
            var sb = new Pay2PubkeyLockBuilder(publicKey);
            return sb.ToScript();
        }

        protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps)
        {
            var ops = scriptPubKeyOps;
            if (ops.Length != 2)
                return false;
            return ops[0].PushData != null && PubKey.Check(ops[0].PushData, false) &&
                   ops[1].Code == OpcodeType.OP_CHECKSIG;
        }

        public Script GenerateScriptSig(ECDSASignature signature)
        {
            return GenerateScriptSig(new TransactionSignature(signature, SigHash.All));
        }
        public Script GenerateScriptSig(TransactionSignature signature)
        {
            return new Script(
                Op.GetPushOp(signature.ToBytes())
            );
        }

        public TransactionSignature ExtractScriptSigParameters(Script scriptSig)
        {
            var ops = scriptSig.ToOps().ToArray();
            if (!CheckScriptSigCore(scriptSig, ops, null, null))
                return null;

            var data = ops[0].PushData;
            if (!TransactionSignature.ValidLength(data.Length))
                return null;
            try
            {
                return new TransactionSignature(data);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps)
        {
            var ops = scriptSigOps;
            if (ops.Length != 1)
                return false;

            return ops[0].PushData != null && PubKey.Check(ops[0].PushData, false);
        }

        protected override bool CheckScriptPubkeyCore(Script scriptPubkey, Operand[] scriptPubKeyOps)
        {
            throw new NotImplementedException();
        }

        protected override bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubKey, Operand[] scriptPubKeyOps)
        {
            throw new NotImplementedException();
        }

        public override TxOutType Type
        {
            get
            {
                return TxOutType.TX_PUBKEY;
            }
        }

        public PubKey ExtractScriptPubKeyParameters(Script script)
        {
            var ops = script.ToOps().ToArray();
            if (!CheckScriptPubKeyCore(script, ops))
                return null;
            try
            {
                return new PubKey(ops[0].PushData);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}

