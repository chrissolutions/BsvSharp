#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System.Collections.Generic;
using CafeLib.BsvSharp.Scripting;

namespace CafeLib.BsvSharp.Extensions
{
    public static class OperandExtensions
    {
        private static readonly IDictionary<Opcode, string> OpcodeMap = new Dictionary<Opcode, string>
        {
            {Opcode.OP_0, "OP_0"},
            {Opcode.OP_PUSH1, "1"},
            {Opcode.OP_PUSH2, "2"},
            {Opcode.OP_PUSH3, "3"},
            {Opcode.OP_PUSH4, "4"},
            {Opcode.OP_PUSH5, "5"},
            {Opcode.OP_PUSH6, "6"},
            {Opcode.OP_PUSH7, "7"},
            {Opcode.OP_PUSH8, "8"},
            {Opcode.OP_PUSH9, "9"},
            {Opcode.OP_PUSH10, "10"},
            {Opcode.OP_PUSH11, "11"},
            {Opcode.OP_PUSH12, "12"},
            {Opcode.OP_PUSH13, "13"},
            {Opcode.OP_PUSH14, "14"},
            {Opcode.OP_PUSH15, "15"},
            {Opcode.OP_PUSH16, "16"},
            {Opcode.OP_PUSH17, "17"},
            {Opcode.OP_PUSH18, "18"},
            {Opcode.OP_PUSH19, "19"},
            {Opcode.OP_PUSH20, "20"},
            {Opcode.OP_PUSH21, "21"},
            {Opcode.OP_PUSH22, "22"},
            {Opcode.OP_PUSH23, "23"},
            {Opcode.OP_PUSH24, "24"},
            {Opcode.OP_PUSH25, "25"},
            {Opcode.OP_PUSH26, "26"},
            {Opcode.OP_PUSH27, "27"},
            {Opcode.OP_PUSH28, "28"},
            {Opcode.OP_PUSH29, "29"},
            {Opcode.OP_PUSH30, "30"},
            {Opcode.OP_PUSH31, "31"},
            {Opcode.OP_PUSH32, "32"},
            {Opcode.OP_PUSH33, "33"},
            {Opcode.OP_PUSH34, "34"},
            {Opcode.OP_PUSH35, "35"},
            {Opcode.OP_PUSH36, "36"},
            {Opcode.OP_PUSH37, "37"},
            {Opcode.OP_PUSH38, "38"},
            {Opcode.OP_PUSH39, "39"},
            {Opcode.OP_PUSH40, "40"},
            {Opcode.OP_PUSH41, "41"},
            {Opcode.OP_PUSH42, "42"},
            {Opcode.OP_PUSH43, "43"},
            {Opcode.OP_PUSH44, "44"},
            {Opcode.OP_PUSH45, "45"},
            {Opcode.OP_PUSH46, "46"},
            {Opcode.OP_PUSH47, "47"},
            {Opcode.OP_PUSH48, "48"},
            {Opcode.OP_PUSH49, "49"},
            {Opcode.OP_PUSH50, "50"},
            {Opcode.OP_PUSH51, "51"},
            {Opcode.OP_PUSH52, "52"},
            {Opcode.OP_PUSH53, "53"},
            {Opcode.OP_PUSH54, "54"},
            {Opcode.OP_PUSH55, "55"},
            {Opcode.OP_PUSH56, "56"},
            {Opcode.OP_PUSH57, "57"},
            {Opcode.OP_PUSH58, "58"},
            {Opcode.OP_PUSH59, "59"},
            {Opcode.OP_PUSH60, "60"},
            {Opcode.OP_PUSH61, "61"},
            {Opcode.OP_PUSH62, "62"},
            {Opcode.OP_PUSH63, "63"},
            {Opcode.OP_PUSH64, "64"},
            {Opcode.OP_PUSH65, "65"},
            {Opcode.OP_PUSH66, "66"},
            {Opcode.OP_PUSH67, "67"},
            {Opcode.OP_PUSH68, "68"},
            {Opcode.OP_PUSH69, "69"},
            {Opcode.OP_PUSH70, "70"},
            {Opcode.OP_PUSH71, "71"},
            {Opcode.OP_PUSH72, "72"},
            {Opcode.OP_PUSH73, "73"},
            {Opcode.OP_PUSH74, "74"},
            {Opcode.OP_PUSH75, "75"},
            {Opcode.OP_PUSHDATA1, "OP_PUSHDATA1"},
            {Opcode.OP_PUSHDATA2, "OP_PUSHDATA2"},
            {Opcode.OP_PUSHDATA4, "OP_PUSHDATA4"},
            {Opcode.OP_1NEGATE, "-1"},
            {Opcode.OP_RESERVED, "OP_RESERVED"},
            {Opcode.OP_1, "1"},
            {Opcode.OP_2, "2"},
            {Opcode.OP_3, "3"},
            {Opcode.OP_4, "4"},
            {Opcode.OP_5, "5"},
            {Opcode.OP_6, "6"},
            {Opcode.OP_7, "7"},
            {Opcode.OP_8, "8"},
            {Opcode.OP_9, "9"},
            {Opcode.OP_10, "10"},
            {Opcode.OP_11, "11"},
            {Opcode.OP_12, "12"},
            {Opcode.OP_13, "13"},
            {Opcode.OP_14, "14"},
            {Opcode.OP_15, "15"},
            {Opcode.OP_16, "16"},

            // control
            {Opcode.OP_NOP, "OP_NOP"},
            {Opcode.OP_VER, "OP_VER"},
            {Opcode.OP_IF, "OP_IF"},
            {Opcode.OP_NOTIF, "OP_NOTIF"},
            {Opcode.OP_VERIF, "OP_VERIF"},
            {Opcode.OP_VERNOTIF, "OP_VERNOTIF"},
            {Opcode.OP_ELSE, "OP_ELSE"},
            {Opcode.OP_ENDIF, "OP_ENDIF"},
            {Opcode.OP_VERIFY, "OP_VERIFY"},
            {Opcode.OP_RETURN, "OP_RETURN"},

            // stack ops
            {Opcode.OP_TOALTSTACK, "OP_TOALTSTACK"},
            {Opcode.OP_FROMALTSTACK, "OP_FROMALTSTACK"},
            {Opcode.OP_2DROP, "OP_2DROP"},
            {Opcode.OP_2DUP, "OP_2DUP"},
            {Opcode.OP_3DUP, "OP_3DUP"},
            {Opcode.OP_2OVER, "OP_2OVER"},
            {Opcode.OP_2ROT, "OP_2ROT"},
            {Opcode.OP_2SWAP, "OP_2SWAP"},
            {Opcode.OP_IFDUP, "OP_IFDUP"},
            {Opcode.OP_DEPTH, "OP_DEPTH"},
            {Opcode.OP_DROP, "OP_DROP"},
            {Opcode.OP_DUP, "OP_DUP"},
            {Opcode.OP_NIP, "OP_NIP"},
            {Opcode.OP_OVER, "OP_OVER"},
            {Opcode.OP_PICK, "OP_PICK"},
            {Opcode.OP_ROLL, "OP_ROLL"},
            {Opcode.OP_ROT, "OP_ROT"},
            {Opcode.OP_SWAP, "OP_SWAP"},
            {Opcode.OP_TUCK, "OP_TUCK"},

            // splice ops
            {Opcode.OP_CAT, "OP_CAT"},
            {Opcode.OP_SPLIT, "OP_SPLIT"},
            {Opcode.OP_NUM2BIN, "OP_NUM2BIN"},
            {Opcode.OP_BIN2NUM, "OP_BIN2NUM"},
            {Opcode.OP_SIZE, "OP_SIZE"},

            // bit logic
            {Opcode.OP_INVERT, "OP_INVERT"},
            {Opcode.OP_AND, "OP_AND"},
            {Opcode.OP_OR, "OP_OR"},
            {Opcode.OP_XOR, "OP_XOR"},
            {Opcode.OP_EQUAL, "OP_EQUAL"},
            {Opcode.OP_EQUALVERIFY, "OP_EQUALVERIFY"},
            {Opcode.OP_RESERVED1, "OP_RESERVED1"},
            {Opcode.OP_RESERVED2, "OP_RESERVED2"},

            // numeric
            {Opcode.OP_1ADD, "OP_1ADD"},
            {Opcode.OP_1SUB, "OP_1SUB"},
            {Opcode.OP_2MUL, "OP_2MUL"},
            {Opcode.OP_2DIV, "OP_2DIV"},
            {Opcode.OP_NEGATE, "OP_NEGATE"},
            {Opcode.OP_ABS, "OP_ABS"},
            {Opcode.OP_NOT, "OP_NOT"},
            {Opcode.OP_0NOTEQUAL, "OP_0NOTEQUAL"},
            {Opcode.OP_ADD, "OP_ADD"},
            {Opcode.OP_SUB, "OP_SUB"},
            {Opcode.OP_MUL, "OP_MUL"},
            {Opcode.OP_DIV, "OP_DIV"},
            {Opcode.OP_MOD, "OP_MOD"},
            {Opcode.OP_LSHIFT, "OP_LSHIFT"},
            {Opcode.OP_RSHIFT, "OP_RSHIFT"},
            {Opcode.OP_BOOLAND, "OP_BOOLAND"},
            {Opcode.OP_BOOLOR, "OP_BOOLOR"},
            {Opcode.OP_NUMEQUAL, "OP_NUMEQUAL"},
            {Opcode.OP_NUMEQUALVERIFY, "OP_NUMEQUALVERIFY"},
            {Opcode.OP_NUMNOTEQUAL, "OP_NUMNOTEQUAL"},
            {Opcode.OP_LESSTHAN, "OP_LESSTHAN"},
            {Opcode.OP_GREATERTHAN, "OP_GREATERTHAN"},
            {Opcode.OP_LESSTHANOREQUAL, "OP_LESSTHANOREQUAL"},
            {Opcode.OP_GREATERTHANOREQUAL, "OP_GREATERTHANOREQUAL"},
            {Opcode.OP_MIN, "OP_MIN"},
            {Opcode.OP_MAX, "OP_MAX"},
            {Opcode.OP_WITHIN, "OP_WITHIN"},

            // crypto
            {Opcode.OP_RIPEMD160, "OP_RIPEMD160"},
            {Opcode.OP_SHA1, "OP_SHA1"},
            {Opcode.OP_SHA256, "OP_SHA256"},
            {Opcode.OP_HASH160, "OP_HASH160"},
            {Opcode.OP_HASH256, "OP_HASH256"},
            {Opcode.OP_CODESEPARATOR, "OP_CODESEPARATOR"},
            {Opcode.OP_CHECKSIG, "OP_CHECKSIG"},
            {Opcode.OP_CHECKSIGVERIFY, "OP_CHECKSIGVERIFY"},
            {Opcode.OP_CHECKMULTISIG, "OP_CHECKMULTISIG"},
            {Opcode.OP_CHECKMULTISIGVERIFY, "OP_CHECKMULTISIGVERIFY"},

            // expansion
            {Opcode.OP_NOP1, "OP_NOP1"},
            {Opcode.OP_CHECKLOCKTIMEVERIFY, "OP_CHECKLOCKTIMEVERIFY"},
            {Opcode.OP_CHECKSEQUENCEVERIFY, "OP_CHECKSEQUENCEVERIFY"},
            {Opcode.OP_NOP4, "OP_NOP4"},
            {Opcode.OP_NOP5, "OP_NOP5"},
            {Opcode.OP_NOP6, "OP_NOP6"},
            {Opcode.OP_NOP7, "OP_NOP7"},
            {Opcode.OP_NOP8, "OP_NOP8"},
            {Opcode.OP_NOP9, "OP_NOP9"},
            {Opcode.OP_NOP10, "OP_NOP10"},

            {Opcode.OP_INVALIDOPCODE, "OP_INVALIDOPCODE"},

            // Note:
            //  The template matching params OP_SMALLINTEGER/etc are defined in
            //  opcodetype enum as kind of implementation hack, they are *NOT*
            //  real opcodes. If found in real Script, just let the default:
            //  case deal with them.
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public static string GetOpcodeName(this Opcode opcode)
        {
            return OpcodeMap.TryGetValue(opcode, out var name)
                ? name
                : "OP_UNKNOWN";
        }
    }
}
