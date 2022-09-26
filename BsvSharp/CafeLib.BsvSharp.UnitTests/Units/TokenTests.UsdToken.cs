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
        public class UsdToken : Token<UsdExchangeRate>
        {
            public UsdToken()
            {
            }

            public UsdToken(Amount amount)
                : base(amount)
            {
            }

            public UsdToken(UsdExchangeRate exchangeRate, decimal tokenQuantity)
                : base(exchangeRate, tokenQuantity)
            {
            }

            public UsdToken(Amount amount, UsdExchangeRate exchangeRate, decimal tokenQuantity)
                : base(amount, exchangeRate, tokenQuantity)
            {
            }
        }
    }
}
