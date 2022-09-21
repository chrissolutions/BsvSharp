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
    public class ExchangeRate
    {
        protected ExchangeRate()
        {
        }

        public ExchangeRate(
            CurrencyTicker ofTicker, 
            CurrencyTicker toTicker, 
            decimal rate, 
            DateTime? timestamp = null)
        {
            OfTicker = ofTicker;
            ToTicker = toTicker;
            Rate = rate;
            TimeStamp = timestamp ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Multiplying a value in OfTicker units by Rate yields value in ToTicker units.
        /// </summary>
        public CurrencyTicker OfTicker { get; set; }
        /// <summary>
        /// Multiplying a value in OfTicker units by Rate yields value in ToTicker units.
        /// </summary>
        public CurrencyTicker ToTicker { get; set; }

        /// <summary>
        /// When this exchange rate was observed.
        /// </summary>
        public DateTime TimeStamp { get; set; }
        
        /// <summary>
        /// Rate is dimensionally ToTicker units divided by OfTicker units.
        /// Multiplying a value in OfTicker units by Rate yields value in ToTicker units.
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Was in KzWdbExchangeRate class but started drawing an error...
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Dividing <paramref name="toValue"/> in ToTicker units by Rate returns value in OfTicker units.
        /// </summary>
        /// <param name="toValue"> divided by Rate to return value in OfTicker units.</param>
        /// <returns>Returns <paramref name="toValue"/> in OfTicker units.</returns>
        public decimal ConvertToValue(decimal toValue) => toValue / Rate;

        /// <summary>
        /// Multiplying <paramref name="ofValue"/> in OfTicker units by Rate returns value in ToTicker units.
        /// </summary>
        /// <param name="ofValue">Multiplied by Rate to return value in ToTicker units.</param>
        /// <returns>Returns <paramref name="ofValue"/> in ToTicker units.</returns>
        public decimal ConvertOfValue(decimal ofValue) => ofValue * Rate;

        ///// <summary>
        ///// Multiplying <paramref name="ofValue"/> in OfTicker units by Rate returns value in ToTicker units.
        ///// </summary>
        ///// <param name="ofValue">Multiplied by Rate to return value in ToTicker units.</param>
        ///// <returns>Returns <paramref name="ofValue"/> in ToTicker units.</returns>
        public decimal ConvertOfValue(Amount ofAmount)
        {
            CheckOfTickerIsBSV();
            return Rate * ofAmount.Satoshis / (decimal)KzBitcoinUnit.BSV;
        }

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if OfTicker is not BSV.
        /// </summary>
        /// <exception cref="InvalidOperationException">If OfTicker is not BSV.</exception>
        public void CheckOfTickerIsBSV() {
            if (OfTicker != CurrencyTicker.BSV)
                throw new InvalidOperationException("OfTicker is not BSV");
        }
    }
}
