using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal abstract class Node<TNode>
        where TNode : Node<TNode>
    {
        public TNode Left, Right;
        public int Count;

        public void RecalculateCount() => this.Count = ComputeCount(this.Left, this.Right);

        public static int ComputeCount(TNode left, TNode right) => (left?.Count ?? 0) + (right?.Count ?? 0) + 1;
    }

    internal abstract class Node<TKey, TNode> : Node<TNode>
        where TNode : Node<TKey, TNode>
    {
        public TKey Key;
    }
}
