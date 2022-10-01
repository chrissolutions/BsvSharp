using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Services;
using Xunit;

namespace CafeLib.BsvSharp.UnitTests.Network
{
    public class NetworkTests
    {
        [Fact]
        public void Network_Type_Test()
        {
            var networkMain = RootService.GetNetwork(NetworkType.Main);
            Assert.Equal(NetworkType.Main, networkMain.NodeType);

            var networkTest = RootService.GetNetwork(NetworkType.Test);
            Assert.Equal(NetworkType.Test, networkTest.NodeType);
        }
    }
}
