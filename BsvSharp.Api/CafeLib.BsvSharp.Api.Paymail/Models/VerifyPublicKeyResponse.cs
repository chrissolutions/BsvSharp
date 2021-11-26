using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    internal class VerifyPublicKeyResponse
    {
        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("pubkey")]
        public string PublicKey { get; set; }

        [JsonProperty("match")]
        public bool Match { get; set; }
    }
}
