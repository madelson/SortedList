using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections.Tests
{
    internal static class NodeValidator<TNode, TKey>
        where TNode : Node<TKey, TNode>
    {
        public static void Validate(TNode node, IComparer<TKey> comparer = null)
        {
            if (node == null) { return; }

            var lowerBound = Traverse.Along(node, n => n.Left).Last().Key;
            var upperBound = Traverse.Along(node, n => n.Right).Last().Key;

            InternalValidate(node, comparer ?? Comparer<TKey>.Default, lowerBound, upperBound);
        }

        private static void InternalValidate(TNode node, IComparer<TKey> comparer, TKey lowerBound, TKey upperBound)
        {
            var expectedCount = Node<TNode>.ComputeCount(node.Left, node.Right);
            if (node.Count != expectedCount)
            {
                Assert.Fail($"Expected count {expectedCount}. Was: {node.Count}");
            }
            
            if (comparer.Compare(node.Key, lowerBound) < 0)
            {
                Assert.Fail($"Found value '{node.Key}' below lower bound '{lowerBound}'");
            }
            if (comparer.Compare(node.Key, upperBound) > 0)
            {
                Assert.Fail($"Found value '{node.Key}' above upper bound '{lowerBound}'");
            }

            if (node.Left != null)
            {
                InternalValidate(node.Left, comparer, lowerBound, node.Key);
            }
            if (node.Right != null)
            {
                InternalValidate(node.Right, comparer, node.Key, upperBound);
            }
        }
    }
}
