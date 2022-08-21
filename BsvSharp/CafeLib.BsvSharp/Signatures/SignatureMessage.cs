using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Signatures
{
    internal static class SignatureMessage
    {
        public static UInt256 GetMessageHash(string message) => CalculateMessageHash(message.Utf8ToBytes());

        #region Helpers

        internal static UInt256 CalculateMessageHash(ReadOnlyByteSpan message)
        {
            const string bitcoinSignedMessageHeader = "Bitcoin Signed Message:\n";
            var bitcoinSignedMessageHeaderBytes = Encoders.Utf8.Decode(bitcoinSignedMessageHeader);
            var msgBytes = new[] { (byte)bitcoinSignedMessageHeaderBytes.Length }.Concat(bitcoinSignedMessageHeaderBytes, new VarInt((ulong)message.Length).ToArray(), message);
            return Hashes.Hash256(msgBytes);
        }

        #endregion
    }
}
