﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using CafeLib.BsvSharp.Numerics;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Extensions
{
    public static class NumericsExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static ByteSpan AsSpan(this int i) => BitConverter.GetBytes(i);

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">integer</param>
        /// <param name="bigEndian">true if big endian flag; otherwise little ending</param>
        /// <returns></returns>
        public static ReadOnlyByteSpan AsReadOnlySpan(this int i, bool bigEndian = false)
        {
            var bytes = BitConverter.GetBytes(i);

            if (BitConverter.IsLittleEndian == bigEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to integer</param>
        /// <returns>ByteSpan</returns>
        public static ByteSpan AsSpan(this ref uint i)
        {
            unsafe
            {
                fixed (uint* p = &i)
                {
                    byte* pb = (byte*)p;
                    var bytes = new Span<byte>(pb, sizeof(int));
                    return bytes;
                }
            }
        }

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to integer</param>
        /// <returns>ReadOnlyByteSpan</returns>
        public static ReadOnlyByteSpan AsReadOnlySpan(this ref uint i) => i.AsSpan();

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to long integer</param>
        /// <returns>ByteSpan</returns>
        public static ByteSpan AsSpan(this ref long i)
        {
            unsafe
            {
                fixed (long* p = &i)
                {
                    var pb = (byte*)p;
                    var bytes = new Span<byte>(pb, sizeof(long));
                    return bytes;
                }
            }
        }

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to long integer</param>
        /// <returns>readonly byte span</returns>
        public static ReadOnlyByteSpan AsReadOnlySpan(this ref long i) => i.AsSpan();

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to integer</param>
        /// <returns>span of bytes</returns>
        public static Span<byte> AsSpan(this ref ulong i)
        {
            unsafe
            {
                fixed (ulong* p = &i)
                {
                    byte* pb = (byte*)p;
                    var bytes = new Span<byte>(pb, sizeof(ulong));
                    return bytes;
                }
            }
        }

        /// <summary>
        /// Returns access to an integer as a span of bytes.
        /// Reflects the endian of the underlying implementation.
        /// </summary>
        /// <param name="i">reference to long integer</param>
        /// <returns>readonly byte span</returns>
        public static ReadOnlyByteSpan AsReadOnlySpan(this ref ulong i) => i.AsSpan();

        /// <summary>
        /// Convert bytes to UInt256 
        /// </summary>
        /// <param name="bytes">byte array</param>
        /// <returns>UInt256</returns>
        public static UInt256 AsUInt256(this byte[] bytes) => (UInt256)(ByteSpan)bytes;

        /// <summary>
        /// Convert integer to array of bytes
        /// </summary>
        /// <param name="i">integer value</param>
        /// <returns>byte array</returns>
        public static byte[] AsVarIntBytes(this int i) => ((VarInt)i).ToArray();

        /// <summary>
        /// Convert long integer to array of bytes
        /// </summary>
        /// <param name="l">long value</param>
        /// <returns>byte array</returns>
        public static byte[] AsVarIntBytes(this long l) => ((VarInt)l).ToArray();

        /// <summary>
        /// Copy to UInt160 from byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this byte[] source, ref UInt160 destination)
        {
            ((ReadOnlyByteSpan)source).CopyTo(destination.Span);
        }

        /// <summary>
        /// Copy to UInt256 from byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this byte[] source, ref UInt256 destination)
        {
            ((ReadOnlyByteSpan)source).CopyTo(destination.Span);
        }

        /// <summary>
        /// Copy to UInt512 from byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this byte[] source, ref UInt512 destination)
        {
            ((ReadOnlyByteSpan)source).CopyTo(destination.Span);
        }

        /// <summary>
        /// Reads an <see cref="UInt256"/> as in bitcoin VarInt format.
        /// </summary>
        /// <param name="reader">byte sequence reader</param>
        /// <param name="destination">UInt256 destination</param>
        /// <param name="reverse">reverse byte pattern</param>
        /// <returns>UInt256 value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadUInt256(this ref ByteSequenceReader reader, ref UInt256 destination, bool reverse = false)
        {
            var span = destination.Span;
            if (!reader.TryCopyTo(span)) return false;
            if (reverse) span.Reverse();
            reader.Advance(span.Length);
            return true;
        }

        /// <summary>
        /// Reads an <see cref="UInt64"/> as in bitcoin Variant format.
        /// </summary>
        /// <returns>False if there wasn't enough data for an <see cref="UInt64"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryReadVariant(this ref ByteSequenceReader reader, out long value)
        {
            value = 0L;

            var b = reader.Data.TryRead(out var b0);
            if (!b) return false;

            switch (b0)
            {
                case <= 0xfc:
                    value = b0;
                    break;

                case 0xfd:
                    b = reader.Data.TryReadLittleEndian(out short v16);
                    value = v16;
                    break;

                case 0xfe:
                    b = reader.Data.TryReadLittleEndian(out int v32);
                    value = v32;
                    break;

                default:
                    b = reader.Data.TryReadLittleEndian(out value);
                    break;
            }

            return b;
        }

    }
}
