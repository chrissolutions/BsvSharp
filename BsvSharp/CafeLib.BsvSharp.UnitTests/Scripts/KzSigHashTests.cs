#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public class KzSigHashTests
    {
        private const string DataFolder = @"..\..\..\data";

        [Theory]
        [InlineData("sighash.json")]
        [InlineData("sighash.dart.json")]
        [InlineData("sighash-sv.json")]
        public void SigHash_ForkId_Tests(string testcaseFile)
        {
            var testcases = FindForkIdTests(testcaseFile);
            RunAllTests(testcases);
        }

        [Theory]
        [InlineData("sighash.json")]
        [InlineData("sighash.dart.json")]
        [InlineData("sighash-sv.json")]
        public void SigHash_NonForkId_Tests(string testcaseFile)
        {
            var testcases = FindNonForkIdTests(testcaseFile);
            RunAllTests(testcases);
        }

        [Fact]
        public void Compute_Sighash_For_A_Coinbase_Tx()
        {
            var tx = new Transaction("02000000010000000000000000000000000000000000000000000000000000000000000000ffffffff2e039b1e1304c0737c5b68747470733a2f2f6769746875622e636f6d2f62636578742f01000001c096020000000000ffffffff014a355009000000001976a91448b20e254c0677e760bab964aec16818d6b7134a88ac00000000");
            var sighash = TransactionSignatureChecker.ComputeSignatureHash(Script.None, tx, 0, new SignatureHashType(SignatureHashEnum.All), Amount.Zero).ToString();
            Assert.Equal("6829f7d44dfd4654749b8027f44c9381527199f78ae9b0d58ffc03fdab3c82f1", sighash);
        }

        #region Helpers

        /// <summary>
        /// Test Vector
        /// </summary>
        private class TestCase
        {
            public readonly string RawTx;
            public readonly string RawScript;
            public readonly int Index;
            public readonly SignatureHashType SigHashType;
            public readonly string SigHashRegHex;
            public readonly string SigHashOldHex;
            public readonly string SigHashRepHex;

            public TestCase(string rawTx, string script, int inputIndex, int hashType, string sigHashReg, string sigHashNoFork, string sigHashFork)
            {
                RawTx = rawTx;
                RawScript = script;
                Index = inputIndex;
                SigHashType = new SignatureHashType((uint)hashType);
                SigHashRegHex = sigHashReg;
                SigHashOldHex = sigHashNoFork;
                SigHashRepHex = sigHashFork;
            }
        }

        private static void RunAllTests(IEnumerable<TestCase> testCase)
        {
            testCase.ForEach(test =>
            {
                var tx = new Transaction(test.RawTx);

                var script = new Script(test.RawScript);
                Assert.Equal(test.RawScript, script.ToHexString());

                var (scriptCodeOk, scriptCode) = Script.ParseHex(test.RawScript, withoutLength: true);
                Assert.True(scriptCodeOk);
                Assert.Equal(script, scriptCode);

                var writer = new ByteDataWriter();
                tx.WriteTo(writer);
                var serializedHex = Encoders.Hex.Encode(writer.Span);
                Assert.Equal(test.RawTx, serializedHex);

                var sighashReg = TransactionSignatureChecker.ComputeSignatureHash(scriptCode, tx, test.Index, test.SigHashType, Amount.Zero).ToString();
                Assert.Equal(test.SigHashRegHex, sighashReg);

                if (string.IsNullOrWhiteSpace(test.SigHashOldHex)) return;
                var sigHashOld = TransactionSignatureChecker.ComputeSignatureHash(scriptCode, tx, test.Index, test.SigHashType, Amount.Zero, 0).ToString();
                Assert.Equal(test.SigHashOldHex, sigHashOld);
            });
        }

        private static IEnumerable<TestCase> FindForkIdTests(string testCaseFile)
        {
            var tvs = ReadTestCases(testCaseFile);
            return tvs.Where(x => x.SigHashType.HasForkId);
        }
        
        private static IEnumerable<TestCase> FindNonForkIdTests(string testCaseFile)
        {
            var tvs = ReadTestCases(testCaseFile);
            return tvs.Where(x => !x.SigHashType.HasForkId);
        }
        
        private static IEnumerable<TestCase> ReadTestCases(string testCaseFile)
        {
            var tvs = new List<TestCase>();
            var json = JArray.Parse(File.ReadAllText(Path.Combine(DataFolder, testCaseFile)));
            json.Children<JToken>().Where(c => c.Count() >= 5)
                .ForEach(x =>
                {
                    var rawTx = x[0]?.Value<string>();
                    var script = x[1]?.Value<string>();
                    var inputIndex = x[2]?.Value<int>() ?? -1;
                    var hashType = x[3]?.Value<int>() ?? default;
                    var sigHashReg = x[4]?.Value<string>();
                    var sigHashNoFork = x.ElementAtOrDefault(5)?.Value<string>();
                    var sigHashFork = x.ElementAtOrDefault(6)?.Value<string>();
                    tvs.Add(new TestCase(rawTx, script, inputIndex, hashType, sigHashReg, sigHashNoFork, sigHashFork));
                });

            return tvs;
        }

        #endregion
    }
}
