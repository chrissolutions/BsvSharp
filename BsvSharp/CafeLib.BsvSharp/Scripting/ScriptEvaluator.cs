﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Runtime.CompilerServices;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Network;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Services;
using CafeLib.BsvSharp.Signatures;
using CafeLib.Core.Buffers;
using CafeLib.Cryptography;

namespace CafeLib.BsvSharp.Scripting
{
    internal class ScriptEvaluator
    {
        private readonly ScriptStack<VarType> _stack;

        private static readonly DefaultSignatureChecker DefaultSignatureChecker = new();

        private readonly NetworkType _networkType;

        public int Count => _stack.Count;

        public VarType Peek() => _stack.Peek();

        internal ScriptEvaluator(NetworkType? networkType = null)
        {
            _networkType = RootService.GetNetwork(networkType).NodeType;
            _stack = new ScriptStack<VarType>();
        }

        /// <summary>
        /// Modeled on Bitcoin-SV interpreter.cpp 0.1.1 lines 384-1520
        /// </summary>
        /// <param name="script">script instance</param>
        /// <param name="flags">script flags</param>
        /// <param name="checker">signature checker</param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool EvalScript(Script script, ScriptFlags flags, ISignatureChecker checker, out ScriptError error)
        {
            var ros = new ReadOnlyByteSequence(script.Data);
            var pStart = ros.Start;
            var pend = ros.End;
            var op = new Operand();
            var vfExec = new ScriptStack<bool>();
            var altStack = new ScriptStack<VarType>();
            checker ??= DefaultSignatureChecker;

            SetError(out error, ScriptError.UNKNOWN_ERROR);

            if (script.Length > RootService.GetNetwork(_networkType).Consensus.MaxScriptSize)
                return SetError(out error, ScriptError.SCRIPT_SIZE);

            var nOpCount = 0;
            var fRequireMinimal = (flags & ScriptFlags.VERIFY_MINIMALDATA) != 0;

            try
            {
                while (ros.Length > 0)
                {
                    var fExec = vfExec.Contains(false) == false;

                    if (!op.TryReadOperand(ref ros))
                    {
                        return SetError(out error, ScriptError.BAD_OPCODE);
                    }

                    if (op.Data.Length > RootService.GetNetwork(_networkType).Consensus.MaxScriptElementSize)
                    {
                        return SetError(out error, ScriptError.PUSH_SIZE);
                    }

                    if (op.Code > Opcode.OP_16)
                    {
                        ++nOpCount;
                        if (!IsValidMaxOpsPerScript(nOpCount))
                        {
                            return SetError(out error, ScriptError.OP_COUNT);
                        }
                    }

                    // Some opcodes are disabled.
                    if (IsOpcodeDisabled(op.Code, flags))
                    {
                        return SetError(out error, ScriptError.DISABLED_OPCODE);
                    }

                    if (fExec && op.Code <= Opcode.OP_PUSHDATA4)
                    {
                        if (fRequireMinimal && !CheckMinimalPush(ref op))
                        {
                            return SetError(out error, ScriptError.MINIMALDATA);
                        }
                        _stack.Push(op.Data);
                        // ( -- value)
                    }
                    else if (fExec || op.Code is >= Opcode.OP_IF and <= Opcode.OP_ENDIF)
                    {
                        switch (op.Code)
                        {
                            //
                            // Push value
                            //
                            case Opcode.OP_1NEGATE:
                            case Opcode.OP_1:
                            case Opcode.OP_2:
                            case Opcode.OP_3:
                            case Opcode.OP_4:
                            case Opcode.OP_5:
                            case Opcode.OP_6:
                            case Opcode.OP_7:
                            case Opcode.OP_8:
                            case Opcode.OP_9:
                            case Opcode.OP_10:
                            case Opcode.OP_11:
                            case Opcode.OP_12:
                            case Opcode.OP_13:
                            case Opcode.OP_14:
                            case Opcode.OP_15:
                            case Opcode.OP_16:
                            {
                                var sn = new ScriptNum((int)op.Code - (int)Opcode.OP_1 + 1);
                                _stack.Push(sn.ToValType());
                                // ( -- value)
                                break;
                            }

                            //
                            // Control
                            //
                            case Opcode.OP_NOP:
                                break;
                            case Opcode.OP_CHECKLOCKTIMEVERIFY:
                                break;
                            case Opcode.OP_CHECKSEQUENCEVERIFY:
                                break;

                            case Opcode.OP_NOP1:
                            case Opcode.OP_NOP4:
                            case Opcode.OP_NOP5:
                            case Opcode.OP_NOP6:
                            case Opcode.OP_NOP7:
                            case Opcode.OP_NOP8:
                            case Opcode.OP_NOP9:
                            case Opcode.OP_NOP10:
                            {
                                if ((flags & ScriptFlags.VERIFY_DISCOURAGE_UPGRADABLE_NOPS) != 0)
                                {
                                    return SetError(out error, ScriptError.DISCOURAGE_UPGRADABLE_NOPS);
                                }
                                break;
                            }

                            case Opcode.OP_IF:
                            case Opcode.OP_NOTIF:
                            {
                                // <expression> if [statements] [else [statements]]
                                // endif
                                var fValue = false;
                                if (fExec)
                                {
                                    if (_stack.Count < 1)
                                    {
                                        return SetError(out error, ScriptError.UNBALANCED_CONDITIONAL);
                                    }
                                    var vch = _stack.Pop();
                                    if ((flags & ScriptFlags.VERIFY_MINIMALIF) != 0)
                                    {
                                        if (vch.Length > 1 || vch.Length == 1 && vch.FirstByte != 1)
                                        {
                                            _stack.Push(vch);
                                            return SetError(out error, ScriptError.MINIMALIF);
                                        }
                                    }
                                    fValue = vch.ToBool();
                                    if (op.Code == Opcode.OP_NOTIF)
                                    {
                                        fValue = !fValue;
                                    }
                                }
                                vfExec.Push(fValue);
                                break;
                            }

                            case Opcode.OP_ELSE:
                            {
                                if (vfExec.Count < 1)
                                {
                                    return SetError(out error, ScriptError.UNBALANCED_CONDITIONAL);
                                }
                                vfExec.Push(!vfExec.Pop());
                                break;
                            }

                            case Opcode.OP_ENDIF:
                            {
                                if (vfExec.Count < 1)
                                {
                                    return SetError(out error, ScriptError.UNBALANCED_CONDITIONAL);
                                }
                                vfExec.Pop();
                                break;
                            }

                            case Opcode.OP_VERIFY:
                            {
                                // (true -- ) or
                                // (false -- false) and return
                                if (_stack.Count < 1)
                                {
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                }
                                var vch = _stack.Pop();
                                var fValue = vch.ToBool();
                                if (!fValue)
                                {
                                    _stack.Push(vch);
                                    return SetError(out error, ScriptError.VERIFY);
                                }
                                break;
                            }

                            case Opcode.OP_RETURN:
                                return SetError(out error, ScriptError.OP_RETURN);

                            //
                            // Stack ops
                            //
                            case Opcode.OP_TOALTSTACK:
                            {
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                altStack.Push(_stack.Pop());
                                break;
                            }

                            case Opcode.OP_FROMALTSTACK:
                            {
                                if (altStack.Count < 1) return SetError(out error, ScriptError.INVALID_ALTSTACK_OPERATION);
                                _stack.Push(altStack.Pop());
                                break;
                            }

                            case Opcode.OP_2DROP:
                            {
                                // (x1 x2 -- )
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Drop2();
                                break;
                            }

                            case Opcode.OP_2DUP:
                            {
                                // (x1 x2 -- x1 x2 x1 x2)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Dup2();
                                break;
                            }

                            case Opcode.OP_3DUP:
                            {
                                // (x1 x2 x3 -- x1 x2 x3 x1 x2 x3)
                                if (_stack.Count < 3) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Dup3();
                                break;
                            }

                            case Opcode.OP_2OVER:
                            {
                                // (x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2)
                                if (_stack.Count < 4) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Over2();
                                break;
                            }

                            case Opcode.OP_2ROT:
                            {
                                // (x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2)
                                if (_stack.Count < 6) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Rot2();
                                break;
                            }

                            case Opcode.OP_2SWAP:
                            {
                                // (x1 x2 x3 x4 -- x3 x4 x1 x2)
                                if (_stack.Count < 4) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Swap2();
                                break;
                            }

                            case Opcode.OP_IFDUP:
                            {
                                // (x - 0 | x x)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var vch = _stack.Peek();
                                if (vch.ToBool())
                                    _stack.Push(vch);
                                break;
                            }

                            case Opcode.OP_DEPTH:
                            {
                                // -- _stacksize
                                _stack.Push(new ScriptNum(_stack.Count).ToValType());
                                break;
                            }

                            case Opcode.OP_DROP:
                            {
                                // (x -- )
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Pop();
                                break;
                            }

                            case Opcode.OP_DUP:
                            {
                                // (x -- x x)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Push(_stack.Peek());
                                break;
                            }

                            case Opcode.OP_NIP:
                            {
                                // (x1 x2 -- x2)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Nip();
                                break;
                            }

                            case Opcode.OP_OVER:
                            {
                                // (x1 x2 -- x1 x2 x1)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Over();
                                break;
                            }

                            case Opcode.OP_PICK:
                            case Opcode.OP_ROLL:
                            {
                                // (xn ... x2 x1 x0 n - xn ... x2 x1 x0 xn)
                                // (xn ... x2 x1 x0 n - ... x2 x1 x0 xn)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var n = _stack.Pop().ToScriptNum(fRequireMinimal).ToInt();
                                if (n < 0 || n >= _stack.Count) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                if (op.Code == Opcode.OP_ROLL)
                                    _stack.Roll(n);
                                else
                                    _stack.Pick(n);
                                break;
                            }

                            case Opcode.OP_ROT:
                            {
                                // (x1 x2 x3 -- x2 x3 x1)
                                if (_stack.Count < 3) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Rot();
                                break;
                            }

                            case Opcode.OP_SWAP:
                            {
                                // (x1 x2 -- x2 x1)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Swap();
                                break;
                            }

                            case Opcode.OP_TUCK:
                            {
                                // (x1 x2 -- x2 x1 x2)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Tuck();
                                break;
                            }

                            case Opcode.OP_SIZE:
                            {
                                // (in -- in size)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var sn = new ScriptNum(_stack.Peek().Length);
                                _stack.Push(sn.ToValType());
                                break;
                            }

                            //
                            // Bitwise logic
                            //
                            case Opcode.OP_AND:
                            case Opcode.OP_OR:
                            case Opcode.OP_XOR:
                            {
                                // (x1 x2 - out)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var x2 = _stack.Pop();
                                var x1 = _stack.Pop();

                                // Inputs must be the same size
                                if (x1.Length != x2.Length) return SetError(out error, ScriptError.INVALID_OPERAND_SIZE);

                                // To avoid allocating, we modify vch1 in place.
                                switch (op.Code)
                                {
                                    case Opcode.OP_AND:
                                        _stack.Push(x1.BitAnd(x2));
                                        break;

                                    case Opcode.OP_OR:
                                        _stack.Push(x1.BitOr(x2));
                                        break;

                                    case Opcode.OP_XOR:
                                        _stack.Push(x1.BitXor(x2));
                                        break;
                                }
                                break;
                            }

                            case Opcode.OP_INVERT:
                            {
                                // (x -- out)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                _stack.Push(_stack.Pop().BitInvert());
                                break;
                            }

                            case Opcode.OP_LSHIFT:
                            case Opcode.OP_RSHIFT:
                            {
                                // (x n -- out)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var nvt = _stack.Pop();
                                var n = nvt.ToInt32();
                                if (n < 0)
                                {
                                    _stack.Push(nvt);
                                    return SetError(out error, ScriptError.INVALID_NUMBER_RANGE);
                                }
                                var x = _stack.Pop();
                                var r = op.Code == Opcode.OP_LSHIFT ? x.LShift(n) : x.RShift(n);
                                _stack.Push(r);
                                break;
                            }

                            case Opcode.OP_EQUAL:
                            case Opcode.OP_EQUALVERIFY:
                            // case OP_NOTEQUAL: // use OP_NUMNOTEQUAL
                            {
                                // (x1 x2 - bool)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var x2 = _stack.Pop();
                                var x1 = _stack.Pop();

                                var fEqual = x1.BitEquals(x2); // (vch1 == vch2);
                                                               // OP_NOTEQUAL is disabled because it would be too
                                                               // easy to say something like n != 1 and have some
                                                               // wiseguy pass in 1 with extra zero bytes after it
                                                               // (numerically, 0x01 == 0x0001 == 0x000001)
                                                               // if (opcode == OP_NOTEQUAL)
                                                               //    fEqual = !fEqual;
                                _stack.Push(fEqual ? VarType.True : VarType.False);
                                if (op.Code == Opcode.OP_EQUALVERIFY)
                                {
                                    if (fEqual)
                                        _stack.Pop();
                                    else
                                        return SetError(out error, ScriptError.EQUALVERIFY);
                                }
                                break;
                            }

                            //
                            // Numeric
                            //
                            case Opcode.OP_1ADD:
                            case Opcode.OP_1SUB:
                            case Opcode.OP_NEGATE:
                            case Opcode.OP_ABS:
                            case Opcode.OP_NOT:
                            case Opcode.OP_0NOTEQUAL:
                            {
                                // (in -- out)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var bn = _stack.Pop().ToScriptNum(fRequireMinimal);
                                switch (op.Code)
                                {
                                    case Opcode.OP_1ADD:
                                        bn += ScriptNum.One;
                                        break;
                                    case Opcode.OP_1SUB:
                                        bn -= ScriptNum.One;
                                        break;
                                    case Opcode.OP_NEGATE:
                                        bn = -bn;
                                        break;
                                    case Opcode.OP_ABS:
                                        if (bn < ScriptNum.Zero)
                                        {
                                            bn = -bn;
                                        }
                                        break;
                                    case Opcode.OP_NOT:
                                        bn = (bn == ScriptNum.Zero);
                                        break;
                                    case Opcode.OP_0NOTEQUAL:
                                        bn = (bn != ScriptNum.Zero);
                                        break;
                                    default:
                                        return SetError(out error, ScriptError.BAD_OPCODE);
                                }
                                _stack.Push(bn.ToValType());
                                break;
                            }

                            case Opcode.OP_ADD:
                            case Opcode.OP_SUB:
                            case Opcode.OP_MUL:
                            case Opcode.OP_DIV:
                            case Opcode.OP_MOD:
                            case Opcode.OP_BOOLAND:
                            case Opcode.OP_BOOLOR:
                            case Opcode.OP_NUMEQUAL:
                            case Opcode.OP_NUMEQUALVERIFY:
                            case Opcode.OP_NUMNOTEQUAL:
                            case Opcode.OP_LESSTHAN:
                            case Opcode.OP_GREATERTHAN:
                            case Opcode.OP_LESSTHANOREQUAL:
                            case Opcode.OP_GREATERTHANOREQUAL:
                            case Opcode.OP_MIN:
                            case Opcode.OP_MAX:
                            {
                                // (x1 x2 -- out)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var bn2 = _stack.Pop().ToScriptNum(fRequireMinimal);
                                var bn1 = _stack.Pop().ToScriptNum(fRequireMinimal);
                                // ReSharper disable once RedundantAssignment
                                var bn = new ScriptNum(0);
                                switch (op.Code)
                                {
                                    case Opcode.OP_ADD:
                                        bn = bn1 + bn2;
                                        break;

                                    case Opcode.OP_SUB:
                                        bn = bn1 - bn2;
                                        break;

                                    case Opcode.OP_MUL:
                                        bn = bn1 * bn2;
                                        break;

                                    case Opcode.OP_DIV:
                                        // denominator must not be 0
                                        if (bn2 == 0) return SetError(out error, ScriptError.DIV_BY_ZERO);
                                        bn = bn1 / bn2;
                                        break;

                                    case Opcode.OP_MOD:
                                        // divisor must not be 0
                                        if (bn2 == 0) return SetError(out error, ScriptError.MOD_BY_ZERO);
                                        bn = bn1 % bn2;
                                        break;

                                    case Opcode.OP_BOOLAND:
                                        bn = (bn1 != ScriptNum.Zero && bn2 != ScriptNum.Zero);
                                        break;

                                    case Opcode.OP_BOOLOR:
                                        bn = (bn1 != ScriptNum.Zero || bn2 != ScriptNum.Zero);
                                        break;

                                    case Opcode.OP_NUMEQUAL:
                                        bn = (bn1 == bn2);
                                        break;

                                    case Opcode.OP_NUMEQUALVERIFY:
                                        bn = (bn1 == bn2);
                                        break;

                                    case Opcode.OP_NUMNOTEQUAL:
                                        bn = (bn1 != bn2);
                                        break;

                                    case Opcode.OP_LESSTHAN:
                                        bn = (bn1 < bn2);
                                        break;

                                    case Opcode.OP_GREATERTHAN:
                                        bn = (bn1 > bn2);
                                        break;

                                    case Opcode.OP_LESSTHANOREQUAL:
                                        bn = (bn1 <= bn2);
                                        break;

                                    case Opcode.OP_GREATERTHANOREQUAL:
                                        bn = (bn1 >= bn2);
                                        break;

                                    case Opcode.OP_MIN:
                                        bn = (bn1 < bn2 ? bn1 : bn2);
                                        break;

                                    case Opcode.OP_MAX:
                                        bn = (bn1 > bn2 ? bn1 : bn2);
                                        break;

                                    default:
                                        return SetError(out error, ScriptError.BAD_OPCODE);
                                }
                                _stack.Push(bn.ToValType());

                                if (op.Code == Opcode.OP_NUMEQUALVERIFY)
                                {
                                    var vch = _stack.Pop();
                                    if (!vch.ToBool())
                                    {
                                        _stack.Push(vch);
                                        return SetError(out error, ScriptError.NUMEQUALVERIFY);
                                    }
                                }
                                break;
                            }

                            case Opcode.OP_WITHIN:
                            {
                                // (x1 min2 max3 -- out)
                                if (_stack.Count < 3) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var bn3 = _stack.Pop().ToScriptNum(fRequireMinimal);
                                var bn2 = _stack.Pop().ToScriptNum(fRequireMinimal);
                                var bn1 = _stack.Pop().ToScriptNum(fRequireMinimal);
                                // ReSharper disable once UnusedVariable
                                var bn = new ScriptNum(0);
                                var fValue = (bn2 <= bn1 && bn1 < bn3);
                                _stack.Push(fValue ? VarType.True : VarType.False);
                                break;
                            }

                            //
                            // Crypto
                            //
                            case Opcode.OP_RIPEMD160:
                            case Opcode.OP_SHA1:
                            case Opcode.OP_SHA256:
                            case Opcode.OP_HASH160:
                            case Opcode.OP_HASH256:
                            {
                                // (in -- hash)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var vch = _stack.Pop();
                                byte[] data;
                                switch (op.Code)
                                {
                                    case Opcode.OP_SHA1:
                                        data = vch.Span.Sha1();
                                        break;

                                    case Opcode.OP_RIPEMD160:
                                        data = vch.Span.Ripemd160();
                                        break;

                                    case Opcode.OP_HASH160:
                                        data = vch.Span.Hash160();
                                        break;

                                    case Opcode.OP_SHA256:
                                        data = vch.Span.Sha256();
                                        break;

                                    case Opcode.OP_HASH256:
                                        data = vch.Span.Hash256();
                                        break;

                                    default:
                                        return SetError(out error, ScriptError.BAD_OPCODE);
                                }
                                _stack.Push(new VarType(data));
                                break;
                            }

                            case Opcode.OP_CODESEPARATOR:
                            {
                                // Hash starts after the code separator
                                pStart = ros.Data.Start;
                                break;
                            }

                            case Opcode.OP_CHECKSIG:
                            case Opcode.OP_CHECKSIGVERIFY:
                            {
                                // (sig pubkey -- bool)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                var vchPubKey = _stack.Pop();
                                var vchSig = _stack.Pop();

                                if (!CheckSignatureEncoding(vchSig, flags, ref error) || !CheckPubkeyEncoding(vchPubKey, flags, ref error))
                                {
                                    // error is set
                                    return false;
                                }

                                // Subset of script starting at the most recent code separator.
                                var subScript = script.Slice(pStart, pend);

                                // Remove signature for pre-fork scripts
                                CleanupScriptCode(subScript, vchSig, flags);

                                var fSuccess = VerifySignature(checker, vchSig, vchPubKey, subScript, flags);
                                if (!fSuccess && (flags & ScriptFlags.VERIFY_NULLFAIL) != 0 && vchSig.Length > 0)
                                {
                                    return SetError(out error, ScriptError.SIG_NULLFAIL);
                                }

                                _stack.Push(fSuccess ? VarType.True : VarType.False);
                                if (op.Code == Opcode.OP_CHECKSIGVERIFY)
                                {
                                    if (fSuccess)
                                    {
                                        _stack.Pop();
                                    }
                                    else
                                    {
                                        return SetError(out error, ScriptError.CHECKSIGVERIFY);
                                    }
                                }
                                break;
                            }

                            case Opcode.OP_CHECKMULTISIG:
                            case Opcode.OP_CHECKMULTISIGVERIFY:
                            {
                                // ([sig ...] num_of_signatures [pubkey ...] num_of_pubkeys -- bool)
                                var i = 1;
                                if (_stack.Count < i) 
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                // initialize to max size of CScriptNum::MAXIMUM_ELEMENT_SIZE (4 bytes) 
                                // because only 4 byte integers are supported by  OP_CHECKMULTISIG / OP_CHECKMULTISIGVERIFY
                                var nKeysCount = new ScriptNum(_stack.Peek(-i), fRequireMinimal).ToInt();
                                if (nKeysCount < 0 || nKeysCount > RootService.Network.Consensus.MaxPubkeysPerMultisig) 
                                    return SetError(out error, ScriptError.PUBKEY_COUNT);

                                nOpCount += nKeysCount;
                                if (!IsValidMaxOpsPerScript(nOpCount))
                                    return SetError(out error, ScriptError.OP_COUNT);

                                var iKey = ++i;
                                i += nKeysCount;

                                // iKey2 is the position of last non-signature item in
                                // the stack. Top stack item = 1. With
                                // SCRIPT_VERIFY_NULLFAIL, this is used for cleanup if
                                // operation fails.
                                var iKey2 = nKeysCount + 2;

                                if (_stack.Count < i)
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                var nSigsCount = new ScriptNum(_stack.Peek(-i), fRequireMinimal).ToInt();
                                if (nSigsCount < 0 || nSigsCount > nKeysCount)
                                    return SetError(out error, ScriptError.SIG_COUNT);

                                var iSig = ++i;
                                i += nSigsCount;
                                if (_stack.Count < i)
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                // Subset of script starting at the most recent // codeseparator
                                var subScript = script.Slice(pStart, pend);

                                // Remove signature for pre-fork scripts
                                for (var k = 0; k < nSigsCount; k++)
                                {
                                    var vchSig = _stack.Peek(index: -iSig - k);
                                    CleanupScriptCode(subScript, vchSig, flags);
                                }

                                var fSuccess = true;
                                while (fSuccess && nSigsCount > 0)
                                {
                                    var vchSig = _stack.Peek(-iSig);
                                    var vchPubkey = _stack.Peek(-iKey);

                                    if (!CheckSignatureEncoding(vchSig, flags, ref error) || !CheckPubkeyEncoding(vchPubkey, flags, ref error))
                                    { 
                                        return false;
                                    }

                                    if (VerifySignature(checker, vchSig, vchPubkey, subScript, flags))
                                    {
                                        iSig++;
                                        nSigsCount--;
                                    }

                                    iKey++;
                                    nKeysCount--;

                                    // If there are more signatures left than keys left, then too many signatures have failed.
                                    if (nSigsCount > nKeysCount)
                                    {
                                        fSuccess = false;
                                    }
                                }

                                // Clean up stack of actual arguments
                                while (i-- > 1)
                                {
                                    // If the operation failed, we require that all signatures must be empty vector
                                    if (!fSuccess && (flags & ScriptFlags.VERIFY_NULLFAIL) != 0 && iKey2 <= 0 && !_stack.Peek().IsEmpty)
                                        return SetError(out error, ScriptError.SIG_NULLFAIL);

                                    if (iKey2 > 0)
                                    {
                                        iKey2--;
                                    }
                                    _stack.Pop();
                                }

                                // A bug causes CHECKMULTISIG to consume one extra argument whose contents were not checked in any way.
                                //
                                // Unfortunately this is a potential source of mutability, so optionally verify it is exactly equal to zero prior
                                // to removing it from the stack.
                                if (_stack.Count < 1)
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);
                                if ((flags & ScriptFlags.VERIFY_NULLDUMMY) != 0 && !_stack.Peek().IsEmpty)
                                    return SetError(out error, ScriptError.SIG_NULLDUMMY);
                                _stack.Pop();

                                _stack.Push(fSuccess ? VarType.True : VarType.False);

                                if (op.Code == Opcode.OP_CHECKMULTISIGVERIFY)
                                {
                                    if (fSuccess)
                                    {
                                        _stack.Pop();
                                    }
                                    else
                                    {
                                        return SetError(out error, ScriptError.CHECKMULTISIGVERIFY);
                                    }
                                }

                                break;
                            }

                            //
                            // Byte string operations
                            //
                            case Opcode.OP_CAT:
                            {
                                // (x1 x2 -- out)
                                if (_stack.Count < 2)
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                var x2 = _stack.Pop();
                                var x1 = _stack.Pop();
                                if (x1.Length + x2.Length > RootService.GetNetwork(_networkType).Consensus.MaxScriptElementSize)
                                    return SetError(out error, ScriptError.PUSH_SIZE);

                                _stack.Push(x1.Cat(x2));
                                break;
                            }

                            case Opcode.OP_SPLIT:
                            {
                                // (data position -- x1 x2)
                                if (_stack.Count < 2)
                                    return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                var position = _stack.Pop().ToScriptNum(fRequireMinimal).ToInt();
                                var data = _stack.Pop();

                                // Make sure the split point is appropriate.
                                if (position < 0 || position > data.Length)
                                    return SetError(out error, ScriptError.INVALID_SPLIT_RANGE);

                                var (x1, x2) = data.Split(position);
                                _stack.Push(x1);
                                _stack.Push(x2);
                                break;
                            }

                            //
                            // Conversion operations
                            //
                            case Opcode.OP_NUM2BIN:
                            {
                                // (in size -- out)
                                if (_stack.Count < 2) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                var size = _stack.Pop().ToScriptNum(fRequireMinimal).ToInt();
                                if (size < 0 || size > RootService.GetNetwork(_networkType).Consensus.MaxScriptElementSize)
                                    return SetError(out error, ScriptError.PUSH_SIZE);

                                var num = _stack.Pop();

                                var (bin, ok) = num.Num2Bin((uint)size);

                                if (!ok) return SetError(out error, ScriptError.IMPOSSIBLE_ENCODING);

                                _stack.Push(bin);
                                break;
                            }

                            case Opcode.OP_BIN2NUM:
                            {
                                // (in -- out)
                                if (_stack.Count < 1) return SetError(out error, ScriptError.INVALID_STACK_OPERATION);

                                var bin = _stack.Pop();

                                var (num, ok) = bin.Bin2Num();

                                if (!ok) return SetError(out error, ScriptError.INVALID_NUMBER_RANGE);

                                _stack.Push(num);
                                break;
                            }

                            default:
                                return SetError(out error, ScriptError.BAD_OPCODE);
                        }
                    }

                    if (_stack.Count + altStack.Count > 1000) return SetError(out error, ScriptError.STACK_SIZE);
                }
            }
            catch (ScriptNum.OverflowError)
            {
                return SetError(out error, ScriptError.SCRIPTNUM_OVERFLOW);
            }
            catch (ScriptNum.MinEncodeError)
            {
                return SetError(out error, ScriptError.SCRIPTNUM_MINENCODE);
            }
            catch
            {
                return SetError(out error, ScriptError.UNKNOWN_ERROR);
            }

            return vfExec.Count != 0
                ? SetError(out error, ScriptError.UNBALANCED_CONDITIONAL)
                : SetSuccess(out error);
        }

        private static SignatureHashType GetHashType(VarType vchSig)
            => new(vchSig.Length == 0 ? SignatureHashEnum.Unsupported : (SignatureHashEnum)vchSig.LastByte);


        private static void CleanupScriptCode(Script scriptCode, VarType vchSig, ScriptFlags flags)
        {
            // Drop the signature in scripts when SIGHASH_FORKID is not used.
            var sigHashType = GetHashType(vchSig);
            if ((flags & ScriptFlags.ENABLE_SIGHASH_FORKID) == 0 || !sigHashType.HasForkId)
            {
                scriptCode.FindAndDelete(vchSig);
            }
        }

        private static bool CheckPubkeyEncoding(VarType vchPubKey, ScriptFlags flags, ref ScriptError error)
        {
            if ((flags & ScriptFlags.VERIFY_STRICTENC) != 0 && !IsCompressedOrUncompressedPubKey(vchPubKey))
                return SetError(out error, ScriptError.PUBKEYTYPE);

            // Only compressed keys are accepted when
            // SCRIPT_VERIFY_COMPRESSED_PUBKEYTYPE is enabled.

            if ((flags & ScriptFlags.VERIFY_COMPRESSED_PUBKEYTYPE) != 0 && !IsCompressedPubKey(vchPubKey))
                return SetError(out error, ScriptError.NONCOMPRESSED_PUBKEY);

            return true;
        }

        private static bool IsCompressedOrUncompressedPubKey(byte[] vchPubKey)
        {
            if (vchPubKey.Length < 33)
            {
                //  Non-canonical public key: too short
                return false;
            }

            switch (vchPubKey[0])
            {
                case 0x04:
                {
                    if (vchPubKey.Length != 65)
                    {
                        //  Non-canonical public key: invalid length for uncompressed key
                        return false;
                    }

                    break;
                }
                case 0x02:
                case 0x03:
                {
                    if (vchPubKey.Length != 33)
                    {
                        //  Non-canonical public key: invalid length for compressed key
                        return false;
                    }

                    break;
                }
                default:
                    //  Non-canonical public key: neither compressed nor uncompressed
                    return false;
            }
            return true;
        }

        private static bool IsCompressedPubKey(VarType vchPubKey)
        {
            var length = vchPubKey.Length;
            var first = vchPubKey.FirstByte;

            return length == 33 && first is 0x02 or 0x03;
        }

        private static bool CheckSignatureEncoding(VarType vchSig, ScriptFlags flags, ref ScriptError error)
        {
            // Empty signature. Not strictly DER encoded, but allowed to provide a
            // compact way to provide an invalid signature for use with CHECK(MULTI)SIG
            if (vchSig.Length == 0) return true;

            if ((flags & (ScriptFlags.VERIFY_DERSIG | ScriptFlags.VERIFY_LOW_S | ScriptFlags.VERIFY_STRICTENC)) != 0
                && !Signature.IsTxDerEncoding(vchSig))
            {
                return SetError(out error, ScriptError.SIG_DER);
            }

            if ((flags & ScriptFlags.VERIFY_LOW_S) != 0 && !IsLowDerSignature(vchSig, ref error))
            {
                // error is set
                return false;
            }

            if ((flags & ScriptFlags.VERIFY_STRICTENC) != 0)
            {
                var ht = GetHashType(vchSig);
                if (!ht.IsDefined) return SetError(out error, ScriptError.SIG_HASHTYPE);
                var usesForkId = ht.HasForkId;
                var forkIdEnabled = (flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0;
                if (!forkIdEnabled && usesForkId) return SetError(out error, ScriptError.ILLEGAL_FORKID);
                if (forkIdEnabled && !usesForkId) return SetError(out error, ScriptError.MUST_USE_FORKID);
            }

            return true;
        }

        private static bool VerifySignature(ISignatureChecker checker, VarType vchSig, VarType vchPubKey, Script subScript, ScriptFlags flags)
        {
            var fSuccess = false;
            try
            {
                fSuccess = checker.CheckSignature(vchSig, vchPubKey, subScript, flags);
            }
            catch
            {
                // ignored
            }

            return fSuccess;
        }

        private static bool IsLowDerSignature(VarType vchSig, ref ScriptError error)
        {
            if (!Signature.IsTxDerEncoding(vchSig)) return SetError(out error, ScriptError.SIG_DER);

            var sigInput = vchSig[..^1];

            return Signature.IsLowS(sigInput) || SetError(out error, ScriptError.SIG_HIGH_S);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetSuccess(out ScriptError ret)
        {
            ret = ScriptError.OK;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetError(out ScriptError output, ScriptError input)
        {
            output = input;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidMaxOpsPerScript(int nOpCount) => nOpCount <= RootService.GetNetwork(_networkType).Consensus.MaxOperationsPerScript;

        // ReSharper disable once UnusedParameter.Local
        private static bool IsOpcodeDisabled(Opcode opcode, ScriptFlags flags)
        {
            return opcode switch
            {
                // Disabled opcodes.
                Opcode.OP_2MUL => true,
                // Disabled opcodes.
                Opcode.OP_2DIV => true,
                _ => false
            };
        }

        private static bool CheckMinimalPush(ref Operand op)
        {
            var opcode = op.Code;
            var dataSize = op.Data.Length;
            if (dataSize == 0)
            {
                // Could have used OP_0.
                return opcode == Opcode.OP_0;
            }

            var b0 = op.Data.FirstByte;
            return dataSize switch
            {
                1 when b0 is >= 1 and <= 16 => (int)opcode == (int)Opcode.OP_1 + (b0 - 1),
                1 when b0 == 0x81 => opcode == Opcode.OP_1NEGATE,
                <= 75 => (int)opcode == dataSize,
                <= 255 => opcode == Opcode.OP_PUSHDATA1,
                <= 65535 => opcode == Opcode.OP_PUSHDATA2,
                _ => true
            };
        }
    }
}
