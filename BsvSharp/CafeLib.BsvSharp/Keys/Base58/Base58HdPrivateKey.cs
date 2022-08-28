#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys.Base58
{
    internal class Base58HdPrivateKey : Base58Data
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

        public Base58HdPrivateKey(string b58)
        {
            SetString(b58, null);
        }

        public Base58HdPrivateKey(string b58, NetworkType networkType)
        {
            SetString(b58, networkType);
        }

        internal void SetKey(HdPrivateKey privateKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).HdSecretKey;
            var data = new byte[prefix.Length + HdKey.Bip32KeySize];
            prefix.CopyTo(data, 0);
            privateKey.Encode(data.Slice(prefix.Length));
            SetData(data, prefix.Length);
        }

        public HdPrivateKey GetKey()
        {
            var privateKey = new HdPrivateKey();
            if (KeyData.Length == HdKey.Bip32KeySize)
            {
                privateKey.Decode(KeyData);
            }
            return privateKey;
        }

        public bool SetString(string b58, NetworkType? networkType)
            => SetString(b58, RootService.GetNetwork(networkType).HdSecretKey.Length);
    }
}
