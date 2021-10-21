using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class Quotes
    {
        [JsonProperty("quotes")]
        public ProviderQuote[] ProviderQuotes { get; set; }
    }
}
