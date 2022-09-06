#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Unicode;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys.Base58;
using CafeLib.BsvSharp.Passphrase;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Crypto.Digests;
using CafeLib.Cryptography.BouncyCastle.Crypto.Macs;
using CafeLib.Cryptography.BouncyCastle.Crypto.Parameters;
using CafeLib.Cryptography.Cryptsharp;

namespace CafeLib.BsvSharp.Keys
{
    public class HdPrivateKey : HdKey
    {
        private const string MasterBip32Key = "Bitcoin seed";

        public PrivateKey PrivateKey { get; private set; } = PrivateKey.FromRandom();

        /// <summary>
        /// Sets this extended private key to be a master (depth 0) with the given private key and chaincode and verifies required key paths.
        /// </summary>
        /// <param name="privkey">Master private key.</param>
        /// <param name="chaincode">Master chaincode.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <returns>Returns this key unless required key paths aren't valid for specified key.</returns>
        public HdPrivateKey SetMaster(UInt256 privkey, UInt256 chaincode, IEnumerable<KeyPath> required = null)
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
        public HdPrivateKey SetMaster(UInt512 vOut, IEnumerable<KeyPath> required = null)
        {
            return SetMaster((UInt256)vOut.Span[..32], (UInt256)vOut.Span.Slice(32, 32), required);
        }

        /// <summary>
        /// Sets Bip32 private key.
        /// Uses a single invocation of HMACSHA512 to generate 512 bits of entropy with which to set master private key and chaincode.
        /// </summary>
        /// <param name="hmacData">Sequence of bytes passed as hmacData to HMACSHA512 along with byte encoding of hmacKey.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="hmacKey">Default is current global Kz.MasterBip32Key which may default to "Bitcoin seed".</param>
        /// <returns>Returns this key unless required key paths aren't valid for generated key.</returns>
        public HdPrivateKey SetMasterBip32(byte[] hmacData, IEnumerable<KeyPath> required = null, string hmacKey = MasterBip32Key)
        {
            var vOut = Hashes.HmacSha512(hmacKey.Utf8NormalizedToBytes(), hmacData);
            return SetMaster(vOut, required);
        }

        /// <summary>
        /// Computes 512 bit Bip39 seed.
        /// phrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="phrase">arbitrary phrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Computes 512 bit Bip39 seed.</returns>
        public static UInt512 Bip39Seed(string phrase, string password = "", string passwordPrefix = "mnemonic")
        {
            var mnemonic = phrase.Utf8NormalizedToBytes();
            var salt = $"{passwordPrefix}{password}".Utf8NormalizedToBytes();
            var mac = new HMac(new Sha512Digest());
            mac.Init(new KeyParameter(mnemonic));
            var key = Pbkdf2.ComputeDerivedKey(mac, salt, 2048, 64);
            return new UInt512(key);
        }

        /// <summary>
        /// Returns a new extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="phrase">arbitrary phrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static HdPrivateKey FromWords(string phrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => MasterBip39(phrase, password, required, passwordPrefix);

        /// <summary>
        /// Returns a new extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="phrase">arbitrary phrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static HdPrivateKey MasterBip39(string phrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => new HdPrivateKey().SetMasterBip39(phrase, password, required, passwordPrefix);

        /// <summary>
        /// Sets this extended private key per Bip39.
        /// passphrase, password, and passwordPrefix are converted to bytes using UTF8 KD normal form encoding.
        /// </summary>
        /// <param name="phrase">arbitrary phrase (typically mnemonic words with checksum but not necessarily)</param>
        /// <param name="password">password and passwordPrefix are combined to generate salt bytes.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="passwordPrefix">password and passwordPrefix are combined to generate salt bytes. Default is "mnemonic".</param>
        /// <returns>Returns this key unless required key paths aren't valid for generated key.</returns>
        public HdPrivateKey SetMasterBip39(string phrase, string password = null, IEnumerable<KeyPath> required = null, string passwordPrefix = "mnemonic")
            => SetMasterBip32(Bip39Seed(phrase, password, passwordPrefix), required);

        /// <summary>
        /// Returns a new extended private key to be a master (depth 0) with the given private key and chaincode and verifies required key paths.
        /// </summary>
        /// <param name="privkey">Master private key.</param>
        /// <param name="chaincode">Master chaincode.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static HdPrivateKey Master(UInt256 privkey, UInt256 chaincode, IEnumerable<KeyPath> required = null)
            => new HdPrivateKey().SetMaster(privkey, chaincode, required);

        /// <summary>
        /// Returns a new extended private key to be a master (depth 0) with the private key and chaincode set from the single 512 bit vout parameter.
        /// Master private key will be set to the first 256 bits.
        /// Chaincode will be set from the last 256 bits.
        /// </summary>
        /// <param name="vout">Master private key will be set to the first 256 bits. Chaincode will be set from the last 256 bits.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the specified key or returns null.</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static HdPrivateKey Master(UInt512 vout, IEnumerable<KeyPath> required = null)
            => new HdPrivateKey().SetMaster(vout, required);

        /// <summary>
        /// Returns a new Bip32 private key.
        /// Uses a single invocation of HMACSHA512 to generate 512 bits of entropy with which to set master private key and chaincode.
        /// </summary>
        /// <param name="hmacData">Sequence of bytes passed as hmacData to HMACSHA512 along with byte encoding of hmacKey.</param>
        /// <param name="required">if not null, each key path will be verified as valid on the generated key or returns null.</param>
        /// <param name="hmacKey">Default is current global Kz.MasterBip32Key which may default to "Bitcoin seed".</param>
        /// <returns>Returns new key unless required key paths aren't valid for specified key in which case null is returned.</returns>
        public static HdPrivateKey MasterBip32(byte[] hmacData, IEnumerable<KeyPath> required = null, string hmacKey = MasterBip32Key)
            => new HdPrivateKey().SetMasterBip32(hmacData, required, hmacKey);

        /// <summary>
        /// BIP32 uses "Neuter" to describe adding the extended key information to the public key
        /// associated with an extended private key.
        /// </summary>
        /// <returns></returns>
        public HdPublicKey GetHdPublicKey() => HdPublicKey.FromPrivateKey(this);

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
        public HdPrivateKey Derive(KeyPath kp) => DeriveBase(kp) as HdPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="path">key path</param>
        /// <returns>extended public key</returns>
        public HdPrivateKey Derive(string path) => DeriveBase(new KeyPath(path)) as HdPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        public HdPrivateKey Derive(int index, bool hardened = false) => DeriveBase(index, hardened) as HdPrivateKey;

        /// <summary>
        /// Derives a child hierarchical deterministic public key specified by a key path.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hardened"></param>
        /// <returns></returns>
        protected override HdKey DeriveBase(int index, bool hardened)
        {
            Trace.Assert(index >= 0);
            var cek = new HdPrivateKey {
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

        public override void Decode(ReadOnlyByteSpan code)
        {
            Depth = code[0];
            Fingerprint = BitConverter.ToInt32(code[1..5]);
            Child = (uint)code[5] << 24 | (uint)code[6] << 16 | (uint)code[7] << 8 | code[8];
            ChainCode = new UInt256(code.Slice(9, UInt256.Length));
            PrivateKey.SetData(code.Slice(42, UInt256.Length));
        }

        internal Base58HdPrivateKey ToBase58() => new(this);
        public override string ToString() => ToBase58().ToString();

        public override int GetHashCode() => base.GetHashCode() ^ ToString().GetHashCode();

        public bool Equals(HdPrivateKey o) => o is not null && base.Equals(o) && PrivateKey.Equals(o.PrivateKey);
        public override bool Equals(object obj) => obj is HdPrivateKey key && this == key;

        public static bool operator ==(HdPrivateKey x, HdPrivateKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(HdPrivateKey x, HdPrivateKey y) => !(x == y);
    }
}
