#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Signatures;
using CafeLib.BsvSharp.Transactions;

namespace CafeLib.BsvSharp.Scripting
{
    public static class ScriptInterpreter
    {
        public static ScriptFlags ParseFlags(string flags)
        {
#if false
            var map = new Dictionary<string, KzScriptFlags>();
            map.Add("NONE", KzScriptFlags.VERIFY_NONE);
            map.Add("P2SH", KzScriptFlags.VERIFY_P2SH);
            map.Add("STRICTENC", KzScriptFlags.VERIFY_STRICTENC);
            map.Add("DERSIG", KzScriptFlags.VERIFY_DERSIG);
            map.Add("LOW_S", KzScriptFlags.VERIFY_LOW_S);
            map.Add("SIGPUSHONLY", KzScriptFlags.VERIFY_SIGPUSHONLY);
            map.Add("MINIMALDATA", KzScriptFlags.VERIFY_MINIMALDATA);
            map.Add("NULLDUMMY", KzScriptFlags.VERIFY_NULLDUMMY);
            map.Add("DISCOURAGE_UPGRADABLE_NOPS", KzScriptFlags.VERIFY_DISCOURAGE_UPGRADABLE_NOPS);
            map.Add("CLEANSTACK", KzScriptFlags.VERIFY_CLEANSTACK);
            map.Add("MINIMALIF", KzScriptFlags.VERIFY_MINIMALIF);
            map.Add("NULLFAIL", KzScriptFlags.VERIFY_NULLFAIL);
            map.Add("CHECKLOCKTIMEVERIFY", KzScriptFlags.VERIFY_CHECKLOCKTIMEVERIFY);
            map.Add("CHECKSEQUENCEVERIFY", KzScriptFlags.VERIFY_CHECKSEQUENCEVERIFY);
            map.Add("COMPRESSED_PUBKEYTYPE", KzScriptFlags.VERIFY_COMPRESSED_PUBKEYTYPE);
            map.Add("SIGHASH_FORKID", KzScriptFlags.ENABLE_SIGHASH_FORKID);
            map.Add("REPLAY_PROTECTION", KzScriptFlags.ENABLE_REPLAY_PROTECTION);
#endif
            var fs = flags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return fs.Select(f => Enum.GetNames(typeof(ScriptFlags))
                .Single(n => n.Contains(f)))
                .Select(Enum.Parse<ScriptFlags>)
                .Aggregate((ScriptFlags)0, (current, sf) => current | sf);
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

        /// <summary>
        /// Modeled on Bitcoin-SV interpreter.cpp 0.1.1 lines 1866-1945
        /// </summary>
        /// <param name="scriptSig"></param>
        /// <param name="scriptPub"></param>
        /// <param name="flags"></param>
        /// <param name="checker"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool VerifyScript(Script scriptSig, Script scriptPub, ScriptFlags flags, ISignatureChecker checker, out ScriptError error)
        {
            SetError(out error, ScriptError.UNKNOWN_ERROR);

            if ((flags & ScriptFlags.ENABLE_SIGHASH_FORKID) != 0)
            {
                flags |= ScriptFlags.VERIFY_STRICTENC;
            }

            if ((flags & ScriptFlags.VERIFY_SIGPUSHONLY) != 0 && !scriptSig.IsPushOnly())
            {
                return SetError(out error, ScriptError.SIG_PUSHONLY);
            }

            var evaluator = new ScriptEvaluator();
            return evaluator switch
            {
                _ when !evaluator.EvalScript(scriptSig, flags, checker, out error) => false,
                _ when !evaluator.EvalScript(scriptPub, flags, checker, out error) => false,
                _ when evaluator.Count == 0 => SetError(out error, ScriptError.EVAL_FALSE),
                _ when !evaluator.Peek() => SetError(out error, ScriptError.EVAL_FALSE),
                _ => SetSuccess(out error)
            };
        }
    }
}
