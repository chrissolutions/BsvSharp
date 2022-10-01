using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public class TestNetwork : BitcoinNetwork
    {
        public TestNetwork()
            : base(NetworkType.Test, GetConsensus(), new Lazy<byte[][]>(GetPrefixes).Value)
        {
        }

        private static Consensus GetConsensus()
        {
            return new Consensus
            {
                SubsidyHalvingInterval = 210000,
                Bip34Height = 21111,
                Bip34Hash = UInt256.FromHex("0000000023b3a96d3484e5abb3755c413e7d41500f8e2a5c3f0dd01299cd8ef8"),
                // 00000000007f6655f22f98e72ed80d8b06dc761d5da09df0fa1dc4be4f861eb6
                Bip65Height = 581885,
                // 000000002104c8c45e99a8853285a3b592602a3ccde2b832481da85e9e4ba182
                Bip66Height = 363725,
                // 00000000025e930139bac5c6c31a403776da130831ab85be56578f3fa75369bb
                CsvHeight = 770112,
                ProofOfWorkLimit = UInt256.FromHex("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
                // two weeks
                ProofOfWorkTargetTimespan = 14 * 24 * 60 * 60,
                ProofOfWorkTargetSpacing = 10 * 60,
                AllowMinDifficultyBlocks = true,
                NoRetargeting = false,
                // 75% for testchains
                RuleChangeActivationThreshold = 1512,
                // Miner confirmation window
                MinerConfirmationWindow = 2016,

                // The best chain should have at least this much work.
                MinimumChainWork = UInt256.FromHex("00000000000000000000000000000000000000000000002a650f6ff7649485da"),

                // By default assume that the signatures in ancestors of this block are valid.
                DefaultAssumeValid = UInt256.FromHex("0000000000327972b8470c11755adf8f4319796bafae01f5a6650490b98a17db"),

                // August 1, 2017 hard fork
                UahfHeight = 1155875,

                // November 13, 2017 hard fork
                DaaHeight = 1188697,

                // February 2020, Genesis Upgrade
                GenesisHeight = 1344302,

                Deployments =
                {
                    [(int) DeploymentPos.DeploymentTestDummy] = new Bip9Deployment
                    {
                        Bit = 28,
                        StartTime = 1199145601, // January 1, 2008
                        Timeout = 1230767999 // December 31, 2008
                    },
                    [(int) DeploymentPos.DeploymentCsv] = new Bip9Deployment
                    {
                        Bit = 0,
                        StartTime = 1462060800, // May 1st, 2016
                        Timeout = 1493596800 // May 1st, 2017
                    }
                },
            };
        }

        private static byte[][] GetPrefixes()
        {
            var prefixes = new byte[(int)Base58Type.MaxBase58Types][];
            prefixes[(int)Base58Type.PrivateKeyCompressed] = new[] { (byte)'c' };
            prefixes[(int)Base58Type.PrivateKeyUncompressed] = new[] { (byte)'9' };
            prefixes[(int)Base58Type.PubkeyAddress] = new byte[] { 111 };
            prefixes[(int)Base58Type.ScriptAddress] = new byte[] { 196 };
            prefixes[(int)Base58Type.SecretKey] = new[] { (byte)NetworkVersion.Test };
            prefixes[(int)Base58Type.HdPublicKey] = new byte[] { 0x04, 0x35, 0x87, 0xCF };
            prefixes[(int)Base58Type.HdSecretKey] = new byte[] { 0x04, 0x35, 0x83, 0x94 };
            return prefixes;
        }
    }
}