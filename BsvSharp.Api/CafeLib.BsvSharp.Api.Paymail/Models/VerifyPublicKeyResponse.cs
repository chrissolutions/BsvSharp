using System;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record VerifyPublicKeyResponse : VerifyPublicKeyResponseBase
    {
        public VerifyPublicKeyResponse() { }

        public VerifyPublicKeyResponse(VerifyPublicKeyResponseBase response)
            : base(response) { }

        public Exception Exception { get; init; }

        public bool IsValid { get; init; }
    }
}
