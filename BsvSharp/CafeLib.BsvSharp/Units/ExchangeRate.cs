#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Units 
{
    /// <summary>
    /// Represent the exchange rate of one currency to another at a specific moment in time.
    /// </summary>
    public record ExchangeRate() : IExchangeRate
    {
        public static readonly ExchangeRate Default = new();

        public ExchangeRate(
            ExchangeUnit domesticUnit, 
            ExchangeUnit foreignUnit, 
            decimal rate, 
            DateTime? timestamp = null)
            : this()
        {
            Domestic = domesticUnit;
            Foreign = foreignUnit;
            Rate = rate;
            Timestamp = timestamp ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Was in KzWdbExchangeRate class but started drawing an error...
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Multiplying a value in Domestic units by Rate yields value in Foreign units.
        /// </summary>
        public ExchangeUnit Domestic { get; init; } = ExchangeUnit.USD;

        /// <summary>
        /// Multiplying a value in Foreign units by Rate yields value in Domestic units.
        /// </summary>
        public ExchangeUnit Foreign { get; init; } = ExchangeUnit.USD;

        /// <summary>
        /// When this exchange rate was observed.
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Rate is dimensionally Foreign units divided by Domestic units.
        /// Multiplying a value in Domestic units by Rate yields value in Foreign units.
        /// </summary>
        public decimal Rate { get; init; } = 1;

        /// <summary>
        /// Divide foreign units by rate to convert into domestic units.
        /// </summary>
        /// <param name="foreignValue"> divided by Rate to return value in OfTicker units.</param>
        /// <returns>value in domestic units.</returns>
        public decimal ToDomesticUnits(decimal foreignValue) => foreignValue / Rate;

        /// <summary>
        /// Multiply domestic units by rate to convert into foreign units.
        /// </summary>
        /// <param name="domesticValue">Multiplied by Rate to return value in foreign units.</param>
        /// <returns>value in foreign units.</returns>
        public decimal ToForeignUnits(decimal domesticValue) => domesticValue * Rate;
    }
}
