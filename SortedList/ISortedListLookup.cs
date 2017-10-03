using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface ISortedListLookup<TKey, TValue> : ISortedList<KeyValuePair<TKey, TValue>>, IReadOnlySortedListLookup<TKey, TValue>
    {
        void Add(TKey key, TValue value);
        void SetValues(TKey key, IEnumerable<TValue> values);
        bool Remove(TKey key);
    }
}
