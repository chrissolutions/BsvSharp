#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    public class Base58PrivateKey : Base58Data
    {
        public void SetKey(PrivateKey privateKey)
        {
            Debug.Assert(privateKey.IsValid);
            SetData(RootService.Network.SecretKey, privateKey.ToArray(), privateKey.IsCompressed);
        }

        public PrivateKey GetKey()
        {
            var data = KeyData;
            Debug.Assert(data.Length >= UInt256.Length);
            var isCompressed = data.Length > UInt256.Length && data[UInt256.Length] == 1;
            var privateKey = new PrivateKey(data[..UInt256.Length], isCompressed);
            return privateKey;
        }

        public bool IsValid
        {
            get 
            {
                var d = KeyData;
                var fExpectedFormat = d.Length == UInt256.Length || d.Length == UInt256.Length + 1 && d[^1] == 1;
                var v = Version;
                var fCorrectVersion = v.Data.SequenceEqual(RootService.Network.SecretKey);
                return fExpectedFormat && fCorrectVersion;
            }
        }

        public bool SetString(string base58) => SetString(base58, RootService.Network.SecretKey.Length) && IsValid;

        public Base58PrivateKey() {}
        public Base58PrivateKey(PrivateKey privateKey) => SetKey(privateKey);
        public Base58PrivateKey(string base58) => SetString(base58);
    }
}
