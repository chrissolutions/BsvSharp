#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Keys
{
    /// <summary>
    /// Wallet import format (WIF) keys.
    ///
    /// WIF is an abbreviation for Wallet Import Format. It is a format based on base58-encoding
    /// a private key so as to make it resistant to accidental user error in copying it. A wallet
    /// should be able to verify that the WIF format represents a valid private key.
    ///
    /// The private key in WIF-encoded format. See https://en.bitcoin.it/wiki/Wallet_import_format.
    /// 
    /// </summary>
    public abstract class WifKey : IEquatable<WifKey>, IComparable<WifKey>
    {
        private byte[] _versionData;
        private int _versionLength;

        protected ByteSpan Version => new Span<byte>(_versionData, 0, _versionLength);
        protected ByteSpan KeyData => new Span<byte>(_versionData, _versionLength, _versionData.Length - _versionLength);
        protected ReadOnlyByteSpan VersionData => _versionData;
        public NetworkType NetworkType { get; protected set; }

        protected void SetData(ReadOnlyByteSpan version, ReadOnlyByteSpan data, bool isCompressed)
        {
            _versionData = new byte[version.Length + data.Length + Convert.ToInt32(isCompressed)];
            _versionLength = version.Length;
            version.CopyTo(Version);
            data.CopyTo(KeyData);
            KeyData.Data[^1] = isCompressed ? (byte)1 : data[^1];
        }

        protected bool SetString(string b58, int nVersionBytes)
        {
            var (data, length, result) = 
                Encoders.Base58Check.TryDecode(b58, out var bytes) && bytes.Length >= nVersionBytes 
                    ? (bytes, nVersionBytes, true) 
                    : (Array.Empty<byte>(), 0, false);

            _versionData = data;
            _versionLength = length;
            NetworkType = FindNetworkType(Version);
            return result;
        }

        private static NetworkType FindNetworkType(ByteSpan version)
        {
            return version switch
            {
                _ when version.Data.SequenceEqual(new[] { (byte)0x80 }) => NetworkType.Main,
                _ when version.Data.SequenceEqual(new[] { (byte)0xef }) => NetworkType.Test,
                _ when version.Data.SequenceEqual(new[] { (byte)0xef }) => NetworkType.Regression,
                _ when version.Data.SequenceEqual(new[] { (byte)0xef }) => NetworkType.Scaling,
                _ => NetworkType.Main
            };
        }

        public override string ToString() => Encoders.Base58Check.Encode(_versionData);

        public override int GetHashCode() => _versionData.GetHashCode();

        public bool Equals(WifKey o) => o is not null && _versionData.SequenceEqual(o._versionData);
        public override bool Equals(object obj) => obj is WifKey wifKey && this == wifKey;

        public static bool operator ==(WifKey x, WifKey y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(WifKey x, WifKey y) => !(x == y);

        public int CompareTo(WifKey o) => o == null ? 1 : VersionData.Data.SequenceCompareTo(o.VersionData);
        public static bool operator <(WifKey a, WifKey b) => a.CompareTo(b) < 0;
        public static bool operator >(WifKey a, WifKey b) => a.CompareTo(b) > 0;
        public static bool operator <=(WifKey a, WifKey b) => a.CompareTo(b) <= 0;
        public static bool operator >=(WifKey a, WifKey b) => a.CompareTo(b) >= 0;
    }
}
