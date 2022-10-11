namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToMultiSigTemplateParameters
{
    public int SignatureCount
    {
        get;
        set;
    }
    public PubKey[] PubKeys
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