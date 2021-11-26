using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public class P2PkhLockBuilder : LockingScriptBuilder
    {
        public P2PkhLockBuilder(Address address)
            : this(address.PubKeyHash)
        {
        }

        public P2PkhLockBuilder(PublicKey publicKey)
            : this(publicKey.ToPubKeyHash())
        {
        }

        private P2PkhLockBuilder(UInt160 pubKeyHash)
            :base(pubKeyHash, TemplateId.Pay2ScriptHash)
        {
            Add(Opcode.OP_DUP)
                .Add(Opcode.OP_HASH160)
                .Push(pubKeyHash.Span)
                .Add(Opcode.OP_EQUALVERIFY)
                .Add(Opcode.OP_CHECKSIG);
        }
    }
}