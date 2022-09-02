#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Transactions
{
    public class Utxo
    {
        public UInt256 TxId { get; set; }
        public int Index { get; set; }
        public Amount Amount { get; set; }
        public Script ScriptPubKey { get; set; }

        public static implicit operator TransactionOutput(Utxo rhs) => new(rhs.TxId, rhs.Index, rhs.Amount, rhs.ScriptPubKey);
        public static implicit operator Utxo(TransactionOutput rhs) => new() {TxId = rhs.TxHash, Index = rhs.Index, Amount = rhs.Amount, ScriptPubKey = rhs.Script};
    }
}
