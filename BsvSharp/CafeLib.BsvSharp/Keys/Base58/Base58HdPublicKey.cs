#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys.Base58
{
    internal class Base58HdPublicKey : Base58Data
    {
        public Base58HdPublicKey(HdPublicKey pubKey, NetworkType? networkType = null)
            => SetKey(pubKey, networkType);

        public Base58HdPublicKey(string base58, NetworkType? networkType = null)
            => FromString(base58, networkType);

        public bool FromString(string base58, NetworkType? networkType)
            => FromString(base58, RootService.GetNetwork(networkType).HdPublicKey.Length);

        public HdPublicKey GetKey() => HdPublicKey.FromKey(KeyData);

        public static HdPublicKey GetKey(string base58) => new Base58HdPublicKey(base58).GetKey();

        public void SetKey(HdPublicKey pubKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).HdPublicKey;
            var data = new byte[prefix.Length + HdKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            pubKey.Encode(data.AsSpan()[prefix.Length..]);
            SetData(data, prefix.Length);
        }
    }
}
