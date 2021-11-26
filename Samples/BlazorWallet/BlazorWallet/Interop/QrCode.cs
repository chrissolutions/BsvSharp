using System;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Keys;
using Microsoft.JSInterop;

namespace BlazorWallet.Interop
{
    public class QrCode
    {
        private readonly IJSObjectReference _jsInstance;

        private QrCode(IJSObjectReference jsInstance)
        {
            _jsInstance = jsInstance ?? throw new ArgumentNullException(nameof(jsInstance));
        }

        public async Task GenerateAsync(Address address)
        {
            await _jsInstance.InvokeVoidAsync("makeCode", address.ToString());
        }
    }
}