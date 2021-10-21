#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Services;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Transactions
{
    public class TransactionTests
    {
        private const string DataFolder = @"..\..\..\data";

        private static readonly Address FromAddress = new Address("mszYqVnqKoQx4jcTdJXxwKAissE3Jbrrc1");
        private static readonly Address ToAddress = new Address("mrU9pEmAx26HcbKVrABvgL7AwA5fjNFoDc");
        private static readonly Address ChangeAddress = new Address("mgBCJAsvzgT2qNNeXsoECg2uPKrUsZ76up");
        private static readonly PrivateKey PrivateKeyFromWif = PrivateKey.FromWif("cSBnVM4xvxarwGQuAfQFwqDg9k5tErHUHzgWsEfD4zdwUasvqRVY");

        private static readonly Utxo UtxoWith1Coin = new Utxo
        {
            TxHash = new UInt256("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458"),
            Index = 1,
            ScriptPubKey = new P2PkhLockBuilder(FromAddress).ToScript(),
            Amount = RootService.Network.Consensus.SatoshisPerCoin
        };

        private static readonly Utxo UtxoWith1MillionSatoshis = new Utxo
        {
            TxHash = new UInt256("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458"),
            Index = 0,
            ScriptPubKey = new P2PkhLockBuilder(FromAddress).ToScript(),
            Amount = 1000000L
        };

        private static readonly Utxo UtxoWith100000Satoshis = new Utxo 
        {
            TxHash = new UInt256("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458"),
            Index = 0,
            ScriptPubKey = new P2PkhLockBuilder(FromAddress).ToScript(),
            Amount = 100000
        };

        [Theory]
        [InlineData("a477af6b2667c29670467e4e0728b685ee07b240235771862318e29ddbe58458")]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000001")]
        public void Verify_TxId_Test(string txId)
        {
            var txIn = new TxIn(new UInt256(txId), 0, 1000L);
            Assert.Equal(txId, txIn.TxHash.ToString());
        }

        [Fact]
        public void Parse_Transaction_Version_As_Signed_Integer()
        {
            var transaction = new Transaction("ffffffff0000ffffffff");
            Assert.Equal(-1, transaction.Version);
            Assert.Equal(0xffffffff, transaction.LockTime);
        }

        [Fact]
        public void Deserialize_Transaction()
        {
            const string txHex = "01000000015884e5db9de218238671572340b207ee85b628074e7e467096c267266baf77a4000000006a473044022013fa3089327b50263029265572ae1b022a91d10ac80eb4f32f291c914533670b02200d8a5ed5f62634a7e1a0dc9188a3cc460a986267ae4d58faf50c79105431327501210223078d2942df62c45621d209fab84ea9a7a23346201b7727b9b45a29c4e76f5effffffff0150690f00000000001976a9147821c0a3768aa9d1a37e16cf76002aef5373f1a888ac00000000";
            var writer = new ByteDataWriter();

            var transaction = new Transaction(txHex);
            transaction.WriteTo(writer);

            var serializedHex = Encoders.Hex.Encode(writer.Span);
            Assert.Equal(txHex, serializedHex);
        }

        [Fact]
        public void Coinbase_Transaction()
        {
            const string txHex = "01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000";
            var transaction = new Transaction(txHex);
            Assert.True(transaction.IsCoinbase);
        }

        [Fact]
        public void Spend_Transaction()
        {
            var changeScriptBuilder = new P2PkhLockBuilder(ChangeAddress);

            var transaction = new Transaction();
                transaction.SpendFrom(UtxoWith1MillionSatoshis.TxHash,
                                      UtxoWith1MillionSatoshis.Index, 
                                      UtxoWith1MillionSatoshis.Amount, 
                                      UtxoWith1MillionSatoshis.ScriptPubKey);
                transaction.SpendTo(ToAddress, 500000L, new P2PkhLockBuilder(ToAddress));
                transaction.SendChangeTo(ChangeAddress, changeScriptBuilder);
                transaction.WithFeePerKb(100000);

            transaction.SignInput(0, PrivateKeyFromWif);

            Assert.Equal(2, transaction.Outputs.Count);
            Assert.Equal(472899L, transaction.Outputs[1].Amount.Satoshis);
            Assert.Equal(changeScriptBuilder.ToScript().ToString(), transaction.Outputs[1].Script.ToString());
        }

        [Fact]
        public void Verify_Valid_Transaction()
        {
            GetValidTransactions()
                .Where(x => (x.VerifyFlags & ScriptFlags.VERIFY_P2SH) == 0)
                .ForEach(x =>
                {
                    var expectedHash = x.Transactions.First().Hash;
                    var expectedIndex = x.Transactions.First().Index;
                    var _ = x.Transactions.First().ScriptPubKey;
                    var transaction = new Transaction(Encoders.Hex.Decode(x.Serialized));
                    var previousHash = transaction.Inputs.First().PrevOut.TxHash;
                    var previousIndex = transaction.Inputs.First().PrevOut.Index;
                    Assert.Equal(expectedHash, previousHash);
                    Assert.Equal(expectedIndex, previousIndex);
                });
        }

        [Fact]
        public void Fail_If_No_Change_Address()
        {
            var tx = new Transaction()
                .SpendFrom(UtxoWith1Coin.TxHash, UtxoWith1Coin.Index, UtxoWith1Coin.Amount, UtxoWith1Coin.ScriptPubKey)
                .SpendTo(ToAddress, 500000L, new P2PkhLockBuilder(ToAddress));

            Assert.Throws<TransactionFeeException>(() => tx.Serialize(true));
        }

        [Fact]
        public void Fail_If_Not_Positive_Amount()
        {
            var destAddress = new Address("mrU9pEmAx26HcbKVrABvgL7AwA5fjNFoDc");
            var tx = new Transaction();

            Assert.Throws<TransactionAmountException>(() => tx.SpendTo(destAddress, Amount.Zero, new P2PkhLockBuilder(destAddress)));
        }

        [Fact]
        public void Fail_If_High_Fee_Was_Set()
        {
            var tx = new Transaction()
                .SpendFromUtxo(UtxoWith1Coin)
                .SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress))
                .WithFee(50000000)
                .SpendTo(ToAddress, 40000000, new P2PkhLockBuilder(ToAddress));

            Assert.Throws<TransactionFeeException>(() => tx.Serialize(true));
        }

        [Fact]
        public void Fail_Fee_Error()
        {
            var tx = new Transaction()
                .SpendFromUtxo(UtxoWith1Coin)
                .SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress))
                .WithFee(50000000)
                .SpendTo(ToAddress, 40000000, new P2PkhLockBuilder(ToAddress));

            Assert.Throws<TransactionFeeException>(() => tx.Serialize(true));
        }

        [Fact]
        public void Fail_If_Dust_Is_Created()
        {
            var tx = new Transaction()
                .SpendFromUtxo(UtxoWith1Coin, new P2PkhUnlockBuilder(PrivateKeyFromWif.CreatePublicKey()))
                .SpendTo(ToAddress, 545, new P2PkhLockBuilder(ToAddress))
                .SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress));

            Assert.Throws<TransactionAmountException>(() => tx.Serialize(true));
        }

        [Fact]
        public void Fail_When_Sum_Of_Outputs_And_Fee_NotEqual_Total_Input()
        {
            var tx = new Transaction()
                .SpendFromUtxo(UtxoWith1Coin, new P2PkhUnlockBuilder(PrivateKeyFromWif.CreatePublicKey()))
                .SpendTo(ToAddress, 99900000, new P2PkhLockBuilder(ToAddress))
                .WithFee(99999);
            
            Assert.Throws<TransactionFeeException>(() => tx.Serialize(true));
        }

        [Fact]
        public void Verify_Zero_Fee_For_A_Coinbase()
        {
            var coinbaseTransaction = new Transaction("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000");
            Assert.True(coinbaseTransaction.IsCoinbase);
            Assert.Equal(Amount.Zero, coinbaseTransaction.GetFee());
        }

        [Fact]
        public void Verify_Dust_Output()
        {   
            var tx = new Transaction();
                tx.SpendFromUtxo(UtxoWith1Coin, new P2PkhUnlockBuilder(PrivateKeyFromWif.CreatePublicKey()));
                tx.SpendTo(ToAddress, 546, new P2PkhLockBuilder(ToAddress));
                tx.SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress));
            
            tx.SignInput(0, PrivateKeyFromWif);

            Assert.True(tx.Verify());
            Assert.NotNull(tx.Serialize());
        }

        [Fact]
        public void Verify_Dust_As_OpReturn()
        {
            var tx = new Transaction();
            tx.SpendFromUtxo(UtxoWith1Coin, new P2PkhUnlockBuilder(PrivateKeyFromWif.CreatePublicKey()));
            tx.AddData("not dust!".Utf8ToBytes());
            tx.SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress));

            tx.SignInput(0, PrivateKeyFromWif);

            Assert.True(tx.Verify());
            Assert.NotNull(tx.Serialize());
        }

        [Fact]
        public void Verify_Hash_Decoded_Correctly()
        {
            const string tx1Hex = "01000000015884e5db9de218238671572340b207ee85b628074e7e467096c267266baf77a4000000006a473044022013fa3089327b50263029265572ae1b022a91d10ac80eb4f32f291c914533670b02200d8a5ed5f62634a7e1a0dc9188a3cc460a986267ae4d58faf50c79105431327501210223078d2942df62c45621d209fab84ea9a7a23346201b7727b9b45a29c4e76f5effffffff0150690f00000000001976a9147821c0a3768aa9d1a37e16cf76002aef5373f1a888ac00000000";
            var tx = new Transaction(tx1Hex);
            Assert.Equal(Encoders.Hex.Encode(tx.Serialize()), tx1Hex);
        }

        [Fact]
        public void Recalculate_Change_Amount()
        {
            var transaction = new Transaction();
            transaction.SpendFromUtxo(UtxoWith100000Satoshis);
            transaction.SpendTo(ToAddress, 50000L, new P2PkhLockBuilder(ToAddress));
            transaction.SendChangeTo(ChangeAddress, new P2PkhLockBuilder(ChangeAddress));
            transaction.WithFee(Amount.Zero);

            transaction.SignInput(0, PrivateKeyFromWif);

            var changeLocker = new P2PkhLockBuilder(ChangeAddress);
            Assert.Equal(new Amount(50000), transaction.GetChangeOutput(changeLocker).Amount);

            transaction = transaction.SpendTo(ToAddress, 20000L);
            transaction.SignInput(0, PrivateKeyFromWif);

            Assert.Equal(3, transaction.Outputs.Length);
            Assert.Equal(new Amount(30000), transaction.Outputs[2].Amount);
            Assert.Equal(changeLocker.ToScript().ToString(), transaction.Outputs[2].Script.ToString());
        }

        [Fact]
        public void Adds_No_Fee_If_No_Available_Change()
        {
            var transaction = new Transaction();
            transaction.SpendFrom(UtxoWith100000Satoshis.TxHash,
                UtxoWith100000Satoshis.Index,
                UtxoWith100000Satoshis.Amount,
                new P2PkhUnlockBuilder(PrivateKeyFromWif.CreatePublicKey()));
            transaction.SpendTo(ToAddress, 99000L, new P2PkhLockBuilder(ToAddress));
            Assert.Equal(1, transaction.Outputs.Length);
            Assert.Equal(1000, transaction.GetFee());
        }

        [Fact]
        public void Utxo_Are_Added_Exactly_Once()
        {
            var transaction = new Transaction();
            transaction.SpendFromUtxo(UtxoWith1Coin, true);
            transaction.SpendFromUtxo(UtxoWith1Coin, true);

            Assert.Equal(1, transaction.Inputs.Length);
        }

        #region Helpers

        private class TxInfo
        {
            public UInt256 Hash { get; set; }
            public int Index { get; set; }
            public string ScriptPubKey { get; set; }
        }

        private class TxTestInput
        {
            /// <summary>
            /// ScriptSig as hex string.
            /// </summary>
            public List<TxInfo> Transactions { get; set; }
            public string Serialized { get; set; }
            public ScriptFlags VerifyFlags { get; set; }
        }

        private static IEnumerable<TxTestInput> GetValidTransactions()
        {
            var values = new List<TxTestInput>();
            var jarray = JArray.Parse(File.ReadAllText(Path.Combine(DataFolder, "tx_valid.json")));

            foreach (var json in jarray)
            {
                var jtoken = FindValueInArray(json);
                if (jtoken.Type == JTokenType.String && jtoken.Parent?.Count >= 3)
                {
                    var txInput = GetTransactionInput(json);
                    values.Add(txInput);
                }
            }

            return values;

            static JToken FindValueInArray(JToken token)
            {
                if (!(token is JArray items)) return string.Empty;

                foreach (var json in items)
                {
                    return json.Type == JTokenType.Array ? FindValueInArray((JArray)json) : json;
                }

                return string.Empty;
            }

            static TxTestInput GetTransactionInput(JToken token)
            {
                var transactions = new List<TxInfo>();
                var parent = token.Parent;
                ScriptFlags verify = 0U;

                while (token != null)
                {
                    token = FindValueInArray(token);
                    parent = token.Parent;
                    var prevTxHash = parent?[0]?.Value<string>();
                    var prevTxIndex = parent?[1]?.Value<string>();
                    var prevScript = parent?[2]?.Value<string>();
                    transactions.Add(BuildTransactionInfo(prevTxHash, prevTxIndex, prevScript));
                    token = parent?.Next;
                }

                var flags = parent?.Parent?.Parent?[2]?.Value<string>() ?? "NONE";
                flags.Split(',').ForEach(x => verify |= Enum.Parse<ScriptFlags>($"VERIFY_{x}"));

                return new TxTestInput
                {
                    Transactions = transactions,
                    Serialized = parent?.Parent?.Parent?[1]?.Value<string>(),
                    VerifyFlags = verify
                };
            }
        }

        private static TxInfo BuildTransactionInfo(string hash, string index, string scriptPubKey)
        {
            return new TxInfo
            {
                Hash = new UInt256(Encoders.HexReverse.Decode(hash)),
                Index = int.Parse(index),
                ScriptPubKey = scriptPubKey
            };
        }

        #endregion
    }
}