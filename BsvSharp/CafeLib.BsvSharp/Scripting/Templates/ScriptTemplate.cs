using System.Linq;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    //TODO : Is*Conform can be used to parses the script

    //https://github.com/bitcoin/bips/blob/master/bip-0016.mediawiki

    public abstract class ScriptTemplate
    {
        public bool CheckScriptPubkey(Script scriptPubkey)
        {
            if (!FastCheckScriptPubkey(scriptPubkey))
                return false;

            return CheckScriptPubkeyCore(scriptPubkey, scriptPubkey.Decode().ToArray());
        }

        protected virtual bool FastCheckScriptPubkey(Script scriptPubKey)
        {
            return true;
        }

        protected abstract bool CheckScriptPubkeyCore(Script scriptPubkey, Operand[] scriptPubkeyOps);

        public bool CheckScriptSig(Script scriptSig, Script scriptPubkey)
        {
            if (!FastCheckScriptSig(scriptSig, scriptPubkey))
                return false;

            return CheckScriptSigCore(scriptSig, scriptSig.Decode().ToArray(), scriptPubkey, scriptPubkey.Decode().ToArray());
        }

        protected virtual bool FastCheckScriptSig(Script scriptSig, Script scriptPubkey)
        {
            return true;
        }

        protected abstract bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubkey, Operand[] scriptPubkeyOps);

        public abstract TxOutType Type
        {
            get;
        }
    }
}
