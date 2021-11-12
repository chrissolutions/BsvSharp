using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models
{
    public class Utxo
    {
        [JsonProperty("flag")]
        public long Page;

        [JsonProperty("address")]
        public string Address;

        [JsonProperty("txid")]
        public string TxId;

        [JsonProperty("outIndex")]
        public long OutIndex;

        [JsonProperty("tx_pos")]
        public int TransactionPosition;

        [JsonProperty("tx_hash")]
        public string TransactionHash;

        [JsonProperty("value")]
        public long Value;

        [JsonProperty("height")]
        public int Height;
    }
}