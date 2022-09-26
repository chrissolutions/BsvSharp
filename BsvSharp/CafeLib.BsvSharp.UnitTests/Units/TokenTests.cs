using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public partial class TokenTests
    {
        [Fact]
        public void Token_Clear_Amount_Test()
        {
            var token = new BsvToken(new Amount(100.0M));
            Assert.True(token.HasAmount);
            Assert.Equal(100, token.Amount);
            token.ClearAmount();
            Assert.False(token.HasAmount);
        }

        [Fact]
        public void Token_Clear_ExchangeRate_Test()
        {
            var token = new BsvToken();
            Assert.False(token.HasRate);
            var exchangeRate = new BsvExchangeRate(ExchangeUnit.USD, 50);
            token.SetExchangeRate(exchangeRate);
            Assert.True(token.HasRate);
            token.ClearExchangeRate();
            Assert.False(token.HasRate);
        }

        [Fact]
        public void Token_Clear_TokenQuantity_Test()
        {
            var token = new BsvToken();
            Assert.False(token.HasQuantity);
            var _ = new BsvExchangeRate(ExchangeUnit.USD, 50);
            token.SetQuantity(500);
            Assert.True(token.HasQuantity);
            token.ClearQuantity();
            Assert.False(token.HasQuantity);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 10, 500)]
        [InlineData(ExchangeUnit.BTC, .0025, 400, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Token_Compute_Amount_Test(ExchangeUnit foreignUnit, decimal rate, decimal bitcoins, decimal foreignAmount)
        {
            var token = new BsvToken();
            var exchangeRate = new BsvExchangeRate(foreignUnit, rate);

            token.SetExchangeRate(exchangeRate);
            token.SetQuantity(bitcoins);

            Assert.Equal(foreignAmount, token.Amount);
            Assert.True(token.HasComputedAmount);
            Assert.True(token.HasAll);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 10, 500)]
        [InlineData(ExchangeUnit.BTC, .0025, 400, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Token_Compute_Quantity_Test(ExchangeUnit foreignUnit, decimal rate, decimal bitcoins, decimal foreignAmount)
        {
            var token = new BsvToken(new Amount(foreignAmount));
            Assert.True(token.HasAmount);
            Assert.False(token.HasComputedQuantity);
            var exchangeRate = new BsvExchangeRate(foreignUnit, rate);
            token.SetExchangeRate(exchangeRate);
            Assert.True(token.HasComputedQuantity);
            Assert.Equal(bitcoins, token.Quantity);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 10, 500)]
        [InlineData(ExchangeUnit.BTC, .0025, 400, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Token_Compute_Rate_Test(ExchangeUnit foreignUnit, decimal rate, decimal bitcoins, decimal foreignAmount)
        {
            var token = new BsvToken(new Amount(foreignAmount));
            Assert.True(token.HasAmount);
            Assert.False(token.HasComputedRate);
            token.SetQuantity(bitcoins, foreignUnit);
            Assert.True(token.HasComputedRate);
            Assert.Equal(foreignUnit, token.ExchangeUnit);
            Assert.Equal(rate, token.ExchangeRate.Rate);
        }

        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 10, 500)]
        [InlineData(ExchangeUnit.BTC, .0025, 400, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Token_Compute_All_Test(ExchangeUnit foreignUnit, decimal rate, decimal bitcoins, decimal foreignAmount)
        {
            var token = new BsvToken(new Amount(foreignAmount), new BsvExchangeRate(foreignUnit, rate), bitcoins);
            Assert.True(token.HasAll);
            Assert.Equal(foreignUnit, token.ExchangeUnit);
            Assert.Equal(rate, token.ExchangeRate.Rate);
            Assert.Equal(foreignAmount, token.Amount);
            Assert.Equal(bitcoins, token.Quantity);
        }

        [Theory]
        [InlineData(ExchangeUnit.BSV, .02, 500, 10)]
        [InlineData(ExchangeUnit.BTC, 0.00005, 20000, 1)]
        [InlineData(ExchangeUnit.BTC, 0.00005, 25000, 1.25)]
        public void Token_Compute_Amount_From_Usd_Test(ExchangeUnit foreignUnit, decimal rate, decimal usd, decimal foreignAmount)
        {
            var token = new UsdToken();
            var exchangeRate = new UsdExchangeRate(foreignUnit, rate);

            token.SetExchangeRate(exchangeRate);
            token.SetQuantity(usd);

            Assert.Equal(foreignAmount, token.Amount);
            Assert.True(token.HasComputedAmount);
            Assert.True(token.HasAll);
        }
    }
}