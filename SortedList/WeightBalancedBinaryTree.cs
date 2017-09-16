using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal struct WeightBalancedBinaryTree<TNode, TKey, TValue, TKeyAndValue, TNodeDriver>
        where TNode : Node<TKey, TNode>
        where TNodeDriver : struct, INodeDriver<TNode, TKey, TValue, TKeyAndValue>
    {
        private TNode _root;
        
        public WeightBalancedBinaryTree(IComparer<TKey> comparer)
        {
            this._root = null;
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

        public int IndexOf(TKey key, int startIndex = 0, int? count = null)
        {
            var foundIndex = this.FindIndex(key, startIndex, count);
            return foundIndex < 0 ? -1 : foundIndex;
        }

        public int LastIndexOf(TKey key, int? startIndex = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public int FindIndex(TKey key, int startIndex = 0, int? count = null)
        {
            var countToUse = count ?? this.Count;
            if (startIndex < 0) { throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "must be non-negative"); }
            if (countToUse < 0) { throw new ArgumentOutOfRangeException(nameof(count), countToUse, "must be non-negative"); }
            //if (startIndex + countToUse > this.Count) { throw new ArgumentOutOfRangeException(nameof(count), count, "in combination with " + nameof(startIndex), " must refer to a valid range of indices"); }

            throw new NotImplementedException();
        }

        public int FindLastIndex(TKey key, int startIndex = 0, int? count = null)
        {
            throw new NotImplementedException();
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

            throw new NotImplementedException();
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
