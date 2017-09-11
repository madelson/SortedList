using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class RandomizationHelperTest
    {
        private const int TableLength = RandomizationHelper.MaxTableIndex + 1;

        [Test]
        public void TestChooseNSmallEvenlyDivisibleValues()
        {
            var evenlyDivisibleValues = Enumerable.Range(0, TableLength + 1)
                .Where(i => TableLength % (i + 1) == 0)
                .ToArray();
            foreach (var n in evenlyDivisibleValues)
            {
                const int Trials = 100000;
                var successes = 0;
                var randomState = 0U;
                for (var i = 0; i < Trials; ++i)
                {
                    if (RandomizationHelper.Choose(n, ref randomState))
                    {
                        ++successes;
                    }
                }

                Assert.AreEqual(expected: 1.0 / (n + 1), actual: successes / (double)Trials, delta: 0.0001, message: "value: " + n);
            }
        }

        [Test]
        public void TestChooseNSmallNonEvenlyDivisibleValues()
        {
            var evenlyDivisibleValues = Enumerable.Range(0, TableLength)
                .Where(i => i == 0 || TableLength % i == 0)
                .ToArray();

            foreach (var n in Enumerable.Range(0, TableLength).Except(evenlyDivisibleValues))
            {
                const int Trials = 100000;
                var successes = 0;
                var randomState = 0U;
                for (var i = 0; i < Trials; ++i)
                {
                    if (RandomizationHelper.Choose(n, ref randomState))
                    {
                        ++successes;
                    }
                }

                var probability = successes / (double)Trials;
                //Assert.GreaterOrEqual(probability, 1.0 / )
            }
        }

        [Test]
        public void GenerateMaxNTable()
        {
            var values = new int[TableLength];
            double accumulatedError = 0;
            for (var n = 1; n < TableLength; ++n)
            {
                var probability = 1.0 / (n + 1);
                var count = probability * values.Length;
                var intCount = (int)Math.Round(count);

                if (count == intCount)
                {
                    accumulatedError = 0; // reset
                }
                else
                {
                    accumulatedError += count - intCount;
                    if (accumulatedError > 1)
                    {
                        accumulatedError--;
                        intCount++;
                    }
                    else if (accumulatedError < -1)
                    {
                        accumulatedError++;
                        intCount--;
                    }
                }

                for (var i = 0; i < intCount; ++i)
                {
                    values[i]++;
                } 
            }

            using (var cryptRandom = RandomNumberGenerator.Create())
            {
                Console.WriteLine("**** OUTPUT ****");
                Console.WriteLine($"new[] {{ {string.Join(", ", values.Shuffled(cryptRandom.AsRandom()))} }}");
            }
        }

        [Test]
        public void GenerateNextDoubleTable()
        {
            using (var cryptoRandom = RandomNumberGenerator.Create())
            {
                var random = cryptoRandom.AsRandom();

                var candidates = Enumerable.Range(0, 1000)
                    .Select(_ => Enumerable.Range(0, RandomizationHelper.MaxTableIndex + 1).Select(i => i == 0 ? 0 : i == 1 ? 1 : random.NextDouble()).ToArray());
                
                double StandardDeviation(double[] values)
                {
                    var mean = values.Average();
                    return Math.Sqrt(values.Sum(v => (v - mean) * (v - mean)) / (values.Length - 1));
                }

                const double ExpectedMean = 0.5;
                var expectedStandardDeviation = 1 / Math.Sqrt(12);

                var bestCandidate = candidates.Select(c => new { values = c, standardDeviation = StandardDeviation(c), mean = c.Average() })
                    .OrderBy(t => Math.Pow(ExpectedMean - t.mean, 2) + Math.Pow(expectedStandardDeviation - t.standardDeviation, 2))
                    .First();

                Console.WriteLine("**** OUTPUT ****");
                Console.WriteLine($"new[] {{ {string.Join(", ", bestCandidate.values.Shuffled(random).Select(v => v.ToString("r")))} }}");
                Console.WriteLine("**** STATS ****");
                Console.WriteLine($"mean = {bestCandidate.mean} (vs {ExpectedMean}), stdev = {bestCandidate.standardDeviation} (vs {expectedStandardDeviation})");
            }
        }
    }
}
