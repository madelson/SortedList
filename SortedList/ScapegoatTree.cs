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
        private int _maxCount;

        public ScapegoatTree(IComparer<TKey> comparer) 
        {
            this._root = null;
            this._maxCount = 0;
            this.Comparer = comparer;
        }

        public IComparer<TKey> Comparer { get; }
        public int Count => this._root?.Count ?? 0;

        public Node AddOrGetExisting(TKey key)
        {
            if (this.Count == int.MaxValue) { throw new InvalidOperationException("collection full"); }

            var (existing, needsRebuild) = this.AddOrGetExisting(ref this._root, key, depth: 0);
            Debug.Assert(!needsRebuild);
            this.UpdateMaxCount();
            return existing;
        }

        private (Node existing, bool needsRebuild) AddOrGetExisting(ref Node node, TKey key, int depth)
        {
            if (node == null)
            {
                node = new Node { Key = key, Count = 1 };
                return (existing: null, needsRebuild: this.NeedsRebuild(depth));
            }
            
            var cmp = this.Comparer.Compare(key, node.Key);
            if (cmp < 0)
            {
                var result = this.AddOrGetExisting(ref node.Left, key, depth + 1);
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
                var result = this.AddOrGetExisting(ref node.Right, key, depth + 1);
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

        public bool Remove(TKey key)
        {
            var result = this.Remove(ref this._root, key);
            var count = this.Count;
            if (this._maxCount - count > count)
            {
                Balance(ref this._root);
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
            if (node.Right.Count > node.Left.Count) { ReplaceForDeletion(node, ref FindMin(ref node.Right)); }
            else { ReplaceForDeletion(node, ref FindMax(ref node.Left)); }
        }

        private static void ReplaceForDeletion(Node toDelete, ref Node replacement)
        {
            toDelete.Key = replacement.Key;
            Delete(ref replacement);
        }

        private static ref Node FindMin(ref Node current)
        {
            if (current.Left == null) { return ref current; }
            return ref FindMin(ref current.Left);
        }

        private static ref Node FindMax(ref Node current)
        {
            if (current.Right == null) { return ref current; }
            return ref FindMax(ref current.Right);
        }

        private static void Balance(ref Node node)
        {
            var count = (node?.Count ?? 0);

            // empty, 1, or 2 trees are always balanced
            if (count < 3) { return; }

            var enumerator = new Enumerator(node);
            Balance(ref enumerator, count);
            enumerator.Dispose();
        }

        private static Node Balance(ref Enumerator enumerator, int count)
        {
            if (count == 0)
            {
                return null;
            }
            if (count == 1)
            {
                var leaf = enumerator.Current;
                enumerator.MoveNext();
                leaf.Left = leaf.Right = null;
                leaf.Count = 1;
                return leaf;
            }

            var subtreeTotal = count - 1;
            var rightSubtreeTotal = subtreeTotal >> 1;
            var left = Balance(ref enumerator, subtreeTotal - rightSubtreeTotal);
            var root = enumerator.Current;
            enumerator.MoveNext();
            var right = Balance(ref enumerator, rightSubtreeTotal);
            root.Left = left;
            root.Right = right;
            root.Count = left.Count + 1 + (right?.Count ?? 0);
            return root;
        }

        private void UpdateMaxCount()
        {
            var count = this.Count;
            if (this._maxCount < count)
            {
                this._maxCount = count;
            }
        }

        private bool NeedsRebuild(int depth)
        {
            return depth > ScapegoatHelper.FloorLog(this.Count) + 2;
        }

        private static bool IsWeightBalanced(Node parent, Node child)
        {
            // unbalanced if
            // size(child) > .5 * size(parent)
            // <=> 2 * size(child) > size(parent)
            // <=> size(child) > size(parent) - size(child)

            var childCount = child.Count;
            return childCount > (parent.Count - child.Count);
        }

        internal struct Enumerator : IEnumerator<Node>
        {
            private LiteStack<Node> _stack;
            private Node _current;

            public Enumerator(Node root)
            {
                this._current = null;
                this._stack = root != null ? LiteStack<Node>.Create() : LiteStack<Node>.Empty;
                this.PushLeft(root);
            }

            public Node Current => this._current;

            object IEnumerator.Current => this.Current;

            public void Dispose() => this._stack.Dispose();

            public bool MoveNext()
            {
                if (this._stack.Count == 0) { return false; }

                this._current = this._stack.Pop();
                this.PushLeft(this._current.Right);
                return true;
            }

            public void Reset() => throw new NotSupportedException();

            private void PushLeft(Node node)
            {
                for (var current = node; current != null; current = current.Left)
                {
                    this._stack.Push(current);
                }
            }
        }

        internal sealed class Node
        {
            public TKey Key;
            public int Count;
            public Node Left, Right;
        }
    }

    internal static class ScapegoatHelper
    {
        private static readonly sbyte[] LogTable = new sbyte[256];

        static ScapegoatHelper()
        {
            LogTable[0] = LogTable[1] = 0;
            for (int i = 2; i < 256; i++)
            {
                LogTable[i] = (sbyte)(1 + LogTable[i / 2]);
            }
            LogTable[0] = -1; // make log(0) to return -1
        }

        public static int FloorLog(int value)
        {
            // based on http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogLookup

            var top16Bits = value >> 16;
            if (top16Bits != 0)
            {
                var top8Bits = top16Bits >> 8;
                return top8Bits != 0 ? 24 + LogTable[top8Bits] : 16 + LogTable[top16Bits];
            }

            var secondByte = value >> 8;
            return secondByte != 0 ? 8 + LogTable[secondByte] : LogTable[value];
        }
    }
}
