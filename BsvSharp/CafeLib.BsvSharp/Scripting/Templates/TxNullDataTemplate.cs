using System;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class TxNullDataTemplate : ScriptTemplate
{
    private static readonly TxNullDataTemplate _Instance = new TxNullDataTemplate();
    public static TxNullDataTemplate Instance
    {
        get
        {
            return _Instance;
        }
    }
    protected override bool FastCheckScriptPubKey(Script scriptPubKey)
    {
        var bytes = scriptPubKey.ToBytes(true);
        return bytes.Length >= 1 && bytes[0] == (byte)OpcodeType.OP_RETURN;
    }
    protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptPubKeyOps;
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
    public byte[] ExtractScriptPubKeyParameters(Script scriptPubKey)
    {
        if (!FastCheckScriptPubKey(scriptPubKey))
            return null;
        var ops = scriptPubKey.ToOps().ToArray();
        if (ops.Length != 2)
            return null;
        if (ops[1].PushData == null || ops[1].PushData.Length > MAX_OPRETURN_SIZE)
            return null;
        return ops[1].PushData;
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        return false;
    }

    public const int MAX_OPRETURN_SIZE = 40;
    public Script GenerateScriptPubKey(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException("data");
        if (data.Length > MAX_OPRETURN_SIZE)
            throw new ArgumentOutOfRangeException("data", "Data in OP_RETURN should have a maximum size of " + MAX_OPRETURN_SIZE + " bytes");

        return new Script(OpcodeType.OP_RETURN,
            Op.GetPushOp(data));
    }

    public override TxOutType Type
    {
        get
        {
            return TxOutType.TX_NULL_DATA;
        }
    }
}