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
        public void ExchangeRate_Null_Test()
        {
            IExchangeRate nullExchangeRate = ExchangeRate.Null;
            Assert.Equal(ExchangeUnit.NULL, nullExchangeRate.Domestic);
            Assert.Equal(ExchangeUnit.NULL, nullExchangeRate.Foreign);
            Assert.Equal(0, nullExchangeRate.Rate);
        }

        [Fact] public void ExchangeRate_Default_Test()
        {
            IExchangeRate defaultRate = new TokenTests.BsvExchangeRate();
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Domestic);
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Foreign);
            Assert.Equal(1, defaultRate.Rate);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 1)]
        public void BsvExchangeRate_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var exchangeRate = new TokenTests.BsvExchangeRate(foreign, rate);
            Assert.Equal(foreign, exchangeRate.Foreign);
            Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
            Assert.Equal(ratio, exchangeRate.ToDomesticUnits(rate));
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 50, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 1, 400)]
        [InlineData(ExchangeUnit.BTC, .0025, 1.25, 500)]
        public void Bsv_Amount_From_Foreign_Exchange_Test(ExchangeUnit foreign, decimal rate, decimal foreignQuantity, decimal bitcoin)
        {
            var exchangeRate = new TokenTests.BsvExchangeRate(foreign, rate);
            var amount = exchangeRate.ToAmount(foreignQuantity);
            Assert.Equal(bitcoin, amount.ToBitcoin());
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 100, 5000)]
        [InlineData(ExchangeUnit.BTC, .0025, 100, .25)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Bsv_Amount_To_ExchangeUnits(ExchangeUnit exchangeUnit, decimal rate, decimal bitcoin, decimal exchangeTotal)
        {
            var exchangeRate = new TokenTests.BsvExchangeRate(exchangeUnit, rate);
            var amount = new Amount(bitcoin, BitcoinUnit.Bitcoin);
            var exchangeUnits = exchangeRate.ToExchangeUnits(amount);
            Assert.Equal(exchangeTotal, exchangeUnits);
        }

        [Fact]
        public void BsvExchangeRate_Default_Test()
        {
            var defaultRate = TokenTests.BsvExchangeRate.Default;
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Domestic);
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Foreign);
            Assert.Equal(1, defaultRate.Rate);
        }
    }
}