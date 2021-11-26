using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class FeeRates
    {
        [JsonProperty("satoshis")]
        public long Satoshis { get; set; }

        [JsonProperty("bytes")]
        public long Bytes { get; set; }
    }
}
