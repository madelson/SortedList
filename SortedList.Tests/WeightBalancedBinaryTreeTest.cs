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

        [Test]
        public void TestRemoveByKey()
        {
            var tree = new WeightBalancedBinaryTree<KeyNode<int>, int, int, int, KeyNode<int>.Driver>(Comparer<int>.Default);
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1 }.ForEach(i => tree.Add(i, allowDuplicates: true));

            tree.Remove(1).ShouldEqual(true);
            tree.TryGetNode(1, out _).ShouldEqual(true);
            tree.Remove(1).ShouldEqual(true);
            tree.Count.ShouldEqual(11);
            tree.TryGetNode(1, out _).ShouldEqual(false);
            tree.Remove(1).ShouldEqual(false);
            tree.Count.ShouldEqual(11);

            tree.Remove(8).ShouldEqual(false);
            tree.Count.ShouldEqual(11);

            tree.Remove(7).ShouldEqual(true);
            tree.Count.ShouldEqual(10);
            tree.TryGetNode(7, out _).ShouldEqual(false);

            NodeValidator<KeyNode<int>, int>.Validate(tree._root);
        }

        [Test]
        public void TestRemoveByIndex()
        {
            var tree = new WeightBalancedBinaryTree<KeyNode<int>, int, int, int, KeyNode<int>.Driver>(Comparer<int>.Default);
            new List<int> { 1, 2, 3, 4, 5 }.ForEach(i => tree.Add(i, allowDuplicates: false));

            Assert.Throws<ArgumentOutOfRangeException>(() => tree.RemoveAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => tree.RemoveAt(tree.Count));

            tree.RemoveAt(1);
            tree.Count.ShouldEqual(4);
            tree.TryGetNode(2, out _).ShouldEqual(false);

            NodeValidator<KeyNode<int>, int>.Validate(tree._root);

            tree.RemoveAt(3);
            tree.Count.ShouldEqual(3);
            tree.TryGetNode(5, out _).ShouldEqual(false);

            NodeValidator<KeyNode<int>, int>.Validate(tree._root);
        }
    }
}
