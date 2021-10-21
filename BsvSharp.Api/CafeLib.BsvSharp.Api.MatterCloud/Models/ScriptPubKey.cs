using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class ScriptPubKey
    {
        [JsonProperty("asm")]
        public string Assembly { get; set; }

        [JsonProperty("hex")]
        public string Hex { get; set; }

        [JsonProperty("reqSigs")]
        public int RequiredSignatureCount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("addresses")]
        public string[] Addresses { get; set; }

        [JsonProperty("opReturn")]
        public OpReturn OpReturn { get; set; }

        [JsonProperty("isTruncated")]
        public bool IsTruncated { get; set; }
    }
}
