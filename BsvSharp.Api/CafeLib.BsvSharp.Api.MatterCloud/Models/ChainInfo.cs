using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class ChainInfo
    {
        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("blocks")]
        public long Blocks { get; set; }

        [JsonProperty("headers")]
        public long Headers { get; set; }

        [JsonProperty("bestblockhash")]
        public string BestBlockHash { get; set; }

        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        [JsonProperty("mediantime")]
        public long MedianTime { get; set; }

        [JsonProperty("verificationprogress")]
        public double VerificationProgress { get; set; }

        [JsonProperty("pruned")]
        public bool Pruned { get; set; }

        [JsonProperty("chainwork")]
        public string ChainWork { get; set; }
    }
}
