﻿using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Network
{
    public class NetworkTests
    {
        [Theory]
        [InlineData(NetworkType.Main, 620538)]
        [InlineData(NetworkType.Test, 1344302)]
        public void Network_Type_Test(NetworkType networkType, int genesis)
        {
            var network = RootService.GetNetwork(networkType);
            Assert.Equal(networkType, network.NodeType);
            Assert.Equal(network.Consensus.GenesisHeight, genesis);
        }
    }
}
