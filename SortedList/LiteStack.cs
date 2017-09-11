﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Medallion.Collections
{
    internal struct LiteStack<T> : IDisposable
    {
        private const int InitialCapacity = 32, MaxCacheableCapacity = 4 * InitialCapacity;

        private static T[] CachedArray;

        private T[] _array;

        public static LiteStack<T> Create()
        {
            return new LiteStack<T> { _array = Interlocked.Exchange(ref CachedArray, null) ?? new T[InitialCapacity] };
        }

        public int Count { get; private set; }

        public void Push(T value)
        {
            if (this.Count == this._array.Length)
            {
                Array.Resize(ref this._array, 2 * this._array.Length);
            }

            this._array[this.Count++] = value;
        }

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

        public void Dispose()
        {
            this.Clear();

            if (this._array.Length <= MaxCacheableCapacity)
            {
                Volatile.Write(ref CachedArray, this._array);
            }
        }
    }
}
