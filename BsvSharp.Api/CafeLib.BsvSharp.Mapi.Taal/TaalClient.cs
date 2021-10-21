using CafeLib.BsvSharp.Network;

namespace CafeLib.BsvSharp.Mapi.Taal 
{
    public class TaalClient : MerchantClient
    {
        private const string BaseUrl = "https://mapi.taal.com";
        private const string ClientName = "taal";
        private const string ApiKey = "Customize Taal.ApiKey retrieval";

        public TaalClient()
            : this(BaseUrl)
        {
        }

        public TaalClient(string url = BaseUrl)
            : this(NetworkType.Main, url)
        {
        }

        public TaalClient(NetworkType networkType = NetworkType.Main, string url = BaseUrl)
            : base(ClientName, url, networkType)
        {
            Headers.Add("Authorization", $"Bearer {GetApiKey()}");
        }

        /// <summary>
        /// Get API key.
        /// </summary>
        private static string GetApiKey() => ApiKey;
    }
}
