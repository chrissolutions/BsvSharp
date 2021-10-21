using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    internal class GetOutputScriptRequest
    {
        [JsonProperty("senderName")]
        public string SenderName { get; set; }

        [JsonProperty("senderHandle")]
        public string SenderHandle { get; set; }

        [JsonProperty("dt")]
        public string Timestamp { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
