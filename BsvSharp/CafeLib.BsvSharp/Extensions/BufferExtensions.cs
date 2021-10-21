using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Extensions
{
    public static class BufferExtensions
    {
        public static string ToHex(this ByteSpan data) => Encoders.Hex.Encode(data);
        public static string ToHexReverse(this ByteSpan data) => Encoders.HexReverse.Encode(data);

        public static string ToHex(this ReadOnlyByteSpan data) => Encoders.Hex.Encode(data);
        public static string ToHexReverse(this ReadOnlyByteSpan data) => Encoders.Hex.Encode(data);

        public static string ToHex(this ReadOnlyByteSequence data) => Encoders.Hex.EncodeSpan(data);
        public static string ToHexReverse(this ReadOnlyByteSequence data) => Encoders.Hex.EncodeSpan(data);
    }
}
