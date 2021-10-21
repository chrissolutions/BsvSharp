#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CafeLib.BsvSharp.Api.UnitTests {
    public class KzApiCoinGeckoApiTests
    {
        [Fact]
        public async Task GetSupportedCoins_Test() 
        {
            var api = new CoinGecko.CoinGecko();
            var coins = await api.GetCoinList();
            Assert.NotNull(coins);
            Assert.True(coins.Any());
        }

        [Fact]
        public async Task GetBitcoinSV_Test()
        {
            var api = new CoinGecko.CoinGecko();
            var bsv = (await api.GetCoinList()).First(x => x.Symbol == "bsv");
            Assert.Equal("bitcoin-cash-sv", bsv.Id);
            Assert.Equal("bsv", bsv.Symbol);
            Assert.Equal("Bitcoin SV", bsv.Name);
        }

        [Fact]
        public async Task GetCurrentData_Test()
        {
            var api = new CoinGecko.CoinGecko();
            var bsv = (await api.GetCoinList()).First(x => x.Symbol == "bsv");
            var data = await api.GetCurrentData(bsv.Id);
            Assert.NotNull(data.MarketData.PriceChange24HInCurrency);
        }
    }
}
