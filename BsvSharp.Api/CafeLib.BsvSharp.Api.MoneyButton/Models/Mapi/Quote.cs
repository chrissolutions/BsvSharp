using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class Quote
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("expiryTime")]
        public DateTime Expiry { get; set; }

        [JsonProperty("minerId")]
        public string MinerId { get; set; }

        [JsonProperty("currentHighestBlockHash")]
        public string CurrentHighestBlockHash { get; set; }

        [JsonProperty("currentHighestBlockHeight")]
        public string CurrentHighestBlockHeight { get; set; }

        [JsonProperty("minerReputation")]
        public string MinerReputation { get; set; }

        [JsonProperty("fees")]
        public Fee[] Fees { get; set; }
    }
}
