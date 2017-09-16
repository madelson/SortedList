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

        [Test]
        public void TestMinAndMax()
        {
            var tree = new WeightBalancedBinaryTree<KeyValueNode<string, int>, string, int, (string key, int value), KeyValueNode<string, int>.Driver>(Comparer<string>.Default);

            Assert.Throws<InvalidOperationException>(() => tree.Min.ToString());
            Assert.Throws<InvalidOperationException>(() => tree.Max.ToString());

            Enumerable.Range(0, 100).Reverse().ToList().ForEach(i => tree.Add((i.ToString("00"), i * i), allowDuplicates: false));
            tree.Min.Key.ShouldEqual("00");
            tree.Min.Value.ShouldEqual(0);
            tree.Max.Key.ShouldEqual("99");
            tree.Max.Value.ShouldEqual(99 * 99);
        }

        [Test]
        public void TestKeyValueAccess()
        {
            var tree = new WeightBalancedBinaryTree<KeyValueNode<string, int>, string, int, (string key, int value), KeyValueNode<string, int>.Driver>(StringComparer.OrdinalIgnoreCase);

            tree.TryGetNode("a", out var aNode).ShouldEqual(false);
            aNode.ShouldEqual(null);

            tree["a"] = 1;
            tree["a"].ShouldEqual(1);

            tree.TryGetNode("A", out aNode).ShouldEqual(true);
            aNode.Key.ShouldEqual("a");
            aNode.Value.ShouldEqual(1);

            tree["A"] = 6;
            tree.TryGetNode("A", out aNode).ShouldEqual(true);
            aNode.Key.ShouldEqual("a");
            aNode.Value.ShouldEqual(6);
        }
    }
}
