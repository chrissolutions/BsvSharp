#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;

namespace CafeLib.BsvSharp.Exceptions
{
    public class BlockException : Exception
    {
        public BlockException(string message)
            : base(message)
        {
        }
    }
}