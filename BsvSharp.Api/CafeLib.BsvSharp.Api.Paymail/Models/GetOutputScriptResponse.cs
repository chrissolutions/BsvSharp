using System;
using CafeLib.BsvSharp.Scripting;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Api.Paymail.Models
{
    public record GetOutputScriptResponse : GetScriptResponse
    {
        public GetOutputScriptResponse()
        {
        }

        public GetOutputScriptResponse(bool successful)
            : base(successful) { }

        internal GetOutputScriptResponse(GetScriptResponse response)
            : base(response, null) { }

        public GetOutputScriptResponse(Exception ex)
            : base(ex) { }

        public Script Script { get; init; }
    }
}
