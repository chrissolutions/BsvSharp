using System.Diagnostics.CodeAnalysis;

namespace CafeLib.BsvSharp.Units 
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum CurrencyTicker 
    {
        // "Crypto" currency symbols
        BSV = 1,
        BCH = 2,
        BTC = 3,
        ETH = 4,

        // "Fiat" ISO 4217 currency symbols
        USD = 1001,
        GBP = 1002,
        EUR = 1003,
    }
}
