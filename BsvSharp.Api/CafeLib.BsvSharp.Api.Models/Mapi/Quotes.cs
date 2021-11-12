using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models.Mapi
{
    public class Quotes
    {
        [JsonProperty("quotes")]
        public ProviderQuote[] ProviderQuotes { get; set; }
    }
}
