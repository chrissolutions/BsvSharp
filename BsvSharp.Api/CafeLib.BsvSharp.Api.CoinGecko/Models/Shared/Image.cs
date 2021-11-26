using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko.Models.Shared
{
    public class Image
    {
        [JsonProperty("thumb")]
        public Uri Thumb { get; set; }

        [JsonProperty("small")]
        public Uri Small { get; set; }

        [JsonProperty("large")]
        public Uri Large { get; set; }
    }
}