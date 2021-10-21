#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Math;
using CafeLib.Cryptography.BouncyCastle.Math.EC;
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable InconsistentNaming

namespace CafeLib.BsvSharp.Keys
{
    /// <summary>
    /// A KzPubKey is fundamentally an array of bytes in one of these states:
    /// null: Key is invalid.
    /// byte[33]: Key is compressed.
    /// byte[65]: Key is uncompressed.
    /// </summary>
    public class PublicKey
    {
        /// <summary>
        /// A KzPubKey is fundamentally an array of bytes in one of these states:
        /// null: Key is invalid.
        /// byte[33]: Key is compressed.
        /// byte[65]: Key is uncompressed.
        /// </summary>
        private byte[] _keyData;

        /// <summary>
        /// Elliptical curve key.
        /// </summary>
        private ECKey _ecKey;
        internal ECKey ECKey => _ecKey ??= new ECKey(_keyData, false);

        // Constants.                        
        internal const int CompressedLength = 33;
        internal const int UncompressedLength = 65;

        /// <summary>
        /// PublicKey default constructor.
        /// </summary>
        public PublicKey()
        {
            Invalidate();
        }

        /// <summary>
        /// PublicKey constructor
        /// </summary>
        /// <param name="compressed">compressed flag</param>
        public PublicKey(bool compressed)
            : this()
        {
            _keyData = new byte[compressed ? CompressedLength : UncompressedLength];
        }

        /// <summary>
        /// PublicKey constructor
        /// </summary>
        /// <param name="bytes">bytes</param>
        public PublicKey(ReadOnlyByteSpan bytes)
            : this()
        {
            if (bytes.Length > 0 && bytes.Length == PredictLength(bytes[0]))
            {
                _keyData = new byte[bytes.Length];
                bytes.CopyTo(_keyData);
            }
        }

        /// <summary>
        /// PublicKey constructor
        /// </summary>
        /// <param name="varType">Variable length type</param>
        public PublicKey(VarType varType)
            : this()
        {
            var firstByte = varType.FirstByte;
            var size = PredictLength(firstByte);

            _keyData = new byte[size];
            varType.CopyTo(_keyData);
        }

        /// <summary>
        /// PublicKey constructor
        /// </summary>
        /// <param name="hex">hexadecimal value</param>
        public PublicKey(string hex)
            : this()
        {
            try
            {
                var vch = Encoders.Hex.Decode(hex);
                if ((vch.Length == CompressedLength || vch.Length == UncompressedLength) && vch.Length == PredictLength(vch[0]))
                    _keyData = vch;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Creates a copy of this key.
        /// </summary>
        /// <returns></returns>
        public PublicKey Clone()
        {
            var clone = new PublicKey();
            if (_keyData != null)
                clone._keyData = _keyData.ToArray();
            return clone;
        }

        /// <summary>
        /// True if key is stored in an array of 33 bytes.
        /// False if invalid or uncompressed.
        /// </summary>
        public bool IsCompressed => _keyData?.Length == CompressedLength;

        /// <summary>
        /// True if key is defined and either compressed or uncompressed.
        /// False if array of bytes is null.
        /// </summary>
        public bool IsValid => _keyData != null;

        /// <summary>
        /// Compute the length of a pubkey with a given first byte.
        /// </summary>
        /// <param name="firstByte">First byte of KzPubKey Span.</param>
        /// <returns>0, 33, or 65</returns>
        private static int PredictLength(byte firstByte)
        {
            if (firstByte == 2 || firstByte == 3) return CompressedLength;
            if (firstByte == 4 || firstByte == 6 || firstByte == 7) return UncompressedLength;
            return 0;
        }

        public ReadOnlyByteSpan Data => _keyData;

        public byte[] ToArray() => Data;

        public void Set(ReadOnlyByteSpan data)
        {
            var len = data.Length == 0 ? 0 : PredictLength(data[0]);
            if (len > 0 && len == data.Length)
            {
                _keyData = new byte[data.Length];
                data.CopyTo(_keyData.AsSpan());
            }
            else
                Invalidate();
        }

        private void Invalidate()
        {
            _keyData = null;
        }

        /// <summary>
        /// The complement function is PrivateKey CreateSignature.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="signatureEncoded"></param>
        /// <returns></returns>
        public PublicKey RecoverKey(UInt256 hash, ReadOnlyByteSpan signatureEncoded)
        {
            if (signatureEncoded.Length < 65)
                throw new ArgumentException("Signature truncated, expected 65 bytes and got " + signatureEncoded.Length);

            int header = signatureEncoded[0];

            // The header byte: 0x1B = first key with even y, 0x1C = first key with odd y,
            //                  0x1D = second key with even y, 0x1E = second key with odd y

            if (header < 27 || header > 34)
                throw new ArgumentException("Header byte out of range: " + header);

            var r = new BigInteger(1, signatureEncoded[1..33]);
            var s = new BigInteger(1, signatureEncoded[33..]);
            var sig = new ECDSASignature(r, s);
            var compressed = false;

            if (header >= 31)
            {
                compressed = true;
                header -= 4;
            }
            int recId = header - 27;

            var key = ECKey.RecoverFromSignature(recId, sig, hash, compressed);
            return PublicKeyFromECKey(key, compressed);
        }

        /// <summary>
        /// Create public key from hex string.
        /// </summary>
        /// <param name="hex">hex string</param>
        /// <returns></returns>
        public static PublicKey FromHex(string hex) => new PublicKey(hex);

        public static PublicKey FromBase58(string base58) => new PublicKey(Encoders.Hex.Encode(Encoders.Base58.Decode(base58)));

        /// <summary>
        /// Recover public key from message and signature.
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="signature">signature text</param>
        /// <returns></returns>
        /// <remarks>
        /// Thanks bitcoinj source code
        /// http://bitcoinj.googlecode.com/git-history/keychain/core/src/main/java/com/google/bitcoin/core/Utils.java
        /// </remarks>
        public static PublicKey FromSignedMessage(string message, string signature)
        {
            var signatureBytes = Encoders.Base64.Decode(signature);
            var hash = KeyExtensions.GetMessageHash(message.Utf8ToBytes());
            return FromSignedHash(hash, signatureBytes);
        }

        /// <summary>
        /// Recover public key from signature and hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static PublicKey FromSignedHash(UInt256 hash, ReadOnlyByteSpan signature)
        {
            var key = new PublicKey();
            return key.RecoverKey(hash, signature);
        }

        /// <summary>
        /// Verify public key
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        public bool Verify(UInt256 hash, VarType sig)
        {
            return sig.Length == 65
                ? VerifyCompact(hash, sig)
                : VerifyTxSig(hash, sig);
        }

        /// <summary>
        /// RIPEMD160 applied to SHA256 of the 33 or 65 public key bytes.
        /// </summary>
        /// <returns>20 byte hash as a KzUInt160</returns>
        public UInt160 GetId() => ToPubKeyHash();

        /// <summary>
        /// RIPEMD160 applied to SHA256 of the 33 or 65 public key bytes.
        /// </summary>
        /// <returns>20 byte hash as a KzUInt160</returns>
        public UInt160 ToPubKeyHash() => Data.Hash160();

        /// <summary>
        /// Obtain an address.
        /// </summary>
        /// <returns></returns>
        public Address ToAddress() => new Address(Encoders.Base58Check.Encode(RootService.Network.PublicKeyAddress.ToArray().Concat(ToPubKeyHash())));

        /// <summary>
        /// Obtain the hex representation of the public key.
        /// </summary>
        /// <returns></returns>
        public string ToHex() => _keyData != null ? Encoders.Hex.Encode(_keyData) : "<invalid>";

        /// <summary>
        /// Obtain a public key string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToAddress().ToString();

        /// <summary>
        /// Derive a child public key.
        /// </summary>
        /// <param name="nChild"></param>
        /// <param name="chainCode"></param>
        /// <returns>child public key</returns>
        public (PublicKey, UInt256 ccChild) Derive(uint nChild, UInt256 chainCode)
        {
            byte[] lr;
            var l = new byte[32];
            var r = new byte[32];
            if (nChild >> 31 == 0)
            {
                var pubKey = ToArray();
                lr = Hashes.Bip32Hash(chainCode, nChild, pubKey[0], pubKey[1..]);
            }
            else
            {
                throw new InvalidOperationException("A public key can't derive an hardened child");
            }

            Buffer.BlockCopy(lr, 0, l, 0, 32);
            Buffer.BlockCopy(lr, 32, r, 0, 32);
            var ccChild = r;


            var N = ECKey.Curve.N;
            var parse256LL = new BigInteger(1, l);

            if (parse256LL.CompareTo(N) >= 0)
                throw new InvalidOperationException("You won a prize ! this should happen very rarely. Take a screenshot, and roll the dice again.");

            var q = ECKey.Curve.G.Multiply(parse256LL).Add(ECKey.GetPublicKeyParameters().Q);
            if (q.IsInfinity)
                throw new InvalidOperationException("You won the big prize ! this would happen only 1 in 2^127. Take a screenshot, and roll the dice again.");

            var p = new FpPoint(ECKey.Curve.Curve, q.X, q.Y, true);
            return (new PublicKey(p.GetEncoded()), new UInt256(ccChild));
        }

        public override int GetHashCode() => _keyData.GetHashCodeOfValues();

        public bool Equals(PublicKey o) => !(o is null) && _keyData.SequenceEqual(o._keyData);
        public override bool Equals(object obj) => obj is PublicKey key && this == key;

        public static explicit operator UInt256(PublicKey rhs) => new UInt256(rhs._keyData[1..(UInt256.Length + 1)]);

        public static explicit operator PublicKey(byte[] rhs) => new PublicKey(rhs);
        public static implicit operator byte[](PublicKey rhs) => rhs._keyData;

        public static implicit operator ByteSpan(PublicKey rhs) => rhs._keyData.AsSpan();
        public static implicit operator PublicKey(ByteSpan rhs) => new PublicKey(rhs);

        public static implicit operator ReadOnlyByteSpan(PublicKey rhs) => rhs._keyData.AsSpan();
        public static implicit operator PublicKey(ReadOnlyByteSpan rhs) => new PublicKey(rhs);

        public static bool operator ==(PublicKey x, PublicKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(PublicKey x, PublicKey y) => !(x == y);

        #region Helpers

        /// <summary>
        /// Get the public key from the elliptical curve key.
        /// </summary>
        /// <param name="ecKey"></param>
        /// <param name="isCompressed"></param>
        /// <returns></returns>
        private static PublicKey PublicKeyFromECKey(ECKey ecKey, bool isCompressed)
        {
            var q = ecKey.GetPublicKeyParameters().Q;
            //Pub key (q) is composed into X and Y, the compressed form only include X, which can derive Y along with 02 or 03 prepent depending on whether Y in even or odd.
            var result = ecKey.Secp256k1.Curve.CreatePoint(q.X.ToBigInteger(), q.Y.ToBigInteger(), isCompressed).GetEncoded();
            return new PublicKey(result);
        }

        /// <summary>
        /// Verify compact signature
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        private bool VerifyCompact(UInt256 hash, VarType sig)
        {
            if (!IsValid || sig.Length == 0) return false;
            var rkey = FromSignedHash(hash, sig);
            return rkey != null && rkey == this;
        }

        /// <summary>
        /// Verify transaction signature.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="sig"></param>
        /// <returns></returns>
        private bool VerifyTxSig(UInt256 hash, VarType sig)
        {
            return ECKey.Verify(hash, ECDSASignature.FromDER(sig));
        }

        #endregion
    }
}
