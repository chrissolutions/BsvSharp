#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Chain;
using CafeLib.Core.Encodings;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Chain
{
    public partial class KzMerkleBlockTests
    {
        private static HexEncoder Hex = new ();

        [Fact]
        public void MerkleBlock_FromJson_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            Assert.Equal("000000000000b731f2eef9e8c63173adfb07e41bd53eb0ef0a6b720d6cb6dea4", merkleBlock.Hash.ToString());
        }

        [Theory]
        [InlineData(MainnetBlock100014, "019f5b01d4195ecbc9398fbf3c3b1fa9bb3183301d7a1fb3bd174fcfa40a2b65")]
        [InlineData(MainnetFilteredBlock399775, "6f64fd5aa9dd01f74c03656d376625cf80328d83d9afebe60cc68b8f0e245bd9")]
        public void MerkleBlock_FromJson_Filtered_Test(string merkleJson, string filteredHash)
        {
            var merkleBlock = MerkleBlock.FromJson(merkleJson);
            var hashOfFilteredTx = UInt256.FromHex(filteredHash);
            var filteredHashes = merkleBlock.FilteredTransactionHashes();
            Assert.Equal(hashOfFilteredTx, filteredHashes.First());
        }

        [Fact] 
        public void MerkleBlock_ValidateTree_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            var result = merkleBlock.ValidMerkleTree();
            Assert.True(result);
        }

        [Fact]
        public void MerkleBlock_FromBuffer_Test()
        {
            var merkleBlock = MerkleBlock.FromHex(MainnetBlock100014Hex);
            var buffer = merkleBlock.Serialize().ToArray();
            var bytes = Hex.Decode(MainnetBlock100014Hex);
            Assert.Equal(bytes, buffer);
        }

        [Fact]
        public void MerkleBlock_Serialize_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            var buffer = merkleBlock.Serialize().ToArray();
            var bytes = Hex.Decode(MainnetBlock100014Hex);
            Assert.Equal(bytes, buffer);
        }
    }
}
