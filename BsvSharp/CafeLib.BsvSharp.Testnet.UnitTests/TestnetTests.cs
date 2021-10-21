using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Services;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Transactions;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.Testnet.UnitTests
{
    public class TestnetTests
    {
        static TestnetTests()
        {
            RootService.Bootstrap(NetworkType.Test);
        }

        [Fact]
        public void Create_Testnet_Address_From_Wif_PrivateKey()
        {
            var privateKey = PrivateKey.FromWif("92VYMmwFLXRwXn5688edGxYYgMFsc3fUXYhGp17WocQhU6zG1kd");
            var publicKey = privateKey.CreatePublicKey();
            var address = publicKey.ToAddress();
            Assert.Equal(NetworkType.Test, address.NetworkType);
            Assert.Equal("moiAvLUw16qgrwhFGo1eDnXHC2wPMYiv7Y", address.ToString());
        }

        [Fact]
        public void Get_Testnet_Address_From_PublicKey()
        {
            var publicKey = new PublicKey("0293126ccc927c111b88a0fe09baa0eca719e2a3e087e8a5d1059163f5c566feef");
            var address = publicKey.ToAddress();
            Assert.Equal("mtX8nPZZdJ8d3QNLRJ1oJTiEi26Sj6LQXS", address.ToString());
        }

        [Fact]
        public void Convert_Testnet_PrivateKey_To_PublicKey()
        {
            var privhex = "906977a061af29276e40bf377042ffbde414e496ae2260bbf1fa9d085637bfff";
            var pubhex = "02a1633cafcc01ebfb6d78e39f687a1f0995c62fc95f51ead10a02ee0be551b5dc";

            var privateKey = new PrivateKey(privhex);
            var publicKey = privateKey.CreatePublicKey();
            
            Assert.Equal(pubhex, publicKey.ToHex());
        }

        [Fact]
        public void Create_Script_From_Testnet_Address()
        {
            var address = new Address("mxRN6AQJaDi5R6KmvMaEmZGe3n5ScV9u33");
            var lockBuilder = new P2PkhLockBuilder(address);
            var script = lockBuilder.ToScript();
            Assert.NotEqual(Script.None, script);
            Assert.Equal("OP_DUP OP_HASH160 20 0xb96b816f378babb1fe585b7be7a2cd16eb99b3e4 OP_EQUALVERIFY OP_CHECKSIG", script.ToString());
            Assert.Equal("mxRN6AQJaDi5R6KmvMaEmZGe3n5ScV9u33", address.ToString());
            Assert.Equal(NetworkType.Test, address.NetworkType);
        }
        
        [Fact]
        public void Create_Script_From_PublicKey()
        {
            var publicKey = new PublicKey("022df8750480ad5b26950b25c7ba79d3e37d75f640f8e5d9bcd5b150a0f85014da");
            var lockBuilder = new P2PkhLockBuilder(publicKey);
            var script = lockBuilder.ToScript();
            Assert.NotEqual(Script.None, script);
            Assert.Equal("OP_DUP OP_HASH160 20 0x9674af7395592ec5d91573aa8d6557de55f60147 OP_EQUALVERIFY OP_CHECKSIG", script.ToString());
            Assert.Equal(NetworkType.Test, publicKey.ToAddress().NetworkType);
        }

        [Fact]
        public void Verify_Script_From_Simple_Transaction()
        {
            var privateKey = PrivateKey.FromWif("cSBnVM4xvxarwGQuAfQFwqDg9k5tErHUHzgWsEfD4zdwUasvqRVY");
            var publicKey = privateKey.CreatePublicKey();
            var fromAddress = publicKey.ToAddress();
            var toAddress = new Address("mrU9pEmAx26HcbKVrABvgL7AwA5fjNFoDc");

            var utxo = new Utxo
            {
                TxHash = new UInt256("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458"),
                Index = 0,
                ScriptPubKey = new P2PkhLockBuilder(fromAddress).ToScript(),
                Amount = 100000
            };

            var tx = new Transaction();
            tx.SpendFromUtxo(utxo, new P2PkhUnlockBuilder(publicKey));
            tx.SpendTo(toAddress, 100000L, new P2PkhLockBuilder(toAddress));
            tx.SignInput(0, privateKey, SignatureHashEnum.All);

            // we then extract the signature from the first input
            var scriptSig = tx.Inputs[0].ScriptSig;
            
            const ScriptFlags flags = ScriptFlags.VERIFY_P2SH | ScriptFlags.VERIFY_STRICTENC;
            var checker = new TransactionSignatureChecker(tx, 0, utxo.Amount);
            var verified = ScriptInterpreter.VerifyScript(scriptSig, utxo.ScriptPubKey, flags, checker, out var _);
            Assert.True(verified);
        }
    }
}
