using System;
using System.Diagnostics;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys.Base58;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Keys
{
    public class HdPublicKey : HdKey
    {
        public PublicKey PublicKey { get; private set; }

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
                Fingerprint = BitConverter.ToInt32(PublicKey.GetId().Span.Slice(0, 4))
            };

            (cek.PublicKey, cek.ChainCode) = PublicKey.Derive(cek.Child, ChainCode);
            return cek;
        }

        public override void Encode(ByteSpan code)
        {
            code[0] = Depth;
            Fingerprint.AsSpan().CopyTo(code.Slice(1, 4));
            code[5] = (byte)((Child >> 24) & 0xFF);
            code[6] = (byte)((Child >> 16) & 0xFF);
            code[7] = (byte)((Child >> 8) & 0xFF);
            code[8] = (byte)(Child & 0xFF);
            ChainCode.Span.CopyTo(code.Slice(9, 32));
            var key = PublicKey.Data;
            Debug.Assert(key.Length == 33);
            key.CopyTo(code.Slice(41, 33));
        }

        public override void Decode(ReadOnlyByteSpan code)
        {
            Depth = code[0];
            Fingerprint = BitConverter.ToInt32(code.Slice(1, 4));
            Child = (uint)code[5] << 24 | (uint)code[6] << 16 | (uint)code[7] << 8 | code[8];
            code.Slice(9, 32).CopyTo(ChainCode.Span);
            PublicKey = new PublicKey();
            PublicKey.Set(code.Slice(41, 33));
        }

        public byte[] ToArray()
        {
            var bytes = new byte[Bip32KeySize];
            Encode(bytes);
            return bytes;
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
