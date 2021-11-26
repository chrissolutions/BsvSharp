using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Mapi.Models
{
    public class TransactionStatus : Cargo
    {
        [JsonProperty("returnResult")]
        public string ReturnResult { get; set; }

        [JsonProperty("resultDescription")]
        public string ResultDescription { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("blockHeight")]
        public int? BlockHeight { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("txSecondMempoolExpiry")]
        public int TxSecondMempoolExpiry { get; set; }
    }

}
