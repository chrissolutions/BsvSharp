#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    public class Base58PrivateKey : Base58Data
    {
        public Base58PrivateKey() { }
        public Base58PrivateKey(PrivateKey privateKey, NetworkType? networkType = null) 
            => SetKey(privateKey, networkType);
        public Base58PrivateKey(string base58, NetworkType? networkType = null) => SetString(base58, networkType);

        public bool IsValid
        {
            get
            {
                var d = KeyData;
                var fExpectedFormat = d.Length == UInt256.Length || d.Length == UInt256.Length + 1 && d[^1] == 1;
                var v = Version;
                var fCorrectVersion = v.Data.SequenceEqual(RootService.GetNetwork(NetworkType).SecretKey);
                return fExpectedFormat && fCorrectVersion;
            }
        }

        public PrivateKey GetKey()
        {
            var data = KeyData;
            Debug.Assert(data.Length >= UInt256.Length);
            var isCompressed = data.Length > UInt256.Length && data[UInt256.Length] == 1;
            var privateKey = new PrivateKey(data[..UInt256.Length], isCompressed);
            return privateKey;
        }

        public void SetKey(PrivateKey privateKey, NetworkType? networkType)
        {
            Debug.Assert(privateKey.IsValid);
            var network = RootService.GetNetwork(networkType);
            NetworkType = network.NodeType;
            SetData(network.SecretKey, privateKey.ToArray(), privateKey.IsCompressed);
        }

        public bool SetString(string base58, NetworkType? networkType)
        {
            var network = RootService.GetNetwork(networkType);
            NetworkType = network.NodeType;
            return SetString(base58, network.SecretKey.Length) && IsValid;
        }
    }
}
