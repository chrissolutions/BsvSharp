using System;
using System.Collections.Generic;

namespace CafeLib.BsvSharp.UnitTests.Extensions
{
    public static class LinqExtensions
    {
        public static (List<T> t, List<T> f) Partition<T>(this IEnumerable<T> s, Func<T, bool> predicate)
        {
            var f = new List<T>();
            var t = new List<T>();
            foreach (var i in s) if (predicate(i)) t.Add(i); else f.Add(i);
            return (t, f);
        }
    }
}