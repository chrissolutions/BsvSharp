#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

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
        #region Constructors

        /// <summary>
        /// WifPrivateKey default constructor.
        /// </summary>
        protected WifPrivateKey() { }

        /// <summary>
        /// WifPrivateKey constructor.
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="networkType">network type</param>
        public WifPrivateKey(PrivateKey privateKey, NetworkType? networkType = null) 
            => FromPrivateKey(privateKey, networkType);

        /// <summary>
        /// WifPrivateKey constructor.
        /// </summary>
        /// <param name="wif">wallet information format string</param>
        /// <param name="networkType">network type</param>
        public WifPrivateKey(string wif, NetworkType? networkType = null)
        {
            var network = RootService.GetNetwork(networkType);
            NetworkType = network.NodeType;
            if (!SetString(wif, network.SecretKey.Length) || !IsValid)
            {
                throw new InvalidKeyException(nameof(wif));
            }
        }

        #endregion

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

        public PrivateKey ToPrivateKey()
        {
            var data = KeyData;
            Debug.Assert(data.Length >= UInt256.Length);
            var isCompressed = data.Length > UInt256.Length && data[UInt256.Length] == 1;
            var privateKey = new PrivateKey(data[..UInt256.Length], isCompressed);
            return privateKey;
        }

        public static WifPrivateKey FromPrivateKey(PrivateKey privateKey, NetworkType? networkType = null)
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
