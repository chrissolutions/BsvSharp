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
            var sig = Script.FromString(scriptSig);
            var pub = Script.FromString(scriptPub);
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
        public void VerifyScript_From_Simple_Transaction()
        {
            var privateKey = PrivateKey.FromWif("L24Rq5hPWMexw5mQi7tchYw6mhtr5ApiHZMN8KJXCkskEv7bTV61");
            var publicKey = privateKey.CreatePublicKey();
            var fromAddress = publicKey.ToAddress();
            var toAddress = new Address("1BpbpfLdY7oBS9gK7aDXgvMgr1DPvNhEB2");

            var utxo = new Utxo
            {
                TxId = UInt256.FromHex("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458"),
                Index = 0,
                ScriptPubKey = new P2PkhLockBuilder(fromAddress).ToScript(),
                Amount = 100000
            };

            var tx = new Transaction()
                .SpendFromUtxo(utxo, new P2PkhUnlockBuilder(publicKey))
                .SpendTo(toAddress, 100000L, new P2PkhLockBuilder(toAddress))
                .Sign(0, privateKey, SignatureHashEnum.All);

            // we then extract the signature from the first input
            var scriptSig = tx.Inputs[0].ScriptSig;
            
            const ScriptFlags flags = ScriptFlags.VERIFY_P2SH | ScriptFlags.VERIFY_STRICTENC;
            var checker = new TransactionSignatureChecker(tx, 0, utxo.Amount);
            var verified = ScriptInterpreter.VerifyScript(scriptSig, utxo.ScriptPubKey, flags, checker, out var _);
            Assert.True(verified);
        }

        [Theory]
        [InlineData(
            "71 0x304402200a5c6163f07b8d3b013c4d1d6dba25e780b39658d79ba37af7057a3b7f15ffa102201fd9b4eaa9943f734928b99a83592c2e7bf342ea2680f6a2bb705167966b742001",
            "65 0x0479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8 OP_CHECKSIG"
        )]
        public void VerifyScript_Using_TransactionSignatureChecker(string scriptSigText, string scriptPubKeyText)
        {
            var scriptSig = Script.FromString(scriptSigText);
            var scriptPubKey = Script.FromString(scriptPubKeyText);

            var txCredit = new Transaction();
            var coinbaseUnlockBuilder = new DefaultUnlockBuilder(Script.FromString("OP_0 OP_0"));
            var txCreditInput = new TxIn(UInt256.Zero, -1, Amount.Zero, new(), coinbaseUnlockBuilder);
            txCredit.AddInput(txCreditInput);

            //add output to credit Transaction
            var txOutLockBuilder = new DefaultLockBuilder(scriptPubKey);
            var txCredOut = new TxOut(UInt256.Zero, 0, txOutLockBuilder);
            txCredit.AddOutput(txCredOut);

            //setup spend Transaction
            var txSpend = new Transaction();
            var defaultUnlockBuilder = new DefaultUnlockBuilder(scriptSig);
            var txSpendInput = new TxIn(txCredit.TxHash, 0, Amount.Zero, new(), defaultUnlockBuilder);
            txSpend.AddInput(txSpendInput);
            var txSpendOutput = new TxOut(UInt256.Zero, 0, Amount.Zero, null);
            txSpend.AddOutput(txSpendOutput);

            var checker = new TransactionSignatureChecker(txSpend, 0, Amount.Zero);
            var verified = ScriptInterpreter.VerifyScript(scriptSig, scriptPubKey, ScriptFlags.VERIFY_NONE, checker, out var error);
            Assert.True(verified);
        }
    }
}
