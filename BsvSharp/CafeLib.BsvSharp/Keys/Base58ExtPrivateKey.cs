﻿#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys
{
    public class Base58ExtPrivateKey : Base58Data
    {
        public Base58ExtPrivateKey()
        {
        }

        public Base58ExtPrivateKey(ExtPrivateKey privateKey)
        {
            SetKey(privateKey, null);
        }

        public Base58ExtPrivateKey(ExtPrivateKey privateKey, NetworkType networkType)
        {
            SetKey(privateKey, networkType);
        }

        public Base58ExtPrivateKey(string b58)
        {
            SetString(b58, null);
        }

        public Base58ExtPrivateKey(string b58, NetworkType networkType)
        {
            SetString(b58, networkType);
        }

        internal void SetKey(ExtPrivateKey privateKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).ExtSecretKey;
            var data = new byte[prefix.Length + ExtKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            privateKey.Encode(data.Slice(prefix.Length));
            SetData(data, prefix.Length);
        }

        public ExtPrivateKey GetKey()
        {
            var privateKey = new ExtPrivateKey();
            if (KeyData.Length == ExtKey.Bip32KeySize)
            {
                privateKey.Decode(KeyData);
            }
            return privateKey;
        }

        public bool SetString(string b58, NetworkType? networkType) 
            => SetString(b58, RootService.GetNetwork(networkType).ExtSecretKey.Length);
    }
}
