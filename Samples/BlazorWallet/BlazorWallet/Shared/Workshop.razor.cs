using System.Threading.Tasks;
using BlazorWallet.Interop;
using CafeLib.Blazor.Interop;
using CafeLib.BsvSharp.Api.WhatsOnChain;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Passphrase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWallet.Shared
{
    public partial class Workshop : ComponentBase
    {
        [Inject] private QrCodeProxy QrCodeProxy { get; set; }
        [Inject] private IJSRuntime JsRuntime { get; set; }

        private QrCode _qrCode;
        private readonly WhatsOnChain _whatsOnChain = new WhatsOnChain();

        private Mnemonic _mnemonic;
        private string _words = "";

        private ExtPrivateKey _extPrivateKey;
        private string _extPrivateKeyText = "";

        private string _keyPath = "m/44'/0'/0'";
        private int _pathDepth;

        private PrivateKey _privateKey;
        private string _privateKeyText = "";

        private PublicKey _publicKey;
        private string _publicKeyText;

        private Address _address;
        private string _addressText;

        private string _balanceText;

        private async Task GenerateMnemonic()
        {
            _mnemonic = new Mnemonic();
            _words = _mnemonic.Words;

            await NewHdPrivateKey();
        }

        private async Task NewHdPrivateKey()
        {
            _extPrivateKey = ExtPrivateKey.FromWords(_words);
            _extPrivateKeyText = _extPrivateKey.ToString();
            await DerivationPath(_pathDepth.ToString());
        }

        private async Task DerivationPath(string pathDepth)
        {
            NewPrivateKey(int.Parse(pathDepth));
            NewPublicKey();
            NewAddress();
            await GenerateQrCode();
            await RefreshBalance();
        }

        private void NewPrivateKey(int pathDepth)
        {
            _keyPath = $"m/44'/0'/{pathDepth}'";
            _pathDepth = pathDepth;
            _privateKey = _extPrivateKey.Derive(_keyPath).PrivateKey;
            _privateKeyText = _privateKey.ToString();
        }

        private void NewPublicKey()
        {
            _publicKey = _privateKey.CreatePublicKey();
            _publicKeyText = _publicKey.ToString();
        }

        private void NewAddress()
        {
            _address = _publicKey.ToAddress();
            _addressText = _address.ToString();
        }

        private async Task GenerateQrCode()
        {
            await _qrCode.GenerateAsync(_address);
        }

        private async Task RefreshBalance()
        {
            var balance = await _whatsOnChain.GetAddressBalance(_addressText);
            _balanceText = $"Confirmed = {balance.Confirmed}; Unconfirmed = {balance.Unconfirmed}";
        }

        private async Task CopyExtPrivateKey()
        {
            if (!string.IsNullOrWhiteSpace(_privateKeyText))
            {
                await CopyText("hdPrivateKeyText", "HD Private Key Copied!");
            }
        }

        private async Task CopyPrivateKey()
        {
            if (!string.IsNullOrWhiteSpace(_privateKeyText))
            {
                await CopyText("privateKeyText", "Private Key Copied!");
            }
        }

        private async Task CopyPublicKey()
        {
            if (!string.IsNullOrWhiteSpace(_publicKeyText))
            {
                await CopyText("publicKeyText", "Public Key Copied!");
            }
        }

        private async Task CopyAddress()
        {
            if (!string.IsNullOrWhiteSpace(_addressText))
            {
                await CopyText("addressText", "Address Copied!");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _qrCode = (await QrCodeProxy.CreateReferenceAsync("qrcode")).CreateObject<QrCode>();
            }
        }

        #region Helpers

        private async ValueTask CopyText(string elementId, string message)
        {
            await JsRuntime.InvokeVoidAsync("copyText", elementId);
            await JsRuntime.InvokeVoidAsync("sweetAlert", message);
        }

        #endregion
    }
}
