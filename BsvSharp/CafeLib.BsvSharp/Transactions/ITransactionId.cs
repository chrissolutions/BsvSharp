#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public interface ITransactionId
    {
        UInt256 TxHash { get; }
    }
}
