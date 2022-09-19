#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys.Base58
{
    internal sealed class Base58HdPrivateKey : Base58Data
    {
        public Base58HdPrivateKey()
        {
        }

        public Base58HdPrivateKey(HdPrivateKey privateKey)
        {
            SetKey(privateKey, null);
        }

        public Base58HdPrivateKey(HdPrivateKey privateKey, NetworkType networkType)
        {
            SetKey(privateKey, networkType);
        }

        public Base58HdPrivateKey(string base58)
        {
            FromString(base58, null);
        }

        public Base58HdPrivateKey(string base58, NetworkType networkType)
        {
            FromString(base58, networkType);
        }

        /// <summary>
        /// Set the Base58HdPrivateKey from base58 string.
        /// </summary>
        /// <param name="base58">base58 string</param>
        /// <param name="networkType">network type</param>
        /// <returns>true if successful</returns>
        public bool FromString(string base58, NetworkType? networkType)
            => FromString(base58, RootService.GetNetwork(networkType).HdSecretKey.Length);

        /// <summary>
        /// Get the HdPrivateKey from Base58HdPrivateKey key data.
        /// </summary>
        /// <returns>HdPrivateKey</returns>
        public HdPrivateKey GetKey() => HdPrivateKey.FromKey(KeyData);

        /// <summary>
        /// Set the Base58HdPrivateKey key data.
        /// </summary>
        /// <param name="privateKey">HD private key</param>
        /// <param name="networkType">network type</param>
        public void SetKey(HdPrivateKey privateKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).HdSecretKey;
            var data = new byte[prefix.Length + HdKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            privateKey.Encode(data.AsSpan()[prefix.Length..]);
            SetData(data, prefix.Length);
        }
    }
}
