using System;

namespace CafeLib.BsvSharp.Scripting.Templates;

public abstract class ScriptTemplate
{
    public bool CheckScriptPubKey(Script scriptPubKey)
    {
        if (scriptPubKey == null)
            throw new ArgumentNullException("scriptPubKey");
        if (!FastCheckScriptPubKey(scriptPubKey))
            return false;
        return CheckScriptPubKeyCore(scriptPubKey, scriptPubKey.ToOps().ToArray());
    }

    protected virtual bool FastCheckScriptPubKey(Script scriptPubKey)
    {
        return true;
    }

    protected abstract bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps);
    public bool CheckScriptSig(Script scriptSig, Script scriptPubKey)
    {
        if (scriptSig == null)
            throw new ArgumentNullException("scriptSig");

        if (!FastCheckScriptSig(scriptSig, scriptPubKey))
            return false;
        return CheckScriptSigCore(scriptSig, scriptSig.ToOps().ToArray(), scriptPubKey, scriptPubKey == null ? null : scriptPubKey.ToOps().ToArray());
    }

    protected virtual bool FastCheckScriptSig(Script scriptSig, Script scriptPubKey)
    {
        return true;
    }

    protected abstract bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps);
    public abstract TxOutType Type
    {
        get;
    }
}