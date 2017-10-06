using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Medallion.Collections
{
    internal struct LiteStack<T>
    {
        private static readonly T[] EmptyArray = Enumerable.Empty<T>() as T[] ?? new T[0];
        
        public static LiteStack<T> Empty => new LiteStack<T> { _array = EmptyArray };

        private T[] _array;

        public int Count { get; private set; }

        public void Push(T value)
        {
            if (this.Count == this._array.Length)
            {
                this.Grow();
            }

            this._array[this.Count++] = value;
        }

        private void Grow() => Array.Resize(ref this._array, Math.Max(16, 2 * this._array.Length));

        public T Pop()
        {
            var popIndex = --this.Count;
            var result = this._array[popIndex];
            this._array[popIndex] = default(T);
            return result;
        }

        public T Peek() => this._array[this.Count - 1];

        public void Clear()
        {
            var count = this.Count;
            for (var i = 0; i < count; ++i)
            {
                this._array[i] = default(T);
            }
            this.Count = 0;
        }
    }
}
