using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Signatures;
using CafeLib.Core.Buffers;
using Newtonsoft.Json;

namespace CafeLib.BsvSharp.Scripting
{
    [JsonConverter(typeof(ScriptConverter))]
    public struct Script : IDataSerializer
    {
        internal VarType Data { get; private set; }

        public static Script None { get; } = new Script(Array.Empty<byte>());

        public long Length => Data.Length;

        private Script(VarType script)
            : this()
        {
            Data = script;
        }

        public Script(byte[] script)
            : this(new VarType(script))
        {
        }

        public Script(string hex)
            : this(Encoders.Hex.Decode(hex))
        {
        }

        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer) => WriteTo(writer, new {withoutCodeSeparators = false});
        
        /// <summary>
        /// Serialize Script to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <param name="parameters">parameters</param>
        /// <returns>data writer</returns>
        public IDataWriter WriteTo(IDataWriter writer, object parameters)
        {
            dynamic args = parameters;
            if (args.withoutCodeSeparators) 
            {
                var ops = Decode().Where(o => o.Code != Opcode.OP_CODESEPARATOR).ToArray();
                writer.Write(ops.Length.AsVarIntBytes());
                foreach (var op in ops)
                    writer.Write(op);
            }
            else 
            {
                writer
                    .Write(Data.Length.AsVarIntBytes())
                    .Write(Data);

            }
            return writer;
        }
        

        public void Read(BinaryReader s)
        {
            var count = s.ReadInt32();
            if (count == -1)
            {
                Data = VarType.Empty;
            }
            else
            {
                var bytes = new byte[count];
                s.Read(bytes);
                Data = new VarType(bytes);
            }
        }

        public void Write(BinaryWriter s)
        {
            if (Data.IsEmpty)
            {
                s.Write(-1);
            }
            else
            {
                s.Write(Data.Length);
                s.Write(Data.Span);
            }
        }

        public Script Slice(SequencePosition start, SequencePosition end) => new Script(Data.Span.Slice(start.GetInteger(), end.GetInteger()));

        public bool IsPushOnly()
        {
            var ros = new ReadOnlyByteSequence(Data);
            var op = new Operand();

            while (ros.Length > 0)
            {
                if (!op.TryReadOperand(ref ros)) return false;
                // Note that IsPushOnly() *does* consider OP_RESERVED to be a push-type
                // opcode, however execution of OP_RESERVED fails, so it's not relevant
                // to P2SH/BIP62 as the scriptSig would fail prior to the P2SH special
                // validation code being executed.
                if (op.Code > Opcode.OP_16) return false;
            }

            Data = new VarType(ros);
            return true;
        }

        public static (bool ok, Script script) ParseHex(string rawScriptHex, bool withoutLength = false)
        {
            var bytes = rawScriptHex.HexToBytes();
            var s = new Script();
            var ros = new ReadOnlyByteSequence(bytes);
            var sr = new ByteSequenceReader(ros);
            return (s.TryReadScript(ref sr, withoutLength), s);
        }

        public int FindAndDelete(VarType vchSig)
        {
            var nFound = 0;
            var s = new ReadOnlyByteSequence(Data);
            var r = s;
            if (vchSig.Length == 0) return nFound;

            var op = new Operand();
            var consumed = 0L;
            var offset = 0L;

            var o = new ReadOnlyByteSequence(vchSig);
            var oLen = o.Length;

            do 
            {
                offset += consumed;
                while (s.StartsWith(o))
                {
                    r = r.RemoveSlice(offset, oLen);
                    s = s.Slice(oLen);
                    ++nFound;
                }
            }
            while (op.TryReadOperand(ref s, out consumed));

            Data = new VarType(r);
            return nFound;
#if false
            CScript result;
            iterator pc = begin(), pc2 = begin();
            opcodetype opcode;

            do {
                result.insert(result.end(), pc2, pc);
                while (static_cast<size_t>(end() - pc) >= b.size() &&
                       std::equal(b.begin(), b.end(), pc)) {
                    pc = pc + b.size();
                    ++nFound;
                }
                pc2 = pc;
            } while (GetOp(pc, opcode));

            if (nFound > 0) {
                result.insert(result.end(), pc2, end());
                *this = result;
            }
#endif
        }

        /// <summary>
        /// Decode script opcodes and push data.
        /// </summary>
        /// <returns></returns>
        public readonly IEnumerable<Operand> Decode()
        {
            var ros = new ReadOnlyByteSequence(Data);

            while (ros.Length > 0)
            {
                var op = new Operand();
                if (op.TryReadOperand(ref ros))
                {
                    yield return op;
                }
                else
                {
                    break;
                }
            }
        }

        public bool TryReadScript(ref ByteSequenceReader r, bool withoutLength = false)
        {
            var length = r.Data.Remaining;

            if (!withoutLength && !r.TryReadVariant(out length)) return false;
            if (r.Data.Remaining < length) return false;

            Data = new VarType((ReadOnlyByteSequence)r.Data.Sequence.Slice(r.Data.Position, length));
            r.Data.Advance(length);
            return true;
        }

        public readonly string ToHexString()
        {
            return Encoders.Hex.Encode(Data);
        }

        public string ToTemplateString()
        {
            var sb = new StringBuilder();
            foreach (var op in Decode())
            {
                var len = op.Data.Length;
                sb.Append(len == 0 ? $"{op.CodeName} " : $"[{op.Data.Length}] ");
            }

            if (sb.Length > 0)
                sb.Length--;

            return sb.ToString();
        }

        /// <summary>
        /// Return the Template String representation of script bytes.
        /// If the returned string does not include all the script opcodes, either because the scriptLen or limitLen
        /// arguments are greater than zero, or if the script sequence ends with an incomplete multi-byte opcode,
        /// then "..." is appended following the last complete opcode.
        ///
        /// scriptLen argument should be used when the actual script is longer than the script sequence provided,
        /// which must then be a subsequence from the start of the script.
        /// If greater than zero it may be longer than the sequence provided in which case "..." will be appended
        /// after the last opcode.
        ///
        /// limitLen argument stops converting opcodes to their template string format after processing this many bytes.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="scriptLen">How long the entire script is, or zero.</param>
        /// <param name="limitLen">How many bytes to process, or zero.</param>
        /// <returns></returns>
        public static string ToTemplateString(byte[] script, long scriptLen = 0, long limitLen = 0) 
        {
            if (script == null)
                return scriptLen > 0 ? "..." : "";
            return ToTemplateString(new ReadOnlySequence<byte>(script), scriptLen, limitLen);
        }

        /// <summary>
        /// Return the Template String representation of script bytes.
        /// If the returned string does not include all the script opcodes, either because the scriptLen or limitLen
        /// arguments are greater than zero, or if the script sequence ends with an incomplete multi-byte opcode,
        /// then "..." is appended following the last complete opcode.
        ///
        /// scriptLen argument should be used when the actual script is longer than the script sequence provided,
        /// which must then be a subsequence from the start of the script.
        /// If greater than zero it may be longer than the sequence provided in which case "..." will be appended
        /// after the last opcode.
        ///
        /// limitLen argument stops converting opcodes to their template string format after processing this many bytes.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="scriptLen">How long the entire script is, or zero.</param>
        /// <param name="limitLen">How many bytes to process, or zero.</param>
        /// <returns></returns>
        public static string ToTemplateString(ReadOnlyByteSequence script, long scriptLen = 0, long limitLen = 0) 
        {
            var ros = script;
            if (limitLen == 0) limitLen = long.MaxValue;
            var ok = true;
            var count = 0L;
            var sb = new StringBuilder();

            while (ros.Length > 0 && ok && limitLen > count)
            {
                var op = new Operand();
                ok = op.TryReadOperand(ref ros, out var consumed);
                count += consumed;
                if (ok && limitLen >= count)
                {
                    var len = op.Data.Length;
                    sb.Append(len == 0 ? $"{op.CodeName} " : $"[{op.Data.Length}] ");
                }
            }
            if (sb.Length > 0)
                sb.Length--;
            if (scriptLen == 0) scriptLen = count;
            if (!ok || limitLen < count || count < scriptLen) {
                sb.Append("...");
            }
            return sb.ToString();
        }

        public string ToVerboseString()
        {
            return string.Join(' ', Decode().Select(op => op.ToVerboseString()));
        }

        public string ToAssemblyString()
        {
            return string.Join(' ', Decode().Select(op => op.ToAssemblyString()));
        }

        public override string ToString()
        {
            return string.Join(' ', Decode().Select(op => op.ToVerboseString()));
        }

        public override int GetHashCode() => Data.GetHashCode();
        public override bool Equals(object obj) => obj is Script script && this == script;
        public bool Equals(Script o) => Length == o.Length; //&& _script.CompareTo(o._script) == 0;
        public static bool operator ==(Script x, Script y) => x.Equals(y);
        public static bool operator !=(Script x, Script y) => !(x == y);

        public static implicit operator Script(VarType rhs) => new Script(rhs);
        public static explicit operator VarType(Script rhs) => rhs.Data;

        /// <summary>
        /// Template 1 P2PK
        /// [65] OP_CHECKSIG
        /// update TxOuts set TemplateId = 1 where ScriptPubLen = 67 and substring(ScriptPubBuf0, 1, 1) = 0x41 and substring(ScriptPubBuf0, 67, 1) = 0xAC
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsPay2PublicKey(ReadOnlyByteSpan script)
        {
            return script.Length == 67 && script[0] == 0x41 && script[66] == 0xAC;
        }

        /// <summary>
        /// Template 2 P2PKH
        /// 0x76A91412AB8DC588CA9D5787DDE7EB29569DA63C3A238C88AC 
        /// OP_DUP OP_HASH160 [20] OP_EQUALVERIFY OP_CHECKSIG
        /// update TxOuts set TemplateId = 2 where ScriptPubLen = 25 and substring(ScriptPubBuf0, 1, 3) = 0x76A914 and substring(ScriptPubBuf0, 24, 2) = 0x88AC
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsPay2PublicKeyHash(ReadOnlyByteSpan script)
        {
            return script.Length == 25 && script[0] == 0x76 && script[1] == 0xA9 && script[2] == 0x14 && script[23] == 0x88 && script[24] == 0xAC;
        }

        /// <summary>
        /// Template 3 OpRetPush4
        /// OP_0 OP_RETURN [4] ...
        /// update TxOuts set TemplateId = 3 where ScriptPubLen >= 7 and substring(ScriptPubBuf0, 1, 3) = 0x006A04
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsOpRetPush4(ReadOnlyByteSpan script)
        {
            return script.Length >= 7 && script[0] == 0x00 && script[1] == 0x6A && script[2] == 0x04;
        }

        private static readonly byte[] OpRetBPrefix = { 0x6a, 0x22, 0x31, 0x39, 0x48, 0x78, 0x69, 0x67, 0x56, 0x34, 0x51, 0x79, 0x42, 0x76, 0x33, 0x74, 0x48, 0x70, 0x51, 0x56, 0x63, 0x55, 0x45, 0x51, 0x79, 0x71, 0x31, 0x70, 0x7a, 0x5a, 0x56, 0x64, 0x6f, 0x41, 0x75, 0x74 };

        /// <summary>
        /// 0x6a2231394878696756345179427633744870515663554551797131707a5a56646f417574
        /// 0x006a2231394878696756345179427633744870515663554551797131707a5a56646f417574
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsOpRetB(ReadOnlyByteSpan script)
        {
            if (script.Length <= OpRetBPrefix.Length + 1)
                return false;
            var o = script[0] == 0 ? 1 : 0;
            return script.Data.Slice(o, OpRetBPrefix.Length).SequenceEqual(OpRetBPrefix);
        }

        private static readonly byte[] OpRetBcatPrefix = { 0x6a, 0x22, 0x31, 0x35, 0x44, 0x48, 0x46, 0x78, 0x57, 0x5a, 0x4a, 0x54, 0x35, 0x38, 0x66, 0x39, 0x6e, 0x68, 0x79, 0x47, 0x6e, 0x73, 0x52, 0x42, 0x71, 0x72, 0x67, 0x77, 0x4b, 0x34, 0x57, 0x36, 0x68, 0x34, 0x55, 0x70 };

        /// <summary>
        /// 0x6a22313544484678575a4a54353866396e6879476e735242717267774b34573668345570
        /// 0x006a22313544484678575a4a54353866396e6879476e735242717267774b34573668345570
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsOpRetBcat(ReadOnlyByteSpan script) {
            if (script.Length <= OpRetBcatPrefix.Length + 1)
                return false;
            var o = script[0] == 0 ? 1 : 0;
            return script.Data.Slice(o, OpRetBcatPrefix.Length).SequenceEqual(OpRetBcatPrefix);
        }

        private static readonly byte[] OpRetBcatPartPrefix = { 0x6a, 0x22, 0x31, 0x43, 0x68, 0x44, 0x48, 0x7a, 0x64, 0x64, 0x31, 0x48, 0x34, 0x77, 0x53, 0x6a, 0x67, 0x47, 0x4d, 0x48, 0x79, 0x6e, 0x64, 0x5a, 0x6d, 0x36, 0x71, 0x78, 0x45, 0x44, 0x47, 0x6a, 0x71, 0x70, 0x4a, 0x4c };

        /// <summary>
        /// 0x6a2231436844487a646431483477536a67474d48796e645a6d3671784544476a71704a4c
        /// 0x006a2231436844487a646431483477536a67474d48796e645a6d3671784544476a71704a4c
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public static bool IsOpRetBcatPart(ReadOnlySpan<byte> script)
        {
            if (script.Length <= OpRetBcatPartPrefix.Length + 1)
                return false;
            var o = script[0] == 0 ? 1 : 0;
            return script.Slice(o, OpRetBcatPartPrefix.Length).SequenceEqual(OpRetBcatPartPrefix);
        }

        /// <summary>
        /// Returns true if the script is an unspendable OP_RETURN.
        /// Prior to the Genesis upgrade (block 620538), an OP_RETURN script could never evaluate to true.
        /// After Genesis, the value at the top of the stack when executing an OP_RETURN determines the script result.
        /// Therefore, after Genesis, a value of zero is pushed before the OP_RETURN (which may be followed by arbitrary push datas)
        /// to create an unspendable output.
        /// Unspendable outputs can be safely pruned by transaction processors.
        /// Unspendable outputs can always be retrieved for a price from archive services.
        /// </summary>
        /// <param name="script">The initial buffer of the transaction output script. Typically up to 256 bytes or so of the script.</param>
        /// <param name="height">The block height of the transaction containing the output script.</param>
        /// <returns></returns>
        public static bool IsOpReturn(ReadOnlyByteSpan script, int? height = null) 
        {
            var result = false;
            if (script.Length > 0 && script[0] == 0x6a)
            {
                if (height <= 620538)
                {
                    result = true;
                }
            } 
            else if (script.Length > 1 && script[1] == 0x6a && script[0] == 0) 
            {
                result = true;
            }
            return result;
        }

        public static (bool unspendable, TemplateId templateId) ParseKnownScriptPubTemplates(ReadOnlySpan<byte> scriptPubBuf0, int? height) {

            // Check for OP_RETURN outputs, these are unspendable and are flagged with a -1 SpentByTxId value.
            // After Genesis, bare OP_RETURN is spendable (anything that pushes true on sig script can spend.
            var unspendable = IsOpReturn(scriptPubBuf0, height);
            
            TemplateId templateId;

            if (unspendable)
            {
                templateId
                    = IsOpRetPush4(scriptPubBuf0) ? TemplateId.OpRetPush4
                    : IsOpRetB(scriptPubBuf0) ? TemplateId.OpRetB
                    : IsOpRetBcat(scriptPubBuf0) ? TemplateId.OpRetBcat
                    : IsOpRetBcatPart(scriptPubBuf0) ? TemplateId.OpRetBcatPart
                    : TemplateId.OpRet;
            }
            else
            {
                // Spendable
                templateId
                    = IsPay2PublicKey(scriptPubBuf0) ? TemplateId.Pay2PublicKey
                    : IsPay2PublicKeyHash(scriptPubBuf0) ? TemplateId.Pay2PublicKeyHash
                    : TemplateId.Unknown;
            }

            return (unspendable, templateId);
        }

        /// <summary>
        /// Without height, returns true only for OP_0 OP_RETURN pattern.
        /// With height, pre 620538 blocks also treat just a bare OP_RETURN to be unspendable.
        /// </summary>
        /// <returns></returns>
        public bool IsOpReturn(int? height = null) => IsOpReturn(Data, height);

        public static (bool ok, SignatureHashEnum sh, byte[] r, byte[] s, PublicKey pk) IsCheckSigScript(byte[] scriptSigBytes) => IsCheckSigScript(new ReadOnlySequence<byte>(scriptSigBytes));
            
        public static (bool ok, SignatureHashEnum sh, byte[] r, byte[] s, PublicKey pk) IsCheckSigScript(ReadOnlyByteSequence scriptSigBytes) {
            var ros = scriptSigBytes;
            var (ok1, op1) = Operand.TryRead(ref ros, out var consumed1);
            var (ok2, op2) = Operand.TryRead(ref ros, out var consumed2);
            if (!ok1 || !ok2 || consumed1 + consumed2 != scriptSigBytes.Length) goto fail;
            if (op2.Data.Length < PublicKey.CompressedLength || op2.Data.Length > PublicKey.UncompressedLength) goto fail;

            var pubkey = new PublicKey(op2.Data);
            if (!pubkey.IsValid) goto fail;

            ReadOnlyByteSpan sig = op1.Data;
            if (sig.Length < 7 || sig[0] != 0x30 || sig[2] != 0x02) goto fail;
            var lenDer = sig[1];
            var lenR = sig[3];
            if (sig.Length != lenDer + 3 || sig.Length - 1 < lenR + 5 || sig[4 + lenR] != 0x02) goto fail;
            var lenS = sig[lenR + 5];
            if (sig.Length != lenR + lenS + 7) goto fail;

            var sh = (SignatureHashEnum)(sig[^1]);

            var r = sig.Slice(4, lenR).ToArray();
            var s = sig.Slice(6 + lenR, lenS).ToArray();

            return (true, sh, r, s, pubkey);

            fail:
            return (false, SignatureHashEnum.Unsupported, null, null, null);
        }
    }
}