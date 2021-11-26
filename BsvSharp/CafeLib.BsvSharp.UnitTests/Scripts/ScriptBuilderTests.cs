#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Signatures;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public class ScriptBuilderTests
    {
        [Fact]
        public void P2PkhUnlockBuilder_Test()
        {
            var publicKey = new PublicKey("04e365859b3c78a8b7c202412b949ebca58e147dba297be29eee53cd3e1d300a6419bc780cc9aec0dc94ed194e91c8f6433f1b781ee00eac0ead2aae1e8e0712c6");
            var signature = Signature.FromHex("3046022100bb3c194a30e460d81d34be0a230179c043a656f67e3c5c8bf47eceae7c4042ee0221008bf54ca11b2985285be0fd7a212873d243e6e73f5fad57e8eb14c4f39728b8c601");
            var script = ScriptBuilder.ParseScript("73 0x3046022100bb3c194a30e460d81d34be0a230179c043a656f67e3c5c8bf47eceae7c4042ee0221008bf54ca11b2985285be0fd7a212873d243e6e73f5fad57e8eb14c4f39728b8c601 65 0x04e365859b3c78a8b7c202412b949ebca58e147dba297be29eee53cd3e1d300a6419bc780cc9aec0dc94ed194e91c8f6433f1b781ee00eac0ead2aae1e8e0712c6");

            var builder = new P2PkhUnlockBuilder(script);
            Assert.Equal(publicKey, builder.PublicKey);
            Assert.Equal(signature, builder.Signatures.First());

            builder = new P2PkhUnlockBuilder(publicKey);
            builder.AddSignature(signature);
            var scriptResult = builder.ToScript();

            Assert.Equal(script.ToString(), scriptResult.ToString());
        }

        [Fact]
        public void P2PkhLockBuilder_Test()
        {
            var address = new Address("1NaTVwXDDUJaXDQajoa9MqHhz4uTxtgK14");
            var lockBuilder = new P2PkhLockBuilder(address);
            var script = lockBuilder.ToScript();

            Assert.Equal("OP_DUP OP_HASH160 20 0xecae7d092947b7ee4998e254aa48900d26d2ce1d OP_EQUALVERIFY OP_CHECKSIG", script.ToString());
            Assert.Equal("1NaTVwXDDUJaXDQajoa9MqHhz4uTxtgK14", address.ToString());
        }
    }
}