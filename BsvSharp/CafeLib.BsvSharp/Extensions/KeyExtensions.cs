using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Signatures;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;
using CafeLib.Cryptography.BouncyCastle.Asn1.X9;

namespace CafeLib.BsvSharp.Extensions
{
    public static class KeyExtensions
    {
        // ReSharper disable once InconsistentNaming
        private static readonly X9ECParameters Secp256k1 = ECKey.CreateCurve();

        //private int SerializedPublicKeyLength => IsCompressed ? Secp256k1.SERIALIZED_COMPRESSED_PUBKEY_LENGTH : Secp256k1.SERIALIZED_UNCOMPRESSED_PUBKEY_LENGTH;

        public static PublicKey CreatePublicKey(this PrivateKey privateKey)
        {
            var ecKey = new ECKey(privateKey.ToArray(), true);
            var q = ecKey.GetPublicKeyParameters().Q;

            //Pub key (q) is composed into X and Y, the compressed form only include X, which can derive Y along with 02 or 03 prepent depending on whether Y in even or odd.
            var result = Secp256k1.Curve.CreatePoint(q.X.ToBigInteger(), q.Y.ToBigInteger(), privateKey.IsCompressed).GetEncoded();
            return new PublicKey(result);
        }

        /// <summary>
        /// Create a signature from private key
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="hash">hash to sign</param>
        /// <param name="hashType"></param>
        /// <returns>signature bytes</returns>
        public static Signature SignTxSignature(this PrivateKey privateKey, UInt256 hash, SignatureHashType hashType = null)
        {
            var signer = new DeterministicECDSA();
            signer.SetPrivateKey(privateKey.ECKey.PrivateKey);
            var sig = ECDSASignature.FromDER(signer.SignHash(hash)).MakeCanonical();
            return new Signature(sig.ToDER(), hashType);
        }

        /// <summary>
        /// Sign the message.
        /// </summary>
        /// <param name="key">private key</param>
        /// <param name="message">message to sign</param>
        /// <returns>signature</returns>
        public static Signature SignMessage(this PrivateKey key, string message)
        {
            return key.SignMessageHash(GetMessageHash(message.Utf8ToBytes()));
        }

        /// <summary>
        /// Sign message hash
        /// </summary>
        /// <param name="key">private key</param>
        /// <param name="hash">message hash</param>
        /// <returns>signature</returns>
        public static Signature SignMessageHash(this PrivateKey key, UInt256 hash)
        {
            return new Signature(key.SignCompact(hash));
        }

        public static bool VerifyMessage(this PublicKey key, string message, Signature signature)
        {
            var rkey = PublicKey.FromSignedMessage(message, signature.ToString());
            return rkey != null && rkey == key;
        }

        public static bool VerifyMessage(this PublicKey key, string message, string signature)
        {
            var rkey = PublicKey.FromSignedMessage(message, signature);
            return rkey != null && rkey == key;
        }

        public static bool VerifyMessage(this UInt160 keyId, UInt256 message, string signature)
        {
            var rkey = PublicKey.FromSignedHash(GetMessageHash(message.Span), Encoders.Base64.Decode(signature));
            return rkey != null && rkey.GetId() == keyId;
        }

        public static bool VerifyMessage(this UInt160 keyId, string message, string signature)
            => VerifyMessage(keyId, (UInt256)message.Utf8ToBytes(), signature);

        #region Helpers

        internal static UInt256 GetMessageHash(ReadOnlyByteSpan message)
        {
            const string bitcoinSignedMessageHeader = "Bitcoin Signed Message:\n";
            var bitcoinSignedMessageHeaderBytes = Encoders.Utf8.Decode(bitcoinSignedMessageHeader);
            var msgBytes = new [] {(byte)bitcoinSignedMessageHeaderBytes.Length}.Concat(bitcoinSignedMessageHeaderBytes, new VarInt((ulong)message.Length).ToArray(), message);
            return Hashes.Hash256(msgBytes);
        }

        #endregion
    }
}
