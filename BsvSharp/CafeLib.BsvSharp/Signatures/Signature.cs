using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Signatures
{
    public struct Signature : IEquatable<Signature>
    {
        private byte[] _data;
        private readonly uint _hashType;

        //private const int SignatureSize = 64;

        /// <summary>
        /// Signature data.
        /// </summary>
        internal ReadOnlyByteSpan Data => _data ??= Array.Empty<byte>();

        /// <summary>
        /// Null signature.
        /// </summary>
        public static readonly Signature Empty = new Signature();

        /// <summary>
        /// Signature constructor.
        /// </summary>
        /// <param name="signature">signature as byte array</param>
        /// <param name="hashType"></param>
        public Signature(byte[] signature, SignatureHashType hashType = null)
        {
            _data = signature;
            _hashType = hashType?.RawSigHashType ?? (uint)SignatureHashEnum.Unsupported;
        }

        /// <summary>
        /// Determine whether a signature is normalized (lower-S).
        /// </summary>
        /// <returns></returns>
        public bool IsLowS()
        {
            try
            {
                return ECDSASignature.FromDER(_data).IsLowS;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determine whether the transaction data is encoded.
        /// </summary>
        /// <returns></returns>

        public bool IsTxDerEncoding() => IsTxDerEncoding(Data);

        public static Signature FromHex(string hex) => new Signature(Encoders.Hex.Decode(hex));

        public static Signature FromBase64(string base64) => new Signature(Encoders.Base64.Decode(base64));

        /// <summary>
        /// Determine whether a signature is normalized (lower-S).
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool IsLowS(ReadOnlyByteSpan signature) => ECDSASignature.FromDER(signature).IsLowS;

        /// <summary>
        /// Returns the signature data.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray() => Data;

        /// <summary>
        /// Convert to transaction signature format.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Appends the hash type to the DER formatted signature.</remarks>
        public Signature ToTxFormat()
        {
            return _hashType == (uint)SignatureHashEnum.Unsupported 
                ? this 
                : new Signature(_data.Concat(new[] { (byte)_hashType }));
        }

        /// <summary>
        /// A canonical signature exists of: &lt;30&gt; &lt;total len&gt; &lt;02&gt; &lt;len R&gt; &lt;R&gt; &lt;02&gt;
        /// &lt;len S&gt; &lt;S&gt; &lt;hashtype&gt;, where R and S are not negative (their first byte has its
        /// highest bit not set), and not excessively padded (do not start with a 0 byte,
        /// unless an otherwise negative number follows, in which case a single 0 byte is
        /// necessary and even required).
        /// 
        /// See https://bitcointalk.org/index.php?topic=8392.msg127623#msg127623
        /// 
        /// This function is consensus-critical since BIP66.
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool IsTxDerEncoding(ReadOnlyByteSpan signature)
        {
            // Format: 0x30 [total-length] 0x02 [R-length] [R] 0x02 [S-length] [S]
            // [sighash]
            // * total-length: 1-byte length descriptor of everything that follows,
            // excluding the sighash byte.
            // * R-length: 1-byte length descriptor of the R value that follows.
            // * R: arbitrary-length big-endian encoded R value. It must use the
            // shortest possible encoding for a positive integers (which means no null
            // bytes at the start, except a single one when the next byte has its
            // highest bit set).
            // * S-length: 1-byte length descriptor of the S value that follows.
            // * S: arbitrary-length big-endian encoded S value. The same rules apply.
            // * sighash: 1-byte value indicating what data is hashed (not part of the
            // DER signature)

            // Minimum and maximum size constraints.
            var length = signature.Length;
            if (length < 9) return false;
            if (length > 73) return false;

            // A signature is of type 0x30 (compound).
            if (signature[0] != 0x30) return false;

            // Make sure the length covers the entire signature.
            if (signature[1] != signature.Length - 3) return false;

            // Extract the length of the R element.
            var lenR = signature[3];

            // Make sure the length of the S element is still inside the signature.
            if (5 + lenR >= signature.Length) return false;

            // Extract the length of the S element.
            var lenS = signature[5 + lenR];

            // Verify that the length of the signature matches the sum of the length
            // of the elements.
            if (lenR + lenS + 7 != signature.Length) return false;

            // Check whether the R element is an integer.
            if (signature[2] != 0x02) return false;

            // Zero-length integers are not allowed for R.
            if (lenR == 0) return false;

            // Negative numbers are not allowed for R.
            if ((signature[4] & 0x80) != 0) return false;

            // Null bytes at the start of R are not allowed, unless R would otherwise be
            // interpreted as a negative number.
            if (lenR > 1 && (signature[4] == 0) && (signature[5] & 0x80) == 0) return false;

            // Check whether the S element is an integer.
            if (signature[lenR + 4] != 0x02) return false;

            // Zero-length integers are not allowed for S.
            if (lenS == 0) return false;

            // Negative numbers are not allowed for S.
            if ((signature[lenR + 6] & 0x80) != 0) return false;

            // Null bytes at the start of S are not allowed, unless S would otherwise be
            // interpreted as a negative number.
            return lenS <= 1 || (signature[lenR + 6] != 0x00) || (signature[lenR + 7] & 0x80) != 0;
        }

        public bool Equals(Signature other)
        {
            return Data.SequenceCompareTo(other.Data) == 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Signature other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashType.GetHashCode();
        }

        public override string ToString()
        {
            return Encoders.Base64.Encode(_data);
        }
    }
}