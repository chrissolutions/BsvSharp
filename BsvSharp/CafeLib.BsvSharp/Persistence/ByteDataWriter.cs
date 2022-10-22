#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.Core.Buffers;
using CafeLib.Core.Buffers.Arrays;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Persistence
{
    public class ByteDataWriter : IDataWriter
    {
        private readonly ByteArrayBuffer _buffer;

        public int Length => _buffer.Length;

        public ReadOnlyByteMemory Memory => _buffer.Memory;
        public ReadOnlyByteSpan Span => _buffer.Span;
        public byte[] ToArray() => Span.ToArray();

        /// <summary>
        /// ByteDataWriter default constructor
        /// </summary>
        public ByteDataWriter()
        {
            _buffer = new ByteArrayBuffer();
        }

        /// <summary>
        /// ByteDataWriter constructor.
        /// </summary>
        /// <param name="bytes"></param>
        public ByteDataWriter(byte[] bytes)
        {
            _buffer = new ByteArrayBuffer(bytes);
        }
        
        public IDataWriter Write(byte[] data)
        {
            _buffer.Add(data);
            return this;
        }

        public IDataWriter Write(byte v)
        {
            _buffer.Add(v);
            return this;
        }

        public IDataWriter Write(int v) => WriteBytes(v.AsReadOnlySpan());

        public IDataWriter Write(uint v) => WriteBytes(v.AsReadOnlySpan());

        public IDataWriter Write(long v) => WriteBytes(v.AsReadOnlySpan());

        public IDataWriter Write(ulong v) => WriteBytes(v.AsReadOnlySpan());

        public IDataWriter Write(string data)
        {
            Write(Encoders.Utf8.Decode(data));
            return this;
        }

        public IDataWriter Write(UInt160 v)
        {
            _buffer.Add(v.Span);
            return this;
        }

        public IDataWriter Write(UInt256 v)
        {
            _buffer.Add(v.Span);
            return this;
        }

        public IDataWriter Write(UInt512 v)
        {
            _buffer.Add(v.Span);
            return this;
        }

        #region Helpers

        private IDataWriter WriteBytes(ReadOnlyByteSpan span)
        {
            if (BitConverter.IsLittleEndian)
            {
                _buffer.Add(span);
            }
            else
            {
                var bytes = new byte[span.Length];
                span.CopyTo(bytes);
                Array.Reverse(bytes);
                _buffer.Add(bytes);
            }

            return this;
        }

        #endregion
    }
}
