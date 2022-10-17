#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Dynamic;
using System.IO;
using CafeLib.BsvSharp.Chain;
using CafeLib.BsvSharp.Chain.Merkle;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Chain
{
    public class KzMerkleBlockTests
    {

        private const string genesisHash = "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f";
        private const string genesisMerkleRoot = "4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b";
        private const string RawBlocksFolder = @"..\..\..\data\RawBlocks";

        [Fact]
        public void MerkleBlock()
        {
            string str = @"
            {
                ""Id"": ""123"",
                ""DateOfRegistration"": ""2012-10-21T00:00:00+05:30"",
                ""Status"": 0
            }";



            var bytes = GetRawBlock("RawBlock000000");
            var kzb = Block.FromBytes(bytes);
            Assert.NotNull(kzb);

            Assert.True(kzb.Bits == 486604799U);
            Assert.True(kzb.Nonce == 2083236893U);
            Assert.True(kzb.Timestamp == 1231006505U);
            Assert.True(kzb.Hash.ToString() == genesisHash);
            Assert.True(kzb.MerkleRoot.ToString() == genesisMerkleRoot);
            Assert.True(kzb.PrevHash.ToString() == "0000000000000000000000000000000000000000000000000000000000000000");
            Assert.True(kzb.Transactions.Length == 1);
            var tx = kzb.Transactions[0];
            Assert.True(tx.TxHash.ToString() == "4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b");
            Assert.True(tx.LockTime == 0U);
            Assert.True(tx.Version == 1);
            Assert.True(tx.Inputs.Length == 1);
            Assert.True(tx.Inputs[0].SequenceNumber == 4294967295U); // -1
            Assert.True(tx.Inputs[0].PrevOut.Index == -1);
            Assert.True(tx.Inputs[0].PrevOut.TxId.ToString() == "0000000000000000000000000000000000000000000000000000000000000000");
            Assert.True(tx.Inputs[0].ScriptSig.ToHexString() == "04ffff001d0104455468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73"); 
            Assert.True(tx.Outputs.Length == 1);
            Assert.True(tx.Outputs[0].Amount == 5000000000L);
            Assert.True(tx.Outputs[0].Script.ToHexString() == "4104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac");
        }

        [Fact]
        public void GenesisBlock_Test()
        {
            var bytes = GetRawBlock("RawBlock000000");
            var genesis = Block.FromBytes(bytes);
            Assert.NotNull(genesis);

            Assert.Equal(genesisHash, RootService.Network.Consensus.Genesis.Hash.ToString());
            Assert.Equal(genesis.Hash, RootService.Network.Consensus.Genesis.Hash);

            Assert.Equal(genesisMerkleRoot, RootService.Network.Consensus.Genesis.MerkleRoot.ToString());
            Assert.Equal(genesis.MerkleRoot, RootService.Network.Consensus.Genesis.MerkleRoot);

            Assert.Equal(UInt256.Zero, RootService.Network.Consensus.Genesis.PrevHash);
            Assert.Equal(UInt256.Zero.ToString(), RootService.Network.Consensus.Genesis.PrevHash.ToString());
        }

        [Fact]
        public void DeserializeBlock_Test()
        {
            var bytes = GetRawBlock("blk86756-testnet");
            var block = Block.FromBytes(bytes[8..]);
            Assert.NotNull(block);
            Assert.Equal(22, block.Transactions.Length);
            Assert.Equal(2, block.Version);
        }

        [Fact]
        public void SerializeBlock_Test()
        {
            var bytes = GetRawBlock("blk86756-testnet");
            var blockBytes = bytes[8..];
            var block = Block.FromBytes(blockBytes);
            Assert.NotNull(block);

            var sequence = block.Serialize();
            Assert.NotNull(block);
            Assert.Equal(blockBytes[..(int)sequence.Data.Length], sequence.ToArray());
        }

        #region Helpers

        private static byte[] GetRawBlock(string filename)
        {
            return File.ReadAllBytes(Path.Combine(RawBlocksFolder, $"{filename}.dat"));
        }

        #endregion
    }
}
