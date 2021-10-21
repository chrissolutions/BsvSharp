#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Buffers;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Exceptions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.Core.Buffers;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace CafeLib.BsvSharp.Scripting
{
    public struct Operand :  IDataSerializer
    {
        public Opcode Code { get; private set; }

        internal VarType Data { get; private set; }

        public string CodeName => GetOpName(Code);

        public int LengthBytesCount => Code switch
        {
            Opcode.OP_PUSHDATA1 => 1,
            Opcode.OP_PUSHDATA2 => 2,
            Opcode.OP_PUSHDATA4 => 4,
            _ => 0
        };

        public long Length => 1 + Data.Length + LengthBytesCount;

        public Operand(Opcode code, VarType data)
        {
            Code = code;
            Data = data;
        }

        public Operand(Opcode code)
        {
            Code = code;
            Data = VarType.Empty;
        }

        public static Operand Push(ReadOnlyByteSpan data)
        {
            var opOnly = data.Length == 1 && data[0] <= 16;
            
            var code = (ulong)data.Length switch
            {
                _ when opOnly => data[0] == 0 ? Opcode.OP_0 : (Opcode)(data[0] - 1 + (int)Opcode.OP_1),
                _ when data.Length < (int)Opcode.OP_PUSHDATA1 => (Opcode)data.Length,
                _ when data.Length <= 0xff => Opcode.OP_PUSHDATA1,
                _ when data.Length < 0xffff => Opcode.OP_PUSHDATA2,
                _ when (ulong)data.Length <= 0xffffffff => Opcode.OP_PUSHDATA4,
                _ => throw new ScriptException("Data push limit exceeded.")
            };
            
            var val = opOnly ? VarType.Empty : new VarType(data.ToArray());
            var op = new Operand(code, val);
            return op;
        }

        public static Operand Push(long v)
        {
            Opcode code;
            var val = VarType.Empty;

            if (v == -1) 
            {
                code = Opcode.OP_1NEGATE;
            }
            else if (v >= 0 && v <= 16) 
            {
                code = v == 0 ? Opcode.OP_0 : (Opcode)(v - 1 + (int)Opcode.OP_1);
            }
            else
            {
                var bytes = BitConverter.GetBytes(v).AsSpan();
                if (v <= 0xff) 
                {
                    code = Opcode.OP_PUSH1;
                    val = new VarType(bytes[..1].ToArray());
                }
                else if (v <= 0xffff) 
                {
                    code = Opcode.OP_PUSH2;
                    val = new VarType(bytes[..2].ToArray());
                }
                else if (v <= 0xffffff) 
                {
                    code = Opcode.OP_PUSH3;
                    val = new VarType(bytes[..3].ToArray());
                }
                else 
                {
                    code = Opcode.OP_PUSH4;
                    val = new VarType(bytes[..4].ToArray());
                }
            }
            var op = new Operand(code, val);
            return op;
        }

        public bool TryCopyTo(ref ByteSpan span)
        {
            var length = Length;
            if (length > span.Length)
                return false;
            span[0] = (byte)Code;
            span = span[1..];
            length = Data.Length;
            if (Code >= Opcode.OP_PUSHDATA1 && Code <= Opcode.OP_PUSHDATA4) 
            {
                if (!BitConverter.IsLittleEndian) return false;
                var lengthBytes = BitConverter.GetBytes((uint)Data.Length).AsSpan(0, LengthBytesCount);
                lengthBytes.CopyTo(span);
                span = span.Slice(lengthBytes.Length);
            }
            if (length > 0) 
            {
                Data.Span.CopyTo(span.Slice(0, Data.Length));
                span = span.Slice((int)length);
            }
            return true;
        }

        /// <summary>
        /// Serialize Operand to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <param name="parameters">parameters</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer, object parameters) => WriteTo(writer);
        
        /// <summary>
        /// Serialize Operand to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer)
        {
            writer.Write((byte)Code);
            if (Code >= Opcode.OP_PUSHDATA1 && Code <= Opcode.OP_PUSHDATA4)
            {
                ByteSpan lengthBytes = BitConverter.GetBytes((uint)Data.Length).AsSpan(0, LengthBytesCount);
                writer.Write(lengthBytes);
            }

            if (Data.Length > 0)
                writer.Write(Data);

            return writer;
        }

        public byte[] GetDataBytes() => Data;

        public byte[] GetBytes()
        {
            var bytes = new byte[Length];
            bytes[0] = (byte)Code;
            if (bytes.Length > 1)
                Data.CopyTo(bytes[1..]);
            return bytes;
        }

        /*
            // script.h lines 527-562
            bool GetOp2(const_iterator &pc, opcodetype &opcodeRet,
                std::vector<uint8_t> *pvchRet) const {
                opcodeRet = OP_INVALIDOPCODE;
                if (pvchRet) pvchRet->clear();
                if (pc >= end()) return false;

                // Read instruction
                if (end() - pc < 1) return false;
                unsigned int opcode = *pc++;

                // Immediate operand
                if (opcode <= OP_PUSHDATA4) {
                    unsigned int nSize = 0;
                    if (opcode < OP_PUSHDATA1) {
                        nSize = opcode;
                    } else if (opcode == OP_PUSHDATA1) {
                        if (end() - pc < 1) return false;
                        nSize = *pc++;
                    } else if (opcode == OP_PUSHDATA2) {
                        if (end() - pc < 2) return false;
                        nSize = ReadLE16(&pc[0]);
                        pc += 2;
                    } else if (opcode == OP_PUSHDATA4) {
                        if (end() - pc < 4) return false;
                        nSize = ReadLE32(&pc[0]);
                        pc += 4;
                    }
                    if (end() - pc < 0 || (unsigned int)(end() - pc) < nSize)
                        return false;
                    if (pvchRet) pvchRet->assign(pc, pc + nSize);
                    pc += nSize;
                }

                opcodeRet = (opcodetype)opcode;
                return true;
            }
        */

        public static (bool ok, Operand op) TryRead(ref ReadOnlyByteSequence ros, out long consumed) {
            var op = new Operand();
            var ok = op.TryReadOperand(ref ros, out consumed);
            return (ok, op);
        }

        public bool TryReadOperand(ref ReadOnlyByteSequence ros) => TryReadOperand(ref ros, out _);

        public bool TryReadOperand(ref ReadOnlyByteSequence ros, out long consumed)
        {
            consumed = 0L;
            var r = new ByteSequenceReader(ros);
            if (!TryReadOperand(ref r)) goto fail;

            consumed = r.Data.Consumed;
            ros = ros.Data.Slice(r.Data.Consumed);

            return true;
        fail:
            return false;
        }

        public bool TryReadOperand(ref ByteSequenceReader r)
        {
            Code = Opcode.OP_INVALIDOPCODE;
            Data = VarType.Empty;

            if (!r.TryRead(out var opcode)) goto fail;

            Code = (Opcode)opcode;

            // Opcodes OP_0 and OP_1 to OP_16 are single byte opcodes that push the corresponding value.
            // Opcodes from zero to 0x4b [0..75] are single byte push commands where the value is the number of bytes to push.
            // Opcode 0x4c (76) takes the next byte as the count and should be used for pushing [76..255] bytes.
            // Opcode 0x4d (77) takes the next two bytes. Used for pushing [256..65536] bytes.
            // Opcode 0x4e (78) takes the next four bytes. Used for pushing [65537..4,294,967,296] bytes.
            
            if (opcode <= (byte)Opcode.OP_PUSHDATA4) 
            {
                var nSize = -1U;
                if (opcode < (byte)Opcode.OP_PUSHDATA1) 
                {
                    nSize = opcode;
                } 
                else if (opcode == (byte)Opcode.OP_PUSHDATA1)
                {
                    if (!r.TryRead(out byte size1)) goto fail;
                    nSize = size1;
                } 
                else if (opcode == (byte)Opcode.OP_PUSHDATA2)
                {
                    if (!r.TryReadLittleEndian(out UInt16 size2)) goto fail;
                    nSize = size2;
                } 
                else if (opcode == (byte)Opcode.OP_PUSHDATA4)
                {
                    if (!r.TryReadLittleEndian(out UInt32 size4)) goto fail;
                    nSize = size4;
                }

                if (nSize >= 0)
                {
                    if (r.Data.Remaining < nSize) goto fail;
                    Data = new VarType(r.Data.Sequence.Slice(r.Data.Position, (Int32)nSize).ToArray());
                    r.Data.Advance(nSize);
                }
            }
            return true;

        fail:
            return false;
        }

        public static string GetOpName(Opcode opcode)
        {
            return opcode.GetOpcodeName();
        }

        public string ToVerboseString()
        {
            switch (Code)
            {
                case Opcode.OP_PUSHDATA1:
                case Opcode.OP_PUSHDATA2:
                case Opcode.OP_PUSHDATA4:
                    return $"{CodeName} {Data.Length} 0x{Encoders.Hex.EncodeSpan(Data)}";

                default:
                    return $"{CodeName}{(Data.Length > 0 ? " 0x" + Encoders.Hex.EncodeSpan(Data) : "")}";
            }
        }

        public string ToAssemblyString()
        {
            switch (Code)
            {
                case Opcode.OP_0:
                case Opcode.OP_1NEGATE:
                    return CodeName;

                case var _ when Code < Opcode.OP_PUSHDATA1:
                    return $"{(Data.Length > 0 ? Encoders.Hex.EncodeSpan(Data) : "")}";

                case Opcode.OP_PUSHDATA1:
                case Opcode.OP_PUSHDATA2:
                case Opcode.OP_PUSHDATA4:
                    return $"{CodeName} {Data.Length} {Encoders.Hex.EncodeSpan(Data)}";

                default:
                    return $"{CodeName}{(Data.Length > 0 ? " " + Encoders.Hex.EncodeSpan(Data) : "")}";
            }
        }

        public override string ToString()
        {
            var len = Data.Length;
            string s;
            if (len == 0)
                s = CodeName;
            else if (len < 100)
                s = ToVerboseString();
            else
            {
                var start = Encoders.Hex.EncodeSpan(Data[..32]);
                var end = Encoders.Hex.EncodeSpan(Data[(len - 32)..]);
                s = $"{start}...[{Data.Length} bytes]...{end}";
            }
            return s;
        }

        public override int GetHashCode() => Code.GetHashCode() ^ Data.GetHashCode();
        public override bool Equals(object obj) => obj is Operand op && this == op;
        public bool Equals(Operand op) => Code == op.Code && Data == op.Data;
        public static bool operator ==(Operand x, Operand y) => x.Equals(y);
        public static bool operator !=(Operand x, Operand y) => !(x == y);
    }
}
