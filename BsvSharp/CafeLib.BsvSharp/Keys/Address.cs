using System;
using System.Linq;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Encodings;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace CafeLib.BsvSharp.Keys
{
    /// <summary>
    /// This class abstracts away the internals of address encoding and provides
    /// a convenient means to both encode and decode information from a bitcoin address.
    ///
    /// Bitcoin addresses are a construct which facilitates interoperability
    /// between different wallets. I.e. an agreement amongst wallet providers to have a
    /// common means of sharing the hashed public key value needed to send someone bitcoin using one
    /// of the standard public-key-based transaction types.
    ///
    /// The Address does not contain a public key, only a hashed value of a public key
    /// which is derived as explained below.
    ///
    /// Bitcoin addresses are not part of the consensus rules of bitcoin.
    ///
    /// Bitcoin addresses are encoded as follows
    /// * 1st byte - indicates the network type which is either MAINNET or TESTNET
    /// * next 20 bytes - the hash value computed by taking the `ripemd160(sha256(PUBLIC_KEY))`
    /// * last 4 bytes  - a checksum value taken from the first four bytes of sha256(sha256(previous_21_bytes))
    /// </summary>
    public class Address : IEquatable<Address>
    {
        private static readonly HexEncoder Hex = Encoders.Hex;
        private static readonly Base58CheckEncoder Base58Check = Encoders.Base58Check;
        private byte[] _bytes;

        /// <summary>
        /// Address default constructor.
        /// </summary>
        private Address()
        {
        }

        /// <summary>
        /// Constructs an Bitcoin address
        /// </summary>
        /// <param name="address">base58encoded Bitcoin address</param>
        public Address(string address)
        {
            FromBase58CheckInternal(address);
        }

        /// <summary>
        /// Version property.
        /// </summary>
        public int Version { get; private set; }
        
        /// <summary>
        /// Public Key Hash.
        /// </summary>
        public UInt160 PubKeyHash => this;

        public AddressType AddressType
        {
            get
            {
                switch (Version)
                {
                    case 0:
                    case 111:
                        return AddressType.PubkeyHash;

                    case 5:
                    case 196:
                        return AddressType.ScriptHash;

                    default:
                        throw new FormatException($"{nameof(Version)} is not a valid address type.");
                }
            }
        }

        public NetworkType NetworkType
        {
            get
            {
                switch (Version)
                {
                    case 0:
                    case 5:
                        return NetworkType.Main;

                    case 111:
                    case 196:
                        return NetworkType.Test;

                    default:
                        throw new FormatException($"{nameof(Version)} is not a valid network type.");
                }
            }
        }

        /// <summary>
        /// Constructs a new Address object from a base58-encoded string.
        ///
        /// Base58-encoded strings are the "standard" means of sharing bitcoin addresses amongst
        /// wallets. This is typically done either using the string of directly, or by using a
        /// QR-encoded form of this string.
        ///
        /// Typically, if someone is sharing their bitcoin address with you, this is the method
        /// you would use to instantiate an [Address] object for use with [Transaction] objects.
        /// </summary>
        /// <param name="base58Address"></param>
        /// <returns></returns>
        public static Address FromBase58(string base58Address) 
        {
            if (base58Address.Length == 25 || base58Address.Length == 34)
            {
                var address = new Address();
                address.FromBase58CheckInternal(base58Address);
                return address;
            }

            throw new FormatException($"Address should be 25 or 34 bytes long. Only [{base58Address.Length}] bytes long.");
        }

        /// <summary>
        /// Constructs a new address instance from a public key.
        /// </summary>
        /// <param name="hexPubKey">hexadecimal encoding of a public key</param>
        /// <returns>address</returns>
        public static Address FromHex(string hexPubKey)
        {
            var address = new Address();
            address.FromHexInternal(hexPubKey);
            return address;
        }

        /// <summary>
        /// Constructs a new P2SH Address object from a script
        /// </summary>
        /// <param name="script"></param>
        /// <returns>address</returns>
        public static Address FromScript(Script script)
        {
            var address = new Address();
            address.FromScriptInternal(script);
            return address;
        }


        /// <summary>
        /// Serialize this address object to a base58-encoded string.
        /// This method is an alias for the [toBase58()] method
        /// </summary>
        /// <returns>base58 encoded address</returns>
        public override string ToString() => Base58Check.Encode(_bytes);

        /// <summary>
        /// Returns the public key hash `ripemd160(sha256(public_key))` encoded as a hexadecimal string
        /// </summary>
        /// <returns>encoded public key hash</returns>
        public string ToHex() => Hex.Encode(_bytes);
        
        public override int GetHashCode() => _bytes.GetHashCodeOfValues();

        public bool Equals(Address o) => o is not null && _bytes.SequenceEqual(o._bytes);
        public override bool Equals(object obj) => Equals((Address)obj);

        public static implicit operator UInt160(Address rhs) => new UInt160(rhs._bytes[1..]);
        public static bool operator ==(Address x, Address y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(Address x, Address y) => !(x == y);

        #region Helpers

        private void FromBase58CheckInternal(string source)
        {
            _bytes = Base58Check.Decode(source);
            Version = _bytes[0];
        }

        private void FromHexInternal(string hexPubKey)
        {
            Version = RootService.Network.PublicKeyAddress[0];
            _bytes = new[]{(byte)Version}.Concat(Hex.Decode(hexPubKey).Hash160().ToArray());
        }

        private void FromScriptInternal(Script script)
        {
            Version = RootService.Network.PublicKeyAddress[0];
            _bytes = new[]{(byte)Version}.Concat(Hex.Decode(script.ToHexString()).Hash160().ToArray());
        }

        #endregion
    }
}
