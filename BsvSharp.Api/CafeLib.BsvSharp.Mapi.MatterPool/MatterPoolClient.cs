using CafeLib.BsvSharp.Network;

namespace CafeLib.BsvSharp.Mapi.MatterPool 
{
    public class MatterPoolClient : MerchantClient
    {
        private const string BaseUrl = "https://merchantapi.matterpool.io";
        private const string ClientName = "matterpool";

        public MatterPoolClient()
            : base(ClientName, BaseUrl)
        {
        }

        public MatterPoolClient(string url = BaseUrl)
            : base(ClientName, url)
        {
        }

        public MatterPoolClient(NetworkType networkType = NetworkType.Main, string url = BaseUrl)
            : base(ClientName, url, networkType)
        {
        }
    }
}
