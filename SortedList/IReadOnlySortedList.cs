using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface IReadOnlySortedList<T> : IReadOnlyList<T>
    {
        IComparer<T> Comparer { get; }

        bool Contains(T value);

        int IndexOf(T value, int startIndex = 0, int? count = null);
        int LastIndexOf(T value, int? startIndex = null, int? count = null);
        int FindIndex(T value, int startIndex = 0, int? count = null);
        int FindLastIndex(T value, int? startIndex = null, int? count = null);

        IReadOnlySortedList<T> Reverse();

        IEnumerable<T> EnumerateRange(int startIndex, int? count = null);
        IEnumerable<T> EnumerateRange(T start);
        IEnumerable<T> EnumerateRange(T start = default(T), T end = default(T), SortedListRangeOptions options = SortedListRangeOptions.None);
    }

    [Flags]
    public enum SortedListRangeOptions
    {
        None = 0,
        Reverse = 1 << 0,
        StartInclusive = 1 << 1,
        EndInclusive = 1 << 2,
        StartUnbounded = 1 << 3,
        EndUnbounded = 1 << 4,
    }
}
