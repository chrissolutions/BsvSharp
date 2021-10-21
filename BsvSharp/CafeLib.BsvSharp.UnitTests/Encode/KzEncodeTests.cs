#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Buffers;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Encode
{
    public class KzEncodeTests
    {
        [Fact]
        public void Hex()
        {
            var hex = Encoders.Hex;

            var b0 = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0 };
            var s0 = "123456789abcdef0";
            var s0u = "123456789ABCDEF0";

            Assert.Equal(s0, hex.Encode(b0));
            Assert.Equal(b0, hex.Decode(s0));
            Assert.Equal(b0, hex.Decode(s0u));

            var hexr = Encoders.HexReverse;

            var b0r = new byte[] { 0x10, 0x32, 0x54, 0x76, 0x98, 0xba, 0xdc, 0xfe };
            var s0r = "fedcba9876543210";
            Assert.Equal(s0r, hexr.Encode(b0r));
            Assert.Equal(b0r, hexr.Decode(s0r));
        }

        [Fact]
        public void Endian()
        {
            var bytes = new ByteSpan(new byte[] { 0xAC, 0x1E, 0xED, 0x88 });

            var le = Encoders.Endian.LittleEndianInt32(bytes);
            var be = Encoders.Endian.BigEndianInt32(bytes);

            Assert.Equal(unchecked((Int32)(0x88ED1EAC)), le);
            Assert.Equal(unchecked((Int32)(0xAC1EED88)), be);
        }
    }
}
