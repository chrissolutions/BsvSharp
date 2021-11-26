using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class MerchantResponse<T> : Envelope where T : Cargo
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        public T Payload { get; set; }
    }
}
