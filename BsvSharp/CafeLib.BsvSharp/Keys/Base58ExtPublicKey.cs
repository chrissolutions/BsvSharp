#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys
{
    public class Base58ExtPublicKey : Base58Data
    {
        public void SetKey(ExtPublicKey pubKey)
        {
            var prefix = RootService.Network.ExtPublicKey;
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

        public bool SetString(string b58) => SetString(b58, RootService.Network.ExtPublicKey.Length);

        public Base58ExtPublicKey(ExtPublicKey pubKey)
        {
            SetKey(pubKey);
        }

        public Base58ExtPublicKey(string base58)
        {
            SetString(base58);
        }

        public static ExtPublicKey GetKey(string base58) => new Base58ExtPublicKey(base58).GetKey();
    }
}
