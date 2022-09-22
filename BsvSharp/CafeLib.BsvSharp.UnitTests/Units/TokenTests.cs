using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public class TokenTests
    {
        [Theory]
        [InlineData(ExchangeUnit.USD, 50, 10, 500)]
        [InlineData(ExchangeUnit.BTC, .0025, 400, 1)]
        [InlineData(ExchangeUnit.BTC, .0025, 500, 1.25)]
        public void Token_Test(ExchangeUnit foreign, decimal rate, decimal tokenQuantity, decimal bitcoin)
        {
            var token = new Token();
            var exchangeRate = new BsvExchangeRate(foreign, rate);

            token.SetExchangeRate(exchangeRate);
            token.SetTokenQuantity(tokenQuantity);

            Assert.Equal(bitcoin, token.Amount.ToBitcoin()) ;
        }
    }
}