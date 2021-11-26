using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Mempool
    {
        [JsonProperty("bytes")]
        public long Bytes { get; set; }

        [JsonProperty("maxmempool")]
        public string MaxMempool { get; set; }

        [JsonProperty("mempoolminfee")]
        public long MinimumFee { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("usage")]
        public long Usage { get; set; }
    }
}
