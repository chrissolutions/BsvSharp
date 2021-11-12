using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class MerkleBranch
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("pos")]
        public string Position { get; set; }
    }
}
