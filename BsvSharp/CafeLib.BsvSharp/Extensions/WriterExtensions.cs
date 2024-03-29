﻿#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;

namespace CafeLib.BsvSharp.Extensions
{
    public static class WriterExtensions
    {
        public static IDataWriter Write(this IDataWriter w, Script script)
            => script.WriteTo(w);

        public static IDataWriter Write(this IDataWriter w, OutPoint op) => op.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, TransactionInput txIn) => txIn.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, Transaction tx) => tx.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, Operand op) => op.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, TransactionOutput txOut) => txOut.WriteTo(w);
    }
}
