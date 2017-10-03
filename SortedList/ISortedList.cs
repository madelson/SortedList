using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public interface ISortedList<T> : IReadOnlySortedList<T>, IList<T>
    {
    }
}
