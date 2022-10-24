using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record VerifyPublicKeyResponse : PaymailResponse
    {
        public VerifyPublicKeyResponse(bool successful = true)
            : base(successful) { }

        internal VerifyPublicKeyResponse(VerifyPublicKeyResponse response, Func<bool> successful)
            : base(successful)
        {
            Handle = response.Handle;
            PublicKey = response.PublicKey;
            Match = response.Match;
        }

        public VerifyPublicKeyResponse(Exception ex)
            : base(ex) { }

        [JsonProperty("handle")]
        public string Handle { get; init; }

        [JsonProperty("pubkey")]
        public string PublicKey { get; init; }

        [JsonProperty("match")]
        public bool Match { get; init; }
    }
}
