#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys
{
    public class Base58ExtPublicKey : Base58Data
    {
        public Base58ExtPublicKey(ExtPublicKey pubKey)
        {
            SetKey(pubKey, null);
        }

        public Base58ExtPublicKey(ExtPublicKey pubKey, NetworkType networkType)
        {
            SetKey(pubKey, networkType);
        }

        public Base58ExtPublicKey(string base58)
        {
            SetString(base58, null);
        }

        public Base58ExtPublicKey(string base58, NetworkType networkType)
        {
            SetString(base58, networkType);
        }

        public void SetKey(ExtPublicKey pubKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).ExtPublicKey;
            var data = new byte[prefix.Length + ExtKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            pubKey.Encode(data.Slice(prefix.Length));
            SetData(data, prefix.Length);
        }

        public ExtPublicKey GetKey()
        {
            var pubKey = new ExtPublicKey();
            if (KeyData.Length == ExtKey.Bip32KeySize) 
            {
                pubKey.Decode(KeyData);
            }
            return pubKey;
        }

        public static ExtPublicKey GetKey(string base58) => new Base58ExtPublicKey(base58).GetKey();

        internal bool SetString(string b58, NetworkType? networkType) 
            => SetString(b58, RootService.GetNetwork(networkType).ExtPublicKey.Length);
    }
}
