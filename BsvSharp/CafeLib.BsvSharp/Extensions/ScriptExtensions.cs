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
        /// <param name="fRequireMinimal"></param>
        /// <returns></returns>
        public static ScriptNum ToScriptNum(this VarType v, bool fRequireMinimal = false)
        {
            return new ScriptNum(v.Span, fRequireMinimal);
        }
    }
}