#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CafeLib.BsvSharp.Signatures;

namespace CafeLib.BsvSharp.Scripting
{
    public static class ScriptInterpreter
    {
        public static ScriptFlags ParseFlags(string flags)
        {
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
        /// <param name="scriptSig">script signature</param>
        /// <param name="scriptPubKey">script public key</param>
        /// <param name="flags">script flags</param>
        /// <param name="checker">signature checker</param>
        /// <param name="error">error output parameter</param>
        /// <returns>true if verified; false if not verified</returns>
        /// <remarks>P2SH scripts are not verified</remarks>
        public static bool VerifyScript(Script scriptSig, Script scriptPubKey, ScriptFlags flags, ISignatureChecker checker, out ScriptError error)
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
                _ when !evaluator.EvalScript(scriptPubKey, flags, checker, out error) => false,
                _ when evaluator.Count == 0 => SetError(out error, ScriptError.EVAL_FALSE),
                _ when !evaluator.Peek() => SetError(out error, ScriptError.EVAL_FALSE),
                _ => SetSuccess(out error)
            };
        }
    }
}
