using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface IReadOnlySortedList<T> : IReadOnlyList<T>
    {
        IComparer<T> Comparer { get; }

        bool Contains(T value);
        int IndexOf(T value, int? startIndex = null, int? count = 0);
        int LastIndexOf(T value, int? startIndex = null, int? count = 0);
        int FindIndex(T value, int? startIndex = null, int? count = 0);
        int FindLastIndex(T value, int? startIndex = null, int? count = 0);
    }
}
