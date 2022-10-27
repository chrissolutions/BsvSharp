using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Signatures;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    public class PayToPubkeyTemplate : ScriptTemplate
    {
        public Script GenerateScriptPubKey(PublicKey publicKey)
        {
            var sb = new Pay2PubkeyScriptBuilder(publicKey);
            return sb.ToScript();
        }

        protected override bool CheckScriptPubkeyCore(Script scriptPubkey, Operand[] scriptPubkeyOps)
        {
            if (scriptPubkeyOps.Length != 2)
                return false;

            return scriptPubkeyOps[0].Data != VarType.Empty &&
                   PublicKey.CheckFormat(scriptPubkeyOps[0].Data, false) &&
                   scriptPubkeyOps[1].Code == Opcode.OP_CHECKSIG;
        }

        protected override bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubkey, Operand[] scriptPubkeyOps)
        {
            if (scriptSigOps.Length != 1)
                return false;

            return scriptSigOps[0].Data != VarType.Empty && PublicKey.CheckFormat(scriptSigOps[0].Data, false);
        }

        //public Script GenerateScriptSig(ECDSASignature signature)
        //{
        //    return GenerateScriptSig(new TransactionSignature(signature, SigHash.All));
        //}

        public Script GenerateScriptSig(Signature signature)
        {
            return new DefaultScriptBuilder()
                    .AddData(signature.Data)
                    .ToScript();
        }

        //public TransactionSignature ExtractScriptSigParameters(Script scriptSig)
        //{
        //    var ops = scriptSig.ToOps().ToArray();
        //    if (!CheckScriptSigCore(scriptSig, ops, null, null))
        //        return null;

        //    var data = ops[0].PushData;
        //    if (!TransactionSignature.ValidLength(data.Length))
        //        return null;
        //    try
        //    {
        //        return new TransactionSignature(data);
        //    }
        //    catch (FormatException)
        //    {
        //        return null;
        //    }
        //}

        public override TxOutType Type => TxOutType.TX_PUBKEY;

        //public PubKey ExtractScriptPubKeyParameters(Script script)
        //{
        //    var ops = script.ToOps().ToArray();
        //    if (!CheckScriptPubKeyCore(script, ops))
        //        return null;
        //    try
        //    {
        //        return new PubKey(ops[0].PushData);
        //    }
        //    catch (FormatException)
        //    {
        //        return null;
        //    }
        //}
    }
}

