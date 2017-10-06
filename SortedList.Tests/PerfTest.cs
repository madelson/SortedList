using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class PerfTest
    {
        [Test]
        public void Test()
        {
            var result1 = BenchmarkCollection(() => new SortedSet<int>());
            var result2 = BenchmarkCollection(() => new SortedList<int>());
            Console.WriteLine($"ss={result1}, sl={result2}");
        }

        private TimeSpan BenchmarkCollection(Func<ICollection<int>> generator)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            var counter = 0L;
            for (var i = 0; i < 10; ++i)
            {
                var collection = generator();
                for (var j = 0; j < 100000; ++j) { collection.Add(i); }
                counter += collection.Count;
                if (counter == long.MaxValue) { Console.WriteLine("make sure the values are used"); }
            }
            return stopwatch.Elapsed;
        }
    }
}
