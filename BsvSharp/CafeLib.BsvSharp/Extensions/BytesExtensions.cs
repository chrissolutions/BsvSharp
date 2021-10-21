using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Extensions
{
    public static class BytesExtensions
    {
        public static int AggregateHashCode(this IEnumerable<byte> bytes) => bytes?.Aggregate(17, (current, b) => current * 31 + b) ?? 0;

        public static ByteSpan Slice(this byte[] a, int start) => a.AsSpan().Slice(start);
        public static ByteSpan Slice(this byte[] a, int start, int length) => a.AsSpan().Slice(start, length);

        public static string ToHex(this byte[] a) => Encoders.Hex.Encode(a);
        public static string ToHexReverse(this byte[] a) => Encoders.HexReverse.Encode(a);

        /// <summary>
        /// Copy to byte array from another byte array.
        /// </summary>
        /// <param name="source">source byte array</param>
        /// <param name="destination">destination byte array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this byte[] source, ref byte[] destination)
        {
            ((ReadOnlyByteSpan)source).CopyTo(destination);
        }

        public static int GetHashCodeOfValues(this IEnumerable<byte> a)
        {
            return a?.Aggregate(17, (current, b) => current * 31 + b) ?? 0;
        }

        public static UInt160 Hash160(this byte[] data)
        {
            return ((ReadOnlyByteSpan)data).Sha1();
        }

        public static UInt256 Hash256(this byte[] data)
        {
            return ((ReadOnlyByteSpan)data).Sha256();
        }

        public static UInt512 Hash512(this byte[] data)
        {
            return ((ReadOnlyByteSpan)data).Sha512();
        }
    }
}
