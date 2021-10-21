using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class TransactionSubmit : Cargo
    {
        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("returnResult")]
        public string ReturnResult { get; set; }

        [JsonProperty("resultDescription")]
        public string ResultDescription { get; set; }

        [JsonProperty("currentHighestBlockHash")]
        public string CurrentHighestBlockHash { get; set; }

        [JsonProperty("currentHighestBlockHeight")]
        public int CurrentHighestBlockHeight { get; set; }

        [JsonProperty("txSecondMempoolExpiry")]
        public int TxSecondMempoolExpiry { get; set; }
    }
}
