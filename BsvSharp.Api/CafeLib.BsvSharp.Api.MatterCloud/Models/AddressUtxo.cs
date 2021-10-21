using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.WhatsOnChain.Models
{
    public class AddressUtxo
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("unspent")]
        public Utxo[] Utxos { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
