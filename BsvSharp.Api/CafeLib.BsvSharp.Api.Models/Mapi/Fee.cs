﻿using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models.Mapi
{
    public class Fee
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("feeType")]
        public string FeeType { get; set; }

        [JsonProperty("miningFee")]
        public FeeRates MiningFee { get; set; }

        [JsonProperty("relayFee")]
        public FeeRates RelayFee { get; set; }
    }
}
