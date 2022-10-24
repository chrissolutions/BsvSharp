using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record GetScriptResponse : PaymailResponse
    {
        public GetScriptResponse()
        {
        }

        public GetScriptResponse(bool successful)
            : base(successful) { }

        public GetScriptResponse(Exception ex)
            : base(ex) { }

        [JsonProperty("output")]
        public string Output { get; set; }


        internal GetScriptResponse(GetScriptResponse response, Func<bool> successful)
            : base(successful)
        {
            Output = response.Output;
        }
    }
}
