#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Linq;
using CafeLib.BsvSharp.Chain;
using CafeLib.Core.Numerics;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Chain
{
    public partial class KzMerkleBlockTests
    {
        [Fact]
        public void MerkleBlock_FromJson_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            Assert.Equal("000000000000b731f2eef9e8c63173adfb07e41bd53eb0ef0a6b720d6cb6dea4", merkleBlock.Hash.ToString());
        }

        [Fact]
        public void MerkleBlock_FromJson_Filtered_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            var hashOfFilteredTx = UInt256.FromHex("019f5b01d4195ecbc9398fbf3c3b1fa9bb3183301d7a1fb3bd174fcfa40a2b65");
            var filteredHashes = merkleBlock.FilteredTransactionHashes();
            Assert.Equal(hashOfFilteredTx, filteredHashes.First());
        }

        [Fact] public void MerkleBlock_ValidateTree_Test()
        {
            var merkleBlock = MerkleBlock.FromJson(MainnetBlock100014);
            var result = merkleBlock.ValidMerkleTree();
            Assert.True(result);
        }
    }
}
