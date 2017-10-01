using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class ScapegoatHelperTest
    {
        [Test]
        public void TestLogTable()
        {
            var logTable = ScapegoatHelper.LogTable;
            var inverseAlpha = 1 / ScapegoatHelper.Alpha;
            var isValid = logTable.Length - 1 == (int)Math.Log(int.MaxValue, inverseAlpha) // long enough
                // first to elements correct
                && logTable[0] == -1
                && logTable[1] == 0
                // all values are the floor values
                && Enumerable.Range(2, count: logTable.Length - 2)
                    .All(i => (int)Math.Log(logTable[i], inverseAlpha) == i && (int)Math.Log(logTable[i] - 1, inverseAlpha) < i);
            if (isValid) { Assert.Pass(); }

            long InverseFloorLog(int logResult)
            {
                var min = 1L;
                var max = (long)int.MaxValue + 1;
                while (max > min + 1)
                {
                    var mid = min + ((max - min) / 2);
                    var log = (int)Math.Log(mid, inverseAlpha);
                    if (log >= logResult) { max = mid; }
                    if (log < logResult) { min = mid; }
                }

                return (int)Math.Log(min, inverseAlpha) == logResult ? min : max;
            }

            var expectedLogTable = new List<int> { -1, 0 };
            while (true)
            {
                var next = InverseFloorLog(expectedLogTable.Count);
                if (next > int.MaxValue) { break; }
                expectedLogTable.Add(checked((int)next));
            }

            Console.WriteLine($@"new[] {{ {string.Join(", ", expectedLogTable)} }};");
            Assert.Fail("Expected log table not correct: see output for correct table");
        }
    }
}
