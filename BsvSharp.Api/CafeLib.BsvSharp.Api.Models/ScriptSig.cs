using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class ScriptSig
    {
        [JsonProperty("asm")]
        public string Assembly { get; set; }

        [JsonProperty("hex")]
        public string Hex { get; set; }
    }
}