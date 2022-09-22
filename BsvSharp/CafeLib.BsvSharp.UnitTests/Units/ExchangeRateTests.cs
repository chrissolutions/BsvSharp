using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public class ExchangeRateTests
    {
        [Theory]
        [InlineData(ExchangeUnit.BSV, ExchangeUnit.USD, 50, 1)]
        [InlineData(ExchangeUnit.BSV, ExchangeUnit.BTC, .0025, 1)]
        public void ExchangeRate_Test(ExchangeUnit domestic, ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var exchangeRate = new ExchangeRate(domestic, foreign, rate);
            Assert.Equal(domestic, exchangeRate.Domestic);
            Assert.Equal(foreign, exchangeRate.Foreign);
            Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
            Assert.Equal(ratio, exchangeRate.ToDomesticUnits(rate));
        }

        [Fact]
        public void ExchangeRate_Default_Test()
        {
            var defaultRate = ExchangeRate.Default;
            Assert.Equal(ExchangeUnit.USD, defaultRate.Domestic);
            Assert.Equal(ExchangeUnit.USD, defaultRate.Foreign);
            Assert.Equal(1, defaultRate.Rate);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 1)]
        public void BsvExchangeRate_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var exchangeRate = new BsvExchangeRate(foreign, rate);
            Assert.Equal(foreign, exchangeRate.Foreign);
            Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
            Assert.Equal(ratio, exchangeRate.ToDomesticUnits(rate));
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 1)]
        public void Bsv_Amount_From_Foreign_Exchange_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var exchangeRate = new BsvExchangeRate(foreign, rate);
            var amount = exchangeRate.ToAmount(rate);
            Assert.Equal(ratio, amount.ToBitcoin());
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 100, 5000)]
        [InlineData(ExchangeUnit.BTC, .0025, 100, .25)]
        public void Bsv_Amount_To_ExchangeUnits(ExchangeUnit exchangeUnit, decimal rate, decimal bitcoin, decimal exchangeTotal)
        {
            var exchangeRate = new BsvExchangeRate(exchangeUnit, rate);
            var amount = new Amount(bitcoin, BitcoinUnit.Bitcoin);
            var exchangeUnits = exchangeRate.ToExchangeUnits(amount);
            Assert.Equal(exchangeTotal, exchangeUnits);
        }

        [Fact]
        public void BsvExchangeRate_Default_Test()
        {
            var defaultRate = BsvExchangeRate.Default;
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Domestic);
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Foreign);
            Assert.Equal(1, defaultRate.Rate);
        }
    }
}