#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Keys
{
    public abstract class ExtKey
    {
        public const uint HardenedBit = 0x80000000;
        public const int Bip32KeySize = 74;

        public byte Depth { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        protected uint Child { get; set; }

        /// <summary>
        /// First four bytes of the corresponding public key's HASH160 which is also called its key ID.
        /// </summary>
        public int Fingerprint { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Hardened => Child >= HardenedBit;

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="code"></param>
        public abstract void Encode(ByteSpan code);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        protected abstract ExtKey DeriveBase(int index, bool hardened);

        /// <summary>
        /// Computes the key specified by a key path.
        /// At each derivation, there's a small chance the index specified will fail.
        /// If any generation fails, null is returned.
        /// </summary>
        /// <param name="kp"></param>
        /// <returns>null on derivation failure. Otherwise the derived private key.</returns>
        public ExtKey DeriveBase(KeyPath kp)
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

        public bool Equals(ExtKey o) => !(o is null) && Depth == o.Depth && Fingerprint == o.Fingerprint && Child == o.Child && ChainCode == o.ChainCode;
        public override bool Equals(object obj) => obj is ExtKey key && Equals(key);

        public static bool operator ==(ExtKey x, ExtKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(ExtKey x, ExtKey y) => !(x == y);
    }
}
