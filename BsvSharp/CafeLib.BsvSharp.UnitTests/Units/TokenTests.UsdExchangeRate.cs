#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Units;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public partial class TokenTests
    {
        /// <summary>
        /// Represent the exchange rate of one currency to another at a specific moment in time.
        /// </summary>
        public sealed class UsdExchangeRate : ExchangeRate
        {
            public static readonly UsdExchangeRate Default = new();

            public UsdExchangeRate()
                : base(ExchangeUnit.USD, ExchangeUnit.USD, 1)
            {
            }

            public UsdExchangeRate(ExchangeUnit exchangeUnit, decimal rate, DateTime? timestamp = null)
                : base(ExchangeUnit.USD, exchangeUnit, rate, timestamp)

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
}
