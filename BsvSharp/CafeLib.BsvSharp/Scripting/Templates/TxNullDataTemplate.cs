using System;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class TxNullDataTemplate : ScriptTemplate
{
    protected override bool FastCheckScriptPubkey(Script scriptPubkey)
    {
        var bytes = scriptPubkey.ToBytes(true);
        return bytes.Length >= 1 && bytes[0] == (byte)OpcodeType.OP_RETURN;
    }

    protected override bool CheckScriptPubkeyCore(Script scriptPubkey, Operand[] scriptPubKeyOps)
    {
        throw new NotImplementedException();
    }

    protected override bool CheckScriptPubkeyCore(Script scriptPubkey, Op[] scriptPubkeyOps)
    {
        var ops = scriptPubkeyOps;
        if (ops.Length < 1)
            return false;
        if (ops[0].Code != OpcodeType.OP_RETURN)
            return false;
        if (ops.Length == 2)
        {
            return ops[1].PushData != null && ops[1].PushData.Length <= MAX_OPRETURN_SIZE;
            throw new NotSupportedException();
        }
        return true;
    }
    public byte[] ExtractScriptPubkeyParameters(Script scriptPubkey)
    {
        if (!FastCheckScriptPubkey(scriptPubkey))
            return null;
        var ops = scriptPubkey.ToOps().ToArray();
        if (ops.Length != 2)
            return null;
        if (ops[1].PushData == null || ops[1].PushData.Length > MAX_OPRETURN_SIZE)
            return null;
        return ops[1].PushData;
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubkey, Op[] scriptPubkeyOps)
    {
        return false;
    }

    public const int MAX_OPRETURN_SIZE = 40;
    public Script GenerateScriptPubkey(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException("data");
        if (data.Length > MAX_OPRETURN_SIZE)
            throw new ArgumentOutOfRangeException("data", "Data in OP_RETURN should have a maximum size of " + MAX_OPRETURN_SIZE + " bytes");

        return new Script(OpcodeType.OP_RETURN,
            Op.GetPushOp(data));
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubKey, Operand[] scriptPubKeyOps)
    {
        throw new NotImplementedException();
    }

    public override TxOutType Type
    {
        get
        {
            return TxOutType.TX_NULL_DATA;
        }
    }
}