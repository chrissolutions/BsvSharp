using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public class LockingScriptBuilder : ScriptBuilder
    {
        public UInt160 PubKeyHash { get; }
        
        public LockingScriptBuilder(Address address, TemplateId templateId = TemplateId.Unknown)
            : this(address.PubKeyHash, templateId)
        {
        }

        public LockingScriptBuilder(PublicKey publicKey, TemplateId templateId = TemplateId.Unknown)
            : this(publicKey.ToPubKeyHash(), templateId)
        {
        }

        protected LockingScriptBuilder(UInt160 pubKeyHash, TemplateId templateId)
            :base(true, templateId)
        {
            PubKeyHash = pubKeyHash;
        }
    }
}