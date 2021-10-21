#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Encoding;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Buffers;

namespace CafeLib.BsvSharp.Builders
{
    /// <summary>
    /// The KzScriptBuilder maintains a list of builder Ops "BOps".
    /// The primary reason is to allow raw bytes to be added to a script.
    /// </summary>
    public struct OperandBuilder
    {
        /// <summary>
        /// true if no changes to this operation will happen
        /// false if this operation still needs to be changed
        /// false is typically used for data placeholders
        /// </summary>
        public bool IsFinal;

        /// <summary>
        /// If IsRaw is true, ignore Op.Code and just add Op.Data bytes to script.
        /// </summary>
        public bool IsRaw;

        /// <summary>
        /// KzOp is the standard script opcode plus data bytes struct.
        /// If IsRaw is true, ignore Op.Code and just add Op.Data bytes to script.
        /// </summary>
        public Operand Operand;

        /// <summary>
        /// Operand opcode.
        /// </summary>
        public Opcode Opcode => Operand.Code;

        public OperandBuilder(Operand op)
            : this()
        {
            IsFinal = true;
            Operand = op;
        }

        public OperandBuilder(VarType data)
            : this()
        {
            IsFinal = true; 
            IsRaw = true;
            Operand = new Operand(Opcode.OP_NOP, data);
        }

        public long Length => IsRaw ? Operand.Data.Length : Operand.Length;

        public static implicit operator OperandBuilder(Operand op) 
            => new OperandBuilder { IsFinal = true, Operand = op};

        public bool TryCopyTo(ref ByteSpan span)
        {
            if (IsRaw)
            {
                var len = (int)Length;
                if (len > span.Length) goto fail;
                Operand.Data.Span.CopyTo(span.Slice(0, len));
                span = span.Slice(len);
            }
            else
            {
                if (!Operand.TryCopyTo(ref span)) goto fail;
            }

            return true;

            fail:
            return false;
        }

        public string ToVerboseString() => IsRaw ? Encoders.Hex.EncodeSpan(Operand.Data) : Operand.ToVerboseString();

        public override string ToString()
        {
            return ToVerboseString();
        }
    }
}
