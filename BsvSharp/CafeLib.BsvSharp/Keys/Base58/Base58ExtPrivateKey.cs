using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;

namespace CafeLib.BsvSharp.Keys.Base58
{
    internal class Base58ExtPrivateKey : Base58Data
    {
        public Base58ExtPrivateKey()
        {
        }

        public Base58ExtPrivateKey(HdPrivateKey privateKey)
        {
            SetKey(privateKey, null);
        }

        public Base58ExtPrivateKey(HdPrivateKey privateKey, NetworkType networkType)
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

        internal void SetKey(HdPrivateKey privateKey, NetworkType? networkType)
        {
            var prefix = RootService.GetNetwork(networkType).ExtSecretKey;
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
            => SetString(b58, RootService.GetNetwork(networkType).ExtSecretKey.Length);
    }
}
