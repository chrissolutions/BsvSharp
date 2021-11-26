using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko.Models.Coins
{
    public class Coin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}