using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko.Models.Coins
{
    public class MarketDataOhlcv
    {
        [JsonProperty("market_cap_rank")]
        public long? MarketCapRank { get; set; }

        [JsonProperty("price_change_24h")]
        public double? PriceChange24H { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public double? PriceChangePercentage24H { get; set; }

        [JsonProperty("market_cap_change_24h")]
        public double? MarketCapChange24H { get; set; }

        [JsonProperty("market_cap_change_percentage_24h")]
        public double? MarketCapChangePercentage24H { get; set; }

        [JsonProperty("circulating_supply")]
        public string CirculatingSupply { get; set; }

        [JsonProperty("total_supply")]
        public double? TotalSupply { get; set; }
        
    }
}