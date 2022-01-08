using System.Threading.Tasks;
using CafeLib.BsvSharp.Mapi.Responses;
using CafeLib.BsvSharp.Network;
using CafeLib.Web.Request;

namespace CafeLib.BsvSharp.Mapi.Taal 
{
    public class TaalClient : MerchantClient
    {
        private const string BaseUrl = "https://mapi.taal.com";
        private const string ClientName = "taal";
        private const string DefaultApiKey = "Taal.ApiKey";

        internal string ApiKey { get; }

        public TaalClient()
            : this(GetApiKey(), BaseUrl)
        {
        }

        public TaalClient(string apiKey)
            : this(apiKey, NetworkType.Main)
        {
        }

        public TaalClient(string apiKey, string url = BaseUrl)
            : this(apiKey, NetworkType.Main, url)
        {
        }

        public TaalClient(string apiKey, NetworkType networkType = NetworkType.Main, string url = BaseUrl)
            : base(ClientName, url, networkType)
        {
            ApiKey = apiKey ?? GetApiKey();
        }

        /// <summary>
        /// Get API key.
        /// </summary>
        private static string GetApiKey() => DefaultApiKey;

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
