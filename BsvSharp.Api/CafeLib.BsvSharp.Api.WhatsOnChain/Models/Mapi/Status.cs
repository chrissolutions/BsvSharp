using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models.Mapi
{
    public class Status
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("returnResult")]
        public string ReturnResult { get; set; }

        [JsonProperty("resultDescription")]
        public string ResultDescription { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("blockHeight")]
        public long BlockHeight { get; set; }

        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }

        [JsonProperty("minerId")]
        public string MinerId { get; set; }

        [JsonProperty("txSecondMempoolExpiry")]
        public long TxSecondMempoolExpiry { get; set; }
    }
}
