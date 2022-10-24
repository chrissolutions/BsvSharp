using System;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record GetPublicKeyResponse : GetIdentityResponse
    {
        public GetPublicKeyResponse() { }

        public GetPublicKeyResponse(bool successful)
            : base(successful) { }

        public GetPublicKeyResponse(Exception ex)
            : base(ex) { }

        internal GetPublicKeyResponse(GetIdentityResponse response, Func<bool> successful)
            : base(response, successful) { }
    }
}
