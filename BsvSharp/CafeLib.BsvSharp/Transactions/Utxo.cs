using CafeLib.BsvSharp.Numerics;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public class Utxo
    {
        public UInt256 TxHash { get; set; }
        public int Index { get; set; }
        public Amount Amount { get; set; }
        public Script ScriptPubKey { get; set; }

        public static implicit operator TxOut(Utxo rhs) => new TxOut(rhs.TxHash, rhs.Index, rhs.Amount, rhs.ScriptPubKey);
        public static implicit operator Utxo(TxOut rhs) => new Utxo {TxHash = rhs.TxHash, Index = (int)rhs.Index, Amount = rhs.Amount, ScriptPubKey = rhs.Script};
    }
}
