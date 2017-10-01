using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class ScapegoatTreeTest
    {
        [Test]
        public void TestAdd()
        {
            var tree = new ScapegoatTree<int>(Comparer<int>.Default);

            tree.Add(1);
            tree.TryGetNode(1, out var node1).ShouldEqual(true);
            node1.Key.ShouldEqual(1);
            tree.Count.ShouldEqual(1);
            Assert.Throws<ArgumentException>(() => tree.Add(1));

            tree.AddOrGetExisting(1).Key.ShouldEqual(1);
            tree.AddOrGetExisting(2).ShouldEqual(null);
            tree.Count.ShouldEqual(2);

            tree.CheckInvariants();
        }

        [Test]
        public void TestIndexedAccess()
        {
            var tree = new ScapegoatTree<string>(StringComparer.Ordinal);

            Enumerable.Range(0, 100).ToList().ForEach(i => tree.Add(i.ToString("00")));
            tree.CheckInvariants();

            for (var i = 0; i < 100; ++i)
            {
                tree.GetNodeAtIndex(i).Key.ShouldEqual(i.ToString("00"));
            }
        }

        [Test]
        public void TestMinAndMax()
        {
            var tree = new ScapegoatTree<string>(Comparer<string>.Default);

            Assert.Throws<InvalidOperationException>(() => tree.Min.ToString());
            Assert.Throws<InvalidOperationException>(() => tree.Max.ToString());

            Enumerable.Range(0, 100).Reverse().ToList().ForEach(i => tree.Add(i.ToString("00")));
            tree.CheckInvariants();
            tree.Min.Key.ShouldEqual("00");
            tree.Max.Key.ShouldEqual("99");
        }

        [Test]
        public void TestRemove()
        {
            var tree = new ScapegoatTree<int>(Comparer<int>.Default);
            new List<int> { 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2, 1 }.ForEach(i => tree.AddAllowDuplicates(i));
            
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

            tree.CheckInvariants();
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
