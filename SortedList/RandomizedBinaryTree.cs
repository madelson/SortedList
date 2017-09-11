using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
   // todo can potentially use weight-balanced BST instead
    internal struct RandomizedBinaryTree<TNode, TKey, TValue, TKeyAndValue, TNodeDriver>
        where TNode : Node<TKey, TNode>
        where TNodeDriver : struct, INodeDriver<TNode, TKey, TValue, TKeyAndValue>
    {
        private TNode _root;
        private uint _randomState;
        
        public RandomizedBinaryTree(IComparer<TKey> comparer)
        {
            this._root = null;
            this._randomState = 0;
            this.Comparer = comparer ?? Comparer<TKey>.Default;
        }
        
        public IComparer<TKey> Comparer { get; }

        public int Count => this._root?.Count ?? 0;

        public void Clear() => this._root = null;

        public TNode Min
        {
            get
            {
                if (this._root == null) { throw new InvalidOperationException("the collection is empty"); }

                var node = this._root;
                while (node.Left != null) { node = node.Left; };
                return node;
            }
        }

        public TNode Max
        {
            get
            {
                if (this._root == null) { throw new InvalidOperationException("the collection is empty"); }

                var node = this._root;
                while (node.Left != null) { node = node.Left; };
                return node;
            }
        }

        public TKeyAndValue Get(int index) => default(TNodeDriver).GetKeyAndValue(this.GetNodeAtIndex(index));

        public void SetValue(int index, TValue value) => default(TNodeDriver).SetValue(this.GetNodeAtIndex(index), value);

        private TNode GetNodeAtIndex(int index)
        {
            if (index < 0 || index >= this.Count) { throw new ArgumentOutOfRangeException(nameof(index), index, "must be non-negative and less than Count"); }

            var node = this._root;
            var adjustedIndex = index;
            while (true)
            {
                var leftCount = node.Left?.Count ?? 0;
                if (adjustedIndex < leftCount) { node = node.Left; }
                else if (adjustedIndex == leftCount) { return node; }
                else
                {
                    adjustedIndex -= (leftCount + 1);
                    node = node.Right;
                }
            }
        }

        public bool TryGetNode(TKey key, out TNode node) => this.TryGetNode(this._root, key, out node);

        private bool TryGetNode(TNode root, TKey key, out TNode node)
        {
            var current = root;
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

        public TValue this[TKey key]
        {
            get => this.TryGetNode(key, out var node) ? default(TNodeDriver).GetValue(node) : throw new KeyNotFoundException();
            set => this.Insert(key, value, DuplicateHandling.Overwrite);
        }

        public bool TryAdd(TKey key, TValue value) => this.Insert(key, value, DuplicateHandling.TryAddUnique);

        public void Add(TKey key, TValue value, bool allowDuplicates) => this.Insert(key, value, allowDuplicates ? DuplicateHandling.Add : DuplicateHandling.AddUnique);

        private bool Insert(TKey key, TValue value, DuplicateHandling duplicateHandling) => this.Insert(key, value, ref this._root, duplicateHandling);

        private bool Insert(TKey key, TValue value, ref TNode nodeRef, DuplicateHandling duplicateHandling)
        {
            var node = nodeRef;

            if (node == null)
            {
                var driver = default(TNodeDriver);
                var newNode = driver.Create();
                newNode.Key = key;
                throw new NotImplementedException();
                //newNode.Value = value;
                newNode.Count = 1;
                nodeRef = nodeRef = newNode;
                return true;
            }

            if (RandomizationHelper.Choose(node.Count, ref this._randomState))
            {
                if ((duplicateHandling & DuplicateHandling.AddUnique) == DuplicateHandling.AddUnique)
                {
                }

                // split and insert at root
                this.Split(node, key, out var left, out var right);
                var newNode = default(TNodeDriver).Create();
                newNode.Key = key;
                throw new NotImplementedException();
                //newNode.Value = value;
                newNode.Left = left;
                newNode.Right = right;
                newNode.Count = Node<TNode>.ComputeCount(left, right);
                nodeRef = newNode;
                return true;
            }

            ++node.Count;
            var cmp = this.Comparer.Compare(key, node.Key);
            if (cmp < 0) { return this.Insert(key, value, ref node.Left, duplicateHandling); }
            else { return this.Insert(key, value, ref node.Right, duplicateHandling); }
        }

        /// <summary>
        /// Splits the subtree represented by <paramref name="node"/> into two
        /// trees based on <paramref name="key"/>
        /// </summary>
        private void Split(TNode node, TKey key, out TNode left, out TNode right)
        {
            var cmp = this.Comparer.Compare(key, node.Key);
            if (cmp < 0)
            {
                right = node;
                if (node.Left == null)
                {
                    left = null;
                }
                else
                {
                    node.Count -= node.Left.Count;
                    this.Split(node.Left, key, out left, out node.Left);
                    node.Count += node.Left?.Count ?? 0;
                }
            }
            else
            {
                left = node;
                if (node.Right == null)
                {
                    right = null;
                }
                else
                {
                    node.Count -= node.Right.Count;
                    this.Split(node.Right, key, out node.Right, out right);
                    node.Count += node.Right?.Count ?? 0;
                }
            }
        }

        // todo set operations
    }

    internal enum DuplicateHandling : byte
    {
        Add = 1 << 0,
        Overwrite = 1 << 1,
        AddUnique = 1 << 2,
        TryAddUnique = AddUnique | 1 << 3,

        EnforceUnique,
        RetainOriginal,
        OverwriteValue,
        RetainAll,
    }
}
