#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Encodings;
using CafeLib.Cryptography;
using System;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Extensions {
    public class KzReadOnlySequenceTests
    {
        [Fact]
        public void TestRemoveSlice()
        {
            var a = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var ros = new ReadOnlyByteSequence(a);
            Assert.Equal(new byte[] { 1, 2 }, ros.RemoveSlice(2, 12).ToArray());
            Assert.Equal(new byte[] { 11, 12 }, ros.RemoveSlice(0, 10).ToArray());
            Assert.Equal(new byte[] { 1, 12 }, ros.RemoveSlice(1, 11).ToArray());
            var r1 = ros.RemoveSlice(1, 5);
            var r2 = r1.RemoveSlice(3, 7);
            Assert.Equal(new byte[] { 1, 6, 7, 12 }, r2.ToArray());
        }

        [Fact]
        public void TestHash256()
        {
            const string text = "Bitcoin protocol is set in stone and there is no need to change it anytime in future as well as most of the global trade financial transactions are possible to be built using the current protocol itself";
            var bytes = text.Utf8ToBytes();
            var shaHash = bytes.Hash256();
            var hexString = new HexEncoder().Encode(shaHash);
            Assert.Equal("9ec3931d0c3da0157f170ebe5158f14a9e0b965ca9697dcff5063d2feb453fd2", hexString);
        }
    }
}
