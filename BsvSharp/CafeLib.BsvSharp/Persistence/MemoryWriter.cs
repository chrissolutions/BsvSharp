#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Persistence
{
    public class MemoryWriter : IDataWriter
    {
        private readonly ByteMemory _memory;
        public int Length => _memory.Length;

        public MemoryWriter()
        {
            _memory = new ByteMemory();
        }

        public MemoryWriter(ByteMemory memory)
        {
            _memory = memory;
        }

        public IDataWriter Write(byte[] data)
        {
            data.CopyTo(_memory.Data);
            return this;
        }

        public IDataWriter Write(byte data)
        {
            _memory[Length] = data;
            return this;
        }

        public IDataWriter Write(int data)
        {
            data.AsSpan().CopyTo(_memory[Length..]);
            return this;
        }

        public IDataWriter Write(uint data)
        {
            data.AsSpan().CopyTo(_memory[Length..]);
            return this;
        }

        public IDataWriter Write(long data)
        {
            data.AsSpan().CopyTo(_memory[Length..]);
            return this;
        }

        public IDataWriter Write(ulong data)
        {
            data.AsSpan().CopyTo(_memory.Data.Span[Length..]);
            return this;
        }

        public IDataWriter Write(string data)
        {
            Write(Encoders.Utf8.Decode(data));
            return this;
        }

        public IDataWriter Write(UInt160 data)
        {
            data.Span.CopyTo(_memory[Length..]);
            return this;
        }

        public IDataWriter Write(UInt256 data)
        {
            data.Span.CopyTo(_memory[Length..]);
            return this;
        }

        public IDataWriter Write(UInt512 data)
        {
            data.Span.CopyTo(_memory[Length..]);
            return this;
        }
    }
}
