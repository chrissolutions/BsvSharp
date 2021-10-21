using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko.Models.Coins
{
    public class SparklineIn7D
    {
        [JsonProperty("price")]
        public double[] Price { get; set; }
    }
}