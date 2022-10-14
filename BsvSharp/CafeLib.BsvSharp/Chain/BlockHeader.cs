#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;
using CafeLib.Core.Extensions;
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
    public record BlockHeader
    {
        public const int BlockHeaderSize = 80;
        private const long MaxTimeOffset = 2 * 60 * 60;

        private int _version;
        private UInt256 _prevHash;
        private UInt256 _merkleRootHash;
        private uint _timestamp;
        private uint _bits;
        private uint _nonce;

        /// Essential fields of a Bitcoin SV block header.
        public UInt256 Hash { get; private set; }

        /// Public access to essential header fields.
        public int Version => _version;
        public UInt256 PrevHash => _prevHash;
        public UInt256 MerkleRoot => _merkleRootHash;
        public uint Timestamp => _timestamp;
        public uint Bits => _bits;
        public uint Nonce => _nonce;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected BlockHeader()
        {
        }

        protected BlockHeader(BlockHeader header)
        {
            Initialize(header._version, header._prevHash, header._merkleRootHash, header._timestamp, header._bits, header._nonce);
            Hash = CalculateHash(Serialize());
        }

        /// <summary>
        /// Constructs a new block header
        /// </summary>
        /// <param name="version">block version number</param>
        /// <param name="prevHash">sha256 hash of the previous block header</param>
        /// <param name="merkleRootHash">Sha256 hash at the root of the transaction merkle tree</param>
        /// <param name="timestamp">current block timestamp as seconds since the unix epoch</param>
        /// <param name="bits">the current difficulty target in compact format</param>
        /// <param name="nonce">the nonce field that miners use to find a sha256 hash value that matches the difficulty target</param>
        public BlockHeader(int version, UInt256 prevHash, UInt256 merkleRootHash, uint timestamp, uint bits, uint nonce)
        {
            Initialize(version, prevHash, merkleRootHash, timestamp, bits, nonce);
            Hash = CalculateHash(Serialize());
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
        public static BlockHeader FromBytes(ReadOnlyByteSpan bytes)
        {
            var blockHeader = new BlockHeader();
            var reader = new ByteSequenceReader(bytes);
            return blockHeader.TryDeserializeHeader(ref reader) ? blockHeader : throw new BlockException(nameof(bytes));
        }

        /// <summary>
        /// Serialize block header
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            var writer = new ByteDataWriter();
            return TrySerializeHeader(writer) ? writer.ToArray() : null;
        }

        /// <summary>
        /// Returns the difficulty target of this block header
        /// </summary>
        /// <returns>block header difficulty</returns>
        public double GetDifficulty()
        {
            var nShift = (_bits >> 24) & 255;
            var dDiff = (double)65535 / (_bits & 16777215);

            while (nShift < 29)
            {
                dDiff *= 256.0;
                nShift++;
            }

            while (nShift > 29)
            {
                dDiff /= 256.0;
                nShift--;
            }

            return double.Parse(dDiff.ToString("F8"));
        }

        /// <summary>
        /// Determine whether the block header has valid proof of work.
        /// </summary>
        /// <returns>true if has valid proof of work; otherwise false</returns>
        public bool HasValidProofOfWork()
        {
            return Hash <= GetTargetDifficulty();
        }

        /// <summary>
        /// Check for valid timestamp.
        /// </summary>
        /// <returns>Returns *true* if the timestamp is smaller than or equal to the [BlockHeader.MAX_TIME_OFFSET], *false* otherwise</returns>
        public bool HasValidTimestamp()
        {
            return Timestamp <= DateTime.UtcNow.ToUnixTime() + MaxTimeOffset;
        }

        /// <summary>
        /// Block header hex string representation.
        /// </summary>
        /// <returns>hex string representation of block header</returns>
        public string ToHex() => Encoders.Hex.Encode(Serialize());

        /// <summary>
        /// Block header string representation.
        /// </summary>
        /// <returns>string representation of block header</returns>
        public override string ToString() => ToHex();

        #region Protected Methods

        /// <summary>
        /// Deserialize the block header from the sequence reader.
        /// </summary>
        /// <param name="reader">sequence reader</param>
        /// <returns>true if successful; false otherwise</returns>
        protected bool TryDeserializeHeader(ref ByteSequenceReader reader)
        {
            if (reader.Data.Remaining < BlockHeaderSize)
                return false;

            var start = reader.Data.Position;
            if (!reader.TryReadLittleEndian(out _version)) return false;
            if (!reader.TryReadUInt256(ref _prevHash)) return false;
            if (!reader.TryReadUInt256(ref _merkleRootHash)) return false;
            if (!reader.TryReadLittleEndian(out _timestamp)) return false;
            if (!reader.TryReadLittleEndian(out _bits)) return false;
            if (!reader.TryReadLittleEndian(out _nonce)) return false;
            var end = reader.Data.Position;

            Hash = CalculateHash(reader.Data.Sequence.Slice(start, end).FirstSpan);
            return true;
        }

        /// <summary>
        /// Serialize block header.
        /// </summary>
        /// <returns></returns>
        protected bool TrySerializeHeader(IDataWriter writer)
        {
            writer
                .Write(_version)
                .Write(_prevHash)
                .Write(_merkleRootHash)
                .Write(_timestamp)
                .Write(_bits)
                .Write(_nonce);
            return true;
        }

        /// <summary>
        /// Returns current difficulty target or calculates a specific difficulty target.
        /// </summary>
        /// <param name="targetBits">The difficulty target to calculate.</param>
        /// <returns>the difficulty target</returns>
        protected UInt256 GetTargetDifficulty(ulong? targetBits = null)
        {
            targetBits ??= _bits;
            var target = new UInt256(targetBits.Value & 0xFFFFFF);
            var mov = 8 * ((targetBits >> 24) - 3);
            return target << (int)mov;
        }

        #endregion

        #region Helpers

        private void Initialize(int version, UInt256 prevHash, UInt256 merkleRootHash, uint timestamp, uint bits, uint nonce)
        {
            _version = version;
            _prevHash = prevHash;
            _merkleRootHash = merkleRootHash;
            _timestamp = timestamp;
            _bits = bits;
            _nonce = nonce;
        }

        /// <summary>
        /// Calculate the block hash
        /// </summary>
        /// <param name="bytes">bytes</param>
        /// <returns>calculated block hash</returns>
        private static UInt256 CalculateHash(ReadOnlyByteSpan bytes)
        {
            var hash1 = Hashes.ComputeSha256(bytes);
            var hash2 = Hashes.ComputeSha256(hash1);
            return new UInt256(hash2);
        }

        #endregion
    }
}
