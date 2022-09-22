﻿using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public class TokenTests
    {
        //[Theory]
        //[InlineData(ExchangeUnit.BSV, ExchangeUnit.USD, 50, 1)]
        //[InlineData(ExchangeUnit.BSV, ExchangeUnit.BTC, 400, 1)]
        //public void ExchangeRate_Test(ExchangeUnit domestic, ExchangeUnit foreign, decimal rate, decimal ratio)
        //{
        //    var exchangeRate = new ExchangeRate(domestic, foreign, rate);
        //    Assert.Equal(domestic, exchangeRate.Domestic);
        //    Assert.Equal(foreign, exchangeRate.Foreign);
        //    Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
        //    Assert.Equal(ratio, exchangeRate.ToDomesticUnits(rate));
        //}

        //[Fact]
        //public void ExchangeRate_Default_Test()
        //{
        //    var defaultRate = ExchangeRate.Default;
        //    Assert.Equal(ExchangeUnit.USD, defaultRate.Domestic);
        //    Assert.Equal(ExchangeUnit.USD, defaultRate.Foreign);
        //    Assert.Equal(1, defaultRate.Rate);
        //}

        //[Theory]
        //[InlineData(ExchangeUnit.USD, 50, 1)]
        //[InlineData(ExchangeUnit.BTC, 400, 1)]
        //public void BsvExchangeRate_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        //{
        //    var exchangeRate = new BsvExchangeRate(foreign, rate);
        //    Assert.Equal(foreign, exchangeRate.Foreign);
        //    Assert.Equal(rate, exchangeRate.ToForeignUnits(ratio));
        //}

        //[Theory]
        //[InlineData(ExchangeUnit.USD, 50, 1)]
        //[InlineData(ExchangeUnit.BTC, 400, 1)]
        //public void Bsv_Amount_Exchange_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        //{
        //    var exchangeRate = new BsvExchangeRate(foreign, rate);
        //    var amount = exchangeRate.ToAmount(rate);
        //    Assert.Equal(ratio, amount.ToBitcoin());
        //}

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 1)]
        public void Token_Test(ExchangeUnit foreign, decimal rate, decimal ratio)
        {
            var token = new Token();
            var exchangeRate = new BsvExchangeRate(foreign, rate);

            token.SetRate(exchangeRate);
            token.SetFiatValue(500);

            var defaultRate = BsvExchangeRate.Default;
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Domestic);
            Assert.Equal(ExchangeUnit.BSV, defaultRate.Foreign);
            Assert.Equal(1, defaultRate.Rate);
        }
    }
}