using CafeLib.BsvSharp.Encoding;

namespace CafeLib.BsvSharp.Extensions
{
    public static class StringExtensions
    {
        public static byte[] AsciiToBytes(this string s) => System.Text.Encoding.ASCII.GetBytes(s);

        public static byte[] HexToBytes(this string s) => Encoders.Hex.Decode(s);

        public static byte[] Utf8ToBytes(this string s) => System.Text.Encoding.UTF8.GetBytes(s);

        public static byte[] Utf8NormalizedToBytes(this string s) => System.Text.Encoding.UTF8.GetBytes(s.Normalize(System.Text.NormalizationForm.FormKD));
    }
}
