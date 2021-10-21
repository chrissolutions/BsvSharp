#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.WhatsOnChain.Models;
using CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Extensions;
using CafeLib.Web.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafeLib.BsvSharp.Api.WhatsOnChain 
{
    public class WhatsOnChain : BasicApiRequest
    {
        public string Network { get; }

        public WhatsOnChain(NetworkType networkType = NetworkType.Main)
        {
            Network = networkType.GetDescriptor();
            Headers.Add("Content-Type", WebContentType.Json);
            Headers.Add("User-Agent", typeof(WhatsOnChain).Namespace);
        }

        #region Address

        public async Task<Balance> GetAddressBalance(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/balance";
            var json = await GetAsync(url);
            var balance = JsonConvert.DeserializeObject<Balance>(json);
            return balance;
        }

        public async Task<AddressBalance[]> GetBulkAddressBalances(IEnumerable<string> addresses)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/addresses/balance";
            var jsonText = $@"{{""addresses"": {JsonConvert.SerializeObject(addresses)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var json = await PostAsync(url, jsonBody);
            var balances = JsonConvert.DeserializeObject<AddressBalance[]>(json);
            return balances;
        }

        public async Task<History[]> GetAddressHistory(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/history";
            var json = await GetAsync(url);
            return JsonConvert.DeserializeObject<History[]>(json);
        }

        public async Task<AddressInfo> GetAddressInfo(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/info";
            var json = await GetAsync(url);
            var addressInfo = JsonConvert.DeserializeObject<AddressInfo>(json);
            return addressInfo;
        }

        public async Task<Utxo[]> GetAddressUtxos(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/unspent";
            var json = await GetAsync(url);
            var unspent = JsonConvert.DeserializeObject<Utxo[]>(json);
            return unspent;
        }

        public async Task<AddressUtxo[]> GetBulkAddressUtxos(IEnumerable<string> addresses)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/addresses/unspent";
            var jsonText = $@"{{""addresses"": {JsonConvert.SerializeObject(addresses)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var json = await PostAsync(url, jsonBody);
            var utxos = JsonConvert.DeserializeObject<AddressUtxo[]>(json);
            return utxos;
        }

        #endregion

        #region Block

        public async Task<Block> GetBlockByHash(string blockHash)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/hash/{blockHash}";
            var json = await GetAsync(url);
            var block = JsonConvert.DeserializeObject<Block>(json);
            return block;
        }

        public async Task<Block> GetBlockByHeight(long blockHeight)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/height/{blockHeight}";
            var json = await GetAsync(url);
            var block = JsonConvert.DeserializeObject<Block>(json);
            return block;
        }

        public async Task<string[]> GetBlockPage(string blockHash, long pageNumber)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/hash/{blockHash}/page/{pageNumber}";
            var json = await GetAsync(url);
            var transactions = JsonConvert.DeserializeObject<string[]>(json);
            return transactions;
        }

        #endregion

        #region Chain

        public async Task<ChainInfo> GetChainInfo()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/chain/info";
            var json = await GetAsync(url);
            var chainInfo = JsonConvert.DeserializeObject<ChainInfo>(json);
            return chainInfo;
        }

        public async Task<double> GetCirculatingSupply()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/circulatingsupply";
            var json = await GetAsync(url);
            return Convert.ToDouble(json);
        }

        #endregion

        #region Exchange

        public async Task<decimal> GetExchangeRate()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/exchangerate";
            var json = await GetAsync(url);

            var er = JsonConvert.DeserializeObject<ExchangeRate>(json);
            return er?.Rate ?? decimal.Zero;
        }

        #endregion

        #region Health

        public async Task<Health> GetHealth()
        {
            try
            {
                var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/woc";
                var _ = await GetAsync(url);
                return new Health();
            }
            catch (WebRequestException e)
            {
                return new Health(e);
            }
        }

        #endregion

        #region Mapi

        public async Task<Quotes> GetFeeQuotes()
        {
            const string url = "https://api.whatsonchain.com/v1/bsv/main/mapi/feeQuotes";
            var json = await GetAsync(url);
            var quotes = JsonConvert.DeserializeObject<Quotes>(json);
            return quotes;
        }

        public async Task<TransactionStatus> GetTranactionStatus(string txHash)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/main/mapi/ab398390/tx/{txHash}";
            var json = await GetAsync(url);
            var status = JsonConvert.DeserializeObject<TransactionStatus>(json);
            return status;
        }

        #endregion

        #region Mempool

        public async Task<Mempool> GetMempoolInfo()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/mempool/info";
            var json = await GetAsync(url);
            var mempool = JsonConvert.DeserializeObject<Mempool>(json);
            return mempool;
        }

        public async Task<string[]> GetMempoolTransactions()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/mempool/raw";
            var json = await GetAsync(url);
            var transactions = JsonConvert.DeserializeObject<string[]>(json);
            return transactions;
        }

        #endregion

        #region Script

        public async Task<Utxo[]> GetScriptUtxos(string scriptHash)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/script/{scriptHash}/unspent";
            var json = await GetAsync(url);
            var unspent = JsonConvert.DeserializeObject<Utxo[]>(json);
            return unspent;
        }

        public async Task<ScriptUtxo[]> GetBulkScriptUtxos(IEnumerable<string> hashes)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/scripts/unspent";
            var jsonText = $@"{{""scripts"": {JsonConvert.SerializeObject(hashes)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var json = await PostAsync(url, jsonBody);
            var utxos = JsonConvert.DeserializeObject<ScriptUtxo[]>(json);
            return utxos;
        }

        public async Task<History[]> GetScriptHistory(string scriptHash)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/script/{scriptHash}/history";
            var json = await GetAsync(url);
            return JsonConvert.DeserializeObject<History[]>(json);
        }

        #endregion

        #region Search

        public async Task<SearchResults> GetExplorerLinks(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/search/links";
            var jsonText = $@"{{""query"": ""{address}""}}";
            var jsonBody = JToken.Parse(jsonText);
            var json = await PostAsync(url, jsonBody);
            var results = JsonConvert.DeserializeObject<SearchResults>(json);
            return results;
        }

        #endregion

        #region Transaction

        public async Task<Response> BroadcastTransaction(string txRaw)
        {
            try
            {
                var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/tx/raw";
                var jsonText = $@"{{""txHex"": ""{txRaw}""}}";
                var jsonBody = JToken.Parse(jsonText);
                await PostAsync(url, jsonBody);
                return new Response();
            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }

        public async Task<Transaction> DecodeTransaction(string txRaw)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/tx/decode";
            var jsonContent = JsonConvert.SerializeObject(new { txHex = txRaw });
            var jsonBody = JToken.Parse(jsonContent);
            var json = await PostAsync(url, jsonBody);
            var tx = JsonConvert.DeserializeObject<Transaction>(json);
            return tx;
        }

        public async Task<Transaction> GetTransactionByHash(string txid)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/tx/hash/{txid}";
            var json = await GetAsync(url);
            var tx = JsonConvert.DeserializeObject<Transaction>(json);
            return tx;
        }

        public async Task<Transaction[]> GetBulkTransactionDetails(IEnumerable<string> txIds)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/txs";
            var jsonBody = JToken.FromObject(new { txids = txIds });
            var json = await PostAsync(url, jsonBody);
            var utxos = JsonConvert.DeserializeObject<Transaction[]>(json);
            return utxos;
        }

        public async Task<MerkleProof> GetTransactionMerkleProof(string txId)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/tx/{txId}/proof";
            var json = await GetAsync(url);
            var proof = JsonConvert.DeserializeObject<MerkleNode[]>(json);
            return new MerkleProof {Nodes = proof};
        }

        #endregion
    }
}
