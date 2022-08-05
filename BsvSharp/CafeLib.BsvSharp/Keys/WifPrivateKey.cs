using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Exceptions;
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
            if (!privateKey.IsValid) throw new InvalidKeyException(nameof(privateKey));
            var network = RootService.GetNetwork(networkType);
            var wifKey = new WifPrivateKey { NetworkType = network.NodeType };
            var data = privateKey.ToArray();
            wifKey.SetData(network.SecretKey, data, privateKey.IsCompressed);
            return wifKey;
        }

        internal static WifPrivateKey FromString(string wif)
        {
            if (string.IsNullOrWhiteSpace(wif)) throw new ArgumentNullException(nameof(wif));
            var wifKey = new WifPrivateKey();
            var result = wifKey.SetString(wif, sizeof(byte));
            return result && wifKey.IsValid
                ? wifKey
                : throw new InvalidKeyException(nameof(wif));
        }

        internal PrivateKey ToPrivateKey()
        {
            var data = KeyData;
            Debug.Assert(data.Length >= UInt256.Length);
            var isCompressed = data.Length > UInt256.Length && data[UInt256.Length] == 1;
            var privateKey = new PrivateKey(data[..UInt256.Length], isCompressed);
            return privateKey;
        }
    }
}
