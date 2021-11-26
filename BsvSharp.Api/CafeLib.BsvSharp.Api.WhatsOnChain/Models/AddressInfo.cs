using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class AddressInfo
    {
        [JsonProperty("isvalid")]
        public bool IsValid { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("scriptPubKey")]
        public string ScriptPubKey { get; set; }

        [JsonProperty("ismine")]
        public bool IsMine { get; set; }

        [JsonProperty("iswatchonly")]
        public bool IsWatchOnly { get; set; }

        [JsonProperty("isscript")]
        public bool IsScript { get; set; }
    }
}
