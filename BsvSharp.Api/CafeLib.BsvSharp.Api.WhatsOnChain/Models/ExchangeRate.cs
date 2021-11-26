using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class ExchangeRate
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("rate")]
        public decimal Rate { get; set; }
    }
}