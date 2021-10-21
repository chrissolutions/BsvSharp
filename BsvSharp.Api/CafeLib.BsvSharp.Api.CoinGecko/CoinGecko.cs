using System.Collections.Generic;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.CoinGecko.Models.Coins;
using CafeLib.Web.Request;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.CoinGecko
{
    public class CoinGecko : BasicApiRequest
    {
        private const string CoinGeckoUrl = "https://api.coingecko.com/api/v3";
 
        public CoinGecko()
        {
            Headers.Add("Accept", WebContentType.Json);
            Headers.Add("User-Agent", typeof(CoinGecko).Namespace);
        }

        public async Task<IEnumerable<Coin>> GetCoinList()
        {
            var endpoint = $"{CoinGeckoUrl}/coins/list";
            var json = await GetAsync(endpoint);
            return JsonConvert.DeserializeObject<IEnumerable<Coin>>(json);
        }

        public async Task<CoinFullData> GetCurrentData(string id)
        {
            var endpoint = $"{CoinGeckoUrl}/coins/{id}";
            var json = await GetAsync(endpoint);
            return JsonConvert.DeserializeObject<CoinFullData>(json);
        }
    }
}
