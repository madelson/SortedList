using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public class SortedList<T> : ISortedList<T>
    {
        private ScapegoatTree<T> _tree;

        public SortedList(IComparer<T> comparer = null)
        {
            this._tree = new ScapegoatTree<T>(comparer ?? Comparer<T>.Default);
        }

        public SortedList(IEnumerable<T> items, IComparer<T> comparer = null)
            : this(comparer)
        {
            this._tree.AddRangeAllowDuplicates(items ?? throw new ArgumentNullException(nameof(items)));
        }

        public T this[int index] => this._tree.GetNodeAtIndex(index).Key;

        T IList<T>.this[int index] { get => this[index]; set => throw new NotSupportedException(); }

        public IComparer<T> Comparer => this._tree.Comparer;

        public int Count => this._tree.Count;

        bool ICollection<T>.IsReadOnly => false;

        public void Add(T item) => this._tree.AddAllowDuplicates(item);

        public void Clear() => this._tree.Clear();

        public bool Contains(T value) => this._tree.TryGetNode(value, out _);

        public void CopyTo(T[] array, int arrayIndex) => this._tree.CopyTo(array, arrayIndex);

        public IEnumerable<T> EnumerateRange(int startIndex, int? count = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateRange(T start)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateRange(T start = default(T), T end = default(T), SortedListRangeOptions options = SortedListRangeOptions.None)
        {
            throw new NotImplementedException();
        }

        public int FindIndex(T value, int startIndex = 0, int? count = null)
        {
            throw new NotImplementedException();
        }

        public int FindLastIndex(T value, int? startIndex = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T value, int startIndex = 0, int? count = null)
        {
            throw new NotImplementedException();
        }

        int IList<T>.IndexOf(T item) => this.IndexOf(item);

        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();

        public int LastIndexOf(T value, int? startIndex = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item) => this._tree.Remove(item);

        public void RemoveAt(int index) => this._tree.RemoveAt(index);

        public IReadOnlySortedList<T> Reverse()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
