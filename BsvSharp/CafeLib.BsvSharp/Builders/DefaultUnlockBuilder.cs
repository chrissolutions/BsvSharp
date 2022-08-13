using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;

namespace CafeLib.BsvSharp.Builders
{
    public class DefaultUnlockBuilder : SignedUnlockBuilder
    {
        internal DefaultUnlockBuilder()
            : this(null)
        {
        }

        public DefaultUnlockBuilder(PublicKey pubKey, TemplateId templateId = TemplateId.Unknown)
            : base(pubKey, templateId)
        {
        }
    }
}