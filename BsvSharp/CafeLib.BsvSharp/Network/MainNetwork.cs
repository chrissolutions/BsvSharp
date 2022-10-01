using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public class MainNetwork : BitcoinNetwork
    {
        public MainNetwork()
            : base(NetworkType.Main, GetConsensus(), new Lazy<byte[][]>(GetPrefixes).Value)
        {
        }

        private static Consensus GetConsensus()
        {
            return new Consensus 
            {
                SubsidyHalvingInterval = 210000,
                Bip34Height = 227931,
                Bip34Hash = UInt256.FromHex("000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                // 000000000000000004c2b624ed5d7756c508d90fd0da2c7c679febfa6c4735f0
                Bip65Height = 388381,
                // 00000000000000000379eaa19dce8c9b722d46ae6a57c2f1a988119488b50931
                Bip66Height = 363725,
                // 000000000000000004a1b34462cb8aeebd5799177f7a29cf28f2d1961716b5b5
                CsvHeight = 419328,
                ProofOfWorkLimit = UInt256.FromHex("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
                // two weeks
                ProofOfWorkTargetTimespan = 14 * 24 * 60 * 60,
                ProofOfWorkTargetSpacing = 10 * 60,
                AllowMinDifficultyBlocks = false,
                NoRetargeting = false,
                // 95% of 2016
                RuleChangeActivationThreshold = 1916,
                // Miner confirmation window
                MinerConfirmationWindow = 2016,

                // The best chain should have at least this much work.
                MinimumChainWork = UInt256.FromHex("000000000000000000000000000000000000000000a0f3064330647e2f6c4828"),

                // By default assume that the signatures in ancestors of this block are valid.
                DefaultAssumeValid = UInt256.FromHex("000000000000000000e45ad2fbcc5ff3e85f0868dd8f00ad4e92dffabe28f8d2"),

                // August 1, 2017 hard fork
                UahfHeight = 478558,

                // November 13, 2017 hard fork
                DaaHeight = 504031,

                // February 2020, Genesis Upgrade
                GenesisHeight = 620538,

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
            // Deployment of BIP68, BIP112, and BIP113.
            var prefixes = new byte[(int)Base58Type.MaxBase58Types][];
            prefixes[(int)Base58Type.PrivateKeyCompressed] = new[] { (byte)'L', (byte)'K' };
            prefixes[(int)Base58Type.PrivateKeyUncompressed] = new[] { (byte)'5' };
            prefixes[(int)Base58Type.PubkeyAddress] = new byte[] { 0 };
            prefixes[(int)Base58Type.PubkeyAddress] = new byte[] { 0 };
            prefixes[(int)Base58Type.ScriptAddress] = new byte[] { 5 };
            prefixes[(int)Base58Type.SecretKey] = new[] { (byte)NetworkVersion.Main };
            prefixes[(int)Base58Type.HdPublicKey] = new byte[] { 0x04, 0x88, 0xB2, 0x1E };
            prefixes[(int)Base58Type.HdSecretKey] = new byte[] { 0x04, 0x88, 0xAD, 0xE4 };
            return prefixes;
        }
    }
}