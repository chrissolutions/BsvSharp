using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class FeeQuote : Cargo
    {
        [JsonProperty("expiryTime")]
        public DateTime Expiry { get; set; }

        [JsonProperty("currentHighestBlockHash")]
        public string CurrentHighestBlockHash { get; set; }

        [JsonProperty("currentHighestBlockHeight")]
        public long CurrentHighestBlockHeight { get; set; }

        [JsonProperty("minerReputation")]
        public string MinerReputation { get; set; }

        [JsonProperty("fees")]
        public Fee[] Fees { get; set; }
    }
}
