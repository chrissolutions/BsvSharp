#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.IO;
using CafeLib.BsvSharp.Chain;
using CafeLib.Core.Buffers;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Chain
{
    public class KzNewBlockTests
    {
        private const string RawBlocksFolder = @"..\..\..\data\RawBlocks";

        private static string GetBlockFilename(int height)
        {
            var filename = Path.Combine(RawBlocksFolder, $"RawBlock{height / 1000:D3}\\RawBlock{height:D6}.dat");
            return filename;
        }

        private static byte[] GetBlockBytes(int height)
        {
            var filename = GetBlockFilename(height);

            return File.ReadAllBytes(filename);
        }

        private static Block GetBlock(int height)
        {
            var bytes = GetBlockBytes(height);

            var kzb = new Block {Height = height};
            var ros = new ReadOnlyByteSequence(bytes);
            var ok = kzb.TryReadBlock(ref ros);

            Assert.True(ros.Length == 0);
            Assert.True(ok);

            return kzb;
        }

        //private static Block FetchBlock(int height)
        //{
        //    var fileName = GetBlockFilename(height);
        //    var block = new Block {Height = height};
        //    using var stream = File.OpenRead(fileName);
        //    block.TryReadBlock(stream);
        //    Assert.Equal(10, block.Txs.LongLength);
        //}

        [Fact]
        public void Block0()
        {
            var kzb = GetBlock(0);

            Assert.True(kzb.Height == 0);
            Assert.True(kzb.Bits == 486604799U);
            Assert.True(kzb.Nonce == 2083236893U);
            Assert.True(kzb.Timestamp == 1231006505U);
            Assert.True(kzb.Hash.ToString() == "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f");
            Assert.True(kzb.MerkleRoot.ToString() ==
                        "4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b");
            Assert.True(kzb.PrevBlock.ToString() == "0000000000000000000000000000000000000000000000000000000000000000");
            Assert.True(kzb.Txs.Count == 1);
            var tx = kzb.Txs[0];
            Assert.True(tx.TxHash.ToString() == "4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b");
            Assert.True(tx.LockTime == 0U);
            Assert.True(tx.Version == 1);
            Assert.True(tx.Inputs.Count == 1);
            Assert.True(tx.Inputs[0].SequenceNumber == 4294967295U); // -1
            Assert.True(tx.Inputs[0].PrevOut.Index == -1);
            Assert.True(tx.Inputs[0].PrevOut.TxId.ToString() ==
                        "0000000000000000000000000000000000000000000000000000000000000000");
            Assert.True(tx.Inputs[0].UtxoScript.ToHexString() ==
                        "04ffff001d0104455468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73");
            Assert.True(tx.Outputs.Count == 1);
            Assert.True(tx.Outputs[0].Amount == 5000000000L);
            Assert.True(tx.Outputs[0].Script.ToHexString() ==
                        "4104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac");
        }
    }
}
