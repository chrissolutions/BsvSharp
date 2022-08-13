using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Builders
{
    public class DefaultLockBuilder : LockingScriptBuilder
    {
        internal DefaultLockBuilder()
            : this(Script.None)
        {
        }

        public DefaultLockBuilder(Script script)
            : base(script)
        {
        }
    }
}