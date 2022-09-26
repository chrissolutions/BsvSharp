#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Units 
{
    public enum ExchangeUnit
    {
        // "Fiat" ISO 4217 currency codes https://en.wikipedia.org/wiki/ISO_4217
        FIAT = 000,
        AUD = 036,          // Australian Dollar
        BRL = 986,          // Brazilian Real
        CAD = 124,          // Canadian Dollar
        CHF = 756,          // Swiss Franc
        CLP = 152,          // Chilean Peso
        CNY = 156,          // China Yuan Renminbi
        DKK = 208,          // Danish Krone
        EUR = 978,          // Euro
        FJD = 242,          // Fiji Dollar
        GBP = 826,          // Great Britain Pound Sterling
        HKD = 344,          // Hong Kong Dollar
        IDR = 360,          // Indonesian Rupiah
        ILS = 376,          // Israeli Sheqel
        INR = 356,          // Indian Rupee
        IRR = 364,          // Iranian Rial
        ISK = 352,          // Iceland Krona
        JMD = 388,          // Jamaican Dollar
        JPY = 392,          // Japanese Yen
        KPW = 408,          // North Korea Won
        KRW = 410,          // South Korea Won
        MAD = 504,          // Moroccan Dirham
        MXN = 484,          // Mexican Peso
        NOK = 578,          // Norwegian Krone    
        NZD = 554,          // New Zealand Dollar
        PHP = 608,          // Philippine Peso
        PKR = 586,          // Pakistan Rupee
        RUB = 643,          // Russian Ruble
        SAR = 682,          // Saudi Riyal
        SEK = 752,          // Swedish Krona
        SGD = 702,          // Singapore Dollar
        TBH = 764,          // Thai Baht
        TTD = 780,          // Trinidad and Tobago Dollar
        UGX = 800,          // Uganda Shilling
        USD = 840,          // U.S. Dollar
        VUV = 548,          // Vanuatu Vatu
        XCD = 978,          // East Caribbean Dollar
        ZAR = 710,          // South African Rand

        // No currency.
        XXX = 999,

        // "Crypto" currency codes
        CRYPTO = 1000,
        BSV = 1001,
        BCH = 1002,
        BTC = 1003,
        ETH = 1004,
        LTC = 1005,

        // NULL exchange unit.
        NULL = -1
    }
}
