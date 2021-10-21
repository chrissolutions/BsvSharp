using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public class ScriptInterpreterTests
    {
        private static readonly TransactionSignatureChecker DefaultChecker = new TransactionSignatureChecker(new Transaction(), 0, Amount.Zero);

        [Theory]
        [InlineData("OP_1", "OP_1", true)]
        [InlineData("OP_1", "OP_0", false)]
        [InlineData("OP_0", "OP_1", true)]
        [InlineData("OP_CODESEPARATOR", "OP_1", true)]
        [InlineData("", "OP_DEPTH OP_0 OP_EQUAL", true)]
        [InlineData("OP_1 OP_2", "OP_2 OP_EQUALVERIFY OP_1 OP_EQUAL", true)]
        [InlineData("9 0x000000000000000010", "", true)]
        [InlineData("OP_1", "OP_15 OP_ADD OP_16 OP_EQUAL", true)]
        [InlineData("OP_0", "OP_IF OP_VER OP_ELSE OP_1 OP_ENDIF", true)]
        public void VerifyTrivialScriptTest(string scriptSig, string scriptPub, bool result)
        {
            var sig = ScriptBuilder.ParseScript(scriptSig);
            var pub = ScriptBuilder.ParseScript(scriptPub);
            var ok = ScriptInterpreter.VerifyScript(sig, pub, ScriptFlags.VERIFY_NONE, DefaultChecker, out _);
            Assert.Equal(result, ok);
        }

        [Theory]
        [InlineData("0x00", false)]
        [InlineData("0x01", true)]
        [InlineData("0x0080", false)]
        [InlineData("", false)]
        public void VerifyVarType_ToBool(string value, bool result)
        {
            Assert.Equal(new VarType(Encoders.Hex.Decode(value)), result);
        }

        [Fact]
        public void Verify_Script_From_Simple_Transaction()
        {
            var privateKey = PrivateKey.FromBase58("L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61");
            var publicKey = privateKey.CreatePublicKey();
            var fromAddress = publicKey.ToAddress();
            var toAddress = new Address("1BpbpfLdY7oBS9gK7aDXgvMgr1DPvNhEB2");

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
