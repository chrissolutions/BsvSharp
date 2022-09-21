#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public sealed class P2PkhLockBuilder : LockingScriptBuilder
    {
        public P2PkhLockBuilder(Address address)
            : this(address.PublicKeyHash)
        {
        }

        public P2PkhLockBuilder(PublicKey publicKey)
            : this(publicKey.ToPublicKeyHash())
        {
        }

        private P2PkhLockBuilder(UInt160 pubKeyHash)
            : base(pubKeyHash, TemplateId.Pay2ScriptHash)
        {
            Add(Opcode.OP_DUP)
                .Add(Opcode.OP_HASH160)
                .AddData(pubKeyHash.Span)
                .Add(Opcode.OP_EQUALVERIFY)
                .Add(Opcode.OP_CHECKSIG);
        }
    }
}