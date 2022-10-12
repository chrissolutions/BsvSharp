#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public sealed class Pay2PubkeyLockBuilder : LockingScriptBuilder
    {
        public Pay2PubkeyLockBuilder(Address address)
            : this(address.PublicKeyHash)
        {
        }

        public Pay2PubkeyLockBuilder(PublicKey publicKey)
            : this(publicKey.ToPublicKeyHash())
        {
        }

        private Pay2PubkeyLockBuilder(UInt160 pubkeyHash)
            : base(pubkeyHash, TemplateId.Pay2PublicKey)
        {
            AddData(pubkeyHash.Span)
                .Add(Opcode.OP_CHECKSIG);
        }
    }
}