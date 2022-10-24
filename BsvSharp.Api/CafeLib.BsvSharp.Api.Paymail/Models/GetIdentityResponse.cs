using Newtonsoft.Json;
using System;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record GetIdentityResponse : PaymailResponse
    {
        public GetIdentityResponse() { }

        public GetIdentityResponse(bool successful = true)
            : base(successful) { }

        internal GetIdentityResponse(GetIdentityResponse response, Func<bool> successful)
            : base(successful)
        {
            BsvAlias = response.BsvAlias;
            Handle = response.Handle;
            PubKey = response.PubKey;
        }

        public GetIdentityResponse(Exception ex)
            : base(ex) { }

        [JsonProperty("bsvalias")]
        public string BsvAlias { get; init; }

        [JsonProperty("handle")]
        public string Handle { get; init; }

        [JsonProperty("pubkey")]
        public string PubKey { get; init; }
    }
}
