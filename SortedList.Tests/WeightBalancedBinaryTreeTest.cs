using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class WeightBalancedBinaryTreeTest
    {
        [Test]
        public void TestAdd()
        {
            var tree = new WeightBalancedBinaryTree<KeyNode<int>, int, int, int, KeyNode<int>.Driver>(Comparer<int>.Default);

            tree.Add(1, allowDuplicates: false);
            Assert.AreEqual(actual: tree[1], expected: 1);
            Assert.AreEqual(actual: tree.Count, expected: 1);
            Assert.Throws<ArgumentException>(() => tree.Add(1, allowDuplicates: false));

            Assert.IsFalse(tree.TryAdd(1));
            Assert.IsTrue(tree.TryAdd(2));

            tree.Add(2, allowDuplicates: true);
            Assert.AreEqual(actual: tree.Count, expected: 3);
        }
    }
}
