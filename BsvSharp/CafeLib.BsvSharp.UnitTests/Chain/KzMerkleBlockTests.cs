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
    public class KzMerkleBlockTests
    {
        private const string MainnetBlock100014 = @"
        {
            ""header"": {
                ""hash"": ""000000000000b731f2eef9e8c63173adfb07e41bd53eb0ef0a6b720d6cb6dea4"",
                ""version"": 1,
                ""prevHash"": '0000000000016780c81d42b7eff86974c36f5ae026e8662a4393a7f39c86bb82',
                ""merkleRoot"": '8772d9d0fdf8c1303c7b1167e3c73b095fd970e33c799c6563d98b2e96c5167f',
                ""time"": 1293629558,
                ""bits"": 453281356,
                ""nonce"": 696601429
            },
            ""numTransactions"": 7,
            ""hashes"": [
                '3612262624047ee87660be1a707519a443b1c1ce3d248cbfc6c15870f6c5daa2',
                '019f5b01d4195ecbc9398fbf3c3b1fa9bb3183301d7a1fb3bd174fcfa40a2b65',
                '41ed70551dd7e841883ab8f0b16bf04176b7d1480e4f0af9f3d4c3595768d068',
                '20d2a7bc994987302e5b1ac80fc425fe25f8b63169ea78e68fbaaefa59379bbf'
            ],
            ""flags"": [ 29]
        }";

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
            var filteredHashes = merkleBlock.FilteredTransactionHashes();
            Assert.Contains("3612262624047ee87660be1a707519a443b1c1ce3d248cbfc6c15870f6c5daa2", filteredHashes.Select(x => x.ToString()));
        }

        [Fact]
        public void MerkleBlockDart_FromJson_Test()
        {
            var merkleBlock = MerkleBlockDart.FromJson(MainnetBlock100014);
            Assert.Equal("000000000000b731f2eef9e8c63173adfb07e41bd53eb0ef0a6b720d6cb6dea4", merkleBlock.Hash.ToString());

            var hashOfFilteredTx = UInt256.FromHex("019f5b01d4195ecbc9398fbf3c3b1fa9bb3183301d7a1fb3bd174fcfa40a2b65");
            var filteredHashes = merkleBlock.FilteredTransactionHashes();
            Assert.Equal(hashOfFilteredTx, filteredHashes.First());
        }
    }
}
