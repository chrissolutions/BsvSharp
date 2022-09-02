#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using CafeLib.BsvSharp.Builders;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Extensions;

namespace CafeLib.BsvSharp.UnitTests.Scripts
{
    public partial class KzScriptTests
    {
        /// <summary>
        /// Test Vector
        /// </summary>
        class TV2
        {
            /// <summary>
            /// ScriptSig as hex string.
            /// </summary>
            public string sig;
            /// <summary>
            /// ScriptPub .
            /// </summary>
            public string pub;
            /// <summary>
            /// Flags
            /// </summary>
            public string flags;
            /// <summary>
            /// Result: Error or OK.
            /// </summary>
            public string error;

            public Script scriptSig;
            public Script scriptPubKey;
            public ScriptFlags scriptFlags;
            public ScriptError scriptError;
            public Opcode[] opcodes;
            public Opcode? keyopcode;

            public TV2(params string[] args)
            {
                sig = args[0];
                pub = args[1];
                flags = args[2];
                error = args[3];

                scriptSig = ScriptBuilder.ParseTestScript(sig).ToScript();
                scriptPubKey = ScriptBuilder.ParseTestScript(pub).ToScript();
                scriptFlags = ScriptInterpreter.ParseFlags(flags);
                scriptError = ToScriptError(error);

                opcodes = scriptSig.Decode().Select(o => o.Code)
                    .Concat(scriptPubKey.Decode().Select(o => o.Code))
                    .Distinct()
                    .OrderBy(o => o).ToArray();

                keyopcode = opcodes.Length == 0 ? (Opcode?)null : opcodes.Last();
            }

            private static ScriptError ToScriptError(string error)
            {
                if (!Enum.TryParse(error, out ScriptError result))
                {
                    result = error switch
                    {
                        "SPLIT_RANGE" => ScriptError.INVALID_SPLIT_RANGE,
                        "OPERAND_SIZE" => ScriptError.INVALID_OPERAND_SIZE,
                        "NULLFAIL" => ScriptError.SIG_NULLFAIL,
                        "MISSING_FORKID" => ScriptError.MUST_USE_FORKID,
                        _ => ScriptError.UNKNOWN_ERROR
                    };
                }
                return result;
            }
        }
    }
}
