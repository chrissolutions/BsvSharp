using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public class ScalingTestNetwork : BitcoinNetwork
    {
        public ScalingTestNetwork()
            : base(NetworkType.Scaling, GetConsensus(), new Lazy<byte[][]>(GetPrefixes).Value)
        {
        }

        private static Consensus GetConsensus()
        {
            return new Consensus
            {
                SubsidyHalvingInterval = 210000,
                Bip34Height = 100000000,
                Bip34Hash = UInt256.Zero,
                Bip65Height = 581885,
                Bip66Height = 363725,
                CsvHeight = 0,
                ProofOfWorkLimit = UInt256.FromHex("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
                // two weeks
                ProofOfWorkTargetTimespan = 14 * 24 * 60 * 60,
                ProofOfWorkTargetSpacing = 10 * 60,
                AllowMinDifficultyBlocks = false,
                NoRetargeting = false,
                // 95% of 2016
                RuleChangeActivationThreshold = 1916,
                // Miner confirmation window
                MinerConfirmationWindow = 144, // fast

                // The best chain should have at least this much work.
                MinimumChainWork = UInt256.Zero,

                // By default assume that the signatures in ancestors of this block are valid.
                DefaultAssumeValid = UInt256.Zero,

                // August 1, 2017 hard fork
                UahfHeight = 15,

                // November 13, 2017 hard fork
                DaaHeight = 2200,  // must be > 2016 - see assert in pow.cpp:268

                // February 2020, Genesis Upgrade
                GenesisHeight = 100,

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
            prefixes[(int)Base58Type.PrivateKeyCompressed] = new byte[] { (byte)'c' };
            prefixes[(int)Base58Type.PrivateKeyUncompressed] = new byte[] { (byte)'9' };
            prefixes[(int)Base58Type.PubkeyAddress] = new byte[] { 111 };
            prefixes[(int)Base58Type.ScriptAddress] = new byte[] { 196 };
            prefixes[(int)Base58Type.SecretKey] = new[] { (byte)NetworkVersion.Test };
            prefixes[(int)Base58Type.HdPublicKey] = new byte[] { 0x04, 0x35, 0x87, 0xCF };
            prefixes[(int)Base58Type.HdSecretKey] = new byte[] { 0x04, 0x35, 0x83, 0x94 };
            return prefixes;
        }
    }
}