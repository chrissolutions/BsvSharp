using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class FeeRate
    {
        [JsonProperty("satoshis")]
        public long Satoshis { get; set; }

        [JsonProperty("bytes")]
        public long Bytes { get; set; }
    }
}
