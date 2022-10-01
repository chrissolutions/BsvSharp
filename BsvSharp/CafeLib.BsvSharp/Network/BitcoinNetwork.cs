#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.Core.Extensions;

namespace CafeLib.BsvSharp.Network
{
    public abstract class BitcoinNetwork : IBitcoinNetwork
    {
        protected static readonly object Mutex = new();

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

        public byte[] PrivateKeyCompressed => new Lazy<byte[]>(() => CreateKey(Base58Type.PrivateKeyCompressed)).Value;

        public byte[] PrivateKeyUncompressed => new Lazy<byte[]>(() => CreateKey(Base58Type.PrivateKeyUncompressed)).Value;

        public byte[] PublicKeyAddress => new Lazy<byte[]>(() => CreateKey(Base58Type.PubkeyAddress)).Value;

        public byte[] ScriptAddress => new Lazy<byte[]>(() => CreateKey(Base58Type.ScriptAddress)).Value;

        public byte[] SecretKey => new Lazy<byte[]>(() => CreateKey(Base58Type.SecretKey)).Value;

        public byte[] HdPublicKey => new Lazy<byte[]>(() => CreateKey(Base58Type.HdPublicKey)).Value;

        public byte[] HdSecretKey => new Lazy<byte[]>(() => CreateKey(Base58Type.HdSecretKey)).Value;

        private byte[] Base58Prefix(Base58Type type) => Base58Prefixes[(int)type];

        private byte[] CreateKey(Base58Type networkType)
        {
            lock (Mutex)
            {
                return Base58Prefix(networkType);
            }
        }
    }
}
