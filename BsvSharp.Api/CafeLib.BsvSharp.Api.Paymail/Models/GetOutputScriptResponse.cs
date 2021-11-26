using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    internal class GetOutputScriptResponse
    {
        [JsonProperty("output")]
        public string Output { get; set; }
    }
}
