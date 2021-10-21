using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Coinbase
    {
        [JsonProperty("hex")]
        public string Hex { get; set; }

        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("locktime")]
        public long LockTime { get; set; }

        [JsonProperty("vin")]
        public Input[] Vin { get; set; }

        [JsonProperty("vout")]
        public Output[] Vout { get; set; }

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

        [JsonProperty("totalFees")]
        public double TotalFees { get; set; }

        [JsonProperty("miner")]
        public string Miner { get; set; }
    }
}
