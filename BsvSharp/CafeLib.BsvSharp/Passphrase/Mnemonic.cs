#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Passphrase
{
	/// <summary>
	/// BIP39 based support for converting binary data of specific lengths into sequences of words to facilitate written record keeping and verbal transmission.
	/// </summary>
	public class Mnemonic
	{
		/// <summary>
		/// Space separated word list. Each word encodes 11 bits. Words are all in Language and are contained in WordList.
		/// In addition to encoding Entropy, Words also encodes a checksum to catch transcription errors.
		/// </summary>
		public string Words { get; }
		/// <summary>
		/// What human language is being used by Words and WordList.
		/// </summary>
		public Languages Language { get; }
		/// <summary>
		/// A list of 2048 words. The index of each word, taken as an 11 bit value, is used to encode Entropy and checksum binary data.
		/// </summary>
		public string[] WordList { get; }
		/// <summary>
		/// The binary data for which Words is a transcription safe encoding, or null on checksum error.
		/// </summary>
		public byte[] Entropy { get; }

		public string ToHex() => Encoders.Hex.Encode(Entropy);
		public string ToDigitsBase10() => new BigInteger(Entropy.Concat(new byte[1]).ToArray()).ToString();
		public string ToDigitsBase6() => ToDigitsBase6(Entropy);

		/// <summary>
		/// Create a new KzMnemonic from a desired entropy length in bits.
		/// length should be a multiple of 32.
		/// </summary>
		/// <param name="length">Optional length in bits, default is 128. Should be a multiple of 32.</param>
		/// <param name="language">Optional language to use, default is English.</param>
		public static Mnemonic FromLength(int length = 128, Languages language = Languages.English) => new Mnemonic(length, language);
		public static Mnemonic FromLength(int length, string[] wordList, Languages language = Languages.Unknown) => new Mnemonic(length, wordList, language);

		/// <summary>
		/// Create a new KzMnemonic from a sequence of words.
		/// </summary>
		/// <param name="words">Space separated words that encode Entropy and checksum.</param>
		/// <param name="language">Optional language key to use in WordLists.</param>
		public static Mnemonic FromWords(string words, Languages language = Languages.Unknown) => new Mnemonic(words, language);
		public static Mnemonic FromWords(string words, string[] wordList, Languages language = Languages.Unknown) => new Mnemonic(words, wordList, language);

		/// <summary>
		/// Create a new KzMnemonic from given Entropy.
		/// </summary>
		/// <param name="entropy">Binary data to encode.</param>
		/// <param name="language">Optional language key to select WordList from WordLists. Defaults to English.</param>
		public static Mnemonic FromEntropy(byte[] entropy, Languages language = Languages.English) => new Mnemonic(entropy, language);
		public static Mnemonic FromEntropy(byte[] entropy, string[] wordList, Languages language = Languages.Unknown) => new Mnemonic(entropy, wordList, language);

		/// <summary>
		/// Create a new KzMnemonic from given entropy encoded as base 6 string of digits. e.g. Die rolls.
		/// </summary>
		/// <param name="base6">Entropy encoded as base 6 string. Use either digits 1-6 or 0-5.</param>
		/// <param name="length">Target Entropy length in bits.</param>
		/// <param name="language">Optional language key to select WordList from WordLists. Defaults to English.</param>
		public static Mnemonic FromBase6(string base6, int length = 128, Languages language = Languages.English) => new Mnemonic(Base6ToEntropy(base6, length), language);
		public static Mnemonic FromBase6(string base6, int length, string[] wordList, Languages language = Languages.Unknown) => new Mnemonic(Base6ToEntropy(base6, length), wordList, language);

		public static Mnemonic FromBase10(string base10, int length = 128, Languages language = Languages.English) => new Mnemonic(Base10ToEntropy(base10, length), language);
		public static Mnemonic FromBase10(string base10, int length, string[] wordList, Languages language = Languages.Unknown) => new Mnemonic(Base10ToEntropy(base10, length), wordList, language);

		/// <summary>
		/// Create a new KzMnemonic from a desired entropy length in bits.
		/// length should be a multiple of 32.
		/// </summary>
		/// <param name="bitLength">Entropy length in bits. Should be a multiple of 32.</param>
		/// <param name="wordList">string[] of 2048 unique words.</param>
		/// <param name="language">optional Languages key to use. Defaults to Unknown.</param>
		public Mnemonic(int bitLength, string[] wordList, Languages language = Languages.Unknown)
		{
            if (bitLength <= 0)
            {
                bitLength = 128;
            }

            if (bitLength % 32 != 0)
            {
                throw new ArgumentException($"{nameof(bitLength)} must be multiple of 32");
            }

            if (bitLength < 128)
            {
                throw new ArgumentException($"{nameof(bitLength)} at least 128 bits");
            }

			Entropy = new byte[bitLength / 8];
			Randomizer.GetStrongRandBytes(Entropy);

			Language = language;
			WordList = wordList;
			Words = ConvertEntropyToWords(Entropy, WordList);
		}

		/// <summary>
		/// Create a new KzMnemonic from a desired entropy length in bits.
		/// length should be a multiple of 32.
		/// </summary>
		/// <param name="bitLength">Optional length in bits, default is 128. Should be a multiple of 32.</param>
		/// <param name="language">Optional language to use, default is english.</param>
		public Mnemonic(int bitLength = 128, Languages language = Languages.English)
			: this(bitLength, WordLists.GetWords(language), language)
		{
		} 

		/// <summary>
		/// Create a new KzMnemonic from a sequence of words.
		/// </summary>
		/// <param name="words"></param>
		/// <param name="wordList"></param>
		/// <param name="language"></param>
		public Mnemonic(string words, string[] wordList, Languages language = Languages.Unknown)
		{
			Words = words.Normalize(NormalizationForm.FormKD);
			if (wordList != null) 
			{
				Language = language;
				WordList = wordList;
			} 
			else if (language != Languages.Unknown) 
			{
				Language = language;
				WordList = WordLists.GetWords(Language);
			} 
			else
				(Language, WordList) = GetWordList(words);

			Entropy = GetEntropy(Words, WordList);
		}

		/// <summary>
		/// Create a new KzMnemonic from a sequence of words.
		/// </summary>
		/// <param name="words"></param>
		/// <param name="language"></param>
		public Mnemonic(string words, Languages language = Languages.Unknown)
			: this(words, null, language)
		{
		}

		/// <summary>
		/// Create a new KzMnemonic from given Entropy.
		/// </summary>
		/// <param name="entropy">Binary data to encode.</param>
		/// <param name="wordList"></param>
		/// <param name="language">Optional language key. Defaults to Unknown.</param>
		public Mnemonic(IEnumerable<byte> entropy, string[] wordList, Languages language = Languages.Unknown)
		{
			Entropy = entropy.ToArray();
			Language = language;
			WordList = wordList;
			Words = ConvertEntropyToWords(Entropy, WordList);
		}

		/// <summary>
		/// Create a new KzMnemonic from given Entropy.
		/// </summary>
		/// <param name="entropy">Binary data to encode.</param>
		/// <param name="language">Optional language key to select WordList from WordLists. Defaults to English.</param>
		public Mnemonic(IEnumerable<byte> entropy, Languages language = Languages.English)
			: this(entropy, WordLists.GetWords(language), language) { }

		private static string ConvertEntropyToWords(ByteSpan entropy, string[] wordList)
		{
			var checksum = GetChecksum(entropy);
			var bin = ConvertBytesToBinaryString(entropy) + checksum;
			var words = BinaryStringToWords(bin, wordList);
			return words;
		}

		/// <summary>
		/// Returns the validated data as bytes from the given words and wordList.
		/// </summary>
		/// <param name="words"></param>
		/// <param name="wordList"></param>
		/// <returns>Returns the validated data as bytes from the given words and wordList.</returns>
		public static byte[] GetEntropy(string words, string[] wordList)
		{
			var bin = WordsToBinaryString(words, wordList);

			if (bin == null) return null;

			var (data, appendedChecksum) = BinaryStringToDataAndChecksum(bin);

			var dataChecksum = GetChecksum(data);

			if (appendedChecksum != dataChecksum) return null;

			return data;
		}

		private static bool BelongsToWordList(string words, IEnumerable<string> wordList)
		{
			words = words.Normalize(NormalizationForm.FormKD);
			return words.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(wordList.Contains);
		}

		private static (Languages language, string[] wordList) GetWordList(string words)
		{
			foreach (var language in EnumExtensions.GetEnumValues<Languages>()) 
			{
				if (language == Languages.Unknown) continue;
				var wl = WordLists.GetWords(language);
				if (BelongsToWordList(words, wl)) return (language, wl);
			}

			return (Languages.Unknown, null);
		}

        /// <summary>
        /// The checksum is a substring of the binary representation of the SHA256 hash of entropy.
        /// For every four bytes of entropy, one additional bit of the hash is used.
        /// </summary>
        /// <param name="entropy"></param>
        /// <returns></returns>
        public static string GetChecksum(byte[] entropy) => GetChecksum(new ReadOnlySequence<byte>(entropy));

        private static string GetChecksum(ByteSpan entropy) => GetChecksum(new ReadOnlySequence<byte>(entropy.ToArray()));

		/// <summary>
		/// The checksum is a substring of the binary representation of the SHA256 hash of entropy.
		/// For every four bytes of entropy, one additional bit of the hash is used.
		/// </summary>
		/// <param name="entropy"></param>
		/// <returns></returns>
		private static string GetChecksum(ReadOnlyByteSequence entropy)
		{
			var hash = entropy.Sha256();
			var bits = (int)entropy.Length * 8;
			var cs = bits / 32;

			var sb = new StringBuilder();
			foreach (var b in hash.Span) 
			{
				sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
				cs -= 8;
				if (cs <= 0) break;
			}
			if (cs < 0) sb.Length += cs;

			return sb.ToString();
		}

		/// <summary>
		/// Returns words converted into a binary string of "0" and "1" based on wordList.
		/// If wordList is specified, then it is used.
		/// Otherwise the wordList is selected based on the words provided.
		/// If a wordList can't be determined, null is returned.
		/// If a word is not found in wordList, null is returned.
		/// </summary>
		/// <param name="words">A sequence of space separated words from wordList, or one of the standard WordLists</param>
		/// <param name="wordList">Optional wordList to be used.</param>
		/// <returns>Returns words converted into a binary string of "0" and "1" based on wordList.</returns>
		public static string WordsToBinaryString(string words, string[] wordList = null)
		{
			words = words.Normalize(NormalizationForm.FormKD);
			wordList ??= GetWordList(words).wordList;

			if (wordList == null) return null;

			var bin = "";
			foreach (var w in words.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
				var i = Array.IndexOf(wordList, w);
				if (i < 0) return null;
				bin += Convert.ToString(i, 2).PadLeft(11, '0');
			}

			return bin;
		}

		public static string BinaryStringToWords(string bin, string[] wordList)
		{
			var words = new StringBuilder();
			for (var j = 0; j < bin.Length; j += 11) {
				var i = Convert.ToInt16(bin.Substring(j, 11), 2);
				if (j > 0) words.Append(" ");
				words.Append(wordList[i]);
			}
			return words.ToString();
		}

		/// <summary>
		/// Returns true if words encode binary data with a valid checksum.
		/// If wordList is specified, then it is used.
		/// Otherwise the wordList is selected based on the words provided.
		/// If a wordList can't be determined, false is returned.
		/// </summary>
		/// <param name="words">A sequence of space separated words from wordList, or one of the standard WordLists</param>
		/// <param name="wordList">Optional wordList to be used.</param>
		/// <returns>Returns true if words encode binary data with a valid checksum.</returns>
		public static bool IsValid(string words, string[] wordList = null)
		{
			var bin = WordsToBinaryString(words, wordList);

			if (bin == null) return false;

			var (data, appendedChecksum) = BinaryStringToDataAndChecksum(bin);

			var dataChecksum = GetChecksum(data);

			return appendedChecksum == dataChecksum;
		}

		public override string ToString()
		{
			return Words;
		}

        /// <summary>
        /// Converts a binary string of "0" and "1" into a byte[].
        /// Length of string must be a multiple of 8.
        /// </summary>
        /// <param name="dataBits"></param>
        /// <returns>dataBits converted to byte array.</returns>
        private static byte[] ConvertBinaryStringToBytes(string dataBits)
        {
            var data = new byte[dataBits.Length / 8];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(dataBits.Substring(i * 8, 8), 2);
            }
            return data;
        }

        /// <summary>
        /// Converts data byte[] to a binary string of "0" and "1".
        /// </summary>
        /// <param name="data"></param>
        /// <returns>data byte[] converted to a binary string.</returns>
        private static string ConvertBytesToBinaryString(ByteSpan data)
        {
            var dataBits = "";
            foreach (var b in data)
            {
                dataBits += Convert.ToString(b, 2).PadLeft(8, '0');
            }
            return dataBits;
        }

		/// <summary>
		/// Returns low order digits of a BigInteger as a byte array length / 8 bytes.
		/// CAUTION: Will pad array with zero bytes if number is small.
		/// </summary>
		/// <param name="big"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private static byte[] BigIntegerToEntropy(BigInteger big, int length = 128)
		{
			if (length % 8 != 0)
				throw new ArgumentException("length must be a multiple of eight.");
			var bytes = big.ToByteArray();
			var count = length / 8;
			if (bytes.Length > count)
				bytes = bytes.Take(count).ToArray();
			if (bytes.Length < count)
				bytes = new byte[count - bytes.Length].Concat(bytes).ToArray();
			return bytes;
		}

		///// <summary>
		///// Returns low order digits of a BigInteger as a byte array length / 8 bytes.
		///// CAUTION: Will pad array with zero bytes if number is small.
		///// </summary>
		///// <param name="big"></param>
		///// <returns></returns>
		//private static string BigIntegerToBase6(BigInteger _)
		//{
		//    return "";
		//}

		/// <summary>
		/// Converts a string of base 6 digits to a BitInteger.
		/// The string can use either digits 1-6, or 0-5.
		/// This is implemented by treating 6 and 0 as the same value 0.
		/// </summary>
		/// <param name="base6"></param>
		/// <returns></returns>
		public static BigInteger Base6ToBigInteger(string base6) 
		{
			if (string.IsNullOrWhiteSpace(base6)) return BigInteger.Zero;

			var n = new BigInteger(0);
			foreach (var c in base6.AsEnumerable())
			{
				var d = c - '0';
				if (d == 6) d = 0;
				if (d < 0 || d >= 6)
					throw new ArgumentException();
				n = n * 6 + d;
			}

			return n;
		}

		public static BigInteger Base10ToBigInteger(string base10)
		{
			var bn = BigInteger.Parse(base10);
			return bn;
		}

		public static string ToDigitsBase10(byte[] bytes)
		{
			var bn = new BigInteger(bytes.Concat(new byte[1]).ToArray());
			return bn.ToString();
		}

		public static string ToDigitsBase6(byte[] bytes)
		{
			var bn = new BigInteger(bytes.Concat(new byte[1]).ToArray());
			var sb = new List<char>();
			while (bn > 0) {
				var r = (int)(bn % 6);
				bn /= 6;
				sb.Add((char)('0' + r));
			}
			sb.Reverse();
			return new string(sb.ToArray());
		}

        /// <summary>
        /// Splits a binary string into its data and checksum parts.
        /// Converts the data to an array of bytes.
        /// Returns (data as byte[], checksum as binary string).
        /// </summary>
        /// <param name="bin">Binary string to be split.</param>
        /// <returns>Returns (data as byte[], checksum as binary string).</returns>
        private static (byte[], string) BinaryStringToDataAndChecksum(string bin)
        {
            var cs = bin.Length / 33; // one bit of checksum for every 32 bits of data.
            var checksum = bin.Substring(bin.Length - cs);
            var dataBits = bin.Substring(0, bin.Length - cs);

            var data = ConvertBinaryStringToBytes(dataBits);

            return (data, checksum);
        }

		/// <summary>
		/// Returns the entropy as a byte[] from a string of base 6 digits.
		/// Verifies that there are at least length / Log2(6) rounded up digits in string.
		/// This is 50 digits for 128 bits, 100 digits for 256 bits.
		/// The string can use either digits 1-6, or 0-5.
		/// This is implemented by treating 6 and 0 as the same value 0.
		/// </summary>
		/// <param name="base6">The string can use either digits 1-6, or 0-5.</param>
		/// <param name="length">Optional entropy length in bits. Must be a multiple of 8.</param>
		/// <returns>Returns the entropy as a byte[] from a string of base 6 digits.</returns>
		private static byte[] Base6ToEntropy(string base6, int length = 128)
		{
			var needDigits = (int)Math.Ceiling(length / Math.Log(6, 2));
			if (base6.Length < needDigits)
				throw new ArgumentException($"For {length} bits of entropy, at least {needDigits} digits of base 6 are needed.");
			return BigIntegerToEntropy(Base6ToBigInteger(base6), length);
		}

		/// <summary>
		/// Returns the entropy as a byte[] from a string of base 6 digits.
		/// Verifies that there are at least length / Log2(6) rounded up digits in string.
		/// This is 50 digits for 128 bits, 100 digits for 256 bits.
		/// The string must use digits 0-9.
		/// </summary>
		/// <param name="base10">The string must use digits 0-9.</param>
		/// <param name="length">Optional entropy length in bits. Must be a multiple of 8.</param>
		/// <returns>Returns the entropy as a byte[] from a string of base 10 digits.</returns>
		private static byte[] Base10ToEntropy(string base10, int length = 128)
		{
			var needDigits = (int)Math.Ceiling(length / Math.Log(10, 2));
			if (base10.Length < needDigits)
				throw new ArgumentException($"For {length} bits of entropy, at least {needDigits} digits of base 10 are needed.");
			return BigIntegerToEntropy(Base10ToBigInteger(base10), length);
		}
	}
}
