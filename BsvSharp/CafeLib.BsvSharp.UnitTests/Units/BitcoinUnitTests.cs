using CafeLib.BsvSharp.Units;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public class BitcoinUnitTests
    {
        [Theory]
        [InlineData(1000000L, 1000000, BitcoinUnit.Satoshi)]
        [InlineData(1000000L, 10000, BitcoinUnit.Bit)]
        [InlineData(1000000L, 2000, BitcoinUnit.Duro)]
        [InlineData(1000000L, 10, BitcoinUnit.MilliBitcoin)]
        [InlineData(1000000L, .01, BitcoinUnit.Bitcoin)]
        public void Amount_Unit_Tests(long expectedAmount, decimal amountInUnit, BitcoinUnit unit)
        {
            var amount = new Amount(amountInUnit, unit);
            Assert.Equal(expectedAmount, (long)amount);
        }
    }
}