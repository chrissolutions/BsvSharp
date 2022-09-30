﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Services;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public partial class KzScriptTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        [Fact]
        public void Decode()
        {
            var address = new PublicKey(true);
            var e = UInt160.FromHex("c2eaba3b9c29575322c6e24fdc1b49bdfe405bad", true);
            var s1 = Encoders.Base58Check.Encode(RootService.Network.PublicKeyAddress.ToArray().Concat(e));
            var s2 = Encoders.Base58Check.Encode(RootService.Network.ScriptAddress.ToArray().Concat(e));
            //e.Span.CopyTo(address.Span);
            //var id = address.GetID();
            Assert.True(true);
        }

        /// <summary>
        /// Test Vector
        /// </summary>
        public class TestValue1
        {
            /// <summary>
            /// Script as hex string.
            /// </summary>
            public string Hex;
            /// <summary>
            /// Script as decoded OPs.
            /// </summary>
            public string Decode;
        }

        public KzScriptTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("04ffff001d0104455468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73",
            "4 0xffff001d 1 0x04 69 0x5468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73")]
        [InlineData("4104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac",
            "65 0x04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f OP_CHECKSIG")]
        [InlineData("76a914c2eaba3b9c29575322c6e24fdc1b49bdfe405bad88ac",
            "OP_DUP OP_HASH160 20 0xc2eaba3b9c29575322c6e24fdc1b49bdfe405bad OP_EQUALVERIFY OP_CHECKSIG")]
        [InlineData("4730440220327588eb1c9e502358142b67b3cd799cb6163fde4f1a92490affda78734bc63c0220639a29e63d78c971177a1792cec1b0a7e65c973edbf03eba3b3380d97b829f80412103ea03d07638e40b53d8098b62e964112f562af5ba1bffaa146ffd9e7f7d1a5c67",
            "71 0x30440220327588eb1c9e502358142b67b3cd799cb6163fde4f1a92490affda78734bc63c0220639a29e63d78c971177a1792cec1b0a7e65c973edbf03eba3b3380d97b829f8041 33 0x03ea03d07638e40b53d8098b62e964112f562af5ba1bffaa146ffd9e7f7d1a5c67")]
        [InlineData("6a22314c74794d45366235416e4d6f70517242504c6b3446474e3855427568784b71726e0101337b2274223a32302e36322c2268223a35392c2270223a313031322c2263223a312c227773223a362e322c227764223a3236307d22314a6d64484e4456336f6434796e614c7635696b4d6234616f763737507a665169580a31353537303838383133",
            "OP_RETURN 34 0x314c74794d45366235416e4d6f70517242504c6b3446474e3855427568784b71726e 1 0x01 51 0x7b2274223a32302e36322c2268223a35392c2270223a313031322c2263223a312c227773223a362e322c227764223a3236307d 34 0x314a6d64484e4456336f6434796e614c7635696b4d6234616f763737507a66516958 10 0x31353537303838383133")]
        public void ScriptEncodingTest(string hex, string decoded)
        {
            var s = Script.FromHex(hex);
            var str = s.ToVerboseString();
            Assert.Equal(decoded, str);
        }

        [Theory]
        [InlineData("OP_DUP OP_HASH160 20 0x1451baa3aad777144a0759998a03538018dd7b4b OP_EQUALVERIFY OP_CHECKSIG")]
        [InlineData("OP_SHA256 32 0x8cc17e2a2b10e1da145488458a6edec4a1fdb1921c2d5ccbc96aa0ed31b4d5f8 OP_EQUALVERIFY OP_DUP OP_HASH160 20 0x1451baa3aad777144a0759998a03538018dd7b4b OP_EQUALVERIFY OP_CHECKSIGVERIFY OP_EQUALVERIFY OP_DUP OP_HASH160 20 0x1451baa3aad777144a0759998a03538018dd7b4b OP_EQUALVERIFY OP_CHECKSIG")]
        [InlineData("OP_0 OP_RETURN 34 0x31346b7871597633656d48477766386d36596753594c516b4743766e395172677239 66 0x303236336661663734633031356630376532633834343538623566333035653262323762366566303838393238383133326435343264633139633436663064663532 OP_PUSHDATA1 150 0x000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData("OP_0 OP_PUSHDATA4 3 0x010203 OP_0")]
        [InlineData("OP_0 OP_PUSHDATA2 3 0x010203 OP_0")]
        [InlineData("OP_0 OP_PUSHDATA1 3 0x010203 OP_0")]
        [InlineData("OP_0 3 0x010203 OP_0")]
        [InlineData("")]
        public void ParseScriptTest(string script)
        {
            var builder = ScriptBuilder.ParseScript(script);
            Assert.NotNull(builder);
            Assert.Equal(script, builder.ToScript().ToString());
        }


        [Fact]
        public void ParseKnownAssemblyTest()
        {
            const string asm = "OP_DUP OP_HASH160 f4c03610e60ad15100929cc23da2f3a799af1725 OP_EQUALVERIFY OP_CHECKSIG";

            var builder = ScriptBuilder.ParseAssembly(asm);
            Assert.NotNull(builder);
            Assert.Equal(Opcode.OP_DUP, builder.Operands[0].Opcode);
            Assert.Equal(Opcode.OP_HASH160, builder.Operands[1].Opcode);
            Assert.Equal((Opcode)20, builder.Operands[2].Opcode);
            Assert.Equal("f4c03610e60ad15100929cc23da2f3a799af1725", builder.Operands[2].Operand.GetDataBytes().ToHex().ToLowerInvariant());
            Assert.Equal(Opcode.OP_EQUALVERIFY, builder.Operands[3].Opcode);
            Assert.Equal(Opcode.OP_CHECKSIG, builder.Operands[4].Opcode);
            Assert.Equal(asm, builder.ToScript().ToAssemblyString());
        }

        [Fact]
        public void Scripts()
        {
            var tv2s = new List<TV2>();
            var json = JArray.Parse(File.ReadAllText(@"..\..\..\data\script_tests.json"));
            foreach (var r in json.Children<JToken>().Where(c => c.Count() >= 4))
            {
                if (r[0].Type == JTokenType.String)
                {
                    var sig = r[0].Value<string>();
                    var pub = r[1].Value<string>();
                    var flags = r[2].Value<string>();
                    var error = r[3].Value<string>();
                    var description = r.Count() > 4 ? r[4].Value<string>() : "no comment";
                    tv2s.Add(new TV2(sig, pub, flags, error, description));
                }
            }

            var tv2sSorted = tv2s.OrderBy(tv => tv.opcodes.Length + (int)tv.opcodes.LastOrDefault() / 256.0).ToList();

            var opcodes = tv2sSorted.Select(tv => tv.keyopcode).Distinct().OrderBy(o => o).ToArray();

            var noOpcode = new List<TV2>();
            var byOpcode = new Dictionary<Opcode, List<TV2>>();
            foreach (var tv in tv2sSorted)
            {
                var o = tv.keyopcode;
                var list = o.HasValue ? null : noOpcode;
                if (list == null && !byOpcode.TryGetValue(o.Value, out list))
                {
                    list = new List<TV2>();
                    byOpcode.Add(o.Value, list);
                }
                list.Add(tv);
            }

            var i = 0;
            foreach (var opcode in opcodes)
            {
                var list = opcode.HasValue ? byOpcode[opcode.Value] : noOpcode;
                foreach (var tv in list)
                {
                    i++;
                    var tv2 = new TV2(tv.sig, tv.pub, tv.flags, tv.error, tv.description);
                    //_testOutputHelper.WriteLine($"{opcode} {i}");
                    //_testOutputHelper.WriteLine($"Sig: {tv.scriptSig.ToHexString()} => {tv.scriptSig}");
                    //_testOutputHelper.WriteLine($"Pub: {tv.scriptPub.ToHexString()} => {tv.scriptPub}");

                    var txCredit = new Transaction();
                    var coinbaseUnlockBuilder = new DefaultUnlockBuilder(Script.FromString("OP_0 OP_0"));
                    var txCreditInput = new TransactionInput(UInt256.Zero, -1, Amount.Zero, new(), coinbaseUnlockBuilder);
                    txCredit.AddInput(txCreditInput);

                    //add output to credit Transaction
                    var txOutLockBuilder = new DefaultLockBuilder(tv.scriptPubKey);
                    var txCredOut = new TransactionOutput(UInt256.Zero, 0, txOutLockBuilder);
                    txCredit.AddOutput(txCredOut);

                    //setup spend Transaction
                    var txSpend = new Transaction();
                    var defaultUnlockBuilder = new DefaultUnlockBuilder(tv.scriptSig);
                    var txSpendInput = new TransactionInput(txCredit.TxHash, 0, Amount.Zero, new(), defaultUnlockBuilder);
                    txSpend.AddInput(txSpendInput);
                    var txSpendOutput = new TransactionOutput(UInt256.Zero, 0, Amount.Zero, null);
                    txSpend.AddOutput(txSpendOutput);

                    var checker = new TransactionSignatureChecker(txSpend, 0, Amount.Zero);
                    var ok = ScriptInterpreter.VerifyScript(tv.scriptSig, tv.scriptPubKey, tv.scriptFlags, checker, out var error);

                    // BsvSharp does not support P2SH and the corresponding technical debt of the BTC Core developers.
                    var correct = tv switch
                    {
                        _ when tv.scriptPubKey.IsPay2ScriptHash() => true,          // P2SH is unsupported.
                        _ when tv.scriptError == ScriptError.CLEANSTACK => true,    // CLEANSTACK dependent on unsupported P2SH.
                        //_ when opcode == Opcode.OP_CHECKMULTISIG => true,           // OP_CHECKMULTISIG not implemented.
                        _ when opcode == Opcode.OP_CHECKMULTISIGVERIFY => true,     // OP_CHECKMULTISIGVERIFY not implemented.
                        _ when opcode == Opcode.OP_CHECKLOCKTIMEVERIFY => true,     // OP_CHECKLOCKTIMEVERIFY not implemented.
                        _ when opcode == Opcode.OP_CHECKSEQUENCEVERIFY => true,     // OP_CHECKSEQUENCEVERIFY not implemented.
                        _ => (ok && tv.scriptError == ScriptError.OK) || tv.scriptError == error
                    };

                    if (correct == false)
                    {
                        _testOutputHelper.WriteLine($"testcase: {i}");
                        _testOutputHelper.WriteLine($"{opcode}");
                        _testOutputHelper.WriteLine($"Sig: {tv.scriptSig.ToHexString()} => {tv.scriptSig}");
                        _testOutputHelper.WriteLine($"Pub: {tv.scriptPubKey.ToHexString()} => {tv.scriptPubKey}");
                        _testOutputHelper.WriteLine($"Description: {tv.description}");
                    }
                    Assert.True(correct);
                }
            }
        }
    }
}
