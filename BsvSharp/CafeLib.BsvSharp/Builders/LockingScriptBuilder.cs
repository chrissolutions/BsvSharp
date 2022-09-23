#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public class LockingScriptBuilder : ScriptBuilder
    {
        public UInt160 ScriptPubkeyHash { get; }

        public LockingScriptBuilder(Address address, TemplateId templateId = TemplateId.Unknown)
            : this(address.PublicKeyHash, templateId)
        {
        }

        public LockingScriptBuilder(PublicKey publicKey, TemplateId templateId = TemplateId.Unknown)
            : this(publicKey.ToPublicKeyHash(), templateId)
        {
        }

        protected LockingScriptBuilder(UInt160 pubkeyHash, TemplateId templateId)
            : base(templateId)
        {
            ScriptPubkeyHash = pubkeyHash;
        }

        protected LockingScriptBuilder(Script script)
            : base(script)
        {
        }
    }
}