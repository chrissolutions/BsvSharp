﻿using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys.Base58
{
    internal class Base58ExtPublicKey : Base58Data
    {
        public Base58ExtPublicKey(HdPublicKey pubKey, NetworkType? networkType = null)
            => SetKey(pubKey, networkType);

        public Base58ExtPublicKey(string base58, NetworkType? networkType = null)
            => SetString(base58, networkType);

        public void SetKey(HdPublicKey pubKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).ExtPublicKey;
            var data = new byte[prefix.Length + HdKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            pubKey.Encode(data.Slice(prefix.Length));
            SetData(data, prefix.Length);
        }

        public HdPublicKey GetKey()
        {
            var pubKey = new HdPublicKey();
            if (KeyData.Length == HdKey.Bip32KeySize)
            {
                pubKey.Decode(KeyData);
            }
            return pubKey;
        }

        public static HdPublicKey GetKey(string base58) => new Base58ExtPublicKey(base58).GetKey();

        internal bool SetString(string b58, NetworkType? networkType)
            => SetString(b58, RootService.GetNetwork(networkType).ExtPublicKey.Length);
    }
}
