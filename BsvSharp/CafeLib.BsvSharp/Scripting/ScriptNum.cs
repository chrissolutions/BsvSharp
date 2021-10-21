#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Numerics;
using CafeLib.Core.Buffers;
using CafeLib.Core.Encodings;

namespace CafeLib.BsvSharp.Scripting
{
    /// <summary>
    /// Numeric opcodes (OP_1ADD, etc) are restricted to operating on 4-byte
    /// integers. The semantics are subtle, though: operands must be in the range
    /// [-2^31 +1...2^31 -1], but results may overflow (and are valid as long as
    /// they are not used in a subsequent numeric operation). CScriptNum enforces
    /// those semantics by storing results as an int64 and allowing out-of-range
    /// values to be returned as a vector of bytes but throwing an exception if
    /// arithmetic is done or the result is interpreted as an integer.
    /// </summary>
    public readonly struct ScriptNum
    {
        private const uint MaximumElementSize = 4;

        private static readonly IEncoder Hex = Encoders.HexReverse;

        public static readonly ScriptNum Zero = new ScriptNum(0);
        public static readonly ScriptNum One = new ScriptNum(1);

        public class OverflowError : Exception { public OverflowError(string message) : base(message) { } }
        public class MinEncodeError : Exception { public MinEncodeError(string message) : base(message) { } }

        private readonly long _value;

        public ScriptNum(long value)
        {
            _value = value;
        }

        public ScriptNum(ReadOnlyByteSpan bytes, bool fRequireMinimal = false, uint nMaximumSize = MaximumElementSize)
            : this()
        {
            if (bytes.Length > nMaximumSize) throw new OverflowError("script number overflow");

            if (fRequireMinimal && !IsMinimallyEncoded(bytes, nMaximumSize)) throw new MinEncodeError("non-minimally encoded script number");

            _value = SetValueFromBytes(bytes);
        }

        public ScriptNum(string hex)
            : this(Hex.Decode(hex))
        {
        }

        public VarType ToValType() => new VarType(ToArray());

        public string GetHex() => Hex.Encode(ToArray());

        public long GetValue() => _value;

        public int GetInt()
        {
            if(_value > int.MaxValue) return int.MaxValue;
            if(_value < int.MinValue) return int.MinValue;
            return (int)_value;
        }

        public byte[] ToArray() => Serialize(_value);

        public override string ToString() => $"{_value}L, {GetInt()}, \"{GetHex()}\"";

        public override int GetHashCode() => GetInt();
        public override bool Equals(object o) => o is ScriptNum num && _value == num._value;

        /// <summary>
        /// Look at a sequence of bytes as an encoded number.
        /// The sign bit is the high bit of the high (last) byte: 0x80
        /// If the number's value uses the 0x80 bit of the last byte with set bits,
        /// then the sign bit (0x00 or 0x80) is added as an extra byte.
        /// </summary>
        /// <param name="data">Bytes to evaluate.</param>
        /// <param name="maxNumSize">Maximum byte length of minimally encoded number. Default is MAXIMUM_ELEMENT_SIZE.</param>
        /// <returns>Tuple of (bool tooLong, bool isNeg, uint extraBytes):
        /// tooLong is true if minimally encoded number doesn't fit in maxNumSize.
        /// isNeg is true if the sign bit is set.
        /// extraBytes is how many bytes can be elliminated by minimally encoding the number.
        /// </returns>
        public static (bool tooLong, bool isNeg, uint extraBytes) EvaluateAsNum(ReadOnlyByteSpan data, uint maxNumSize = MaximumElementSize)
        {
            var isNeg = false;
            var extraBytes = 0u;

            if (data.Length == 0) goto done;

            var signByte = data[^1];
            isNeg = (signByte & 0x80) != 0;
            var signByteHasNumBits = (signByte & 0x7f) != 0;

            if (data.Length == 1) {
                if (!signByteHasNumBits)
                    // Value is either +0 or -0. Should be encoded as zero length 0.
                    extraBytes++;
                goto done;
            }

            Debug.Assert(data.Length >= 2);

            if (!signByteHasNumBits) {
                // Find the first byte index after the last with a set bit, or -1 if all zero.
                var i = data.Length - 1; // indexOfLastByteWithNumBits
                while (--i >= 0 && data[i] == 0x00) extraBytes++;

                // See if we can push the sign into the first byte with set number bits. e.g. Bit 0x80 isn't already set.
                if (i >= 0 && (data[i] & 0x80) == 0) extraBytes++;
            }

            done:
            var tooLong = data.Length - extraBytes > maxNumSize;
            return (tooLong, isNeg, extraBytes);
        }

        public static ScriptNum operator *(ScriptNum a, ScriptNum b) => new ScriptNum(a._value * b._value);
        public static ScriptNum operator /(ScriptNum a, ScriptNum b) => new ScriptNum(a._value / b._value);
        public static ScriptNum operator %(ScriptNum a, ScriptNum b) => new ScriptNum(a._value % b._value);
        public static ScriptNum operator +(ScriptNum a, ScriptNum b) => new ScriptNum(a._value + b._value);
        public static ScriptNum operator -(ScriptNum a, ScriptNum b) => new ScriptNum(a._value - b._value);
        public static ScriptNum operator -(ScriptNum a) => new ScriptNum(-a._value);
        public static bool operator <(ScriptNum a, ScriptNum b) => a._value < b._value;
        public static bool operator >(ScriptNum a, ScriptNum b) => a._value > b._value;
        public static bool operator <=(ScriptNum a, ScriptNum b) => a._value <= b._value;
        public static bool operator >=(ScriptNum a, ScriptNum b) => a._value >= b._value;
        public static bool operator ==(ScriptNum a, ScriptNum b) => a._value == b._value;
        public static bool operator !=(ScriptNum a, ScriptNum b) => a._value != b._value;
        public static implicit operator ScriptNum(Int64 a) => new ScriptNum(a);
        public static implicit operator ScriptNum(bool a) => a ? One : Zero;

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] Serialize(long value)
        {
            if (value == 0) return new byte[0];

            var neg = value < 0;
            var absvalue = (ulong)(neg ? -value : value);

            var result = new List<byte>();
            while (absvalue > 0)
            {
                result.Add((byte)(absvalue & 0xff));
                absvalue >>= 8;
            }

            // - If the most significant byte is >= 0x80 and the value is positive,
            // push a new zero-byte to make the significant byte < 0x80 again.
            // - If the most significant byte is >= 0x80 and the value is negative,
            // push a new 0x80 byte that will be popped off when converting to an
            // integral.
            // - If the most significant byte is < 0x80 and the value is negative,
            // add 0x80 to it, since it will be subtracted and interpreted as a
            // negative when converting to an integral.
            var last = result.Count - 1;
            if ((result[last] & 0x80) != 0)
            {
                result.Add(neg ? (byte)0x80 : (byte)0);
            }
            else if (neg)
            {
                result[last] |= 0x80;
            }

            return result.ToArray();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vch"></param>
        /// <returns></returns>
        private static long SetValueFromBytes(ReadOnlyByteSpan vch)
        {
            if (vch.Length == 0)
                return 0;

            long result = 0;
            for (var i = 0; i < vch.Length; ++i)
                result |= ((Int64)vch[i]) << 8 * i;

            // If the input vector's most significant byte is 0x80, remove it from
            // the result's msb and return a negative.
            var last = vch.Length - 1;
            if ((vch[last] & 0x80) != 0) {
                return -((Int64)((UInt64)result & ~(0x80UL << (8 * last))));
            }

            return result;
        }
        
        /// <summary>
        /// The minimal encoding of zero is data.Length == 0,
        /// any sequence of zero bytes with or without the sign bit set is not minimal.
        ///
        /// The sign bit is the high bit of the high (last) byte: 0x80
        /// 
        /// The minimal encoding of positive numbers has a single most significant zero
        /// byte if and only if the number's most significant set bit is the 0x80 byte bit.
        /// 
        /// Negative numbers have the 0x80 bit set on the most significant byte.
        /// The minimal encoding of negative numbers has a single most significant 0x80
        /// byte if and only if the the number's absolute value's most significant set bit is the 0x80 byte bit.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="maxNumSize"></param>
        /// <returns>True, minimal, if length is zero or not greater than maxNumSize,
        /// and last byte has more bits than just sign bit set,
        /// or more than one byte and second to last byte has the "sign" bit set.
        /// </returns>
        private static bool IsMinimallyEncoded(ReadOnlySpan<byte> data, uint maxNumSize = MaximumElementSize)
        {
            if (data.Length == 0) return true;

            if (data.Length > maxNumSize) return false;

            // True, minimal, if last byte has more bits than just sign bit set,
            // or more than one byte and second to last byte has the "sign" bit set.
            return (data[^1] & 0x7f) != 0 || data.Length > 1 && (data[^2] & 0x80) != 0;
        }
        
        private static bool IsMinimallyEncodedOriginal(ReadOnlySpan<byte> data, uint maxNumSize = MaximumElementSize)
        {
            if (data.Length == 0) return true;

            if (data.Length > maxNumSize) return false;

            if ((data[^1] & 0x7f) == 0) {
                if (data.Length <= 1 || (data[^2] & 0x80) == 0) return false;
            }
            return true;

        }
        
        /// <summary>
        /// Strips high order zero bytes if possible.
        /// Preserves the sign bit.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true if span was modified, in which case it may also refer to new storage</returns>
        private static bool MinimallyEncode(ref Span<byte> data)
        {
            // Minimal zero, no change.
            if (data.Length == 0) return false;

            // remember the current last byte for sign bit value.
            var last = data[^1];

            // If any of the non-sign bits of the last byte are set,
            // already minimal, no change.
            if (0 != (last & 0x7f)) return false;

            // last is now only the sign bit, either 0x00 or 0x80.

            // A single byte of zero or just sign bit are both zero.
            // Which when minimally encoded is an empty span.
            if (data.Length == 1) {
                data = Span<byte>.Empty;
                return true;
            }

            // If the second to last byte has its sign bit set, then we are minimaly encoded.
            if ((data[^2] & 0x80) != 0) return false;

            // Remove zero byte from second to last byte up to first non-zero byte.
            for (var i = data.Length - 2; i >= 0; i--) {
                if (data[i] != 0) {
                    // Found a non-zero byte, if sign bit is set, back up to previous byte. 
                    if ((data[i] & 0x80) != 0) i++;
                    // Must keep bytes 0..i, with sign bit in last restored to data[i].
                    data = data[..(i + 1)];
                    if (last == 0x80) {
                        // Have to move set sign bit to a new byte, must allocate new storage.
                        data = data.ToArray().AsSpan();
                        // Add the negative sign bit.
                        data[i] |= last;
                    }
                    return true;
                }
            }

            // The whole thing is zeros, we have a zero.
            data = Span<byte>.Empty;
            return true;
        }
        
        #endregion
    }
}
