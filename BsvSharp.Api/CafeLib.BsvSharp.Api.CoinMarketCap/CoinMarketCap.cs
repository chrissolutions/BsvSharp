#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Threading.Tasks;
using CafeLib.Web.Request;

namespace CafeLib.BsvSharp.Api.CoinMarketCap
{
    public class CoinMarketCap : BasicApiRequest
    {
        public CoinMarketCap(string apiKey)
        {
            var key = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            Headers.Add("Accepts", WebContentType.Json);
            Headers.Add("X-CMC_PRO_API_KEY", key);
        }

        public async Task<string> LatestListings()
        {
            var url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/listings/latest?start=1&limit=5000&convert=USD";
            var json = await GetAsync(url);
            /*
            {
                "status": {
                    "timestamp": "2019-12-09T20:52:15.002Z",
                    "error_code": 0,
                    "error_message": null,
                    "elapsed": 217,
                    "credit_count": 12,
                    "notice": null
                },
                "data": [
                    {
                        "id": 1,
                        "name": "Bitcoin",
                        "symbol": "BTC",
                        "slug": "bitcoin",
                        "num_market_pairs": 7721,
                        "date_added": "2013-04-28T00:00:00.000Z",
                        "tags": [
                            "mineable"
                        ],
                        "max_supply": 21000000,
                        "circulating_supply": 18092737,
                        "total_supply": 18092737,
                        "platform": null,
                        "cmc_rank": 1,
                        "last_updated": "2019-12-09T20:51:37.000Z",
                        "quote": {
                            "USD": {
                                "price": 7400.54230927,
                                "volume_24h": 17531781840.1686,
                                "percent_change_1h": -0.685321,
                                "percent_change_24h": -2.09101,
                                "percent_change_7d": 1.02466,
                                "market_cap": 133896065658.99478,
                                "last_updated": "2019-12-09T20:51:37.000Z"
                            }
                        }
                    },
                    {
                        "id": 1027,
                        "name": "Ethereum",
                        "symbol": "ETH",
                        "slug": "ethereum",
                        "num_market_pairs": 5268,
                        "date_added": "2015-08-07T00:00:00.000Z",
                        "tags": [
                            "mineable"
                        ],
                        "max_supply": null,
                        "circulating_supply": 108857249.624,
                        "total_supply": 108857249.624,
                        "platform": null,
                        "cmc_rank": 2,
                        "last_updated": "2019-12-09T20:51:23.000Z",
                        "quote": {
                            "USD": {
                                "price": 148.420503045,
                                "volume_24h": 6773088025.36599,
                                "percent_change_1h": -0.0833446,
                                "percent_change_24h": -1.85883,
                                "percent_change_7d": -0.134673,
                                "market_cap": 16156647749.289217,
                                "last_updated": "2019-12-09T20:51:23.000Z"
                            }
                        }
                    }
                ]
            } */
            return json;
        }
    }
}
