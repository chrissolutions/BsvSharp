using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class Balance
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("confirmed")]
        public long Confirmed { get; set; }

        [JsonProperty("unconfirmed")]
        public long Unconfirmed { get; set; }
    }
}
