﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;

namespace CafeLib.BsvSharp.Extensions
{
    public static class ScriptExtensions
    {
        /// <summary>
        /// Convert VarType to ScriptNum
        /// </summary>
        /// <param name="v">var type</param>
        /// <param name="fRequireMinimal">require minimal flag</param>
        /// <returns></returns>
        public static ScriptNum ToScriptNum(this VarType v, bool fRequireMinimal = false)
        {
            return new ScriptNum(v, fRequireMinimal);
        }
    }
}