#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CafeLib.BsvSharp.Scripting
{
    internal class ScriptStack<T> : IEnumerable<T>
    {
        private Stack<T> _stack;

        public ScriptStack()
        {
            _stack = new Stack<T>();
        }

        public int Count { get; private set; }

        public void Clear() => _stack.Clear();

        public T Peek() => _stack.Peek();

        public T Pop() => _stack.Pop();

        public void Push(T item) => _stack.Push(item);

        public bool Contains(T v) => _stack.Contains(v);

        public T[] ToArray() => _stack.ToArray();

        public void Drop2()
        {
            // (x1 x2 -- )
            Pop();
            Pop();
        }

        public void Dup2()
        {
            // (x1 x2 -- x1 x2 x1 x2)
            (T, T) temp = new()
            {
                Item1 = _stack.ElementAt(_stack.Count - 2),
                Item2 = _stack.ElementAt(_stack.Count - 1)
            };

            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Dup3()
        {
            // (x1 x2 x3 -- x1 x2 x3 x1 x2 x3)
            (T, T, T) temp = new()
            {
                Item1 = _stack.ElementAt(_stack.Count - 3),
                Item2 = _stack.ElementAt(_stack.Count - 2),
                Item3 = _stack.ElementAt(_stack.Count - 1),
            };

            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
            _stack.Push(temp.Item3);
        }

        public void Over()
        {
            // (x1 x2 -- x1 x2 x1)
            var item = _stack.ElementAt(_stack.Count - 2);
            _stack.Push(item);
        }

        public void Over2()
        {
            // (x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2)
            (T, T) temp = new()
            {
                Item1 = _stack.ElementAt(_stack.Count - 4),
                Item2 = _stack.ElementAt(_stack.Count - 3)
            };

            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Rot()
        {
            // (x1 x2 x3 -- x2 x3 x1)
            //  x2 x1 x3  after first swap
            //  x2 x3 x1  after second swap
            (T, T, T) temp = new()
            {
                Item3 = _stack.Pop(),
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item2);
            _stack.Push(temp.Item3);
            _stack.Push(temp.Item1);
        }

        public void Rot2()
        {
            // (x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2)
            (T, T, T, T, T, T) temp = new()
            {
                Item6 = _stack.Pop(),
                Item5 = _stack.Pop(),
                Item4 = _stack.Pop(),
                Item3 = _stack.Pop(),
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item3);
            _stack.Push(temp.Item4);
            _stack.Push(temp.Item5);
            _stack.Push(temp.Item6);
            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Swap()
        {
            // (x1 x2 -- x2 x1)
            (T, T) temp = new()
            {
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Swap2()
        {
            // (x1 x2 x3 x4 -- x3 x4 x1 x2)
            (T, T, T, T) temp = new()
            {
                Item4 = _stack.Pop(),
                Item3 = _stack.Pop(),
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item3);
            _stack.Push(temp.Item4);
            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Nip()
        {
            // (x1 x2 -- x2)
            (T, T) temp = new()
            {
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item2);
        }

        public void Tuck()
        {
            // (x1 x2 -- x2 x1 x2)
            (T, T) temp = new()
            {
                Item2 = _stack.Pop(),
                Item1 = _stack.Pop(),
            };

            _stack.Push(temp.Item2);
            _stack.Push(temp.Item1);
            _stack.Push(temp.Item2);
        }

        public void Roll(int n)
        {
            // (xn ... x2 x1 x0 - xn-1 ... x2 x1 x0 xn)
            var array = _stack.ToArray();
            var xni = Count - 1 - n;
            var xn = array[xni];    
            Array.Copy(array, xni + 1, array, xni, n);
            array[Count - 1] = xn;
            _stack = new Stack<T>(array);
        }

        public void Pick(int n)
        {
            // (xn ... x2 x1 x0 - xn ... x2 x1 x0 xn)
            Push(_array[Count - 1 - n]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
