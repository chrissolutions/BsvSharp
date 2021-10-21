#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Builders
{
    public class ScriptBuilder
    {
        /// <summary>
        /// true if no more additions or removals from the operations will occur,
        /// but note that individual operations may still NOT be final.
        /// false by default.
        /// </summary>
        private bool _isFinal;

        /// <summary>
        /// true if script is associated with a scriptPub.
        /// false if script is associated with a scriptSig.
        /// null if script purpose is unknown.
        /// </summary>
        private bool? _isPub;

        /// <summary>
        /// The sequence of operations where each operation is an opcode and optional data.
        /// To support testing and unimplemented features, an operation's IsRaw flag can be set in
        /// which case the opcode is ignored and the data is treated as unparsed script code.
        /// </summary>
        public List<OperandBuilder> Ops { get; protected set; } = new List<OperandBuilder>();
        
        /// <summary>
        /// true when no more additions, deletions or changes to existing operations will occur.
        /// </summary>
        public bool IsFinal
        {
            get => _isFinal && Ops.All(op => op.IsFinal);
            protected set => _isFinal = value;
        }

        public bool IsPub
        {
            get => _isPub == true; 
            set => _isPub = value ? (bool?)true : null;
        }

        public bool IsSig
        {
            get => _isPub == false; 
            set => _isPub = value ? (bool?)false : null;
        }

        /// <summary>
        /// If the script implements a known template, this will be the template type.
        /// Otherwise it will be Unknown.
        /// </summary>
        public TemplateId TemplateId { get; }

        /// <summary>
        /// ScriptBuilder default constructor.
        /// </summary>
        public ScriptBuilder()
        {
        }

        protected ScriptBuilder(bool isPub, TemplateId templateId)
        {
            _isPub = isPub;
            TemplateId = templateId;
        }

        public ScriptBuilder(byte[] script)
        {
            Set(new Script(script));
        }

        public ScriptBuilder(Script script)
        {
            Set(script);
        }

        public virtual ScriptBuilder Clear()
        {
            Ops.Clear(); 
            return this;
        }

        public ScriptBuilder Set(Script script)
        {
            Ops.Clear(); 
            return Add(script);
        }

        public ScriptBuilder Add(Opcode opc)
        {
            Ops.Add(new Operand(opc));
            return this;
        }

        public ScriptBuilder Add(Opcode opc, VarType v)
        {
            Ops.Add(new Operand(opc, v)); 
            return this;
        }

        public ScriptBuilder Add(OperandBuilder opBuilder)
        {
            Ops.Add(opBuilder); 
            return this;
        }

        public ScriptBuilder Add(Script script)
        {
            Ops.AddRange(script.Decode().Select(o => new OperandBuilder(o)));
            return this;
        }

        public ScriptBuilder Add(string hex)
        {
            var script = !string.IsNullOrWhiteSpace(hex)
                ? new Script(hex)
                : new Script(new[] {(byte) Opcode.OP_FALSE, (byte) Opcode.OP_RETURN});
            return Add(script);
        }

        public virtual ScriptBuilder Add(byte[] raw)
        {
            Ops.Add(new OperandBuilder(new VarType(raw)));
            return this;
        }

        /// <summary>
        /// Push a zero as a non-final placeholder.
        /// </summary>
        /// <returns></returns>
        public ScriptBuilder Push() => Add(new OperandBuilder { IsFinal = false, IsRaw = false, Operand = new Operand(Opcode.OP_0) });

        public ScriptBuilder Push(ReadOnlyByteSpan data)
        {
            Ops.Add(Operand.Push(data)); 
            return this;
        }

        public ScriptBuilder Push(long v)
        {
            Ops.Add(Operand.Push(v));
            return this;
        }

        /// <summary>
        /// Build Script.
        /// </summary>
        /// <returns>script</returns>
        public virtual Script ToScript() => new Script(ToBytes());

        /// <summary>
        /// Convert script builder to byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            var bytes = new byte[Ops.Sum(o => o.Length)];
            var span = (ByteSpan)bytes;
            foreach (var op in Ops) 
            {
                op.TryCopyTo(ref span);
            }

            return bytes;
        }

        //public string ToHex() => ToBytes().ToHex();

        public override string ToString()
        {
            return string.Join(' ', Ops.Select(o => o.ToVerboseString()));
        }

        public string ToTemplateString()
        {
            var sb = new StringBuilder();
            foreach (var bop in Ops) 
            {
                var op = bop.Operand;
                var len = op.Data.Length;
                sb.Append(len == 0 ? $"{op.CodeName} " : $"[{op.Data.Length}] ");
            }
            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }

        /// <summary>
        /// Converts hex and ascii strings to a specific byte count, if len has a value and disagrees it is an error.
        /// Converts integer values to little endian bytes where the most significant bit is set if negative.
        /// For integer values, if len has a value, the result is expanded if necessary. If len is too small it is an error.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private static byte[] ParseCompactValueToBytes(string s, uint? len = null) => ParseLiteralValueToBytes(s, len).bytes;

        /// <summary>
        /// Parses signed decimals, hexadecimal strings prefixed with 0x, and ascii strings enclosed in single quotes.
        /// Each format is converted to a byte array.
        /// Converts hex and ascii strings to a specific byte count, if len has a value and disagrees it is an error.
        /// Converts integer values to little endian bytes where the most significant bit is set if negative.
        /// For integer values, if len has a value, the result is expanded if necessary. If len is too small it is an error.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="len"></param>
        /// <returns>Tuple of the parsed byte[] data and a boolean true if the literal was specified in hexadecimal.
        /// Returns null for bytes if can't be parsed as a literal.</returns>
        private static (byte[] bytes, bool isHex) ParseLiteralValueToBytes(string s, uint? len = null)
        {
            var bytes = (byte[])null;
            var isHex = false;

            if (s.StartsWith("'") && s.EndsWith("'"))
            {
                s = s.Substring(1, s.Length - 2);
                if (s.Contains("'"))
                    throw new InvalidOperationException();
                bytes = System.Text.Encoding.ASCII.GetBytes(s);
            } 
            else if (s.StartsWith("0x"))
            {
                isHex = true;
                bytes = Encoders.Hex.Decode(s[2..]);
            } 
            else if (long.TryParse(s, out var v))
            {
                bytes = new ScriptNum(v).ToArray();
            }

            if (len.HasValue && bytes != null && len.Value != bytes.Length)
                throw new InvalidOperationException();
            
            return (bytes, isHex);
        }

        /// <summary>
        /// Parses format used by script_tests.json file shared with C++ bitcoin-sv codebase.
        /// Primary difference is that hex literals are never treated as push data.
        /// Hex literals are also treated as unparsed bytes. e.g. multiple opcodes in a single literal.
        /// The use of "OP" before a literal is not used to create opcodes from literals.
        /// Instead, single byte hex literals are interpreted as opcodes directly.
        /// Test scripts also wish to encode invalid scripts to make sure the interpreter will
        /// catch the errors.
        /// </summary>
        /// <param name="testScript"></param>
        /// <returns></returns>
        internal static ScriptBuilder ParseTestScript(string testScript)
        {
            var sb = new ScriptBuilder();
            var ps = testScript.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
            while (ps.Length > 0) {
                var arg = 0;
                var (bytes, isHex) = ParseLiteralValueToBytes(ps[arg]);
                if (bytes != null) {
                    if (isHex)
                        // Hex literals are treated as raw, unparsed bytes added to the script.
                        sb.Add(bytes);
                    else
                        sb.Push(bytes);
                } else {
                    var data = (byte[])null;
                    if (!Enum.TryParse("OP_" + ps[arg], out Opcode opcode))
                        throw new InvalidOperationException();
                    if (opcode > Opcode.OP_0 && opcode < Opcode.OP_PUSHDATA1) {
                        // add next single byte value to op.
                        arg++;
                        data = ParseCompactValueToBytes(ps[arg]);
                        if (data == null) {
                            // Put this arg back. Treat missing data as zero length.
                            data = new byte[0];
                            arg--;
                        }
                    } else if (opcode >= Opcode.OP_PUSHDATA1 && opcode <= Opcode.OP_PUSHDATA4) {
                        // add next one, two, or four byte value as length of following data value to op.
                        arg++;
                        var lengthBytes = ParseCompactValueToBytes(ps[arg]);
                        var len = 0u;
                        if (!BitConverter.IsLittleEndian)
                            throw new NotSupportedException();
                        if (opcode == Opcode.OP_PUSHDATA1) {
                            // add next one byte value as length of following data value to op.
                            if (lengthBytes.Length != 1)
                                throw new InvalidOperationException();
                            len = lengthBytes[0];
                        } else if (opcode == Opcode.OP_PUSHDATA2) {
                            // add next two byte value as length of following data value to op.
                            if (lengthBytes.Length != 2)
                                throw new InvalidOperationException();
                            len = BitConverter.ToUInt16(lengthBytes);
                        } else if (opcode == Opcode.OP_PUSHDATA4) {
                            // add next four byte value as length of following data value to op.
                            if (lengthBytes.Length != 4)
                                throw new InvalidOperationException();
                            len = BitConverter.ToUInt32(lengthBytes);
                        }
                        if (len > 0) {
                            arg++;
                            data = arg < ps.Length ? ParseCompactValueToBytes(ps[arg], len) : new byte[0];
                        }
                    }
                    if (data == null)
                        sb.Add(opcode);
                    else
                        sb.Add(opcode, new VarType(data));
                }
                ps = ps[Math.Min(arg + 1, ps.Length)..];
            }
            return sb;
        }

                    //if (!isOp && ps[arg] == "OP") {
                    //    arg++;
                    //    var opcodeBytes = ParseCompactValueToBytes(ps[arg]);
                    //    if (opcodeBytes == null || opcodeBytes.Length > 1)
                    //        throw new InvalidOperationException();
                    //    op = (KzOpcode)opcodeBytes[0];
                    //}
        public static ScriptBuilder ParseCompact(string compactScript)
        {
            var sb = new ScriptBuilder();
            var ps = compactScript.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
            while (ps.Length > 0) {
                var s = ps[0];
                var bytes = ParseCompactValueToBytes(s);
                if (bytes != null) {
                    sb.Push(bytes);
                    ps = ps.Slice(1);
                } else if (Enum.TryParse("OP_" + s, out Opcode op)) {
                    var args = 1;
                    var data = (byte[])null;
                    if (op > Opcode.OP_0 && op < Opcode.OP_PUSHDATA1) {
                        // add next single byte value to op.
                        args = 2;
                        data = ParseCompactValueToBytes(ps[1]);
                        if (data.Length >= (int)Opcode.OP_PUSHDATA1)
                            throw new InvalidOperationException();
                    } else if (op >= Opcode.OP_PUSHDATA1 && op <= Opcode.OP_PUSHDATA4) {
                        // add next one, two, or four byte value as length of following data value to op.
                        args = 2;
                        var lengthBytes = ParseCompactValueToBytes(ps[1]);
                        var len = 0u;
                        if (!BitConverter.IsLittleEndian)
                            throw new NotSupportedException();
                        if (op == Opcode.OP_PUSHDATA1) {
                            // add next one byte value as length of following data value to op.
                            if (lengthBytes.Length != 1)
                                throw new InvalidOperationException();
                            len = lengthBytes[0];
                        } else if (op == Opcode.OP_PUSHDATA2) {
                            // add next two byte value as length of following data value to op.
                            if (lengthBytes.Length != 2)
                                throw new InvalidOperationException();
                            len = BitConverter.ToUInt16(lengthBytes);
                        } else if (op == Opcode.OP_PUSHDATA4) {
                            // add next four byte value as length of following data value to op.
                            if (lengthBytes.Length != 4)
                                throw new InvalidOperationException();
                            len = BitConverter.ToUInt32(lengthBytes);
                        }
                        if (len > 0) {
                            args = 3;
                            data = ParseCompactValueToBytes(ps[2], len);
                        }
                    }
                    if (data == null)
                        sb.Add(op);
                    else
                        sb.Add(op, new VarType(data));
                    ps = ps.Slice(args);
                } else
                    throw new InvalidOperationException();
            }
            return sb;
        }

        /// <summary>
        /// Parse encoded script.
        /// </summary>
        /// <param name="assembly">script assembly code</param>
        /// <returns>script builder</returns>
        public static ScriptBuilder ParseAssembly(string assembly)
        {
            var builder = new ScriptBuilder();
            var tokens = assembly.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
            for (var i = 0; i < tokens.Length; ++i)
            {
                if (Enum.TryParse<Opcode>(tokens[i], out var op))
                {
                    switch (op)
                    {
                        case Opcode.OP_0:
                        case Opcode.OP_1NEGATE:
                            builder.Add(op);
                            break;

                        case var _ when op < Opcode.OP_PUSHDATA1:
                        {
                            var data = Encoders.Hex.Decode(tokens[++i]);
                            if (data.Length >= (int)Opcode.OP_PUSHDATA1) throw new InvalidOperationException();
                            builder.Add(op, data);
                            break;
                        }

                        case var _ when op <= Opcode.OP_PUSHDATA4:
                        {
                            if (!BitConverter.IsLittleEndian) throw new NotSupportedException();
                            var data = Encoders.Hex.Decode(tokens[++i]);
                            var len = 0u;
                            
                            switch (op)
                            {
                                case Opcode.OP_PUSHDATA1:
                                    // add next one byte value as length of following data value to op.
                                    if (data.Length != 1) throw new InvalidOperationException();
                                    len = data[0];
                                    break;
                                    
                                case Opcode.OP_PUSHDATA2:
                                    // add next two byte value as length of following data value to op.
                                    if (data.Length != 2) throw new InvalidOperationException();
                                    len = BitConverter.ToUInt16(data);
                                    break;
                                    
                                case Opcode.OP_PUSHDATA4:
                                    // add next four byte value as length of following data value to op.
                                    if (data.Length != 4) throw new InvalidOperationException();
                                    len = BitConverter.ToUInt32(data);
                                    break;
                            }
                                    
                            // add next one, two, or four byte value as length of following data value to op.
                            if (len > 0)
                            {
                                data = Encoders.Hex.Decode(tokens[++i]);
                            }
                            
                            if (data == null)
                                builder.Add(op);
                            else
                                builder.Add(op, new VarType(data));
                            
                            break;
                        }
                            
                        default:
                            builder.Add(op);
                            break;
                    }
                }
                else
                {
                    var bytes = Encoders.Hex.Decode(tokens[i]);
                    if (bytes != null)
                    {
                        builder.Push(bytes);
                    }
                }
            }

            return builder;
        }
        
        /// <summary>
        /// Parse encoded script.
        /// </summary>
        /// <param name="script">encoded script</param>
        /// <returns>script builder</returns>
        public static ScriptBuilder ParseScript(string script)
        {
            var builder = new ScriptBuilder();
            var tokens = script.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
            for (var i = 0; i < tokens.Length; ++i)
            {
                if (!Enum.TryParse<Opcode>(tokens[i], out var op)) throw new InvalidOperationException();
                switch (op)
                {
                    case Opcode.OP_0:
                    case Opcode.OP_1NEGATE:
                        builder.Add(op);
                        break;

                    case var _ when op < Opcode.OP_PUSHDATA1:
                    {
                        var data = Encoders.Hex.Decode(tokens[++i]);
                        if (data.Length >= (int)Opcode.OP_PUSHDATA1) throw new InvalidOperationException();
                        builder.Add(op, data);
                        break;
                    }

                    case Opcode.OP_PUSHDATA1:
                    case Opcode.OP_PUSHDATA2:
                    case Opcode.OP_PUSHDATA4:
                    {
                        var data = Encoders.Hex.Decode(tokens[i+=2]);
                        builder.Add(op, new VarType(data));
                        break;
                    }

                    default:
                        builder.Add(op);
                        break;
                }
            }

            return builder;
        }
        
        public static implicit operator Script(ScriptBuilder sb) => sb.ToScript();
        public static implicit operator ScriptBuilder(Script v) => new ScriptBuilder(v);
    }
}
