using System.Threading.Tasks;
using CafeLib.Blazor.Interop;
using Microsoft.JSInterop;

namespace BlazorWallet.Interop
{
    public class QrCodeProxy : JsObjectProxy
    {
        public QrCodeProxy(IJSRuntime jsRuntime)
            : base("./js/QrCodeProxy.js", jsRuntime)
        {
        }

        public async Task<IJSObjectReference> CreateReferenceAsync(string elementId)
        {
            return await base.CreateReferenceAsync(elementId);
        }
    }
}