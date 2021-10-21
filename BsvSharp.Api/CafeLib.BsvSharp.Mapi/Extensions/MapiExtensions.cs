using System.Linq;
using CafeLib.BsvSharp.Mapi.Models;

namespace CafeLib.BsvSharp.Mapi.Extensions
{
    public static class MapiExtensions
    {
        public static FeeRate GetStandardMiningFee(this FeeQuote quote) 
            => quote.Fees?.SingleOrDefault(x => x.FeeType == "standard")?.MiningFee;

        public static FeeRate GetDataMiningFee(this FeeQuote quote)
            => quote.Fees?.SingleOrDefault(x => x.FeeType == "data")?.MiningFee;

        public static FeeRate GetStandardRelayFee(this FeeQuote quote)
            => quote.Fees?.SingleOrDefault(x => x.FeeType == "standard")?.RelayFee;

        public static FeeRate GetDataRelayFee(this FeeQuote quote)
            => quote.Fees?.SingleOrDefault(x => x.FeeType == "data")?.RelayFee;
    }
}
