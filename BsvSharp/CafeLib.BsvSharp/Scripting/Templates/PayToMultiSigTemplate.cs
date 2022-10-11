using System;
using System.Collections.Generic;
using System.Linq;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToMultiSigTemplate : ScriptTemplate
{
    private static readonly PayToMultiSigTemplate _Instance = new PayToMultiSigTemplate();
    public static PayToMultiSigTemplate Instance
    {
        get
        {
            return _Instance;
        }
    }
    public Script GenerateScriptPubKey(int sigCount, params PubKey[] keys)
    {
        List<Op> ops = new List<Op>();
        var push = Op.GetPushOp(sigCount);
        if (!push.IsSmallUInt)
            throw new ArgumentOutOfRangeException("sigCount should be less or equal to 16");
        ops.Add(push);
        var keyCount = Op.GetPushOp(keys.Length);
        if (!keyCount.IsSmallUInt)
            throw new ArgumentOutOfRangeException("key count should be less or equal to 16");
        foreach (var key in keys)
        {
            ops.Add(Op.GetPushOp(key.ToBytes()));
        }
        ops.Add(keyCount);
        ops.Add(OpcodeType.OP_CHECKMULTISIG);
        return new Script(ops);
    }
    protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptPubKeyOps;
        if (ops.Length < 3)
            return false;

        var sigCount = ops[0];
        if (!sigCount.IsSmallUInt)
            return false;
        var pubKeyCount = ops[ops.Length - 2];
        if (!pubKeyCount.IsSmallUInt)
            return false;
        var keyCount = (uint)pubKeyCount.GetValue();
        if (1 + keyCount + 1 + 1 != ops.Length)
            return false;
        for (int i = 1; i < keyCount + 1; i++)
        {
            if (ops[i].PushData == null)
                return false;
        }
        return ops[ops.Length - 1].Code == OpcodeType.OP_CHECKMULTISIG;
    }

    public PayToMultiSigTemplateParameters ExtractScriptPubKeyParameters(Script scriptPubKey)
    {
        if (!FastCheckScriptPubKey(scriptPubKey))
            return null;
        var ops = scriptPubKey.ToOps().ToArray();
        if (!CheckScriptPubKeyCore(scriptPubKey, ops))
            return null;

        var sigCount = (int)ops[0].GetValue();
        var keyCount = (int)ops[ops.Length - 2].GetValue();

        List<PubKey> keys = new List<PubKey>();
        List<byte[]> invalidKeys = new List<byte[]>();
        for (int i = 1; i < keyCount + 1; i++)
        {
            if (!PubKey.Check(ops[i].PushData, false))
                invalidKeys.Add(ops[i].PushData);
            else
            {
                try
                {
                    keys.Add(new PubKey(ops[i].PushData));
                }
                catch (FormatException)
                {
                    invalidKeys.Add(ops[i].PushData);
                }
            }
        }

        return new PayToMultiSigTemplateParameters()
        {
            SignatureCount = sigCount,
            PubKeys = keys.ToArray(),
            InvalidPubKeys = invalidKeys.ToArray()
        };
    }

    protected override bool FastCheckScriptSig(Script scriptSig, Script scriptPubKey)
    {
        var bytes = scriptSig.ToBytes(true);
        return bytes.Length >= 1 &&
               bytes[0] == (byte)OpcodeType.OP_0;
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        if (!scriptSig.IsPushOnly)
            return false;
        if (scriptSigOps[0].Code != OpcodeType.OP_0)
            return false;
        if (scriptSigOps.Length == 1)
            return false;
        if (!scriptSigOps.Skip(1).All(s => TransactionSignature.ValidLength(s.PushData.Length) || s.Code == OpcodeType.OP_0))
            return false;
        if (scriptPubKeyOps != null)
        {
            if (!CheckScriptPubKeyCore(scriptPubKey, scriptPubKeyOps))
                return false;
            var sigCountExpected = scriptPubKeyOps[0].GetValue();
            return sigCountExpected == scriptSigOps.Length + 1;
        }
        return true;

    }

    public TransactionSignature[] ExtractScriptSigParameters(Script scriptSig)
    {
        if (!FastCheckScriptSig(scriptSig, null))
            return null;
        var ops = scriptSig.ToOps().ToArray();
        if (!CheckScriptSigCore(scriptSig, ops, null, null))
            return null;
        try
        {
            return ops.Skip(1).Select(i => i.Code == OpcodeType.OP_0 ? null : new TransactionSignature(i.PushData)).ToArray();
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public override TxOutType Type
    {
        get
        {
            return TxOutType.TX_MULTISIG;
        }
    }

    public Script GenerateScriptSig(TransactionSignature[] signatures)
    {
        return GenerateScriptSig((IEnumerable<TransactionSignature>)signatures);
    }

    public Script GenerateScriptSig(IEnumerable<TransactionSignature> signatures)
    {
        List<Op> ops = new List<Op>();
        ops.Add(OpcodeType.OP_0);
        foreach (var sig in signatures)
        {
            if (sig == null)
                ops.Add(OpcodeType.OP_0);
            else
                ops.Add(Op.GetPushOp(sig.ToBytes()));
        }
        return new Script(ops);
    }
}