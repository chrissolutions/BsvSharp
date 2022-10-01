using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using CafeLib.Core.Extensions;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Network
{
    public class NetworkTests
    {
        [Theory]
        [InlineData(NetworkType.Main, 620538)]
        [InlineData(NetworkType.Test, 1344302)]
        [InlineData(NetworkType.Regression, 10000)]
        [InlineData(NetworkType.Scaling, 100)]
        public void Network_Type_Test(NetworkType networkType, int genesis)
        {
            var network = RootService.GetNetwork(networkType);
            Assert.Equal(networkType, network.NodeType);
            Assert.Equal(networkType.GetDescriptor(), network.NetworkId);
            Assert.Equal(network.Consensus.GenesisHeight, genesis);
        }
    }
}
