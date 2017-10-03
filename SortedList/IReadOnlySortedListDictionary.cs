using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface IReadOnlySortedListDictionary<TKey, TValue> : IReadOnlySortedList<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
    {
    }
}
