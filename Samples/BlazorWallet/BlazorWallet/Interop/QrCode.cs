using System;
using System.Threading.Tasks;
using CafeLib.Blazor.Interop;
using CafeLib.BsvSharp.Keys;
using Microsoft.JSInterop;

namespace BlazorWallet.Interop
{
    public class QrCode : JsInteropObject
    {
        private QrCode(IJSObjectReference jsInstance)
            : base(jsInstance)
        {
        }

        public async Task Generate(Address address)
        {
            await Instance.InvokeVoidAsync("makeCode", address.ToString());
        }
    }
}