#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

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

        private static readonly BlockHeader TestHeader = new(
            version: 2,
            prevBlockHash: UInt256.FromHex("000000003c35b5e70b13d5b938fef4e998a977c17bea978390273b7c50a9aa4b"),
            merkleRootHash: UInt256.FromHex("58e6d52d1eb00470ae1ab4d5a3375c0f51382c6f249fff84e9888286974cfc97"),
            timestamp: 1371410638,
            bits: 473956288,
            nonce: 3594009557);

        [Fact]
        public void DeserializeBlockHeader_Test()
        {
            var raw = GetRawBlock("blk86756-testnet");
            var header = BlockHeader.FromBytes(raw[8..88]);
            Assert.NotNull(header);
            Assert.Equal(2, header.Version);
        }

        [Fact]
        public void SerializeBlockHeader_Test()
        {
            var headerBytes= TestHeader.Serialize();
            var blockHeader = BlockHeader.FromBytes(headerBytes);
            Assert.NotNull(blockHeader);
            Assert.Equal(TestHeader, blockHeader);
        }

        [Fact]
        public void GetDifficulty_Test()
        {
            var difficulty = TestHeader.GetDifficulty();
            Assert.Equal(4, difficulty);
        }

        [Fact]
        public void HasValidProofOfWork_Test()
        {
            var valid = TestHeader.HasValidProofOfWork();
            Assert.True(valid);
        }

        [Fact]
        public void HasValidTimestamp_Test()
        {
            var valid = TestHeader.HasValidTimestamp();
            Assert.True(valid);
        }

        [Fact]
        public void BlockHeader_String_Test()
        {
            var headerBytes = TestHeader.Serialize();
            var headerString = Encoders.Hex.Encode(headerBytes);
            Assert.Equal(TestHeader.ToString(), headerString);

            var header2 = BlockHeader.FromHex(headerString);
            Assert.Equal(TestHeader, header2);
        }

        #region Helpers

        private static byte[] GetRawBlock(string filename)
        {
            return File.ReadAllBytes(Path.Combine(RawBlocksFolder, $"{filename}.dat"));
        }

        #endregion
    }
}
