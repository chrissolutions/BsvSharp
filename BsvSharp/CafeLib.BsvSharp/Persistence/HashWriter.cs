#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;
using CafeLib.Cryptography.BouncyCastle.Crypto.Digests;

namespace CafeLib.BsvSharp.Persistence
{
    public class HashWriter : IDisposable, IDataWriter
    {
        private readonly Sha256Digest _sha256 = new Sha256Digest();

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool alsoCleanupManaged)
        {
        }

        public UInt256 GetHashFinal()
        {
            var rv = new byte[32];
            _sha256.DoFinal(rv, 0);
            _sha256.BlockUpdate(rv, 0, rv.Length);
            _sha256.DoFinal(rv, 0);
            return rv.AsUInt256();
        }

        public IDataWriter Write(ReadOnlyByteSpan data)
        {
            _sha256.BlockUpdate(data, 0, data.Length);
            return this;
        }

        public IDataWriter Write(ReadOnlyByteSequence data)
        {
            _sha256.BlockUpdate(data.ToArray(), 0, (int)data.Length);
            return this;
        }

        public IDataWriter Write(byte[] data)
        {
            _sha256.BlockUpdate(data, 0, data.Length);
            return this;
        }

        public IDataWriter Write(byte data)
        {
            _sha256.BlockUpdate(new[] { data }, 0, sizeof(byte));
            return this;
        }

        public IDataWriter Write(int data)
        {
            _sha256.BlockUpdate(data.AsReadOnlySpan(), 0, sizeof(int));
            return this;
        }

        public IDataWriter Write(uint data)
        {
            _sha256.BlockUpdate(data.AsReadOnlySpan(), 0, sizeof(uint));
            return this;
        }

        public IDataWriter Write(long data)
        {
            _sha256.BlockUpdate(data.AsReadOnlySpan(), 0, sizeof(long));
            return this;
        }

        public IDataWriter Write(ulong data)
        {
            _sha256.BlockUpdate(data.AsReadOnlySpan(), 0, sizeof(ulong));
            return this;
        }

        public IDataWriter Write(string data)
        {
            var bytes = ((VarInt)data.Length).ToArray();
            _sha256.BlockUpdate(bytes, 0, bytes.Length);
            return this;
        }

        public IDataWriter Write(UInt160 data)
        {
            _sha256.BlockUpdate(data.Span, 0, data.Span.Length);
            return this;
        }

        public IDataWriter Write(UInt256 data)
        {
            _sha256.BlockUpdate(data.Span, 0, data.Span.Length);
            return this;
        }

        public IDataWriter Write(UInt512 data)
        {
            _sha256.BlockUpdate(data.Span, 0, data.Span.Length);
            return this;
        }
    }
}
