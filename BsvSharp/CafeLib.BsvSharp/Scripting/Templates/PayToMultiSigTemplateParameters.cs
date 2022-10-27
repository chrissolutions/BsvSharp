using CafeLib.BsvSharp.Keys;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToMultiSigTemplateParameters
{
    public int SignatureCount
    {
        get;
        set;
    }
    public PublicKey[] PublicKeys
    {
        get;
        set;
    }

    public byte[][] InvalidPubKeys
    {
        get;
        set;
    }
}