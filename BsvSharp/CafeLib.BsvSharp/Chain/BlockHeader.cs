#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Chain
{

    /// <summary>
    /// Closely mirrors the data and layout of a serialized Bitcoin block header.
    /// Focus is on efficiency when processing large blocks.
    /// Not intended to facilitate making dynamic changes to a block header (mining).
    /// Includes the following meta data in addition to standard Bitcoin block header data:
    /// <list type="table">
    /// <item><term>Height</term><description>The chain height associated with this block.</description></item>
    /// </list>
    /// </summary>
    public class BlockHeader
    {
        public const int BlockHeaderSize = 80;
        private const long MaxTimeOffset = 2 * 60 * 60;

        /// Essential fields of a Bitcoin SV block header.
        private int _version;
        private UInt256 _prevBlockHash;
        private UInt256 _merkleRootHash;
        private uint _timestamp;
        private uint _bits;
        private uint _nonce;

        private readonly UInt256 _hash;
        public UInt256 Hash => _hash;

        public int Height { get; set; }

        /// Public access to essential header fields.
        public int Version => _version;
        public UInt256 PrevBlock => _prevBlockHash;
        public UInt256 MerkleRoot => _merkleRootHash;
        public uint Timestamp => _timestamp;
        public uint Bits => _bits;
        public uint Nonce => _nonce;

        public BlockHeader()
        {
            _hash = new UInt256();
        }

        /// <summary>
        /// Constructs a new block header
        /// </summary>
        /// <param name="version">block version number</param>
        /// <param name="prevBlockHash">sha256 hash of the previous block header</param>
        /// <param name="merkleRootHash">Sha256 hash at the root of the transaction merkle tree</param>
        /// <param name="timestamp">current block timestamp as seconds since the unix epoch</param>
        /// <param name="bits">the current difficulty target in compact format</param>
        /// <param name="nonce">the nonce field that miners use to find a sha256 hash value that matches the difficulty target</param>
        public BlockHeader(int version, UInt256 prevBlockHash, UInt256 merkleRootHash, uint timestamp, uint bits, uint nonce)
            : this()
        {
            _version = version;
            _prevBlockHash = prevBlockHash;
            _merkleRootHash = merkleRootHash;
            _timestamp = timestamp;
            _bits = bits;
            _nonce = nonce;
        }

        /// <summary>
        /// Create a block header from hexadecimal string.
        /// </summary>
        /// <param name="hex">Hexadecimal string containing the block header</param>
        /// <returns></returns>
        public static BlockHeader FromHex(string hex)
        {
            var bytes = Encoders.Hex.Decode(hex);
            return FromBytes(bytes);
        }

        /// <summary>
        /// Create a block header from bytes.
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns></returns>
        public static BlockHeader FromBytes(byte[] bytes)
        {
            var blockHeader = new BlockHeader();
            var reader = new ByteSequenceReader(bytes);
            return blockHeader.TryReadBlockHeader(ref reader) ? blockHeader : throw new FormatException(nameof(bytes));
        }

        public bool TryReadBlockHeader(ref ByteSequenceReader reader)
        {
            if (reader.Data.Remaining < BlockHeaderSize)
                return false;

            var start = reader.Data.Position;

            if (!reader.TryReadLittleEndian(out _version)) return false;
            if (!reader.TryReadUInt256(ref _prevBlockHash)) return false;
            if (!reader.TryReadUInt256(ref _merkleRootHash)) return false;
            if (!reader.TryReadLittleEndian(out _timestamp)) return false;
            if (!reader.TryReadLittleEndian(out _bits)) return false;
            if (!reader.TryReadLittleEndian(out _nonce)) return false;

            var end = reader.Data.Position;

            var blockBytes = reader.Data.Sequence.Slice(start, end).ToArray();
            var hash1 = Hashes.ComputeSha256(blockBytes);
            var hash2 = Hashes.ComputeSha256(hash1);
            hash2.CopyTo(_hash.Span);
            return true;
        }

        /// <summary>
        /// Check for valid timestamp.
        /// </summary>
        /// <returns>Returns *true* if the timestamp is smaller than or equal to the [BlockHeader.MAX_TIME_OFFSET], *false* otherwise</returns>
        public bool HasValidTimestamp()
        {
            var currentTime = (DateTime.Now - DateTime.UnixEpoch).Milliseconds / 1000;
            return Timestamp <= currentTime + MaxTimeOffset;
        }
    }
}
