using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Medallion.Collections
{
    internal struct ScapegoatTree<TKey>
    {
        private Node _root;
        private int _logTableIndex;
        private int _maxCount;

        public ScapegoatTree(IComparer<TKey> comparer) 
        {
            this._root = null;
            this._maxCount = 0;
            this._logTableIndex = 0;
            this.Comparer = comparer;
        }

        public IComparer<TKey> Comparer { get; }
        public int Count => this._root?.Count ?? 0;

        #region ---- Insertion ----
        public void Add(TKey key)
        {
            var existing = this.AddOrGetExisting(key);
            if (existing != null) { throw new ArgumentException("The given item was already present in the collection"); }
        }

        public void AddAllowDuplicates(TKey key) => this.AddOrGetExisting(key, allowDuplicates: true);

        public Node AddOrGetExisting(TKey key, bool allowDuplicates = false)
        {
            var (existing, needsRebuild) = this.AddOrGetExisting(ref this._root, key, depth: 0, allowDuplicates: allowDuplicates);
            Debug.Assert(!needsRebuild);
            this.UpdateMaxCount();
            return existing;
        }

        private (Node existing, bool needsRebuild) AddOrGetExisting(ref Node node, TKey key, int depth, bool allowDuplicates)
        {
            if (node == null)
            {
                if (this.Count == int.MaxValue) { throw new InvalidOperationException("collection full"); }
                node = new Node { Key = key, Count = 1 };
                return (existing: null, needsRebuild: this.CheckRebuildAfterInsert(depth));
            }
            
            var cmp = this.Comparer.Compare(key, node.Key);

            // if we're allowing duplicates and we are a duplicate, place the new node such that it falls
            // in the subtree with smaller count. If both subtrees have equal count, place it arbitrarily on
            // the left
            if (allowDuplicates && cmp == 0)
            {
                var countCmp = node.LeftCount.CompareTo(node.RightCount);
                if (countCmp != 0) { cmp = countCmp; }
                else { cmp = -1; }
            }

            if (cmp < 0)
            {
                var result = this.AddOrGetExisting(ref node.Left, key, depth + 1, allowDuplicates);
                if (result.existing != null) { return result; }

                ++node.Count;
                if (result.needsRebuild && !IsWeightBalanced(node, node.Left))
                {
                    Balance(ref node);
                    return (existing: null, needsRebuild: false);
                }

                return result;
            }
            if (cmp > 0)
            {
                var result = this.AddOrGetExisting(ref node.Right, key, depth + 1, allowDuplicates);
                if (result.existing != null) { return result; }

                ++node.Count;
                if (result.needsRebuild && !IsWeightBalanced(node, node.Right))
                {
                    Balance(ref node);
                    return (existing: null, needsRebuild: false);
                }

                return result;
            }
            // cmp == 0
            return (existing: node, needsRebuild: false);
        }

        private void UpdateMaxCount()
        {
            var count = this.Count;
            if (this._maxCount < count)
            {
                this._maxCount = count;
            }
        }

        private bool CheckRebuildAfterInsert(int depth)
        {
            // after insert, our new count will be this.Count + 1. 
            // We need to rebuild if our tree height > log(newCount, 1/Alpha) + 1
            // <=> depth > log(newCount, 1/Alpha) + 1
            // <=> depth > logTableIndex + 1

            // first, determine if we need to increase the log table index due to
            // the growth
            if (this._logTableIndex < ScapegoatHelper.LogTable.Length - 1
                && this.Count + 1 >= ScapegoatHelper.LogTable[this._logTableIndex + 1])
            {
                ++this._logTableIndex;    
            }
     
            return depth > this._logTableIndex + 1;
        }

        private static bool IsWeightBalanced(Node parent, Node child) => child.Count <= ScapegoatHelper.Alpha * parent.Count;
        #endregion

        #region ---- Removal ----
        public bool Remove(TKey key)
        {
            var result = this.Remove(ref this._root, key);
            var count = this.Count;
            if (count <= ScapegoatHelper.Alpha * this._maxCount)
            {
                BalanceAfterDeletion(ref this._root);
                this._maxCount = count;
            }
            return result;
        }

        private bool Remove(ref Node node, TKey key)
        {
            if (node == null) { return false; }

            var cmp = this.Comparer.Compare(key, node.Key);
            if (cmp < 0)
            {
                var result = this.Remove(ref node.Left, key);
                if (result) { --node.Count; }
                return result;
            }
            if (cmp > 0)
            {
                var result = this.Remove(ref node.Right, key);
                if (result) { --node.Count; }
                return result;
            }
            // cmp == 0
            Delete(ref node);
            return true;
        }

        private static void Delete(ref Node node)
        {
            // no children => just delete
            if (node.Count == 1)
            {
                node = null;
                return;
            }

            // one child => promote that child
            if (node.Left == null)
            {
                node = node.Right;
                return;
            }
            if (node.Right == null)
            {
                node = node.Left;
                return;
            }

            // two children => replace with next or previous child
            --node.Count;
            if (node.Right.Count > node.Left.Count) { ReplaceForDeletion(node, ref FindMinForDeletion(ref node.Right)); }
            else { ReplaceForDeletion(node, ref FindMaxForDeletion(ref node.Left)); }
        }

        private static void ReplaceForDeletion(Node toDelete, ref Node replacement)
        {
            toDelete.Key = replacement.Key;
            Delete(ref replacement);
        }

        private static ref Node FindMinForDeletion(ref Node current)
        {
            if (current.Left == null) { return ref current; }
            --current.Count;
            return ref FindMinForDeletion(ref current.Left);
        }

        private static ref Node FindMaxForDeletion(ref Node current)
        {
            if (current.Right == null) { return ref current; }
            --current.Count;
            return ref FindMaxForDeletion(ref current.Right);
        }
        #endregion

        #region ---- Balancing ----
        /// <summary>
        /// As <see cref="Balance(ref Node)"/>, but avoids balancing any nodes
        /// where the balance is already perfect
        /// </summary>
        private static void BalanceAfterDeletion(ref Node node)
        {
            if (node == null) { return; }

            // perfectly balanced if |leftCount - rightCount| <= 1
            if (Math.Abs(node.LeftCount - node.RightCount) <= 1)
            {
                BalanceAfterDeletion(ref node.Left);
                BalanceAfterDeletion(ref node.Right);
                return;
            }

            Balance(ref node);
        }

        private static void Balance(ref Node node)
        {
            var enumerator = new DestructiveEnumerator(node);
            node = Balance(ref enumerator, node.Count);
            Debug.Assert(!enumerator.MoveNext());
            enumerator.Dispose();
        }

        private static Node Balance<TEnumerator>(ref TEnumerator enumerator, int count)
            where TEnumerator : struct, IEnumerator<Node>
        {
            Debug.Assert(count > 0, nameof(count));

            if (count == 1)
            {
                enumerator.MoveNext();
                var leaf = enumerator.Current;
                leaf.Left = leaf.Right = null;
                leaf.Count = 1;
                return leaf;
            }

            var subtreeTotal = count - 1;
            var rightSubtreeTotal = subtreeTotal >> 1;
            var left = Balance(ref enumerator, count: subtreeTotal - rightSubtreeTotal);

            enumerator.MoveNext();
            var root = enumerator.Current;
            
            var right = rightSubtreeTotal != 0 ? Balance(ref enumerator, rightSubtreeTotal) : null;
            root.Left = left;
            root.Right = right;
            root.Count = left.Count + 1 + (right?.Count ?? 0);
            return root;
        }

        /// <summary>
        /// An <see cref="IEnumerator{T}"/> of <see cref="Node"/> which
        /// does not require a stack but does destroy the tree structure as it
        /// enumerates. This is useful for rebalancing since we are rewriting the
        /// structure anyway
        /// </summary>
        private struct DestructiveEnumerator : IEnumerator<Node>
        {
            private Node _next, _current;

            public DestructiveEnumerator(Node root)
            {
                this._next = GetLeftList(root, last: null);
                this._current = null;
            }

            public Node Current => this._current;

            object IEnumerator.Current => this.Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                var next = this._next;
                this._current = next;
                if (next == null) { return false; }

                this._next = GetLeftList(next.Right, last: next.Left);
                return true;
            }

            /// <summary>
            /// Given <paramref name="node"/>, returns a linked list starting from the min of <paramref name="node"/>
            /// and traversing up back to <paramref name="node"/> and then ending at <paramref name="last"/>.
            /// 
            /// The left pointers of the nodes are used to traverse the resulting list. This corrupts the existing
            /// tree structure.
            /// </summary>
            private static Node GetLeftList(Node node, Node last)
            {
                Node previous = last, current = node, next = null;
                while (current != null)
                {
                    next = current.Left;
                    current.Left = previous;
                    previous = current;
                    current = next;
                }
                return previous;
            }

            public void Reset() => throw new NotSupportedException();
        }
        #endregion

        #region ---- Lookup ----
        public Node Min
        {
            get
            {
                if (this._root == null) { throw new InvalidOperationException("the collection is empty"); }

                var node = this._root;
                while (node.Left != null) { node = node.Left; };
                return node;
            }
        }

        public Node Max
        {
            get
            {
                if (this._root == null) { throw new InvalidOperationException("the collection is empty"); }

                var node = this._root;
                while (node.Right != null) { node = node.Right; };
                return node;
            }
        }

        public bool TryGetNode(TKey key, out Node node)
        {
            var current = this._root;
            while (current != null)
            {
                var cmp = this.Comparer.Compare(key, current.Key);
                if (cmp < 0) { current = current.Left; }
                else if (cmp > 0) { current = current.Right; }
                else
                {
                    node = current;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public Node GetNodeAtIndex(int index)
        {
            if (index < 0 || index >= this.Count) { throw new ArgumentOutOfRangeException(nameof(index), index, "must be non-negative and less than Count"); }

            var node = this._root;
            var adjustedIndex = index;
            while (true)
            {
                var leftCount = node.LeftCount;
                if (adjustedIndex < leftCount) { node = node.Left; }
                else if (adjustedIndex == leftCount) { return node; }
                else
                {
                    adjustedIndex -= (leftCount + 1);
                    node = node.Right;
                }
            }
        }
        #endregion

        #region ---- Bulk Operations ----
        public void Clear() => this._root = null;
        #endregion 

        #region ---- Helpers ----
        internal void CheckInvariants(Node node = null)
        {
            var nodeToCheck = node ?? this._root;
            var comparer = this.Comparer;

            int CheckTreeInvariantsAndGetMaxDepth(Node current)
            {
                if (current == null) { return -1; }

                if (current.Left != null
                    && comparer.Compare(current.Left.Key, current.Key) > 0)
                {
                    throw new InvalidOperationException($"Bad left child '{current.Left.Key}' of '{current.Key}'");
                }
                if (current.Right != null
                    && comparer.Compare(current.Right.Key, current.Key) < 0)
                {
                    throw new InvalidOperationException($"Bad left child '{current.Right.Key}' of '{current.Key}'");
                }

                var childMaxDepth = Math.Max(CheckTreeInvariantsAndGetMaxDepth(current.Left), CheckTreeInvariantsAndGetMaxDepth(current.Right));

                if (current.Count != (current.LeftCount + 1 + current.RightCount))
                {
                    throw new InvalidOperationException($"Bad count at '{current.Key}'");
                }

                return childMaxDepth + 1;
            }

            var maxDepth = CheckTreeInvariantsAndGetMaxDepth(nodeToCheck);

            // when checking the root, also check global properties of the tree
            if (nodeToCheck == this._root)
            {
                if (this._maxCount < this.Count)
                {
                    throw new InvalidOperationException($"Max count {this._maxCount} < count {this.Count}");
                }
                if (this.Count < ScapegoatHelper.Alpha * this._maxCount)
                {
                    throw new InvalidOperationException($"Count {this.Count} below {ScapegoatHelper.Alpha} * {this._maxCount}");
                }
                if (ScapegoatHelper.LogTable[this._logTableIndex] > this.Count)
                {
                    throw new InvalidOperationException($"Log table index ({this._logTableIndex}) too large for count {this.Count}");
                }
                if (ScapegoatHelper.LogTable[this._logTableIndex + 1] <= this.Count)
                {
                    throw new InvalidOperationException($"Log table index ({this._logTableIndex}) too small for count {this.Count}");
                }
                if (maxDepth > this._logTableIndex + 1)
                {
                    throw new InvalidOperationException($"Tree unbalanced. Max depth is {maxDepth} but threshold is {this._logTableIndex + 1}");
                }
            }
        }
        #endregion

        //private struct Enumerator : IEnumerator<Node>
        //{
        //    private LiteStack<Node> _stack;
        //    private Node _current;

        //    public Enumerator(Node root)
        //    {
        //        this._current = null;
        //        this._stack = root != null ? LiteStack<Node>.Create() : LiteStack<Node>.Empty;
        //        this.PushLeft(root);
        //    }

        //    public Node Current => this._current;

        //    object IEnumerator.Current => this.Current;

        //    public void Dispose() => this._stack.Dispose();

        //    public bool MoveNext()
        //    {
        //        if (this._stack.Count == 0) { return false; }

        //        this._current = this._stack.Pop();
        //        this.PushLeft(this._current.Right);
        //        return true;
        //    }

        //    public void Reset() => throw new NotSupportedException();

        //    private void PushLeft(Node node)
        //    {
        //        for (var current = node; current != null; current = current.Left)
        //        {
        //            this._stack.Push(current);
        //        }
        //    }
        //}

        internal sealed class Node
        {
            public TKey Key;
            public int Count;
            public Node Left, Right;

            public int LeftCount => this.Left?.Count ?? 0;
            public int RightCount => this.Right?.Count ?? 0;
        }
    }

    internal static class ScapegoatHelper
    {
        public const double Alpha = 0.693;

        /// <summary>
        /// <see cref="LogTable"/>[i] is the lowest value x for which floor(log(x, 1/<see cref="Alpha"/>)) == i
        /// </summary>
        public static readonly int[] LogTable = new[] { -1, 0, 3, 4, 5, 7, 10, 14, 19, 28, 40, 57, 82, 118, 170, 245, 354, 510, 736, 1062, 1533, 2212, 3191, 4605, 6644, 9587, 13834, 19962, 28806, 41566, 59980, 86551, 124893, 180221, 260058, 375264, 541507, 781395, 1127554, 1627062, 2347852, 3387954, 4888822, 7054576, 10179764, 14689414, 21196845, 30587077, 44137196, 63690038, 91904816, 132618782, 191369094, 276145879, 398478902, 575005630, 829733953, 1197307291, 1727716149 };
    }
}
