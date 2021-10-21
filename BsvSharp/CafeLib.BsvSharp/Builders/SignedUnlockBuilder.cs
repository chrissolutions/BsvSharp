using System.Collections.Generic;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;

namespace CafeLib.BsvSharp.Builders
{
    public abstract class SignedUnlockBuilder : ScriptBuilder
    {
        public PublicKey PublicKey { get; protected set; }

        public IEnumerable<Signature> Signatures { get; } = new List<Signature>();

        public void AddSignature(Signature signature) => 
            (Signatures as ICollection<Signature>)?.Add(signature);

        protected SignedUnlockBuilder(PublicKey pubKey, TemplateId templateId = TemplateId.Unknown)
            : base(false, templateId)
        {
            PublicKey = pubKey;
        }

        public virtual void Sign(Script scriptSig)
        {
            Set(scriptSig);
        }
    }
}