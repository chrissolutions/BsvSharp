using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class MerchantResponse<T> : Envelope
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        public T Cargo { get; set; }
    }
}
