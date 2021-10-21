#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Extensions;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public abstract class BitcoinNetwork : IBitcoinNetwork
    {
        protected static readonly object Mutex = new object();

        public Consensus Consensus { get; }

        public string NetworkId { get; }
        
        public NetworkType NodeType { get; }

        protected byte[][] Base58Prefixes { get; }

        protected BitcoinNetwork(NetworkType nodeType, Consensus consensus, byte[][] base58Prefixes)
        {
            NodeType = nodeType;
            Consensus = consensus;
            NetworkId = nodeType.GetDescriptor();
            Base58Prefixes = base58Prefixes;
        }

        protected BitcoinNetwork(NetworkType nodeType, byte[][] base58Prefixes)
            : this(nodeType, DefaultConsensus(), base58Prefixes)
        {
        }

        public byte[] PublicKeyAddress => new Lazy<byte[]>(() => CreateKey(Base58Type.PubkeyAddress)).Value;

        public byte[] ScriptAddress => new Lazy<byte[]>(() => CreateKey(Base58Type.ScriptAddress)).Value;

        public byte[] SecretKey => new Lazy<byte[]>(() => CreateKey(Base58Type.SecretKey)).Value;

        public byte[] ExtPublicKey => new Lazy<byte[]>(() => CreateKey(Base58Type.ExtPublicKey)).Value;

        public byte[] ExtSecretKey => new Lazy<byte[]>(() => CreateKey(Base58Type.ExtSecretKey)).Value;

        private byte[] Base58Prefix(Base58Type type) => Base58Prefixes[(int)type];

        private byte[] CreateKey(Base58Type networkType)
        {
            lock (Mutex)
            {
                return Base58Prefix(networkType);
            }
        }

        private static Consensus DefaultConsensus()
        {
            return new Consensus
            {
                SubsidyHalvingInterval = 210000,
                Bip34Height = 227931,
                Bip34Hash = new UInt256("000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                // 000000000000000004c2b624ed5d7756c508d90fd0da2c7c679febfa6c4735f0
                Bip65Height = 388381,
                // 00000000000000000379eaa19dce8c9b722d46ae6a57c2f1a988119488b50931
                Bip66Height = 363725,
                ProofOfWorkLimit = new UInt256("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff"),
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
                MinimumChainWork = new UInt256("000000000000000000000000000000000000000000a0f3064330647e2f6c4828"),

                // By default assume that the signatures in ancestors of this block are valid.
                DefaultAssumeValid = new UInt256("000000000000000000e45ad2fbcc5ff3e85f0868dd8f00ad4e92dffabe28f8d2"),

                // August 1, 2017 hard fork
                UahfHeight = 478558,

                // November 13, 2017 hard fork
                DaaHeight = 504031,
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
    }
}
