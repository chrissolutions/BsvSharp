using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Balance
    {
        [JsonProperty("confirmed")]
        public long Confirmed { get; set; }

        [JsonProperty("unconfirmed")]
        public long Unconfirmed { get; set; }
    }
}
