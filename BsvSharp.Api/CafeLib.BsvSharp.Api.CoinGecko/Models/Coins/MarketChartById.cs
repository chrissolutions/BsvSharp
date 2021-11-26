using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko.Models.Coins
{
    public class MarketChartById
    {
        [JsonProperty("prices")]
        public double[][] Prices { get; set; }

        [JsonProperty("market_caps")]
        public double[][] MarketCaps { get; set; }

        [JsonProperty("total_volumes")]
        public double[][] TotalVolumes { get; set; }
    }
}