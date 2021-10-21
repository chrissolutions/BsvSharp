#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Crypto.Digests;
using CafeLib.Cryptography.BouncyCastle.Crypto.Macs;
using CafeLib.Cryptography.BouncyCastle.Crypto.Parameters;
using CafeLib.Cryptography.Cryptsharp;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace CafeLib.BsvSharp.Keys
{
    public class ExtPrivateKey : ExtKey
    {
        private const string MasterBip32Key = "Bitcoin seed";

        public PrivateKey PrivateKey { get; private set; } = new PrivateKey();

        /// <summary>
        /// Sets this extended private key to be a master (depth 0) with the given private key and chaincode and verifies required key paths.
        /// </summary>
        /// <param name="privkey">Master private key.</param>
        /// <param name="chaincode">Master chaincode.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <returns>Returns this key unless required key paths aren't valid for specified key.</returns>
        public ExtPrivateKey SetMaster(UInt256 privkey, UInt256 chaincode, IEnumerable<KeyPath> required = null)
        {
            PrivateKey = new PrivateKey(privkey);
            ChainCode = chaincode;
            Depth = 0;
            Child = 0;
            Fingerprint = 0;

            if (PrivateKey == null || !PrivateKey.IsValid) return null;

            // Verify that all the required derivation paths yield valid keys.
            if (required == null) return this;
            return required.Any(r => Derive(r) == null) ? null : this;
        }

        /// <summary>
        /// Sets this extended private key to be a master (depth 0) with the private key and chaincode set from the single 512 bit vout parameter.
        /// Master private key will be set to the first 256 bits.
        /// Chaincode will be set from the last 256 bits.
        /// </summary>
        /// <param name="vOut">Master private key will be set to the first 256 bits. Chaincode will be set from the last 256 bits.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the specified key or returns null.</param>
        /// <returns>Returns this key unless required key paths aren't valid for specified key.</returns>
        public ExtPrivateKey SetMaster(UInt512 vOut, IEnumerable<KeyPath> required = null)
        {
            return SetMaster((UInt256)vOut.Span.Slice(0, 32), (UInt256)vOut.Span.Slice(32, 32), required);
        }

        /// <summary>
        /// Sets Bip32 private key.
        /// Uses a single invocation of HMACSHA512 to generate 512 bits of entropy with which to set master private key and chaincode.
        /// </summary>
        /// <param name="hmacData">Sequence of bytes passed as hmacData to HMACSHA512 along with byte encoding of hmacKey.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="hmacKey">Default is current global Kz.MasterBip32Key which may default to "Bitcoin seed".</param>
        /// <returns>Returns this key unless required key paths aren't valid for generated key.</returns>
        public ExtPrivateKey SetMasterBip32(byte[] hmacData, IEnumerable<KeyPath> required = null, string hmacKey = null)
        {
            hmacKey ??= MasterBip32Key;
            var vOut = Hashes.HmacSha512(hmacKey.Utf8NormalizedToBytes(), hmacData);
            return SetMaster(vOut, required);
        }

        /// <summary>
        /// Computes 512 bit Bip39 seed.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="passphrase">arbitrary passphrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Computes 512 bit Bip39 seed.</returns>
        public static UInt512 Bip39Seed(string passphrase, string password = null, string passwordPrefix = "mnemonic")
        {
            var salt = $"{passwordPrefix}{password}".Utf8NormalizedToBytes();
            var bytes = passphrase.Utf8NormalizedToBytes();

            var mac = new HMac(new Sha512Digest());
            mac.Init(new KeyParameter(bytes));
            var key = Pbkdf2.ComputeDerivedKey(mac, salt, 2048, 64);
            return new UInt512(key);
        }

        /// <summary>
        /// Returns a new extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="passphrase">arbitrary passphrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static ExtPrivateKey FromWords(string passphrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => MasterBip39(passphrase, password, required, passwordPrefix);

        /// <summary>
        /// Returns a new extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="passphrase">arbitrary passphrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static ExtPrivateKey MasterBip39(string passphrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => new ExtPrivateKey().SetMasterBip39(passphrase, password, required, passwordPrefix);

        /// <summary>
        /// Sets this extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="passphrase">arbitrary passphrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns this key unless required key paths aren't valid for generated key.</returns>
        public ExtPrivateKey SetMasterBip39(string passphrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => SetMasterBip32(Bip39Seed(passphrase, password, passwordPrefix), required);

        /// <summary>
        /// Returns a new extended private key to be a master (depth 0) with the given private key and chaincode and verifies required key paths.
        /// </summary>
        /// <param name="privkey">Master private key.</param>
        /// <param name="chaincode">Master chaincode.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static ExtPrivateKey Master(UInt256 privkey, UInt256 chaincode, IEnumerable<KeyPath> required = null)
            => new ExtPrivateKey().SetMaster(privkey, chaincode, required);

        /// <summary>
        /// Returns a new extended private key to be a master (depth 0) with the private key and chaincode set from the single 512 bit vout parameter.
        /// Master private key will be set to the first 256 bits.
        /// Chaincode will be set from the last 256 bits.
        /// </summary>
        /// <param name="vout">Master private key will be set to the first 256 bits. Chaincode will be set from the last 256 bits.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the specified key or returns null.</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static ExtPrivateKey Master(UInt512 vout, IEnumerable<KeyPath> required = null)
            => new ExtPrivateKey().SetMaster(vout, required);

        /// <summary>
        /// Returns a new Bip32 private key.
        /// Uses a single invocation of HMACSHA512 to generate 512 bits of entropy with which to set master private key and chaincode.
        /// </summary>
        /// <param name="hmacData">Sequence of bytes passed as hmacData to HMACSHA512 along with byte encoding of hmacKey.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="hmacKey">Default is current global Kz.MasterBip32Key which may default to "Bitcoin seed".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static ExtPrivateKey MasterBip32(byte[] hmacData, IEnumerable<KeyPath> required = null, string hmacKey = null)
            => new ExtPrivateKey().SetMasterBip32(hmacData, required, hmacKey);

        /// <summary>
        /// BIP32 uses "Neuter" to describe adding the extended key information to the public key
        /// associated with an extended private key.
        /// </summary>
        /// <returns></returns>
        public ExtPublicKey GetExtPublicKey() => ExtPublicKey.FromPrivateKey(this);

        /// <summary>
        /// Get public key from ExtPrivateKey.
        /// </summary>
        /// <returns></returns>
        public PublicKey GetPublicKey() => PrivateKey.CreatePublicKey();

        /// <summary>
        /// Computes the private key specified by a key path.
        /// At each derivation, there's a small chance the index specified will fail.
        /// If any generation fails, null is returned.
        /// </summary>
        /// <param name="kp"></param>
        /// <returns>null on derivation failure. Otherwise the derived private key.</returns>
        public ExtPrivateKey Derive(KeyPath kp) => DeriveBase(kp) as ExtPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="path">key path</param>
        /// <returns>extended public key</returns>
        public ExtPrivateKey Derive(string path) => DeriveBase(new KeyPath(path)) as ExtPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        public ExtPrivateKey Derive(int index, bool hardened = false) => DeriveBase(index, hardened) as ExtPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        protected override ExtKey DeriveBase(int index, bool hardened)
        {
            Trace.Assert(index >= 0);
            var cek = new ExtPrivateKey {
                Depth = (byte)(Depth + 1),
                Child = (uint)index | (hardened ? HardenedBit : 0),
                Fingerprint = BitConverter.ToInt32(PrivateKey.CreatePublicKey().GetId().Span.Slice(0, 4))
            };

            (cek.PrivateKey, cek.ChainCode) = PrivateKey.Derive(cek.Child, ChainCode);
            return cek;
        }

        public override void Encode(ByteSpan code)
        {
            code[0] = Depth;
            var s = Fingerprint.AsSpan();
            s.CopyTo(code.Slice(1, 4));
            code[5] = (byte)((Child >> 24) & 0xFF);
            code[6] = (byte)((Child >> 16) & 0xFF);
            code[7] = (byte)((Child >> 8) & 0xFF);
            code[8] = (byte)(Child & 0xFF);
            ChainCode.Span.CopyTo(code.Slice(9, UInt256.Length));
            code[41] = 0;
            var key = PrivateKey.ToArray();
            Debug.Assert(key.Length == UInt256.Length);
            key.CopyTo(code.Slice(42, UInt256.Length));
        }

        public void Decode(ReadOnlyByteSpan code)
        {
            Depth = code[0];
            Fingerprint = BitConverter.ToInt32(code[1..5]);
            Child = (uint)code[5] << 24 | (uint)code[6] << 16 | (uint)code[7] << 8 | code[8];
            ChainCode = new UInt256(code.Slice(9, UInt256.Length));
            PrivateKey.SetData(code.Slice(42, UInt256.Length));
        }

        public Base58ExtPrivateKey ToBase58() => new Base58ExtPrivateKey(this);
        public override string ToString() => ToBase58().ToString();

        public override int GetHashCode() => base.GetHashCode() ^ PrivateKey.GetHashCode();

        public bool Equals(ExtPrivateKey o) => !(o is null) && base.Equals(o) && PrivateKey.Equals(o.PrivateKey);
        public override bool Equals(object obj) => obj is ExtPrivateKey key && this == key;

        public static bool operator ==(ExtPrivateKey x, ExtPrivateKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(ExtPrivateKey x, ExtPrivateKey y) => !(x == y);
    }
}
