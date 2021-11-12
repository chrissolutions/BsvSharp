using System.Collections.Generic;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.Models;
using CafeLib.BsvSharp.Mapi;
using CafeLib.BsvSharp.Mapi.Responses;
using CafeLib.BsvSharp.Network;
using CafeLib.Web.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafeLib.BsvSharp.Api.Metasv 
{
    public class MetasvClient : MerchantClient
    {
        private const string BaseUrl = "https://apiv2.metasv.com";
        private const string ClientName = "metasv";
        private const string DefaultApiKey = "Metasv.ApiKey";

        internal string ApiKey { get; }

        public MetasvClient()
            : this(GetApiKey(), BaseUrl)
        {
        }

        public MetasvClient(string apiKey)
            : this(apiKey, NetworkType.Main)
        {
        }

        public MetasvClient(string apiKey, string url = BaseUrl)
            : this(apiKey, NetworkType.Main, url)
        {
        }

        public MetasvClient(string apiKey, NetworkType networkType = NetworkType.Main, string url = BaseUrl)
            : base(ClientName, url, networkType)
        {
            ApiKey = apiKey ?? GetApiKey();
        }

        /// <summary>
        /// Get API key.
        /// </summary>
        private static string GetApiKey() => DefaultApiKey;

        #region Address

        public async Task<Balance> GetAddressBalance(string address)
        {
            var url = $"{Url}/address/{address}/balance";
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
            var url = $"{Url}/address/{address}/utxo";
            var json = await GetAsync(url);
            var utxos = JsonConvert.DeserializeObject<Utxo[]>(json);
            return utxos;
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



        public override Task<ApiResponse<TransactionSubmitResponse>> SubmitTransaction(string txRaw)
        {
            try
            {
                Headers.Add("Authorization", $"Bearer {ApiKey}");
                return base.SubmitTransaction(txRaw);
            }
            finally
            {
                Headers.Remove("Authorization");
            }
        }
    }
}
