using System;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Keys;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    public class PayToPubkeyHashTemplate : ScriptTemplate
    {
        public Script GenerateScriptPubKey(Address address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            var sb = new P2PkhLockBuilder(address);
            return sb.ToScript();
        }

        public Script GenerateScriptPubKey(PublicKey publicKey)
        {
            if (publicKey == null) throw new ArgumentNullException(nameof(publicKey));
            var sb = new P2PkhLockBuilder(publicKey);
            return sb.ToScript();
        }

        //public Script GenerateScriptSig(TransactionSignature signature, PubKey publicKey)
        //{
        //    if (publicKey == null)
        //        throw new ArgumentNullException("publicKey");
        //    return new Script(
        //        signature == null ? OpcodeType.OP_0 : Op.GetPushOp(signature.ToBytes()),
        //        Op.GetPushOp(publicKey.ToBytes())
        //    );
        //}

        protected override bool FastCheckScriptPubKey(Script scriptPubKey)
        {
            return scriptPubKey.Data.Length >= 3 &&
                   scriptPubKey.Data[0] == (byte)Opcode.OP_DUP &&
                   scriptPubKey.Data[1] == (byte)Opcode.OP_HASH160 &&
                   scriptPubKey.Data[2] == 0x14;
        }

        protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Operand[] scriptPubKeyOps)
        {
            var ops = scriptPubKeyOps;
            if (ops.Length != 5)
                return false;

            return ops[0].Code == Opcode.OP_DUP &&
                   ops[1].Code == Opcode.OP_HASH160 &&
                   ops[2].Data != null && ops[2].Data.Length == 0x14 &&
                   ops[3].Code == Opcode.OP_EQUALVERIFY &&
                   ops[4].Code == Opcode.OP_CHECKSIG;
        }

        //public KeyId ExtractScriptPubKeyParameters(Script scriptPubKey)
        //{
        //    var ops = scriptPubKey.ToOps().ToArray();
        //    if (!CheckScriptPubKeyCore(scriptPubKey, ops))
        //        return null;
        //    return new KeyId(ops[2].PushData);
        //}

        protected override bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubKey, Operand[] scriptPubKeyOps)
        {
            if (scriptSigOps.Length != 2)
                return false;

            return scriptSigOps[0].Data != null && scriptSigOps[1].Data != null && PublicKey.CheckFormat(scriptSigOps[1].Data, false);
        }

        public bool CheckScriptSig(Script scriptSig)
        {
            return CheckScriptSig(scriptSig, Script.None);
        }

        //public PayToPubkeyHashScriptSigParameters ExtractScriptSigParameters(Script scriptSig)
        //{
        //    var ops = scriptSig.ToOps().ToArray();
        //    if (!CheckScriptSigCore(scriptSig, ops, null, null))
        //        return null;
        //    try
        //    {
        //        return new PayToPubkeyHashScriptSigParameters()
        //        {
        //            TransactionSignature = ops[0].Code == OpcodeType.OP_0 ? null : new TransactionSignature(ops[0].PushData),
        //            PublicKey = new PubKey(ops[1].PushData, true),
        //        };
        //    }
        //    catch (FormatException)
        //    {
        //        return null;
        //    }
        //}

        public override TxOutType Type => TxOutType.TX_PUBKEYHASH;

        //public Script GenerateScriptSig(PayToPubkeyHashScriptSigParameters parameters)
        //{
        //    return GenerateScriptSig(parameters.TransactionSignature, parameters.PublicKey);
        //}
    }
}

