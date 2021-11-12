using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Models.Mapi
{
    public class TransactionPayload
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("returnResult")]
        public string ReturnResult { get; set; }

        [JsonProperty("resultDescription")]
        public string ResultDescription { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("blockHeight")]
        public long BlockHeight { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("minerId")]
        public string MinerId { get; set; }

        [JsonProperty("txSecondMempoolExpiry")]
        public int TxSecondMempoolExpiry { get; set; }
    }
}
