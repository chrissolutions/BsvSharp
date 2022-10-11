namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToPubkeyHashScriptSigParameters
{
    public TransactionSignature TransactionSignature
    {
        get;
        set;
    }
    public PubKey PublicKey
    {
        get;
        set;
    }
}