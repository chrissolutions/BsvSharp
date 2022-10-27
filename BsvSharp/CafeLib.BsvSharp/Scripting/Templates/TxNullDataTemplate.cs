using System;
using CafeLib.BsvSharp.Builders;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class TxNullDataTemplate : ScriptTemplate
{
    public const int MaxOpReturnSize = 40;

    protected override bool FastCheckScriptPubkey(Script scriptPubkey)
    {
        return scriptPubkey.Data.Length >= 1 && scriptPubkey.Data[0] == (byte)Opcode.OP_RETURN;
    }

    protected override bool CheckScriptPubkeyCore(Script scriptPubkey, Operand[] scriptPubkeyOps)
    {
        if (scriptPubkeyOps.Length < 1)
            return false;

        if (scriptPubkeyOps[0].Code != Opcode.OP_RETURN)
            return false;

        if (scriptPubkeyOps.Length == 2)
        {
            return scriptPubkeyOps[1].Data.Length <= MaxOpReturnSize;
        }

        return true;
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Operand[] scriptSigOps, Script scriptPubKey, Operand[] scriptPubKeyOps)
    {
        return false;
    }

    //public byte[] ExtractScriptPubkeyParameters(Script scriptPubkey)
    //{
    //    if (!FastCheckScriptPubkey(scriptPubkey))
    //        return null;
    //    var ops = scriptPubkey.ToOps().ToArray();
    //    if (ops.Length != 2)
    //        return null;
    //    if (ops[1].PushData == null || ops[1].PushData.Length > MaxOpReturnSize)
    //        return null;
    //    return ops[1].PushData;
    //}

    public Script GenerateScriptPubkey(ReadOnlyByteSpan data)
    {
        if (data.Length > MaxOpReturnSize)
            throw new ArgumentOutOfRangeException(nameof(data), "Data in OP_RETURN should have a maximum size of " + MaxOpReturnSize + " bytes");

        var builder = new DefaultScriptBuilder();
        builder.Add(Opcode.OP_RETURN)
            .AddData(data);

        return builder.ToScript();
    }

    public override TxOutType Type => TxOutType.TX_NULL_DATA;
}