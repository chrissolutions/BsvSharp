using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class Cargo
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("minerId")]
        public string MinerId { get; set; }
    }
}
