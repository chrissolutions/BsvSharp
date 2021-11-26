#region Copyright
// Copyright (c) 2020 TonesNotes
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
        void TxStart(Transaction t, long offset);
        void TxParsed(Transaction t, long offset);
        void TxOutStart(TxOut to, long offset);
        void TxOutParsed(TxOut to, long offset);
        void TxInStart(TxIn ti, long offset);
        void TxInParsed(TxIn ti, long offset);
        void ScriptStart(Script s, long offset);
        void ScriptParsed(Script s, long offset);
    }
}
