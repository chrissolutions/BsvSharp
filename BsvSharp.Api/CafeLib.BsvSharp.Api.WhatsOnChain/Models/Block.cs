using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Block
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("versionHex")]
        public string VersionHex { get; set; }

        [JsonProperty("merkleroot")]
        public string MerkleRoot { get; set; }

        [JsonProperty("txcount")]
        public long TransactionCount { get; set; }

        [JsonProperty("tx")]
        public string[] Transactions { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("mediantime")]
        public long MedianTime { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("bits")]
        public string Bits { get; set; }

        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        [JsonProperty("chainwork")]
        public string Chainwork { get; set; }

        [JsonProperty("previousblockhash")]
        public string PreviousBlockHash { get; set; }

        [JsonProperty("nextblockhash")]
        public string NextBlockHash { get; set; }

        [JsonProperty("coinbaseTx")]
        public Coinbase Coinbase { get; set; }

        [JsonProperty("totalFees")]
        public double TotalFees { get; set; }

        [JsonProperty("miner")]
        public string Miner { get; set; }
    }
}
