﻿#region Copyright
// Copyright (c) 2020 TonesNotes
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
    /// Base class for Base58 encoded objects.
    /// </summary>
    public abstract class Base58Data : IComparable<Base58Data>
    {
        private byte[] _versionData;
        private int _versionLength;

        protected ByteSpan Version => new Span<byte>(_versionData, 0, _versionLength);
        protected ByteSpan KeyData => new Span<byte>(_versionData, _versionLength, _versionData.Length - _versionLength);
        protected ReadOnlyByteSpan VersionData => _versionData;
        public NetworkType NetworkType { get; protected set; }

        protected void SetData(byte[] versionData, int versionLength = 1)
        {
            _versionData = versionData;
            _versionLength = versionLength;
        }

        protected void SetData(ReadOnlyByteSpan version, ReadOnlyByteSpan data, bool flag = false)
        {
            _versionData = new byte[version.Length + data.Length + Convert.ToInt32(flag)];
            _versionLength = version.Length;
            version.CopyTo(Version);
            data.CopyTo(KeyData);
            KeyData.Data[^1] = flag ? (byte)1 : data[^1];
        }

        protected bool SetString(string b58, int nVersionBytes)
        {
            var (data, length, result) = 
                Encoders.Base58Check.TryDecode(b58, out var bytes) && bytes.Length >= nVersionBytes 
                    ? (bytes, nVersionBytes, true) 
                    : (Array.Empty<byte>(), 0, false);

            _versionData = data;
            _versionLength = length;
            return result;
        }

        public override string ToString() => Encoders.Base58Check.Encode(_versionData);

        public override int GetHashCode() => ToString().GetHashCode();

        public bool Equals(Base58Data o) => o is not null && _versionData.SequenceEqual(o._versionData);
        public override bool Equals(object obj) => obj is Base58Data base58Data && this == base58Data;

        public static bool operator ==(Base58Data x, Base58Data y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(Base58Data x, Base58Data y) => !(x == y);

        public int CompareTo(Base58Data o) => o == null ? 1 : VersionData.Data.SequenceCompareTo(o.VersionData);
        public static bool operator <(Base58Data a, Base58Data b) => a.CompareTo(b) < 0;
        public static bool operator >(Base58Data a, Base58Data b) => a.CompareTo(b) > 0;
        public static bool operator <=(Base58Data a, Base58Data b) => a.CompareTo(b) <= 0;
        public static bool operator >=(Base58Data a, Base58Data b) => a.CompareTo(b) >= 0;
    }
}
