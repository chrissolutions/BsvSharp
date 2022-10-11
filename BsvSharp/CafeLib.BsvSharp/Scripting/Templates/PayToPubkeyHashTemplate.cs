using System;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToPubkeyHashTemplate : ScriptTemplate
{
    private static readonly PayToPubkeyHashTemplate _Instance = new PayToPubkeyHashTemplate();
    public static PayToPubkeyHashTemplate Instance
    {
        get
        {
            return _Instance;
        }
    }
    public Script GenerateScriptPubKey(BitcoinAddress address)
    {
        if (address == null)
            throw new ArgumentNullException("address");
        return GenerateScriptPubKey((KeyId)address.Hash);
    }
    public Script GenerateScriptPubKey(PubKey pubKey)
    {
        if (pubKey == null)
            throw new ArgumentNullException("pubKey");
        return GenerateScriptPubKey(pubKey.Hash);
    }
    public Script GenerateScriptPubKey(KeyId pubkeyHash)
    {
        return new Script(
            OpcodeType.OP_DUP,
            OpcodeType.OP_HASH160,
            Op.GetPushOp(pubkeyHash.ToBytes()),
            OpcodeType.OP_EQUALVERIFY,
            OpcodeType.OP_CHECKSIG
        );
    }

    public Script GenerateScriptSig(TransactionSignature signature, PubKey publicKey)
    {
        if (publicKey == null)
            throw new ArgumentNullException("publicKey");
        return new Script(
            signature == null ? OpcodeType.OP_0 : Op.GetPushOp(signature.ToBytes()),
            Op.GetPushOp(publicKey.ToBytes())
        );
    }

    protected override bool FastCheckScriptPubKey(Script scriptPubKey)
    {
        var bytes = scriptPubKey.ToBytes(true);
        return bytes.Length >= 3 &&
               bytes[0] == (byte)OpcodeType.OP_DUP &&
               bytes[1] == (byte)OpcodeType.OP_HASH160 &&
               bytes[2] == 0x14;
    }

    protected override bool CheckScriptPubKeyCore(Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptPubKeyOps;
        if (ops.Length != 5)
            return false;
        return ops[0].Code == OpcodeType.OP_DUP &&
               ops[1].Code == OpcodeType.OP_HASH160 &&
               ops[2].PushData != null && ops[2].PushData.Length == 0x14 &&
               ops[3].Code == OpcodeType.OP_EQUALVERIFY &&
               ops[4].Code == OpcodeType.OP_CHECKSIG;
    }
    public KeyId ExtractScriptPubKeyParameters(Script scriptPubKey)
    {
        var ops = scriptPubKey.ToOps().ToArray();
        if (!CheckScriptPubKeyCore(scriptPubKey, ops))
            return null;
        return new KeyId(ops[2].PushData);
    }

    protected override bool CheckScriptSigCore(Script scriptSig, Op[] scriptSigOps, Script scriptPubKey, Op[] scriptPubKeyOps)
    {
        var ops = scriptSigOps;
        if (ops.Length != 2)
            return false;
        return ops[0].PushData != null &&
               ops[1].PushData != null && PubKey.Check(ops[1].PushData, false);
    }

    public bool CheckScriptSig(Script scriptSig)
    {
        return CheckScriptSig(scriptSig, null);
    }



    public PayToPubkeyHashScriptSigParameters ExtractScriptSigParameters(Script scriptSig)
    {
        var ops = scriptSig.ToOps().ToArray();
        if (!CheckScriptSigCore(scriptSig, ops, null, null))
            return null;
        try
        {
            return new PayToPubkeyHashScriptSigParameters()
            {
                TransactionSignature = ops[0].Code == OpcodeType.OP_0 ? null : new TransactionSignature(ops[0].PushData),
                PublicKey = new PubKey(ops[1].PushData, true),
            };
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
            return TxOutType.TX_PUBKEYHASH;
        }
    }

    public Script GenerateScriptSig(PayToPubkeyHashScriptSigParameters parameters)
    {
        return GenerateScriptSig(parameters.TransactionSignature, parameters.PublicKey);
    }
}