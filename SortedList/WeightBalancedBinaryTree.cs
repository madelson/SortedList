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
            set => this.Insert(default(TNodeDriver).CreateKeyAndValue(key, value), DuplicateHandling.OverwriteValue);
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

        public bool TryAdd(TKeyAndValue keyAndValue)
        {
            var initialCount = this.Count;
            this.Insert(keyAndValue, DuplicateHandling.RetainOriginal);
            return this._root.Count != initialCount;
        }

        public void Add(TKeyAndValue keyAndValue, bool allowDuplicates) => this.Insert(keyAndValue, allowDuplicates ? DuplicateHandling.RetainAll : DuplicateHandling.EnforceUnique);

        private void Insert(TKeyAndValue keyAndValue, DuplicateHandling duplicateHandling)
        {
            this.Insert(default(TNodeDriver).GetKey(keyAndValue), ref keyAndValue, ref this._root, duplicateHandling);
        }

        // note: keyAndValue is a ref purely to avoid struct copies
        private void Insert(TKey key, ref TKeyAndValue keyAndValue, ref TNode nodeRef, DuplicateHandling duplicateHandling)
        {
            var node = nodeRef;

            if (node == null) // reached a leaf
            {
                var driver = default(TNodeDriver);
                var newNode = driver.Create();
                driver.SetKeyAndValue(newNode, keyAndValue);
                newNode.Count = 1;
                nodeRef = newNode;
            }
            else // compare to current key
            {
                var cmp = this.Comparer.Compare(key, node.Key);
                if (cmp == 0)
                {
                    switch (duplicateHandling)
                    {
                        case DuplicateHandling.EnforceUnique:
                            throw new ArgumentException("a duplicate item cannot be added to the collection");
                        case DuplicateHandling.OverwriteValue:
                            var driver = default(TNodeDriver);
                            driver.SetValue(node, driver.GetValue(keyAndValue));
                            return;
                        case DuplicateHandling.RetainOriginal:
                            return;
                        case DuplicateHandling.RetainAll:
                            // when inserting dupes, insert in the smaller side to maintain balance
                            cmp = (node.Left?.Count ?? 0) <= (node.Right?.Count ?? 0) ? -1 : 1;
                            break;
                    }
                }

                if (cmp > 0)
                {
                    this.Insert(key, ref keyAndValue, ref node.Right, duplicateHandling);
                    node.RecalculateCount();
                    Rotations<TNode>.BalanceLeft(ref nodeRef);
                }
                else
                {
                    this.Insert(key, ref keyAndValue, ref node.Left, duplicateHandling);
                    node.RecalculateCount();
                    Rotations<TNode>.BalanceRight(ref nodeRef);
                }
            }
        }

        // todo set operations
    }

    internal enum DuplicateHandling : byte
    {
        EnforceUnique,
        RetainOriginal,
        OverwriteValue,
        RetainAll,
    }
}
