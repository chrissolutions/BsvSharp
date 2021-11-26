using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class TransactionStatus
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
    }
}
