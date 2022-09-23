﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
// ReSharper disable UnusedMember.Local

namespace CafeLib.BsvSharp.Units 
{
    /// <summary>
    /// Represent the exchange rate of one currency to another at a specific moment in time.
    /// </summary>
    public sealed record BsvExchangeRate : ExchangeRate
    {
        public new static readonly BsvExchangeRate Default = new();

        private BsvExchangeRate()
            : base(ExchangeUnit.BSV, ExchangeUnit.BSV, 1)
        {
        }

        public BsvExchangeRate(ExchangeUnit exchangeUnit, decimal rate, DateTime? timestamp = null)
            : base(ExchangeUnit.BSV, exchangeUnit, rate, timestamp)

        {
        }

        /// <summary>
        /// Convert BSV amount to the exchange units.
        /// </summary>
        /// <param name="amount">BSV amount</param>
        /// <returns>Return the exchange units.</returns>
        public decimal ToExchangeUnits(Amount amount)
        {
            return ToForeignUnits(amount.ToBitcoin());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Amount ToAmount(decimal value)
        {
            return new Amount(ToDomesticUnits(value), BitcoinUnit.Bitcoin);
        }
    }
}