#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;

namespace CafeLib.BsvSharp.Builders
{
    public sealed class Pay2PubkeyScriptBuilder : LockingScriptBuilder
    {
        public Pay2PubkeyScriptBuilder(Address address)
            : base(address, TemplateId.Pay2PublicKey)
        {
            BuildScript();
        }

        public Pay2PubkeyScriptBuilder(PublicKey publicKey)
            : base(publicKey, TemplateId.Pay2PublicKey)
        {
            BuildScript();
        }

        private void BuildScript()
        {
            AddData(ScriptPubkeyHash.Span)
                .Add(Opcode.OP_CHECKSIG);
        }
    }
}