using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class MerkleNode
    {
        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("branches")]
        public MerkleBranch[] Branches { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("merkleRoot")]
        public string MerkleRoot { get; set; }
    }
}
