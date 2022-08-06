using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace CafeLib.BsvSharp.UnitTests.Extensions
{
    public static class AssertExtensions
    {
        public static void Any<T>(this CustomAssert _, IEnumerable<T> collection, Action<T> action)
        {
            Stack<Tuple<int, object, Exception>> stack = new();
            T[] array = collection.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                try
                {
                    action(array[i]);
                }
                catch (Exception item)
                {
                    stack.Push(new Tuple<int, object, Exception>(i, array[i], item));
                }
            }

            if (stack.Count == array.Length)
            {
                throw new AllException(array.Length, stack.ToArray());
            }
        }
    }
}