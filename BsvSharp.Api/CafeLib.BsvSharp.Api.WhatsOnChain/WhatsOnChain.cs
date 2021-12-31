#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.WhatsOnChain.Models;
using CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi;
using CafeLib.BsvSharp.Mapi;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Support;
using CafeLib.Web.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafeLib.BsvSharp.Api.WhatsOnChain 
{
    public class WhatsOnChain : MerchantClient
    {
        private const string BaseUrl = "https://mapi.taal.com";
        private const string ClientName = "taal";

        public WhatsOnChain(NetworkType networkType = NetworkType.Main)
            : base(ClientName, BaseUrl, networkType)
        {
            Headers.Add("Content-Type", WebContentType.Json);
            Headers.Add("User-Agent", typeof(WhatsOnChain).Namespace);
        }

        #region Address

        public async Task<ApiResponse<Balance>> GetAddressBalance(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/balance";
            var response = await GetRequest<Balance>(url);
            return response;
        }

        public async Task<ApiResponse<AddressBalance[]>> GetBulkAddressBalances(IEnumerable<string> addresses)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/addresses/balance";
            var jsonText = $@"{{""addresses"": {JsonConvert.SerializeObject(addresses)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var response = await PostRequest<AddressBalance[]>(url, jsonBody);
            return response;
        }

        public async Task<ApiResponse<History[]>> GetAddressHistory(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/history";
            var response = await GetRequest<History[]>(url);
            return response;
        }

        public async Task<ApiResponse<AddressInfo>> GetAddressInfo(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/info";
            var response = await GetRequest<AddressInfo>(url);
            return response;
        }

        public async Task<ApiResponse<Utxo[]>> GetAddressUtxos(string address)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/address/{address}/unspent";
            var response = await GetRequest<Utxo[]>(url);
            return response;
        }

        public async Task<ApiResponse<AddressUtxo[]>> GetBulkAddressUtxos(IEnumerable<string> addresses)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/addresses/unspent";
            var jsonText = $@"{{""addresses"": {JsonConvert.SerializeObject(addresses)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var response = await PostRequest<AddressUtxo[]>(url, jsonBody);
            return response;
        }

        #endregion

        #region Block

        public async Task<ApiResponse<Block>> GetBlockByHash(string blockHash)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/hash/{blockHash}";
            var response = await GetRequest<Block>(url);
            return response;
        }

        public async Task<ApiResponse<Block>> GetBlockByHeight(long blockHeight)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/height/{blockHeight}";
            var response = await GetRequest<Block>(url);
            return response;
        }

        public async Task<ApiResponse<string[]>> GetBlockPage(string blockHash, long pageNumber)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/block/hash/{blockHash}/page/{pageNumber}";
            var response = await GetRequest<string[]>(url);
            return response;
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

        public async Task<ApiResponse<Health>> GetHealth()
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/woc";
            var response = await GetRequest <Health>(url);
            return response;
        }

        #endregion

        #region Mapi

        [Obsolete("Use GetFeeQuote")]
        public async Task<ApiResponse<Quote>> GetFeeQuotes()
        {
            const string url = "https://api.whatsonchain.com/v1/bsv/main/mapi/feeQuotes";
            var response = await GetRequest<Quote>(url);
            return response;
        }

        public async Task<ApiResponse<TransactionStatus>> GetTransactionStatus(string txHash)
        {
            var url = $"https://mapi.taal.com/mapi/tx/{txHash}";
            var response = await GetRequest<TransactionStatus>(url);
            return response;
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

        public async Task<ApiResponse<ScriptUtxo[]>> GetBulkScriptUtxos(IEnumerable<string> hashes)
        {
            var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/scripts/unspent";
            var jsonText = $@"{{""scripts"": {JsonConvert.SerializeObject(hashes)}}}";
            var jsonBody = JToken.Parse(jsonText);
            var response = await PostRequest<ScriptUtxo[]>(url, jsonBody);
            return response;
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

        public async Task<ApiResponse> BroadcastTransaction(string txRaw)
        {
            try
            {
                var url = $"https://api.whatsonchain.com/v1/bsv/{Network}/tx/raw";
                var jsonText = $@"{{""txHex"": ""{txRaw}""}}";
                var jsonBody = JToken.Parse(jsonText);
                await PostAsync(url, jsonBody);
                return new ApiResponse();
            }
            catch (Exception ex)
            {
                return new ApiResponse(ex);
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

        #region Helpers

        private async Task<ApiResponse<TResult>> GetRequest<TResult>(string url)
        {
            try
            {
                var json = await GetAsync(url);
                var response = JsonConvert.DeserializeObject<TResult>(json);
                if (response == null) throw new Exception("null response");
                return Creator.CreateInstance<ApiResponse<TResult>>(response);
            }
            catch (Exception ex)
            {
                return Creator.CreateInstance<ApiResponse<TResult>>(ex);
            }
        }

        private async Task<ApiResponse<TResult>> PostRequest<TResult>(string url, JToken body)
        {
            try
            {
                var json = await PostAsync(url, body);
                var response = JsonConvert.DeserializeObject<TResult>(json);
                if (response == null) throw new Exception("null response");
                return Creator.CreateInstance<ApiResponse<TResult>>(response);
            }
            catch (Exception ex)
            {
                return Creator.CreateInstance<ApiResponse<TResult>>(ex);
            }
        }

        #endregion

    }
}
