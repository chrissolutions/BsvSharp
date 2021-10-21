#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CafeLib.BsvSharp.Keys
{

    /// <summary>
    /// Represent a BIP32 style key path.
    /// </summary>
    public class KeyPath : IEnumerable<uint>
    {
        /// <summary>
        /// True if the path starts with m.
        /// False if the path starts with M.
        /// null if the path starts with an index.
        /// </summary>
        private bool? _fromPrivateKey;

        /// <summary>
        /// Path indices, in order.
        /// Hardened indices have the 0x80000000u bit set.
        /// </summary>
        private readonly uint[] _indices;

        public IEnumerator<uint> GetEnumerator() => ((IEnumerable<uint>) _indices).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates an empty path (zero indices) with FromPriv set to null.
        /// </summary>
        public KeyPath()
            : this(new uint[0])
        {
        }

        /// <summary>
        /// Creates a path with the properties provided.
        /// </summary>
        /// <param name="indices">Sets the indices. Hardened indices must have the HardenedBit set.</param>
        public KeyPath(params uint[] indices)
        {
            _indices = indices;
        }

        /// <summary>
        /// Creates a path based on its formatted string representation.
        /// </summary>
        /// <param name="path">The KzHDKeyPath formatted like a/b/c'/d. Apostrophe indicates hardened/private. a,b,c,d must convert to 0..2^31.
        /// Optionally the path can start with "m/" for private extended master key derivations or "M/" for public extended master key derivations.
        /// </param>
        /// <returns></returns>
        public KeyPath(string path)
        {
            _fromPrivateKey = path.StartsWith('m') ? true : path.StartsWith('M') ? false : null;
            _indices = ParseIndices(path);
        }

        /// <summary>
        /// Creates a path with the properties provided.
        /// </summary>
        /// <param name="fromPrivate">From private key true or false.</param>
        /// <param name="indices">Sets the indices. Hardened indices must have the HardenedBit set.</param>
        public KeyPath(bool fromPrivate, params uint[] indices)
        {
            _fromPrivateKey = fromPrivate;
            _indices = indices;
        }

        public uint this[int index] => _indices[index];

        /// <summary>
        /// How many numeric Indices there are.
        /// </summary>
        public int Count => _indices.Length;

        /// <summary>
        /// HardenedBit is 0x80000000u.
        /// </summary>
        public const uint HardenedBit = 0x80000000u;

        /// <summary>
        /// Parse Indices.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static uint[] ParseIndices(string path)
        {
			return path.Split('/').Where(p => p != "m" && p != "M" && p != "").Select(ParseIndex).ToArray();
        }

        /// <summary>
        /// Returns a sequence of KzKeyPaths from comma separated string of paths.
        /// </summary>
        /// <param name="v">Comma separated string of paths.</param>
        /// <returns></returns>
        public static IEnumerable<KeyPath> AsEnumerable(string v)
        {
            return v.Split(',').Select(kp => new KeyPath(kp));
        }

        /// <summary>
        /// Parse a KzHDKeyPath
        /// </summary>
        /// <param name="path">The KzHDKeyPath formatted like a/b/c'/d. Apostrophe indicates hardened/private. a,b,c,d must convert to 0..2^31.
        /// Optionally the path can start with "m/" for private extended master key derivations or "M/" for public extended master key derivations.
        /// </param>
        /// <returns></returns>
        public static KeyPath Parse(string path)
		{
			return new KeyPath(path);
		}

        /// <summary>
        /// Extends path with additional indices.
        /// FromPrivateKey of additionalIndices is ignored.
        /// </summary>
        /// <param name="additionalIndices"></param>
        /// <returns>New path with concatenated indices.</returns>
		public KeyPath Derive(KeyPath additionalIndices)
		{
            return new KeyPath(_indices.Concat(additionalIndices._indices).ToArray())
            {
                _fromPrivateKey = _fromPrivateKey
            };
		}

        /// <summary>
        /// Extends path with additional index.
        /// </summary>
        /// <param name="index">Values with HardenedBit set are hardened.</param>
        /// <returns>New path with concatenated index.</returns>
		public KeyPath Derive(uint index)
		{
            return new KeyPath(_indices.Concat(new[] { index }).ToArray())
            {
                _fromPrivateKey = _fromPrivateKey
            };
        }

        /// <summary>
        /// Extends path with additional index.
        /// </summary>
        /// <param name="index">Value must be non-negative and less than HardenedBit (which an int always is...)</param>
        /// <param name="hardened">If true, HardenedBit will be added to index.</param>
        /// <returns>New path with concatenated index.</returns>
		public KeyPath Derive(int index, bool hardened)
		{
			if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative.");
			var i = (uint)index;
            return Derive(hardened ? i | HardenedBit : i);
		}

        /// <summary>
        /// Extends path with additional indices from string formatted path.
        /// Any "m/" or "M/" prefix in path will be ignored.
        /// </summary>
        /// <param name="path">The indices in path will be concatenated.</param>
        /// <returns>New path with concatenated indices.</returns>
		public KeyPath Derive(string path)
		{
			return Derive(new KeyPath(path));
		}

        /// <summary>
        /// Returns a new path with one less index, or null if path has no indices.
        /// </summary>
		public KeyPath Parent => Count == 0 ? null  : new KeyPath(_indices.Take(_indices.Length - 1).ToArray()) { _fromPrivateKey = _fromPrivateKey };

        /// <summary>
        /// Returns a new path with the last index incremented by one.
        /// Throws InvalidOperation if path contains no indices.
        /// </summary>
        /// <returns>Returns a new path with the last index incremented by one.</returns>
        public KeyPath Increment()
        {
            if (Count == 0) throw new InvalidOperationException();
            var indices = _indices.ToArray();
            indices[Count - 1]++;
            return new KeyPath(indices) { _fromPrivateKey = _fromPrivateKey };
        }

        public override string ToString()
		{
            var sb = new StringBuilder();
            sb.Append(_fromPrivateKey != null && _fromPrivateKey.Value ? "m/" : "M/");

            foreach (var index in _indices) 
            {
                sb.Append(index & ~HardenedBit);
                if (index >= HardenedBit) sb.Append("'");
                sb.Append("/");
            }

            sb.Length--;
            return sb.ToString();
		}

        public override int GetHashCode() => ToString().GetHashCode();

        public bool Equals(KeyPath o) => !(o is null) && ToString().Equals(o.ToString());

        public override bool Equals(object obj) => obj is KeyPath path && this == path;

        public static bool operator ==(KeyPath x, KeyPath y) => x?.Equals(y) ?? y is null;
        public static bool operator !=(KeyPath x, KeyPath y) => !(x == y);

        /// <summary>
        /// Returns true if HardenedBit is set on last index.
        /// Throws InvalidOperation if there are no indices.
        /// </summary>
		public bool IsHardened
		{
			get
            {
                if (Count == 0) throw new InvalidOperationException("No index found in this KzHDKeyPath");
                return (_indices[Count - 1] & HardenedBit) != 0;
            }
        }

        public static implicit operator KeyPath(string s) => new KeyPath(s);

        #region Helpers

        /// <summary>
        /// Parse path index.
        /// </summary>
        /// <param name="pathIndex">path index</param>
        /// <returns>index</returns>
        private static uint ParseIndex(string pathIndex)
        {
            var hardened = pathIndex.Length > 0 && pathIndex[^1] == '\'' || pathIndex[^1] == 'H';
            var index = uint.Parse(hardened ? pathIndex[..^1] : pathIndex);
            if (index >= HardenedBit) throw new ArgumentException($"Indices must be less than {HardenedBit}.");
            return hardened ? index | HardenedBit : index;
        }

        #endregion
    }
}
