using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class ScriptUtxo
    {
        [JsonProperty("script")]
        public string Script { get; set; }

        [JsonProperty("unspent")]
        public Utxo[] Utxos { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
