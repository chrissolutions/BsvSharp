using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Signatures;

namespace CafeLib.BsvSharp.Scripting.Templates;

public class PayToPubkeyHashScriptSigParameters
{
    public Signature TransactionSignature
    {
        get;
        set;
    }
    public PublicKey PublicKey
    {
        get;
        set;
    }
}