#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Math;
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable InconsistentNaming

namespace CafeLib.BsvSharp.Keys
{
    /// <summary>
    /// 
    /// </summary>
    public class PrivateKey : IEquatable<PrivateKey>
    {
        private const int KeySize = UInt256.Length;

        /// <summary>
        /// PrivateKey data.
        /// </summary>
        private UInt256 _keyData;

        /// <summary>
        /// Internal elliptical curve key.
        /// </summary>
        internal ECKey ECKey { get; set; }

        ///// <summary>
        ///// HardenedBit.
        ///// </summary>
        //private const uint HardenedBit = 0x80000000;

        /// <summary>
        /// PrivateKey default constructor.
        /// </summary>
        public PrivateKey()
            : this(false)
        {
        }

        /// <summary>
        /// PrivateKey constructor.
        /// </summary>
        /// <param name="compressed"></param>
        public PrivateKey(bool compressed)
        {
            byte[] data;
            do
            {
                data = Randomizer.GetStrongRandBytes(KeySize);
            } while (!VerifyData(data));

            SetData(data, compressed);
        }

        /// <summary>
        /// PrivateKey constructor.
        /// </summary>
        /// <param name="keyData"></param>
        public PrivateKey(UInt256 keyData)
            : this(keyData, true)
        {
        }

        /// <summary>
        /// PrivateKey constructor.
        /// </summary>
        /// <param name="keyData"></param>
        /// <param name="compressed"></param>
        private PrivateKey(UInt256 keyData, bool compressed)
            : this(keyData.Span, compressed)
        {
        }

        /// <summary>
        /// PrivateKey constructor.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="compressed"></param>
        public PrivateKey(ReadOnlyByteSpan span, bool compressed = true)
        {
            SetData(span, compressed);
        }

        /// <summary>
        /// PrivateKey constructor.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="compressed"></param>
        public PrivateKey(string hex, bool compressed = true)
            : this(new UInt256(hex, true), compressed)
        {
        }

        /// <summary>
        /// Determines validity of the private key.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// True if the corresponding public key is compressed.
        /// </summary>
        public bool IsCompressed { get; private set; }

        /// <summary>
        /// PublicKey.
        /// </summary>
        private PublicKey _publicKey;
        public PublicKey PublicKey
        {
            get
            {
                if (_publicKey == null)
                {
                    _publicKey = this.CreatePublicKey();
                }
                return _publicKey;
            }
        }


        public byte[] ToArray() => _keyData;

        public static PrivateKey FromHex(string hex, bool compressed = true) => new PrivateKey(new UInt256(hex, true), compressed);
        public static PrivateKey FromBase58(string base58) => new Base58PrivateKey(base58).GetKey();
        public static PrivateKey FromWif(string wif) => new Base58PrivateKey(wif).GetKey();
        public static PrivateKey FromRandom() => new PrivateKey();

        /// <summary>
        /// Derive a new private key.
        /// </summary>
        /// <param name="chainCode"></param>
        /// <param name="nChild"></param>
        /// <returns></returns>
        internal (PrivateKey keyChild, UInt256 ccChild) Derive(uint nChild, UInt256 chainCode)
        {
            byte[] l;
            var ll = new byte[32];
            var lr = new byte[32];

            if (nChild >> 31 == 0)
            {
                var pubKey = this.CreatePublicKey().ToArray();
                l = Hashes.Bip32Hash(chainCode, nChild, pubKey[0], pubKey[1..]);
            }
            else
            {
                l = Hashes.Bip32Hash(chainCode, nChild, 0, ToArray());
            }

            Buffer.BlockCopy(l, 0, ll, 0, 32);
            Buffer.BlockCopy(l, 32, lr, 0, 32);
            var ccChild = lr;

            var parse256LL = new BigInteger(1, ll);
            var kPar = new BigInteger(1, _keyData);
            var N = ECKey.Curve.N;

            if (parse256LL.CompareTo(N) >= 0)
                throw new InvalidOperationException("You won a prize ! this should happen very rarely. Take a screenshot, and roll the dice again.");
            var key = parse256LL.Add(kPar).Mod(N);
            if (Equals(key, BigInteger.Zero))
                throw new InvalidOperationException("You won the big prize ! this would happen only 1 in 2^127. Take a screenshot, and roll the dice again.");

            var keyBytes = key.ToByteArrayUnsigned();
            if (keyBytes.Length < 32)
                keyBytes = new byte[32 - keyBytes.Length].Concat(keyBytes);

            return (new PrivateKey(new UInt256(keyBytes)), new UInt256(ccChild));
        }

        /// <summary>
        /// Verify thoroughly whether a private key and a public key match.
        /// This is done using a different mechanism than just regenerating it.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public bool VerifyPubKey(PublicKey publicKey)
        {
            if (publicKey.IsCompressed != IsCompressed)
                return false;

            var rnd = Randomizer.GetStrongRandBytes(8).ToArray();
            const string str = "Bitcoin key verification\n";

            var hash = str.AsciiToBytes().Concat(rnd).Hash256();

            var sig = SignCompact(hash);
            return sig != null && publicKey.Verify(hash, sig);
        }

        public string ToHex() => _keyData.ToStringFirstByteFirst();
        public Base58PrivateKey ToBase58() => new Base58PrivateKey(this);
        public override string ToString() => ToBase58().ToString();

        public override int GetHashCode() => _keyData.GetHashCode();
        public bool Equals(PrivateKey o) => !(o is null) && IsCompressed.Equals(o.IsCompressed) && _keyData.Equals(o._keyData);
        public override bool Equals(object obj) => obj is PrivateKey key && this == key;

        public static bool operator ==(PrivateKey x, PrivateKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(PrivateKey x, PrivateKey y) => !(x == y);

        public static implicit operator ReadOnlyByteSpan(PrivateKey rhs) => rhs._keyData.Span;

        #region Helpers

        internal void SetData(ReadOnlyByteSpan data, bool compressed = true)
        {
            if (data.Length != UInt256.Length || !VerifyData(data))
            {
                IsValid = false;
            }
            else
            {
                _keyData = new UInt256();
                data.CopyTo(_keyData.Span);
                IsCompressed = compressed;
                IsValid = true;
                ECKey = new ECKey(_keyData, true);
            }
        }

        internal byte[] SignCompact(UInt256 hash)
        {
            var sig = ECKey.Sign(hash);
            // Now we have to work backwards to figure out the recId needed to recover the signature.
            var recId = -1;
            for (var i = 0; i < 4; i++)
            {
                var k = ECKey.RecoverFromSignature(i, sig, hash, IsCompressed);
                if (k != null && k.GetPublicKeyPoint(IsCompressed).AsSpan().SequenceEqual(PublicKey.Data))
                {
                    recId = i;
                    break;
                }
            }

            if (recId == -1)
                throw new InvalidOperationException("Could not construct a recoverable key. This should never happen.");

            var headerByte = recId + 27 + (IsCompressed ? 4 : 0);

            // 1 header + 32 bytes for R + 32 bytes for S
            var sigData = new ByteSpan(new byte[65])
            {
                [0] = (byte)headerByte
            };

            sig.R.ToByteArrayUnsigned().CopyTo(sigData[1..33]);
            sig.S.ToByteArrayUnsigned().CopyTo(sigData[33..]);
            return sigData;
        }

        private static bool VerifyData(ReadOnlyByteSpan data)
        {
            // Do not convert to OpenSSL's data structures for range-checking keys,
            // it's easy enough to do directly.
            byte[] vchMax = {
                0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
                0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFE,
                0xBA,0xAE,0xDC,0xE6,0xAF,0x48,0xA0,0x3B,
                0xBF,0xD2,0x5E,0x8C,0xD0,0x36,0x41,0x40
            };
            bool fIsZero = true;
            for (int i = 0; i < KeySize && fIsZero; i++)
                if (data[i] != 0)
                    fIsZero = false;
            if (fIsZero)
                return false;
            for (int i = 0; i < KeySize; i++)
            {
                if (data[i] < vchMax[i])
                    return true;
                if (data[i] > vchMax[i])
                    return false;
            }
            return true;
        }

        #endregion
    }
}
