using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    public interface IReadOnlySortedListLookup<TKey, TValue> : ILookup<TKey, TValue>, IReadOnlySortedList<KeyValuePair<TKey, TValue>>
    {
    }
}
