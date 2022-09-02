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

        public TaalClient(string apiEnv)
            : this(apiEnv, BaseUrl)
        {
        }

        public TaalClient(string apiEnv, string url = BaseUrl)
            : this(apiEnv, NetworkType.Main, url)
        {
        }

        public TaalClient(string apiEnv, NetworkType networkType = NetworkType.Main, string url = BaseUrl)
            : base(ClientName, url, apiEnv, networkType)
        {
        }
    }
}
