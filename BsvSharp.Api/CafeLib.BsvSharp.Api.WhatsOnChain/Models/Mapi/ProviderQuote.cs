using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class ProviderQuote
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        [JsonProperty("quote")]
        public Quote Quote { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("txSubmissionUrl")]
        public string TxSubmissionUrl { get; set; }

        [JsonProperty("txStatusUrl")]
        public string TxStatusUrl { get; set; }
    }
}
