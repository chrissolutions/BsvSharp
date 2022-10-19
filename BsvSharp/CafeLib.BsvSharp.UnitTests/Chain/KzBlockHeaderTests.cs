#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.IO;
using CafeLib.BsvSharp.Chain;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Chain
{
    public class KzBlockHeaderTests
    {
        private const string RawBlocksFolder = @"..\..\..\data\RawBlocks";

        private static readonly BlockHeader TestNet_86756 = new(
            version: 2,
            prevHash: UInt256.FromHex("000000003c35b5e70b13d5b938fef4e998a977c17bea978390273b7c50a9aa4b"),
            merkleRoot: UInt256.FromHex("58e6d52d1eb00470ae1ab4d5a3375c0f51382c6f249fff84e9888286974cfc97"),
            timestamp: 1371410638,
            bits: 473956288,
            nonce: 3594009557);

        private static readonly BlockHeader TestNet_552065 = new(
            version: 3,
            prevHash: UInt256.FromHex("0000000000001fb81830e9b50a9973b275a843b4158460ac5a5dc53d310c217d"),
            merkleRoot: UInt256.FromHex("8dafcc0119abff36c6dcfcbc0520a6395255d08f792b79ce49173c0de6f5ab62"),
            timestamp: (uint)DateTime.Parse("2015-09-04 21:26:02").Millisecond,
            bits: 0x1b00c2a8,
            nonce: 163555806);

        [Fact]
        public void DeserializeBlockHeader_Test()
        {
            var raw = GetRawBlock("blk86756-testnet");
            var header = BlockHeader.FromBytes(raw[8..(8 + BlockHeader.BlockHeaderSize)]);
            Assert.NotNull(header);
            Assert.Equal(2, header.Version);
        }

        [Fact]
        public void SerializeBlockHeader_Test()
        {
            var headerBytes= TestNet_86756.Serialize();
            var blockHeader = BlockHeader.FromBytes(headerBytes);
            Assert.NotNull(blockHeader);
            Assert.Equal(TestNet_86756, blockHeader);
        }

        [Fact]
        public void GetDifficulty_Test()
        {
            var difficulty = TestNet_86756.GetDifficulty();
            Assert.Equal(4, difficulty);
        }

        [Fact]
        public void GetDifficulty_Testnet_552065_Test()
        {
            Assert.Equal((double)86187.62562209, TestNet_552065.GetDifficulty());
        }

        [Theory]
        [InlineData(0x18134dc1, 56957648455.01001)]
        [InlineData(0x1819012f, 43971662056.08958)]
        [InlineData(0x0900c2a8, 1.9220482782645836 * 1e48)]
        public void GetDifficulty_Livenet_Test(uint bits, double difficulty)
        {
            var header = new BlockHeader(0, UInt256.Zero, UInt256.Zero, 0, bits, 0);
            Assert.Equal(difficulty, header.GetDifficulty());
        }

        [Fact]
        public void HasValidProofOfWork_Test()
        {
            var valid = TestNet_86756.HasValidProofOfWork();
            Assert.True(valid);
        }

        [Fact]
        public void HasInvalidProofOfWork_Test()
        {
            var header = new BlockHeader(
                version: 2,
                prevHash: UInt256.FromHex("000000003c35b5e70b13d5b938fef4e998a977c17bea978390273b7c50a9aa4b"),
                merkleRoot: UInt256.FromHex("58e6d52d1eb00470ae1ab4d5a3375c0f51382c6f249fff84e9888286974cfc97"),
                timestamp: 1371410638,
                bits: 473956288,
                nonce: 0);

            var valid = header.HasValidProofOfWork();
            Assert.False(valid);
        }

        [Fact]
        public void HasValidTimestamp_Test()
        {
            var valid = TestNet_86756.HasValidTimestamp();
            Assert.True(valid);
        }

        [Fact]
        public void BlockHeader_Hex_String_Test()
        {
            var hexString = TestNet_86756.ToHex();
            var header = BlockHeader.FromHex(hexString);
            Assert.Equal(TestNet_86756, header);
        }

        [Fact]
        public void BlockHeader_String_Test()
        {
            var headerBytes = TestNet_86756.Serialize();
            var headerString = Encoders.Hex.Encode(headerBytes);
            Assert.Equal(TestNet_86756.ToString(), headerString);
        }

        #region Helpers

        private static byte[] GetRawBlock(string filename)
        {
            return File.ReadAllBytes(Path.Combine(RawBlocksFolder, $"{filename}.dat"));
        }

        #endregion
    }
}
