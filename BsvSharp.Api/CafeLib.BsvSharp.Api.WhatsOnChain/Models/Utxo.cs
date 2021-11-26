using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class Utxo
    {
        [JsonProperty("height")]
        public int Height;

        [JsonProperty("tx_pos")]
        public int TransactionPosition;

        [JsonProperty("tx_hash")]
        public string TransactionHash;

        [JsonProperty("value")]
        public long Value;
    }
}