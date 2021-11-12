using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models.Mapi
{
    public class FeeRates
    {
        [JsonProperty("satoshis")]
        public long Satoshis { get; set; }

        [JsonProperty("bytes")]
        public long Bytes { get; set; }
    }
}
