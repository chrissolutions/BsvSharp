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
        LTC = 5,

        // "Fiat" ISO 4217 currency symbols https://en.wikipedia.org/wiki/ISO_4217
        AUD = 036,
        CAD = 124,
        CNY = 156,
        EUR = 978,
        GBP = 826,
        NZD = 554,
        USD = 840,

        // NULL exchange unit.
        NULL = -1
    }
}
