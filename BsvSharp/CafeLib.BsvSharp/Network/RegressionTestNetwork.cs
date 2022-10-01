using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public class RegressionTestNetwork : BitcoinNetwork
    {
        public RegressionTestNetwork()
            : base(NetworkType.Regression, GetConsensus(), new Lazy<byte[][]>(GetPrefixes).Value)
        {
        }

        private static Consensus GetConsensus()
        {
            return new Consensus
            {
                SubsidyHalvingInterval = 150,
                Bip34Height = 100000000,
                Bip34Hash = UInt256.Zero,
                // BIP65 activated on regtest (Used in rpc activation tests)
                Bip65Height = 581885,
                // BIP66 activated on regtest (Used in rpc activation tests)
                Bip66Height = 363725,
                // CSV activated on regtest (Used in rpc activation tests)
                CsvHeight = 576,
                ProofOfWorkLimit = UInt256.FromHex("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
                // two weeks
                ProofOfWorkTargetTimespan = 14 * 24 * 60 * 60,
                ProofOfWorkTargetSpacing = 10 * 60,
                AllowMinDifficultyBlocks = true,
                NoRetargeting = true,
                // 75% for testchains
                RuleChangeActivationThreshold = 108,
                // Miner confirmation window
                // Faster than normal for regtest (144 instead of 2016)
                MinerConfirmationWindow = 144,

                // The best chain should have at least this much work.
                MinimumChainWork = UInt256.Zero,

                // By default assume that the signatures in ancestors of this block are valid.
                DefaultAssumeValid = UInt256.Zero,

                // August 1, 2017 hard fork
                UahfHeight = 0,

                // November 13, 2017 hard fork
                DaaHeight = 0,

                // February 2020, Genesis Upgrade
                GenesisHeight = 10000,

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