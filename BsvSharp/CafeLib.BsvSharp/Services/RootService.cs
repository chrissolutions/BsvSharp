using System;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Extensions;

namespace CafeLib.BsvSharp.Services
{
    public static class RootService
    {
        private static IBitcoinNetwork _bitcoinNetwork;
        private static readonly object Mutex = new();
        private static readonly Lazy<IBitcoinNetwork[]> Networks = new(() => new IBitcoinNetwork[EnumExtensions.GetNames<NetworkType>().Length]);

        public static IBitcoinNetwork Network => _bitcoinNetwork ??= CreateNetwork(NetworkType.Main);

        public static void Bootstrap(NetworkType networkType)
        {
            if (_bitcoinNetwork != null) throw new InvalidOperationException();
            AssignNetwork(networkType);
        }

        public static IBitcoinNetwork AssignNetwork(NetworkType networkType)
        {
            lock (Mutex)
            {
                _bitcoinNetwork = GetNetwork(networkType);
            }

            return _bitcoinNetwork;
        }

        public static IBitcoinNetwork GetNetwork(NetworkType? networkType = null)
        {
            return networkType switch
            {
                null => _bitcoinNetwork ?? CreateNetwork(NetworkType.Main),
                _ => CreateNetwork(networkType.Value)
            };
        }

        private static IBitcoinNetwork CreateNetwork(NetworkType networkType)
        {
            return Networks.Value[(int)networkType] ??= CreateNetworkInternal(networkType);
        }

        private static IBitcoinNetwork CreateNetworkInternal(NetworkType networkType)
        {
            return networkType switch
            {
                NetworkType.Main => new MainNetwork(),
                NetworkType.Test => new TestNetwork(),
                NetworkType.Regression => new RegressionTestNetwork(),
                NetworkType.Scaling => new ScalingTestNetwork(),
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), networkType, null)
            };
        }

    }
}
