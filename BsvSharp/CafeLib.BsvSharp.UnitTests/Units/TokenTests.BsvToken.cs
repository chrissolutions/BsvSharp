#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Units;

namespace CafeLib.BsvSharp.UnitTests.Units
{
    public partial class TokenTests
    {
        /// <summary>
        /// Test Vector
        /// </summary>
        public class BsvToken : Token<BsvExchangeRate>
        {
            public BsvToken()
            {
            }

            public BsvToken(Amount amount)
                : base(amount)
            {
            }

            public BsvToken(BsvExchangeRate exchangeRate, decimal tokenQuantity)
                : base(exchangeRate, tokenQuantity)
            {
            }

            public BsvToken(Amount amount, BsvExchangeRate exchangeRate, decimal tokenQuantity)
                : base(amount, exchangeRate, tokenQuantity)
            {
            }
        }
    }
}
