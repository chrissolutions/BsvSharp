using System;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record VerifyPublicKeyResponseBase
    {
        [JsonProperty("handle")]
        public string Handle { get; init; }

        [JsonProperty("pubkey")]
        public string PublicKey { get; init; }

        [JsonProperty("match")]
        public bool Match { get; init; }
    }
}
