#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Diagnostics.CodeAnalysis;

namespace CafeLib.BsvSharp.Units 
{
    /// <summary>
    /// Represent the exchange rate of one currency to another at a specific moment in time.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class BsvExchangeRate : ExchangeRate
    {
        private BsvExchangeRate()
        {
        }

        public BsvExchangeRate(CurrencyTicker toTicker, decimal rate, DateTime? timestamp = null)
            : base(CurrencyTicker.BSV, toTicker, rate, timestamp)

        {
        }

        public BsvExchangeRate(CurrencyTicker ofTicker, CurrencyTicker toTicker, decimal rate, DateTime? timestamp = null) : base(ofTicker, toTicker, rate, timestamp)
        {
        }

        /// <summary>
        /// Multiplying <paramref name="ofValue"/> in OfTicker units by Rate returns value in ToTicker units.
        /// </summary>
        /// <param name="ofAmount">Multiplied by Rate to return value in ToTicker units.</param>
        /// <returns>Returns <paramref name="ofValue"/> in ToTicker units.</returns>
        public decimal ConvertOfAmount(Amount ofAmount)
        {
            return ConvertOfValue(ofAmount.Satoshis);
        }
    }
}
