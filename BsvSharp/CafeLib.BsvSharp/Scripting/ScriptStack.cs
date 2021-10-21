#region Copyright
// Copyright (c) 2020 TonesNotes
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using System;
using System.Runtime.CompilerServices;

namespace CafeLib.BsvSharp.Scripting
{
    internal class ScriptStack<T>
    {
        private T[] _array;

        private const int DefaultCapacity = 4;

        public ScriptStack()
        {
            _array = new T[DefaultCapacity];
        }

        public ScriptStack(int capacity)
        {
            _array = new T[capacity];
        }

        public int Count { get; private set; }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_array, 0, Count);
            }

            Count = 0;
        }

        public void TrimExcess()
        {
            var threshold = (int)(_array.Length * 0.9);
            if (Count < threshold)
            {
                Array.Resize(ref _array, Count);
            }
        }

        public T Peek()
        {
            return _array[Count - 1];
        }

        public bool TryPeek(out T result)
        {
            if (Count == 0) {
                result = default;
                return false;
            }
            result = _array[Count - 1];
            return true;
        }

        public T Pop()
        {
            var item = _array[--Count];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                _array[Count] = default;     // Free memory quicker.
            }
            return item;
        }

        public bool TryPop(out T result)
        {
            if (Count == 0) {
                result = default;
                return false;
            }

            result = _array[--Count];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                _array[Count] = default;     // Free memory quicker.
            }
            return true;
        }

        public void Push(T item)
        {
            if (Count == _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[Count++] = item;
        }

        public T[] ToArray()
        {
            return _array.AsSpan(0, Count).ToArray();
        }

        public void Drop2()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) {
                _array[--Count] = default;
                _array[--Count] = default;
            } else {
                Count -= 2;
            }
        }

        public void Dup2()
        {
            // (x1 x2 -- x1 x2 x1 x2)
            if (Count + 2 > _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[Count++] = _array[Count - 3];
            _array[Count++] = _array[Count - 3];
        }

        public bool Contains(T v)
        {
            return Count != 0 && Array.LastIndexOf(_array, v, Count - 1) != -1;
        }

        public void Dup3()
        {
            // (x1 x2 x3 -- x1 x2 x3 x1 x2 x3)
            if (Count + 3 > _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[Count++] = _array[Count - 4];
            _array[Count++] = _array[Count - 4];
            _array[Count++] = _array[Count - 4];
        }

        public void Over()
        {
            // (x1 x2 -- x1 x2 x1)
            if (Count + 1 > _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[Count++] = _array[Count - 3];
        }

        public void Over2()
        {
            // (x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2)
            if (Count + 2 > _array.Length)
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            _array[Count++] = _array[Count - 5];
            _array[Count++] = _array[Count - 5];
        }

        public void Rot()
        {
            // (x1 x2 x3 -- x2 x3 x1)
            var x1 = _array[Count - 3];
            _array[Count - 3] = _array[Count - 2];
            _array[Count - 2] = _array[Count - 1];
            _array[Count - 1] = x1;
        }

        public void Rot2()
        {
            // (x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2)
            var x1 = _array[Count - 6];
            var x2 = _array[Count - 5];
            _array[Count - 6] = _array[Count - 4];
            _array[Count - 5] = _array[Count - 3];
            _array[Count - 4] = _array[Count - 2];
            _array[Count - 3] = _array[Count - 1];
            _array[Count - 2] = x1;
            _array[Count - 1] = x2;
        }

        public void Swap()
        {
            // (x1 x2 -- x2 x1)
            var x1 = _array[Count - 2];
            _array[Count - 2] = _array[Count - 1];
            _array[Count - 1] = x1;
        }

        public void Swap2()
        {
            // (x1 x2 x3 x4 -- x3 x4 x1 x2)
            var x1 = _array[Count - 4];
            var x2 = _array[Count - 3];
            _array[Count - 4] = _array[Count - 2];
            _array[Count - 3] = _array[Count - 1];
            _array[Count - 2] = x1;
            _array[Count - 1] = x2;
        }

        public void Nip()
        {
            // (x1 x2 -- x2)
            _array[Count - 2] = _array[Count - 1];
            Count--;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _array[Count] = default;
        }

        public void Tuck()
        {
            // (x1 x2 -- x2 x1 x2)
            var x2 = _array[Count - 1];
            _array[Count - 1] = _array[Count - 2];
            _array[Count - 2] = x2;
            Push(x2);
        }

        public void Roll(int n)
        {
            // (xn ... x2 x1 x0 - xn-1 ... x2 x1 x0 xn)
            var xni = Count - 1 - n;
            var xn = _array[xni];
            Array.Copy(_array, xni + 1, _array, xni, n);
            _array[Count - 1] = xn;
        }

        public void Pick(int n)
        {
            // (xn ... x2 x1 x0 - xn ... x2 x1 x0 xn)
            Push(_array[Count - 1 - n]);
        }
    }
}
