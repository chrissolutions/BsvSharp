namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToScriptHashSigParameters
{
    public Script RedeemScript
    {
        get;
        set;
    }
    public TransactionSignature[] Signatures
    {
        get;
        set;
    }
}