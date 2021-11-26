using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    internal class GetPublicKeyResponse
    {
        [JsonProperty("bsvalias")]
        public string BsvAlias { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("pubkey")]
        public string PubKey { get; set; }
    }
}
