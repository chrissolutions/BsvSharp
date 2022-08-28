#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;

namespace CafeLib.BsvSharp.Chain 
{
    public interface IBlockParser
    {
        void BlockStart(BlockHeader bh, long offset);
        void BlockParsed(BlockHeader bh, long offset);
        void TxStart(Transaction tx, long offset);
        void TxParsed(Transaction tx, long offset);
        void TxOutStart(TransactionOutput txOut, long offset);
        void TxOutParsed(TransactionOutput txOut, long offset);
        void TxInStart(TransactionInput txIn, long offset);
        void TxInParsed(TransactionInput txIn, long offset);
        void ScriptStart(Script script, long offset);
        void ScriptParsed(Script script, long offset);
    }
}
