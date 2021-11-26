using System;
using CafeLib.BsvSharp.Encoding;

namespace CafeLib.BsvSharp.Network
{
    public class ScalingTestNetwork : BitcoinNetwork
    {
        public ScalingTestNetwork()
            : base(NetworkType.Scaling, new Lazy<byte[][]>(GetPrefixes).Value)
        {
        }

        private static byte[][] GetPrefixes()
        {
            var prefixes = new byte[(int)Base58Type.MaxBase58Types][];
            prefixes[(int)Base58Type.PubkeyAddress] = new byte[] { 111 };
            prefixes[(int)Base58Type.ScriptAddress] = new byte[] { 196 };
            prefixes[(int)Base58Type.SecretKey] = new byte[] { 239 };
            prefixes[(int)Base58Type.ExtPublicKey] = new byte[] { 0x04, 0x35, 0x87, 0xCF };
            prefixes[(int)Base58Type.ExtSecretKey] = new byte[] { 0x04, 0x35, 0x83, 0x94 };
            return prefixes;
        }
    }
}