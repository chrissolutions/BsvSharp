#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    public class WifPrivateKey : WifKey
    {
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

        internal static WifPrivateKey FromPrivateKey(PrivateKey privateKey, NetworkType? networkType = null)
        {
            Debug.Assert(privateKey.IsValid);
            var network = RootService.GetNetwork(networkType);
            var wifKey = new WifPrivateKey { NetworkType = network.NodeType };

            var data = privateKey.ToArray();
            wifKey.SetData(network.SecretKey, data, privateKey.IsCompressed);
            return wifKey;
        }
    }
}
