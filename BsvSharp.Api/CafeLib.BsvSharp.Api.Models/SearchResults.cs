using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class SearchResults
    {
        [JsonProperty("results")]
        public ExplorerLink[] Links { get; set; }
    }
}
