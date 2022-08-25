using System.Diagnostics.CodeAnalysis;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface ITransactionId
    {
        UInt256 TxHash { get; }
    }
}
