using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class MerkleBranch
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("pos")]
        public string Position { get; set; }
    }
}
