using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void TestIndexedAccess()
        {
            var tree = new WeightBalancedBinaryTree<KeyValueNode<string, int>, string, int, (string key, int value), KeyValueNode<string, int>.Driver>(Comparer<string>.Default);

            Enumerable.Range(0, 100).ToList().ForEach(i => tree.Add((i.ToString("00"), i), allowDuplicates: false));
            
            for (var i = 0; i < 100; ++i)
            {
                tree.Get(i).key.ShouldEqual(i.ToString("00"));
                tree.Get(i).value.ShouldEqual(i);
            }

            tree.SetValue(55, -1);
            tree["55"].ShouldEqual(-1);
        }
    }
}
