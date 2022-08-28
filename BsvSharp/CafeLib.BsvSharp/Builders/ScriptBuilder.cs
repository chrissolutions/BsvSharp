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
        /// true if script is associated with a scriptPubKey.
        /// false if script is associated with a scriptSig.
        /// null if script purpose is unknown.
        /// </summary>
        private bool? _scriptPubKeyFlag;

        /// <summary>
        /// The sequence of operations where each operation is an opcode and optional data.
        /// To support testing and unimplemented features, an operation's IsRaw flag can be set in
        /// which case the opcode is ignored and the data is treated as unparsed script code.
        /// </summary>
        public List<OperandBuilder> Operands { get; protected set; } = new List<OperandBuilder>();

        /// <summary>
        /// true when no more additions, deletions or changes to existing operations will occur.
        /// </summary>
        public bool IsFinal
        {
            get => _isFinal && Operands.All(op => op.IsFinal);
            protected set => _isFinal = value;
        }

        public bool IsScriptPubKey
        {
            get => _scriptPubKeyFlag == true; 
            set => _scriptPubKeyFlag = value ? true : null;
        }

        public bool IsScriptSig
        {
            get => _scriptPubKeyFlag == false; 
            set => _scriptPubKeyFlag = value ? false : null;
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
            _scriptPubKeyFlag = isPub;
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
            Operands.Clear(); 
            return this;
        }

        public ScriptBuilder Set(Script script)
        {
            Operands.Clear(); 
            return Add(script);
        }

        public ScriptBuilder Add(Opcode opcode)
        {
            Operands.Add(new Operand(opcode));
            return this;
        }

        public ScriptBuilder Add(Opcode opcode, VarType v)
        {
            Operands.Add(new Operand(opcode, v)); 
            return this;
        }

        public ScriptBuilder Add(OperandBuilder opBuilder)
        {
            Operands.Add(opBuilder); 
            return this;
        }

        public ScriptBuilder Add(Script script)
        {
            Operands.AddRange(script.Decode().Select(o => new OperandBuilder(o)));
            return this;
        }

        public ScriptBuilder Add(string hex)
        {
            var script = !string.IsNullOrWhiteSpace(hex)
                ? Script.FromHex(hex)
                : new Script(new[] {(byte) Opcode.OP_FALSE, (byte) Opcode.OP_RETURN});
            return Add(script);
        }

        public virtual ScriptBuilder Add(byte[] raw)
        {
            Operands.Add(new OperandBuilder(new VarType(raw)));
            return this;
        }

        /// <summary>
        /// Push a zero as a non-final placeholder.
        /// </summary>
        /// <returns></returns>
        public ScriptBuilder AddData() => Add(new OperandBuilder { IsFinal = false, IsRaw = false, Operand = new Operand(Opcode.OP_0) });

        public ScriptBuilder AddData(ReadOnlyByteSpan data)
        {
            Operands.Add(Operand.Pushdata(data)); 
            return this;
        }

        public ScriptBuilder AddData(long v)
        {
            Operands.Add(Operand.Pushdata(v));
            return this;
        }

        /// <summary>
        /// Build Script.
        /// </summary>
        /// <returns>script</returns>
        public virtual Script ToScript() => new(ToArray());

        /// <summary>
        /// Convert script builder to byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            var bytes = new byte[Operands.Sum(o => o.Length)];
            var span = (ByteSpan)bytes;
            foreach (var op in Operands) 
            {
                op.TryCopyTo(ref span);
            }

            return bytes;
        }

        public override string ToString()
        {
            return string.Join(' ', Operands.Select(o => o.ToVerboseString()));
        }

        public string ToTemplateString()
        {
            var sb = new StringBuilder();
            foreach (var bop in Operands) 
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
        /// </summary>
        /// <param name="value">value to parse into bytes</param>
        /// <returns>byte array</returns>
        private static byte[] ParseValueToBytes(string value) => ParseLiteralValueToBytes(value).bytes;

        /// <summary>
        /// Parses signed decimals, hexadecimal strings prefixed with 0x, and ascii strings enclosed in single quotes.
        /// Each format is converted to a byte array.
        /// Converts hex and ascii strings to a specific byte count, if len has a value and disagrees it is an error.
        /// Converts integer values to little endian bytes where the most significant bit is set if negative.
        /// </summary>
        /// <param name="token">token to parse</param>
        /// <returns>Tuple of the parsed byte[] data and a boolean true if the literal was specified in hexadecimal.
        /// Returns null for bytes if can't be parsed as a literal.</returns>
        private static (byte[] bytes, bool isHex) ParseLiteralValueToBytes(string token)
        {
            switch (token)
            {
                case var _ when token.StartsWith("0x"):
                    var bytes = Encoders.Hex.Decode(token[2..]);
                    return (bytes, true);

                case var _ when token.StartsWith("'") && token.EndsWith("'"):
                    token = token[1..^1];
                    if (token.Contains('\'')) throw new InvalidOperationException();
                    bytes = Encoders.Ascii.Decode(token);
                    return (bytes, false);

                case var _ when long.TryParse(token, out var v):
                    bytes = new ScriptNum(v).ToArray();
                    return (bytes, false);

                default:
                    return (null, false);
            }
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
            var tokens = testScript.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
            while (tokens.Length > 0) {
                var index = 0;
                var (bytes, isHex) = ParseLiteralValueToBytes(tokens[index]);
                if (bytes != null) {
                    if (isHex)
                        // Hex literals are treated as raw, unparsed bytes added to the script.
                        sb.Add(bytes);
                    else
                        sb.AddData(bytes);
                }
                else
                {
                    byte[] data = null;
                    if (!Enum.TryParse($"OP_{tokens[index]}", out Opcode opcode))
                        throw new InvalidOperationException();
                    switch (opcode)
                    {
                        case > Opcode.OP_0 and < Opcode.OP_PUSHDATA1:
                        {
                            // add next single byte value to op.
                            index++;
                            data = ParseValueToBytes(tokens[index]);
                            if (data == null)
                            {
                                // Put this token back. Treat missing data as zero length.
                                data = Array.Empty<byte>();
                                index--;
                            }

                            break;
                        }
                        case >= Opcode.OP_PUSHDATA1 and <= Opcode.OP_PUSHDATA4:
                        {
                            // add next one, two, or four byte value as length of following data value to op.
                            index++;
                            var lengthBytes = ParseValueToBytes(tokens[index]);
                            if (!BitConverter.IsLittleEndian)
                                throw new NotSupportedException();
                            var len = opcode switch
                            {
                                // add next one byte value as length of following data value to op.
                                Opcode.OP_PUSHDATA1 when lengthBytes.Length != 1 => throw new InvalidOperationException(), 
                                Opcode.OP_PUSHDATA1 => lengthBytes[0],
                                
                                // add next two byte value as length of following data value to op.
                                Opcode.OP_PUSHDATA2 when lengthBytes.Length != 2 => throw new InvalidOperationException(),
                                Opcode.OP_PUSHDATA2 => BitConverter.ToUInt16(lengthBytes),
                                
                                // add next four byte value as length of following data value to op.
                                Opcode.OP_PUSHDATA4 when lengthBytes.Length != 4 => throw new InvalidOperationException(),
                                Opcode.OP_PUSHDATA4 => BitConverter.ToUInt32(lengthBytes),
                                
                                _ => 0u
                            };
                            
                            if (len > 0) {
                                index++;
                                data = index < tokens.Length ? ParseValueToBytes(tokens[index]) : Array.Empty<byte>();
                            }

                            break;
                        }
                    }
                    if (data == null)
                        sb.Add(opcode);
                    else
                        sb.Add(opcode, new VarType(data));
                }
                tokens = tokens[Math.Min(index + 1, tokens.Length)..];
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

                        case < Opcode.OP_PUSHDATA1:
                        {
                            var data = Encoders.Hex.Decode(tokens[++i]);
                            if (data.Length >= (int)Opcode.OP_PUSHDATA1) throw new InvalidOperationException();
                            builder.Add(op, data);
                            break;
                        }

                        case <= Opcode.OP_PUSHDATA4:
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
                        builder.AddData(bytes);
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

                    case < Opcode.OP_PUSHDATA1:
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
        public static implicit operator ScriptBuilder(Script v) => new(v);
    }
}
