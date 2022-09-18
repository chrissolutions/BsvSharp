#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys.Base58;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    public class HdPublicKey : HdKey
    {
        protected HdPublicKey()
        {
        }

        public PublicKey PublicKey { get; private set; }

        /// <summary>
        /// Create a hierarchal deterministic public key from HdPrivateKey.
        /// </summary>
        /// <param name="privateKey">HD private key</param>
        /// <returns>HdPublicKey</returns>
        public static HdPublicKey FromPrivateKey(HdPrivateKey privateKey)
        {
            return new HdPublicKey
            {
                Depth = privateKey.Depth,
                Fingerprint = privateKey.Fingerprint,
                Child = privateKey.IndexWithHardened,
                ChainCode = privateKey.ChainCode,
                PublicKey = privateKey.PrivateKey.CreatePublicKey()
            };
        }

        /// <summary>
        /// Create a hierarchal deterministic public key from key.
        /// </summary>
        /// <param name="keyData">key data</param>
        /// <returns>hierarchal deterministic public key</returns>
        public static HdPublicKey FromKey(ReadOnlyByteSpan keyData)
        {
            var pubKey = new HdPublicKey();
            if (keyData.Length == Bip32KeySize)
            {
                pubKey.Decode(keyData);
            }
            return pubKey;
        }

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// At each derivation, there's a small chance the index specified will fail.
        /// If any generation fails, null is returned.
        /// </summary>
        /// <param name="keyPath">key path</param>
        /// <returns>null on derivation failure. Otherwise the derived private key.</returns>
        public HdPublicKey Derive(KeyPath keyPath) => DeriveBase(keyPath) as HdPublicKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="path">key path</param>
        /// <returns>extended public key</returns>
        public HdPublicKey Derive(string path) => DeriveBase(new KeyPath(path)) as HdPublicKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public HdPublicKey Derive(int index) => DeriveBase(index, false) as HdPublicKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        protected override HdKey DeriveBase(int index, bool hardened)
        {
            var cek = new HdPublicKey 
            {
                Depth = (byte)(Depth + 1),
                Child = (uint)index | (hardened ? HardenedBit : 0),
                Fingerprint = BitConverter.ToInt32(PublicKey.GetId().Span[..4])
            };

            (cek.PublicKey, cek.ChainCode) = PublicKey.Derive(cek.Child, ChainCode);
            return cek;
        }

        /// <summary>
        /// Encode the hierarchical deterministic public key.
        /// </summary>
        /// <param name="data">byte span ref struct to copy key data</param>
        public override void Encode(ByteSpan data)
        {
            var i = 0;
            data[i++] = Depth;
            Fingerprint.AsSpan().CopyTo(data[i..(i += sizeof(int))]);
            data[i++] = (byte)((Child >> 24) & 0xFF);
            data[i++] = (byte)((Child >> 16) & 0xFF);
            data[i++] = (byte)((Child >> 8) & 0xFF);
            data[i++] = (byte)(Child & 0xFF);
            ChainCode.Span.CopyTo(data[i..(i += UInt256.Length)]);
            var key = PublicKey.Data;
            Debug.Assert(key.Length == 33);
            key.CopyTo(data[i..(i += key.Length)]);
        }

        /// <summary>
        /// Decode the key data into the private key.
        /// </summary>
        /// <param name="data">byte span ref struct that retrieves key data</param>
        public override void Decode(ReadOnlyByteSpan data)
        {
            var i = 0;
            Depth = data[i++];
            Fingerprint = BitConverter.ToInt32(data[i..(i += sizeof(int))]);
            Child = (uint)data[i++] << 24 | (uint)data[i++] << 16 | (uint)data[i++] << 8 | data[i++];
            ChainCode = new UInt256(data[i..(i += UInt256.Length)]);
            PublicKey = new PublicKey();
            PublicKey.SetData(data[i..]);
        }

        internal Base58HdPublicKey ToBase58() => new(this);
        
        public override string ToString() => ToBase58().ToString();

        public override int GetHashCode() => ToString().GetHashCode();

        public bool Equals(HdPublicKey o) => o is not null && base.Equals(o) && PublicKey == o.PublicKey;
        public override bool Equals(object obj) => obj is HdPublicKey key && this == key;

        public static bool operator ==(HdPublicKey x, HdPublicKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(HdPublicKey x, HdPublicKey y) => !(x == y);

        public static explicit operator byte[](HdPublicKey rhs) => rhs.ToArray();
    }
}
