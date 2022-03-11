using CafeLib.Blazor.Interop;
using Microsoft.JSInterop;

namespace BlazorWallet.Interop
{
    public class QrCodeProxy : JsInteropProxy<QrCode>
    {
        public QrCodeProxy(IJSRuntime jsRuntime)
            : base("./js/QrCodeProxy.js", jsRuntime)
        {
        }
    }
}