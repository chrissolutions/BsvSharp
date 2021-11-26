using System.Diagnostics.CodeAnalysis;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface ITxId
    {
        UInt256 TxHash { get; }
    }
}
