using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections.Tests
{
    public abstract class ReadOnlySortedListTest
    {
        [Test]
        public void TestEmpty()
        {
            var empty = this.Create<string>(Enumerable.Empty<string>());
            empty.Count.ShouldEqual(0);
            CollectionAssert.AreEquivalent(Enumerable.Empty<string>(), empty);
            empty.Contains(null).ShouldEqual(false);
        }

        protected abstract IReadOnlySortedList<T> Create<T>(IEnumerable<T> values, IComparer<T> comparer = null);
    }
}
