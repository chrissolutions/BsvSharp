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
        public void Amount_Unit_Tests(long amountSatoshis, decimal unitAmount, BitcoinUnit unit)
        {
            var amount = new Amount(unitAmount, unit);
            Assert.Equal(amountSatoshis, (long)amount);
        }

        [Theory]
        [InlineData(1000000L, 1000000, BitcoinUnit.Satoshi)]
        [InlineData(1000000L, 10000, BitcoinUnit.Bit)]
        [InlineData(1000000L, 2000, BitcoinUnit.Duro)]
        [InlineData(1000000L, 10, BitcoinUnit.MilliBitcoin)]
        [InlineData(1000000L, .01, BitcoinUnit.Bitcoin)]
        public void Amount_ToBitcoinUnit_Tests(long amountSatoshis, decimal unitAmount, BitcoinUnit unit)
        {
            var amount = new Amount(unitAmount, unit);
            var value = amount.ToBitcoinUnit(unit);
            Assert.Equal(amountSatoshis, (long)amount);
            Assert.Equal(unitAmount, value);
        }

        [Theory]
        [InlineData(1_000_000L, .01)]
        [InlineData(100_000_000L, 1)]
        [InlineData(500L, .000005)]
        [InlineData(1L, .00000001)]
        public void Amount_ToBitcoin_Tests(long amountSatoshis, decimal bitcoinAmount)
        {
            var amount = new Amount(amountSatoshis);
            var value = amount.ToBitcoin();
            Assert.Equal(bitcoinAmount, value);
        }
    }
}