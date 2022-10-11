using System.Linq;

namespace CafeLib.BsvSharp.Scripting.Templates
{
    //TODO : Is*Conform can be used to parses the script

    //https://github.com/bitcoin/bips/blob/master/bip-0016.mediawiki

    public abstract class ScriptTemplate
    {
        public bool CheckScriptPubKey(Script scriptPubKey)
        {
            if (!FastCheckScriptPubKey(scriptPubKey))
                return false;

            return CheckScriptPubKeyCore(scriptPubKey, scriptPubKey.Decode().ToArray());
        }

        protected virtual bool FastCheckScriptPubKey(Script scriptPubKey)
        {
            return true;
        }

        protected abstract bool CheckScriptPubKeyCore(Script scriptPubKey, Operand[] scriptPubKeyOps);

        public bool CheckScriptSig(Script scriptSig, Script scriptPubKey)
        {
            if (!FastCheckScriptSig(scriptSig, scriptPubKey))
                return false;

            return CheckScriptSigCore(scriptSig, scriptSig.Decode().ToArray(), scriptPubKey, scriptPubKey.Decode().ToArray());
        }

        protected virtual bool FastCheckScriptSig(Script scriptSig, Script scriptPubKey)
        {
            return true;
        }

        protected abstract bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubKey, Operand[] scriptPubKeyOps);
        public abstract TxOutType Type
        {
            get;
        }
    }
}
