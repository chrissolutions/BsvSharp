using CafeLib.BsvSharp.Keys;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record GetPublicKeyResponse : GetIdentityResponse
    {
        public GetPublicKeyResponse(bool successful = true)
            : base(successful) { }

        public GetPublicKeyResponse(Exception ex)
            : base(ex) { }

        internal GetPublicKeyResponse(GetIdentityResponse response, Func<bool> successful)
            : base(response, successful) { }
    }
}
