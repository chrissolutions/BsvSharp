#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Units 
{
    public enum ExchangeUnit
    {
        // "Crypto" currency symbols
        BSV = 1,
        BCH = 2,
        BTC = 3,
        ETH = 4,

        // "Fiat" ISO 4217 currency symbols https://en.wikipedia.org/wiki/ISO_4217
        USD = 840,
        GBP = 826,
        EUR = 978,

        // NULL exchange unit.
        NULL = -1
    }
}
