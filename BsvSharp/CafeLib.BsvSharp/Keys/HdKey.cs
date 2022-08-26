using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    /// <summary>
    /// Hierarchical Deterministic Key.
    /// </summary>
    public abstract class HdKey
    {
        protected const uint HardenedBit = 0x80000000;
        public const int Bip32KeySize = 74;

        /// <summary>
        /// Key depth.
        /// </summary>
        public byte Depth { get; protected set; }

        /// <summary>
        /// Child key.
        /// </summary>
        protected uint Child { get; set; }

        /// <summary>
        /// First four bytes of the corresponding public key's HASH160 which is also called its key ID.
        /// </summary>
        public int Fingerprint { get; protected set; }

        /// <summary>
        /// Indicates whether the key is hardened.
        /// </summary>
        public bool Hardened => Child >= HardenedBit;

        /// <summary>
        /// Chain code.
        /// </summary>
        public UInt256 ChainCode { get; protected set; }

        /// <summary>
        /// Always excludes the HardenedBit.
        /// </summary>
        public int Index => (int)(Child & ~HardenedBit);

        /// <summary>
        /// 
        /// </summary>
        public uint IndexWithHardened => Child;

        /// <summary>
        /// Encode the key.
        /// </summary>
        /// <param name="code"></param>
        public abstract void Encode(ByteSpan code);

        /// <summary>
        /// Decode the key.
        /// </summary>
        /// <param name="code"></param>
        public abstract void Decode(ReadOnlyByteSpan code);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        protected abstract HdKey DeriveBase(int index, bool hardened);

        /// <summary>
        /// Computes the key specified by a key path.
        /// At each derivation, there's a small chance the index specified will fail.
        /// If any generation fails, null is returned.
        /// </summary>
        /// <param name="kp"></param>
        /// <returns>null on derivation failure. Otherwise the derived private key.</returns>
        protected HdKey DeriveBase(KeyPath kp)
        {
            var k = this;
            foreach (var i in kp)
            {
                k = k.DeriveBase((int)(i & ~HardenedBit), (i & HardenedBit) != 0);
                if (k == null) break;
            }
            return k;
        }

        public override int GetHashCode() => Depth.GetHashCode() ^ Fingerprint.GetHashCode() ^ Child.GetHashCode() ^ ChainCode.GetHashCode();

        public bool Equals(HdKey o) => o is not null && Depth == o.Depth && Fingerprint == o.Fingerprint && Child == o.Child && ChainCode == o.ChainCode;
        public override bool Equals(object obj) => obj is HdKey key && Equals(key);

        public static bool operator ==(HdKey x, HdKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(HdKey x, HdKey y) => !(x == y);
    }
}
