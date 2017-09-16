using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class BulkTreeBuilderTest
    {
        [Test]
        public void TestBuildFromSortedList()
        {
            var node = BulkTreeBuilder<KeyNode<int>, int, int, KeyNode<int>.Driver>.BuildFromSortedList(new[] { 1, 2, 3, 4, 5 });
            NodeValidator<KeyNode<int>, int>.Validate(node);
        }

        [Test]
        public void TestBuildUnique()
        {
            var node = BulkTreeBuilder<KeyNode<int>, int, int, KeyNode<int>.Driver>.BuildFrom(new[] { 1, 3, 4, 2, 5 }, Comparer<int>.Default, DuplicateHandling.EnforceUnique);
            NodeValidator<KeyNode<int>, int>.Validate(node);

            Assert.Throws<ArgumentException>(
                () => BulkTreeBuilder<KeyNode<int>, int, int, KeyNode<int>.Driver>.BuildFrom(new[] { 1, 2, 3, 4, 3 }, Comparer<int>.Default, DuplicateHandling.EnforceUnique)
            );
        }

        [Test]
        public void TestBuildUniqueRetailOriginal()
        {
            var node = BulkTreeBuilder<KeyValueNode<string, int>, string, (string, int), KeyValueNode<string, int>.Driver>.BuildFrom(
                new[] { ("z", 1), ("b", 2), ("a", 3), ("B", 4) },
                StringComparer.OrdinalIgnoreCase,
                DuplicateHandling.RetainOriginal
            );

            NodeValidator<KeyValueNode<string, int>, string>.Validate(node, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual(actual: node.Key, expected: "b");
            Assert.AreEqual(actual: node.Value, expected: 2);
        }
    }
}
