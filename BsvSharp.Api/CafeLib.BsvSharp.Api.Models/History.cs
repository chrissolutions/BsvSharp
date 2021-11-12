﻿using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class History
    {
        [JsonProperty("tx_hash")]
        public string TxHash { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }
    }
}
