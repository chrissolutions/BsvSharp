#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Extensions;
using CafeLib.Web.Request;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests 
{
    public class WhatsOnChainTests
    {
        private WhatsOnChain.WhatsOnChain Api { get; } = new WhatsOnChain.WhatsOnChain();

        #region Health

        [Fact]
        public async Task GetHealth_Test()
        {
            var health = await Api.GetHealth();
            Assert.True(health.IsSuccessful);
        }

        #endregion

        #region Address

        [Theory]
        [InlineData("1PgZT1K9gKVtoAjCFnmQsviThu7oYDSCTR", 107297900, 0)]
        public async Task GetAddressBalance_Test(string address, long confirm, long unconfirmed)
        {
            var balance = await Api.GetAddressBalance(address);
            Assert.Equal(confirm, balance.Confirmed);
            Assert.Equal(unconfirmed, balance.Unconfirmed);
        }

        [Fact]
        public async Task GetBulkAddressBalances_Test()
        {
            var addresses = new[]
            {
                "16ZBEb7pp6mx5EAGrdeKivztd5eRJFuvYP",
                "1KGHhLTQaPr4LErrvbAuGE62yPpDoRwrob"
            };

            var balances = await Api.GetBulkAddressBalances(addresses);
            Assert.NotEmpty(balances);
            Assert.Equal(2, balances.Length);
            Assert.Equal(addresses[0], balances.First().Address);
        }

        [Theory]
        [InlineData("16ZqP5Tb22KJuvSAbjNkoiZs13mmRmexZA", "6b22c47e7956e5404e05c3dc87dc9f46e929acfd46c8dd7813a34e1218d2f9d1", 563052)]
        public async Task GetAddressHistory_Test(string address, string firstTxHash, long firstHeight)
        {
            var addressHistory = await Api.GetAddressHistory(address);
            Assert.NotEmpty(addressHistory);
            Assert.Equal(firstTxHash, addressHistory.First().TxHash);
            Assert.Equal(firstHeight, addressHistory.First().Height);
        }

        [Theory]
        [InlineData("1PgZT1K9gKVtoAjCFnmQsviThu7oYDSCTR", true)]
        public async Task GetAddressInfo_Test(string address, bool isValid)
        {
            var addressInfo = await Api.GetAddressInfo(address);
            Assert.Equal(address, addressInfo.Address);
            Assert.Equal(isValid, addressInfo.IsValid);
        }

        [Theory]
        [InlineData("1PgZT1K9gKVtoAjCFnmQsviThu7oYDSCTR", 107297900, 0)]
        public async Task GetAddressUtxos_Test(string address, long value, int position)
        {
            var unspentTransactions = await Api.GetAddressUtxos(address);
            Assert.NotEmpty(unspentTransactions);
            Assert.Equal(value, unspentTransactions.First().Value);
            Assert.Equal(position, unspentTransactions.First().TransactionPosition);
        }

        [Fact]
        public async Task GetBulkAddressUtxos_Test()
        {
            var addresses = new[]
            {
                "1PgZT1K9gKVtoAjCFnmQsviThu7oYDSCTR",
                "1KGHhLTQaPr4LErrvbAuGE62yPpDoRwrob"
            };

            var utxos = await Api.GetBulkAddressUtxos(addresses);
            Assert.NotEmpty(utxos);
            Assert.Equal(2, utxos.Length);
            Assert.Equal(addresses[0], utxos.First().Address);
            Assert.Equal(107297900, utxos.First().Utxos.First().Value);
        }

        #endregion

        #region Block

        [Theory]
        [InlineData("000000000000000009322213dd454961301f2126b7e73bd01c0bf042641df24c")]
        public async Task GetBlockByHash_Test(string blockHash)
        {
            var block = await Api.GetBlockByHash(blockHash);
            Assert.Equal(blockHash, block.Hash);
        }

        [Theory]
        [InlineData(577267)]
        public async Task GetBlockByHeight_Test(long blockHeight)
        {
            var block = await Api.GetBlockByHeight(blockHeight);
            Assert.Equal(blockHeight, block.Height);
        }

        [Theory]
        [InlineData("000000000000000009322213dd454961301f2126b7e73bd01c0bf042641df24c")]
        public async Task GetBlockPage_Test(string blockHash)
        {
            var transactions = await Api.GetBlockPage(blockHash, 1);
            Assert.NotNull(transactions);
            Assert.NotEmpty(transactions);
            Assert.Equal(2063, transactions.Length);
        }


        #endregion

        #region Chain

        [Fact]
        public async Task GetChainInfo_Test()
        {
            var chainInfo = await Api.GetChainInfo();
            Assert.NotNull(chainInfo);
            Assert.Equal(NetworkType.Main.GetDescriptor(), chainInfo.Chain);
        }

        [Fact]
        public async Task GetCirculatingSupply_Test()
        {
            var supply = await Api.GetCirculatingSupply();
            Assert.True(Math.Round(supply, 2) > 18865981.25);
        }

        #endregion

        #region Exchange

        [Fact]
        public async Task GetExchangeRate_Test()
        {
            var rate = await Api.GetExchangeRate();
            Assert.True(rate > 0 && rate < 1000000);
        }

        #endregion

        #region Mapi

        [Fact]
        public async Task GetFeeQuotes_Test()
        {
            var quotes = await Api.GetFeeQuotes();
            Assert.NotEmpty(quotes.ProviderQuotes);
            Assert.Contains(quotes.ProviderQuotes, quote => quote.ProviderName == "taal");
        }

        [Theory]
        [InlineData("995ea8d0f752f41cdd99bb9d54cb004709e04c7dc4088bcbbbb9ea5c390a43c3")]
        public async Task GetTxStatus_Test(string txHash)
        {
            var status = await Api.GetTranactionStatus(txHash);
            Assert.NotNull(status.Payload);
            Assert.Equal("mempool", status.ProviderName);
        }

        [Theory]
        [InlineData("010000000200010000000000000000000000000000000000000000000000000000000000000000000049483045022100d180fd2eb9140aeb4210c9204d3f358766eb53842b2a9473db687fa24b12a3cc022079781799cd4f038b85135bbe49ec2b57f306b2bb17101b17f71f000fcab2b6fb01ffffffff0002000000000000000000000000000000000000000000000000000000000000000000004847304402205f7530653eea9b38699e476320ab135b74771e1c48b81a5d041e2ca84b9be7a802200ac8d1f40fb026674fe5a5edd3dea715c27baa9baca51ed45ea750ac9dc0a55e81ffffffff010100000000000000015100000000")]
        public async Task Broadcast_Test(string txRaw)
        {
            var response = await Api.BroadcastTransaction(txRaw);
            Assert.False(response.IsSuccessful);
            Assert.Contains("dust", response.GetException<WebRequestException>().Response.Content);
        }

        #endregion

        #region Mempool

        [Fact]
        public async Task GetMempoolInfo_Test()
        {
            var mempool = await Api.GetMempoolInfo();
            Assert.NotNull(mempool);
            Assert.True(mempool.Bytes > 0);
        }

        [Fact]
        public async Task GetMempoolTransactions_Test()
        {
            var transactions = await Api.GetMempoolTransactions();
            Assert.NotNull(transactions);
            Assert.NotEmpty(transactions);
        }

        #endregion

        #region Search

        [Theory]
        [InlineData("1GJ3x5bcEnKMnzNFPPELDfXUCwKEaLHM5H")]
        public async Task GetExplorerLinks(string address)
        {
            var searchResult = await Api.GetExplorerLinks(address);
            Assert.NotNull(searchResult);
            Assert.NotEmpty(searchResult.Links);
            Assert.Equal("address", searchResult.Links.First().Type);
            Assert.Contains(address, searchResult.Links.First().Url);
        }


        #endregion

        #region Script

        [Theory]
        [InlineData("995ea8d0f752f41cdd99bb9d54cb004709e04c7dc4088bcbbbb9ea5c390a43c3")]
        public async Task GetScriptUtxos_Test(string scriptHash)
        {
            var unspentTransactions = await Api.GetScriptUtxos(scriptHash);
            Assert.Empty(unspentTransactions);
        }

        [Fact]
        public async Task GetBulkScriptUtxos_Test()
        {
            var hashes = new[]
            {
                "f814a7c3a40164aacc440871e8b7b14eb6a45f0ca7dcbeaea709edc83274c5e7",
                "995ea8d0f752f41cdd99bb9d54cb004709e04c7dc4088bcbbbb9ea5c390a43c3"
            };

            var utxos = await Api.GetBulkScriptUtxos(hashes);
            Assert.NotEmpty(utxos);
            Assert.Equal(2, utxos.Length);
        }

        [Theory]
        [InlineData("995ea8d0f752f41cdd99bb9d54cb004709e04c7dc4088bcbbbb9ea5c390a43c3")]
        public async Task GetScriptHistory_Test(string scriptHash)
        {
            var scriptHistory = await Api.GetScriptHistory(scriptHash);
            Assert.NotEmpty(scriptHistory);
        }

        #endregion

        #region Transaction

        [Theory]
        [InlineData("c1d32f28baa27a376ba977f6a8de6ce0a87041157cef0274b20bfda2b0d8df96")]
        public async Task GetTransactionByHash_Test(string hash)
        {
            var tx = await Api.GetTransactionByHash(hash);
            Assert.Equal(hash, tx.Hash);
        }

        [Theory]
        [InlineData("c1d32f28baa27a376ba977f6a8de6ce0a87041157cef0274b20bfda2b0d8df96", 2)]
        [InlineData("4c9e510077f5e5a961211100c0ed20173fdeae0e3575551e44b74581f74e7719", 16)]
        public async Task GetTransactionMerkleProof_Test(string hash, int count)
        {
            var tree = await Api.GetTransactionMerkleProof(hash);
            Assert.NotNull(tree);
            Assert.Equal(count, tree.Nodes.First().Branches.Length);
        }

        [Theory]
        [InlineData("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff1c030f47092f7376706f6f6c2e636f6d2f7051963bec2a64968d340100ffffffff01daa9944a0000000017a9141314c7eace4d4da3f65a1341197bb58038aa9dbc8700000000",
            "7c1a5ab633302d2299948420fafe55d0a784fd41588c2b692ffd2a339bf143b1"
        )]
        public async Task GetTransactionDecode_Test(string txRaw, string txHash)
        {
            var tx = await Api.DecodeTransaction(txRaw);
            Assert.Equal(txHash, tx.TxId);
            Assert.Equal(txHash, tx.Hash);
        }

        [Fact]
        public async Task GetBulkTransactionDetails_Test()
        {
            var txIds = new[]
            {
                "294cd1ebd5689fdee03509f92c32184c0f52f037d4046af250229b97e0c8f1aa",
                "91f68c2c598bc73812dd32d60ab67005eac498bef5f0c45b822b3c9468ba3258"
            };

            var txs = await Api.GetBulkTransactionDetails(txIds);
            Assert.NotEmpty(txs);
            Assert.Equal(2, txs.Length);
        }

        #endregion
    }
}
