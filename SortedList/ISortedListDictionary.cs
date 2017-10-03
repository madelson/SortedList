using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface ISortedListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISortedList<KeyValuePair<TKey, TValue>>, IReadOnlySortedListDictionary<TKey, TValue>
    {
    }
}
