#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;

namespace CafeLib.BsvSharp.Builders
{
    public sealed class DefaultUnlockBuilder : SignedUnlockBuilder
    {
        internal DefaultUnlockBuilder()
            : base(Script.None)
        {
        }

        public DefaultUnlockBuilder(PublicKey pubKey, TemplateId templateId = TemplateId.Unknown)
            : base(pubKey, templateId)
        {
        }

        public DefaultUnlockBuilder(Script script)
            : base(script)
        {
        }
    }
}