#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Chain;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Network
{
    public class Consensus
    {
        public Consensus()
        {
            Deployments = new Bip9Deployment[(int)DeploymentPos.MaxVersionBitsDeployments];
        }

        public UInt256 HashGenesisBlock;

        public int SubsidyHalvingInterval { get; internal set; }

        /// <summary>
        /// Block height at which BIP34 becomes active.
        /// </summary>
        public int Bip34Height { get; internal set; }

        /// <summary>
        /// Block hash at which BIP34 becomes active.
        /// </summary>
        public UInt256 Bip34Hash { get; internal set; }

        /** Block height at which BIP65 becomes active */
        public int Bip65Height;
        /** Block height at which BIP66 becomes active */
        public int Bip66Height;
        /** Block height at which UAHF kicks in */
        public int UahfHeight;
        /** Block height at which the new DAA becomes active */
        public int DaaHeight;
        /**
         * Minimum blocks including miner confirmation of the total of 2016 blocks
         * in a retargeting period, (nPowTargetTimespan / nPowTargetSpacing) which
         * is also used for BIP9 deployments.
         * Examples: 1916 for 95%, 1512 for testchains.
         */
        public UInt32 RuleChangeActivationThreshold;
        public UInt32 MinerConfirmationWindow;

        public Bip9Deployment[] Deployments;

        /** Proof of work parameters */
        public UInt256 ProofOfWorkLimit;
        public bool AllowMinDifficultyBlocks;
        public bool NoRetargeting;
        public Int64 ProofOfWorkTargetSpacing;
        public Int64 ProofOfWorkTargetTimespan;
        public UInt256 MinimumChainWork;
        public UInt256 DefaultAssumeValid;


        public Int64 DifficultyAdjustmentInterval => ProofOfWorkTargetTimespan / ProofOfWorkTargetSpacing;

        /// <summary>
        /// satoshis per coin. Main is 100 million satoshis per coin.
        /// </summary>
        public long SatoshisPerCoin => 100_000_000L;

        /// <summary>
        /// Initial block reward. Main is 50 coins, 5 billion satoshis.
        /// </summary>
        public long InitialReward => 5_000_000_000L;

        /// <summary>
        /// Value used for fee estimation (satoshis per kilobyte)
        /// </summary>
        public long FeePerKilobyte => 1000L;

        /// <summary>
        /// Minimum amount for an output for it not to be considered a dust output
        /// </summary>
        public long DustLimit => 546L;

        /// <summary>
        /// Margin of error to allow fees in the vicinity of the expected value but doesn't allow a big difference
        /// </summary>
        public long FeeSecurityMargin => 150L;

        /// <summary>
        /// nlocktime limit to be considered block height rather than a timestamp
        /// </summary>
        public long LocktimeBlockheightLimit => 500_000_000_000L;

        /// <summary>
        /// Default sequence number.
        /// </summary>
        public ulong DefaultSeqnumber => 0xFFFFFFFF;

        /// <summary>
        /// Default nLockTime sequence number.
        /// </summary>
        public ulong DefaultLocktimeSeqnumber => DefaultSeqnumber - 1;

        /// <summary>
        /// Max value for an unsigned 32 bit value
        /// </summary>
        public long LocktimeMaxValue => 4294967295;

        /// Safe upper bound for change address script size in bytes
        public int ChangeOutputMaxSize => 20 + 4 + 34 + 4;
        public int MaximumExtraSize => 4 + 9 + 9 + 4;

        /// <summary>
        /// How many blocks between reductions in the block reward rate.
        /// </summary>
        public int RewardHalvingInterval => SubsidyHalvingInterval;

        public string GenesisBlockHash => "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f";

        /// <summary>
        /// Maximum number of bytes pushable to the stack
        /// </summary>
        public uint ScriptMaxElementSize => 520;
        /// <summary>
        /// Maximum number of non-push operations per script
        /// </summary>
        public int ScriptMaxOpsPer => 500;
        /// <summary>
        /// Maximum number of public keys per multisig
        /// </summary>
        public const int ScriptMaxPubKeysPerMultiSig = 20;
        /// <summary>
        /// Maximum script length in bytes. 
        /// </summary>
        public int ScriptMaxSize => 149;
        /// <summary>
        /// Threshold for nLockTime: below this value it is interpreted as block number,
        /// otherwise as UNIX timestamp. Threshold is Tue Nov 5 00:53:20 1985 UTC
        /// </summary>
        public uint LocktimeThreshold => 500000000U;

        public uint MaxScriptElementSize => ScriptMaxElementSize;
        public int MaxOperationsPerScript => ScriptMaxOpsPer;
        public int MaxScriptSize => 10000;
        public int MaxPubkeysPerMultisig => ScriptMaxPubKeysPerMultiSig;
#if false
            /// Block height and hash at which BIP34 becomes active
            public int BIP34Height;
            KzHash256 BIP34Hash;

            /// Block height at which BIP65 becomes active
            public int BIP65Height;

            /// Block height at which BIP66 becomes active
            public int BIP66Height;

            /// Block height at which UAHF kicks in

            public int uahfHeight;

            /// Block height at which the new DAA becomes active
            public int daaHeight;

            //
            // Minimum blocks including miner confirmation of the total of 2016 blocks
            // in a retargeting period, (nPowTargetTimespan / nPowTargetSpacing) which
            // is also used for BIP9 deployments.
            // Examples: 1916 for 95%, 1512 for testchains.
            //
            UInt32 nRuleChangeActivationThreshold;
            UInt32 nMinerConfirmationWindow;

            //BIP9Deployment vDeployments[MAX_VERSION_BITS_DEPLOYMENTS];

            // Proof of work parameters
            KzUInt256 powLimit;

            bool fPowAllowMinDifficultyBlocks;

            bool fPowNoRetargeting;

            UInt64 nPowTargetSpacing;

            UInt64 nPowTargetTimespan;

            //UInt64 DifficultyAdjustmentInterval() const {
            //    return nPowTargetTimespan / nPowTargetSpacing;
            //}

            KzUInt256 nMinimumChainWork;

            KzUInt256 defaultAssumeValid;
#endif
        private Block CreateGenesisBlock
        (
            string pszTimestamp,
            Script genesisOutputScript,
            uint nTime, 
            uint nNonce,
            uint nBits, 
            int nVersion,
            long genesisReward)
        {
            var txs = new Transaction[] 
            {
                new Transaction
                (
                    version: 1,
                    vin: new TxInCollection(new[] { new TxIn(UInt256.Zero, -1, Amount.Zero, Script.None, 0 ) }),
                    vout: new TxOutCollection(new[] { new TxOut(UInt256.Zero, -1, Amount.Zero, Script.None) }),
                    lockTime: 0
                )
            };

            var hashMerkleRoot = txs.ComputeMerkleRoot();
            var genesis = new Block(
                txs: txs,
                version: 1,
                hashPrevBlock: UInt256.Zero,
                hashMerkleRoot: hashMerkleRoot,
                time: 1231006506,
                bits: 0x1d00ffff,
                nonce: 2083236893
                );
            return genesis;
        }

#if false
        CMutableTransaction txNew;
        txNew.nVersion = 1;
    txNew.vin.resize(1);
    txNew.vout.resize(1);
    txNew.vin[0].scriptSig =
        CScript() << 486604799 << CScriptNum(4)
                  << std::vector<uint8_t>((const uint8_t*)pszTimestamp,
                                          (const uint8_t*)pszTimestamp +
                                              strlen(pszTimestamp));
    txNew.vout[0].nValue = genesisReward;
    txNew.vout[0].scriptPubKey = genesisOutputScript;

    CBlock genesis;
        genesis.nTime = nTime;
    genesis.nBits = nBits;
    genesis.nNonce = nNonce;
    genesis.nVersion = nVersion;
    genesis.vtx.push_back(MakeTransactionRef(std::move(txNew)));
    genesis.hashPrevBlock.SetNull();
    genesis.hashMerkleRoot = BlockMerkleRoot(genesis);
    return genesis;
54 68 65 20 54 69 6D 65 73 20 30 33 2F 4A 61 6E 2F 32 30 30 39 20 43 68 61 6E 63 65 6C 6C 6F 72 20 6F 6E 20 62 72 69 6E 6B 20 6F 66 20 73 65 63 6F 6E 64 20 62 61 69 6C 6F 75 74 20 66 6F 72 20 62 61 6E 6B 73  
#endif
    }

}
