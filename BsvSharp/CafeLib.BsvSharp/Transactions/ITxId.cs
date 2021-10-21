using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public interface ITxId
    {
        UInt256 TxHash { get; }
    }
}
