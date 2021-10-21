using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class OpReturn
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("parts")]
        public string Parts { get; set; }
    }
}
