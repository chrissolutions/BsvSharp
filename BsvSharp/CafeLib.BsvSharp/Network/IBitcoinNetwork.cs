#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Network
{
    public interface IBitcoinNetwork
    {
        /// <summary>
        /// Consensus.
        /// </summary>
        Consensus Consensus { get; }

        /// <summary>
        /// Network id.
        /// </summary>
        string NetworkId { get; }
        
        /// <summary>
        /// Node type.
        /// </summary>
        NetworkType NodeType { get; }

        /// <summary>
        /// Base58 encoding prefix for public key addresses for the active network.
        /// </summary>
        byte[] PublicKeyAddress { get; }

        /// <summary>
        /// Base58 encoding prefix for script addresses for the active network.
        /// </summary>
        byte[] ScriptAddress { get; }

        /// <summary>
        /// Base58 encoding prefix for private keys for the active network.
        /// </summary>
        byte[] SecretKey { get; }

        /// <summary>
        /// Base58 encoding prefix for extended public keys for the active network.
        /// </summary>
        byte[] ExtPublicKey { get; }

        /// <summary>
        /// Base58 encoding prefix for extended private keys for the active network.
        /// </summary>
        byte[] ExtSecretKey { get; }
    }
}
