#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Buffers;
using CafeLib.Core.Buffers.Arrays;

namespace CafeLib.BsvSharp.Numerics
{
    public struct VarType : IEquatable<VarType>
    {
        private ByteArrayBuffer _buffer;

        private ByteArrayBuffer Buffer => _buffer ??= new ByteArrayBuffer();

        public VarType(ReadOnlyByteSpan bytes)
        {
            _buffer = new ByteArrayBuffer(bytes);
        }

        public static readonly VarType Empty = new VarType();
        public static readonly VarType Zero = Empty;
        public static readonly VarType False = Empty;
        public static readonly VarType True = new VarType(new byte[] { 1 });

        public bool IsEmpty => Length == 0;
        public int Length => Buffer.Length;
        public byte FirstByte => Buffer[0];
        public byte LastByte => Buffer[Length - 1];


        public VarType Slice(int start, int length) => new VarType(Buffer.Span.Slice(start, length));

        public override string ToString() => Encoders.Hex.EncodeSpan(Buffer.Span);

        private byte[] ToArray() => Buffer.Span.ToArray();

        public void CopyTo(byte[] bytes) => Buffer.Span.CopyTo(bytes);

        internal ReadOnlyByteSpan Span => Buffer.Span;

        /// <summary>
        /// Return first four bytes as a big endian integer.
        /// </summary>
        /// <returns></returns>
        public uint AsUInt32BigEndian() => BinaryPrimitives.TryReadUInt32BigEndian(Buffer.Span, out var v) ? v : throw new InvalidOperationException();

        public bool ToBool() => Buffer.Any(x => x != 0 && x != 0x80);

        public int ToInt32() => new ScriptNum(Span).GetInt();

        public VarType BitAnd(VarType b)
        {
            if (Length != b.Length) throw new InvalidOperationException();
            var sa = Span;
            var sb = b.Span;
            var r = new byte[sa.Length];
            for (var i = 0; i < sa.Length; i++)
            {
                r[i] = (byte)(sa[i] & sb[i]);
            }
            return new VarType(r);
        }

        public VarType BitOr(VarType b)
        {
            if (Length != b.Length) throw new InvalidOperationException();
            var sa = Span;
            var sb = b.Span;
            var r = new byte[sa.Length];
            for (var i = 0; i < sa.Length; i++)
            {
                r[i] = (byte)(sa[i] | sb[i]);
            }
            return new VarType(r);
        }

        public VarType BitXor(VarType b)
        {
            if (Length != b.Length) throw new InvalidOperationException();
            var sa = Span;
            var sb = b.Span;
            var r = new byte[sa.Length];
            for (var i = 0; i < sa.Length; i++)
            {
                r[i] = (byte)(sa[i] ^ sb[i]);
            }
            return new VarType(r);
        }

        public VarType BitInvert()
        {
            var sa = Span;
            var r = new byte[sa.Length];
            for (var i = 0; i < sa.Length; i++)
            {
                r[i] = (byte)(~sa[i]);
            }
            return new VarType(r);
        }

        private static readonly byte[] MaskLShift = { 0xFF, 0x7F, 0x3F, 0x1F, 0x0F, 0x07, 0x03, 0x01 };
        private static readonly byte[] MaskRShift = { 0xFF, 0xFE, 0xFC, 0xF8, 0xF0, 0xE0, 0xC0, 0x80 };

        public VarType LShift(int n)
        {
            var bitShift = n % 8;
            var byteShift = n / 8;

            var mask = MaskLShift[bitShift];
            var overflowMask = (byte)~mask;

            var x = Span;
            var r = new byte[Length];
            for (int i = r.Length - 1; i >= 0; i--)
            {
                int k = i - byteShift;
                if (k >= 0)
                {
                    var val = (byte)(x[i] & mask);
                    val <<= bitShift;
                    r[k] |= val;
                }

                if (k - 1 >= 0)
                {
                    var carryVal = (byte)(x[i] & overflowMask);
                    carryVal >>= 8 - bitShift;
                    r[k - 1] |= carryVal;
                }
            }
            return new VarType(r);
        }

        public VarType RShift(int n)
        {
            var bitShift = n % 8;
            var byteShift = n / 8;

            var mask = MaskRShift[bitShift];
            var overflowMask = (byte)~mask;

            var x = Span;
            var r = new byte[Length];
            for (int i = 0; i < r.Length; i++)
            {
                var k = i + byteShift;
                if (k < r.Length)
                {
                    var val = (byte)(x[i] & mask);
                    val >>= bitShift;
                    r[k] |= val;
                }

                if (k + 1 < r.Length)
                {
                    var carryVal = (byte)(x[i] & overflowMask);
                    carryVal <<= 8 - bitShift;
                    r[k + 1] |= carryVal;
                }
            }
            return new VarType(r);
        }

        public bool BitEquals(VarType x2)
        {
            if (Length != x2.Length) return false;
            var s1 = Span;
            var s2 = x2.Span;
            for (var i = 0; i < s1.Length; i++)
                if (s1[i] != s2[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Returns this value as a number resized to a specified size.
        /// If size is null, attempts to encode as the minimum size.
        /// The current value's most significant bit is assumed to be the sign bit.
        /// There can be any amount of zero byte padding between the sign bit and
        /// the first set numeric value bit.
        /// A single "padding" zero byte is required if the x80 bit is set on the byte following it (to keep the value positive).
        /// Effectively adjusts the position of the sign bit after adjusting zero byte padding.
        /// </summary>
        /// <param name="size">Target size or null to minimally encode.</param>
        /// <returns>Tuple of (bin, ok) where:
        /// bin may be original value if size matches, otherwise new storage is allocated. None on failure.
        /// ok is true on success. False if can't fit in size.
        /// </returns>
        private (VarType bin, bool ok) NumResize(uint? size = null)
        {
            var data = Span;
            var (tooLong, isNeg, extraBytes) = ScriptNum.EvaluateAsNum(data);

            if (size == null && tooLong) goto fail;

            var length = (uint)data.Length - extraBytes;
            size ??= length;

            if (length > size) goto fail;

            if (Length == size) return (this, true);

            var bytes = new byte[(int)size];

            if (size > 0)
            {
                data.Slice(0, (int)length).CopyTo(bytes.AsSpan());

                // Remove the sign bit, add padding 0x00 bytes, restore the sign bit.
                // If number is positive, they start cleared, nothing to do.
                if (isNeg)
                {
                    // Move the set sign bit.
                    // Only clear the old bit in new array if we copied the byte.
                    if (extraBytes == 0) bytes[length - 1] &= 0x7f;
                    // Only set the new bit in the new array if the value isn't zero.
                    // Always convert negative zero to positive zero.
                    if (extraBytes < data.Length) bytes[^1] |= 0x80;
                }
            }

            return (new VarType(bytes), true);

            fail:
            return (Empty, false);
        }

        /// <summary>
        /// Returns this value as a number resized to a specified size.
        /// The current value's most significant bit is assumed to be the sign bit.
        /// There can be any amount of zero byte padding between the sign bit and
        /// the first set numeric value bit.
        /// A single "padding" zero byte is required if the x80 bit is set on the byte following it (to keep the value positive).
        /// Effectively adjusts the position of the sign bit after adjusting zero byte padding.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>Tuple of (bin, ok) where:
        /// bin may be original value if size matches, otherwise new storage is allocated. None on failure.
        /// ok is true on success. False if can't fit in size.
        /// </returns>
        public (VarType bin, bool ok) Num2Bin(uint size) => NumResize(size);

        /// <summary>
        /// Returns this value as a number resized to the minimal size.
        /// The current value's most significant bit is assumed to be the sign bit.
        /// There can be any amount of zero byte padding between the sign bit and
        /// the first set numeric value bit.
        /// A single "padding" zero byte is required if the x80 bit is set on the byte following it (to keep the value positive).
        /// Effectively adjusts the position of the sign bit after adjusting zero byte padding.
        /// Fails if length of minimally encoded value exceeds the current maximum number size: KzScriptNum.MAXIMUM_ELEMENT_SIZE.
        /// </summary>
        /// <returns>Tuple of (num, ok) where:
        /// num may be original value if already minimally encoded, otherwise new storage is allocated. None on failure.
        /// ok is true on success. False if can't fit in KzScriptNum.MAXIMUM_ELEMENT_SIZE.
        /// </returns>
        public (VarType num, bool ok) Bin2Num() => NumResize();

        /// <summary>
        /// Assumes size is greater than current length.
        /// Copies bytes to new larger array and moves the sign bit.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public VarType SignExtend(int size)
        {
            Trace.Assert(size >= Length);
            var r = new byte[size];
            var s = Span;
            s.CopyTo(r);
            var isNeg = (s[^1] & 0x80) != 0;
            r[s.Length - 1] &= 0x7f;
            if (isNeg) r[^1] |= 0x80;
            return new VarType(r);
        }

        public VarType Cat(VarType vch2)
        {
            var r = new byte[Length + vch2.Length];
            var s1 = Span;
            var s2 = vch2.Span;
            var sr = r.AsSpan();
            s1.CopyTo(sr);
            s2.CopyTo(sr[s1.Length..]);
            return new VarType(r);
        }

        public (VarType x1, VarType x2) Split(int position)
        {
            var s = Span;
            var x1 = new VarType(s.Slice(0, position).ToArray());
            var x2 = new VarType(s.Slice(position).ToArray());
            return (x1, x2);
        }

        public override int GetHashCode() => Buffer.GetHashCode();

        public override bool Equals(object obj) => obj is VarType type && this == type;
        public bool Equals(VarType rhs) => Buffer.Span.SequenceEqual(rhs.Buffer.Span);

        public static implicit operator VarType(byte[] rhs) => new VarType(rhs);
        public static implicit operator byte[](VarType rhs) => rhs.ToArray();

        public static implicit operator bool(VarType rhs) => rhs.ToBool();

        public static implicit operator ReadOnlyByteMemory(VarType rhs) => rhs.ToArray();
        public static implicit operator ReadOnlyByteSpan(VarType rhs) => rhs.Span;

        public static bool operator ==(VarType x, VarType y) => x.Equals(y);
        public static bool operator !=(VarType x, VarType y) => !(x == y);
    }
}
