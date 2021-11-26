using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Output
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("n")]
        public int Index { get; set; }

        [JsonProperty("scriptPubKey")]
        public ScriptPubKey ScriptPubKey { get; set; }
    }
}