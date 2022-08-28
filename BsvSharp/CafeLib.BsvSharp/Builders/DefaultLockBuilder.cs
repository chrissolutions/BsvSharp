#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

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