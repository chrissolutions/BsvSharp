using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class Envelope
    {
        [JsonProperty("payload")]
        public string JsonPayload { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }
    }
}
