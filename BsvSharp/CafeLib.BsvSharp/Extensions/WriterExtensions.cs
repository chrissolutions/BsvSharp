using System.Diagnostics.CodeAnalysis;
using CafeLib.BsvSharp.Persistence;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Transactions;

namespace CafeLib.BsvSharp.Extensions
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class WriterExtensions
    {
        public static IDataWriter Write(this IDataWriter w, Script script, bool withoutCodeSeparators = false)
            => script.WriteTo(w);

        public static IDataWriter Write(this IDataWriter w, OutPoint op) => op.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, TransactionInput txIn) => txIn.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, Transaction tx) => tx.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, Operand op) => op.WriteTo(w);
        public static IDataWriter Write(this IDataWriter w, TransactionOutput txOut) => txOut.WriteTo(w);
    }
}
