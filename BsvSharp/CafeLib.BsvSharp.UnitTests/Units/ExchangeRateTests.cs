using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public class ExchangeRateTests
    {
        [Theory]
        [InlineData(ExchangeUnit.BSV, ExchangeUnit.USD, 50, 1)]
        [InlineData(ExchangeUnit.BSV, ExchangeUnit.BTC, 400, 1)]
        public void ExchangeRate_Test(ExchangeUnit domestic, ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var exchangeRate = new ExchangeRate(domestic, foreign, rate);
            Assert.Equal(domestic, exchangeRate.Domestic);
            Assert.Equal(foreign, exchangeRate.Foreign);
            Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
            Assert.Equal(ratio, exchangeRate.ToDomesticUnits(rate));
        }

        //[Fact]
        //public void ExchangeRate_Test()
        //{
        //    var exchangeRate = new ExchangeRate(ExchangeUnit.BSV, ExchangeUnit.USD, 50);
        //    Assert.Equal(50, exchangeRate.ToForeignUnits(1));
        //}
    }
}