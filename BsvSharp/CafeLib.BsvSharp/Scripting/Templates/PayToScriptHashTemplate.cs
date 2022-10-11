using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToScriptHashTemplate : ScriptTemplate
{
    private static readonly PayToScriptHashTemplate _Instance = new PayToScriptHashTemplate();
    public static PayToScriptHashTemplate Instance
    {
        get
        {
            return _Instance;
        }
    }
    public Script GenerateScriptPubKey(ScriptId scriptId)
    {
        return new Script(
            OpcodeType.OP_HASH160,
            Op.GetPushOp(scriptId.ToBytes()),
            OpcodeType.OP_EQUAL);
    }
    public Script GenerateScriptPubKey(Script scriptPubKey)
    {
        return GenerateScriptPubKey(scriptPubKey.Hash);
    }

    protected override bool FastCheckScriptPubKey(Script scriptPubKey)
    {
        var bytes = scriptPubKey.ToBytes(true);
        return
            bytes.Length >= 2 &&
            bytes[0] == (byte)OpcodeType.OP_HASH160 &&
            bytes[1] == 0x14;
    }

    protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptPubKeyOps;
        if (ops.Length != 3)
            return false;
        return ops[0].Code == OpcodeType.OP_HASH160 &&
               ops[1].Code == (OpcodeType)0x14 &&
               ops[2].Code == OpcodeType.OP_EQUAL;
    }

    public Script GenerateScriptSig(Op[] ops, Script script)
    {
        var pushScript = Op.GetPushOp(script._Script);
        return new Script(ops.Concat(new[] { pushScript }));
    }
    public PayToScriptHashSigParameters ExtractScriptSigParameters(Script scriptSig)
    {
        return ExtractScriptSigParameters(scriptSig, null);
    }
    public PayToScriptHashSigParameters ExtractScriptSigParameters(Script scriptSig, Script scriptPubKey)
    {
        var ops = scriptSig.ToOps().ToArray();
        var ops2 = scriptPubKey == null ? null : scriptPubKey.ToOps().ToArray();
        if (!CheckScriptSigCore(scriptSig, ops, scriptPubKey, ops2))
            return null;
        try
        {
            var multiSig = ops.Length > 0 && ops[0].Code == OpcodeType.OP_0;
            PayToScriptHashSigParameters result = new PayToScriptHashSigParameters();
            result.Signatures =
                ops
                    .Skip(multiSig ? 1 : 0)
                    .Take(ops.Length - 1 - (multiSig ? 1 : 0))
                    .Select(o => o.Code == OpcodeType.OP_0 ? null : new TransactionSignature(o.PushData))
                    .ToArray();
            result.RedeemScript = Script.FromBytesUnsafe(ops[ops.Length - 1].PushData);
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Script GenerateScriptSig(TransactionSignature[] signatures, Script redeemScript)
    {
        List<Op> ops = new List<Op>();
        PayToMultiSigTemplate multiSigTemplate = new PayToMultiSigTemplate();
        bool multiSig = multiSigTemplate.CheckScriptPubKey(redeemScript);
        if (multiSig)
            ops.Add(OpcodeType.OP_0);
        foreach (var sig in signatures)
        {
            ops.Add(sig == null ? OpcodeType.OP_0 : Op.GetPushOp(sig.ToBytes()));
        }
        return GenerateScriptSig(ops.ToArray(), redeemScript);
    }

    public Script GenerateScriptSig(ECDSASignature[] signatures, Script redeemScript)
    {
        return GenerateScriptSig(signatures.Select(s => new TransactionSignature(s, SigHash.All)).ToArray(), redeemScript);
    }
    protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptSigOps;
        if (ops.Length == 0)
            return false;
        if (!scriptSig.IsPushOnly)
            return false;
        if (scriptPubKey != null)
        {
            var expectedHash = ExtractScriptPubKeyParameters(scriptPubKey);
            if (expectedHash == null)
                return false;
            if (expectedHash != Script.FromBytesUnsafe(ops[ops.Length - 1].PushData).Hash)
                return false;
        }

        var redeemBytes = ops[ops.Length - 1].PushData;
        if (redeemBytes.Length > 520)
            return false;
        return Script.FromBytesUnsafe(ops[ops.Length - 1].PushData).IsValid;
    }



    public override TxOutType Type
    {
        get
        {
            return TxOutType.TX_SCRIPTHASH;
        }
    }

    public ScriptId ExtractScriptPubKeyParameters(Script scriptPubKey)
    {
        if (!FastCheckScriptPubKey(scriptPubKey))
            return null;
        var ops = scriptPubKey.ToOps().ToArray();
        if (!CheckScriptPubKeyCore(scriptPubKey, ops))
            return null;
        return new ScriptId(ops[1].PushData);
    }

    public Script GenerateScriptSig(PayToScriptHashSigParameters parameters)
    {
        return GenerateScriptSig(parameters.Signatures, parameters.RedeemScript);
    }
}