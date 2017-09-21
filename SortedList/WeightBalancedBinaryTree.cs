using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal struct WeightBalancedBinaryTree<TNode, TKey, TValue, TKeyAndValue, TNodeDriver>
        where TNode : Node<TKey, TNode>
        where TNodeDriver : struct, INodeDriver<TNode, TKey, TValue, TKeyAndValue>
    {
        internal TNode _root;
        
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
                while (node.Right != null) { node = node.Right; };
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

        public bool Remove(TKey key) => this.Remove(key, ref this._root);

        private bool Remove(TKey key, ref TNode node)
        {
            if (node == null) { return false; }

            var cmp = this.Comparer.Compare(key, node.Key);

            if (cmp < 0)
            {
                if (this.Remove(key, ref node.Left))
                {
                    --node.Count;
                    Rotations<TNode>.BalanceLeft(ref node);
                    return true;
                }
                return false;
            }
            if (cmp > 0)
            {
                if (this.Remove(key, ref node.Right)) // cmp > 0
                {
                    --node.Count;
                    Rotations<TNode>.BalanceRight(ref node);
                    return true;
                }
                return false;
            }
            
            node = Join(node.Left, node.Right);
            return true;
        }

        private static TNode Join(TNode left, TNode right)
        {
            if (left == null) { return right; }
            if (right == null) { return left; }

            if ((left.Left?.Count ?? 0) > (right.Right?.Count ?? 0))
            {
                left.Right = Join(left.Right, right);
                left.RecalculateCount();
                Rotations<TNode>.BalanceLeft(ref left);
                return left;
            }

            right.Left = Join(left, right.Left);
            right.RecalculateCount();
            Rotations<TNode>.BalanceRight(ref right);
            return right;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.Count) { throw new ArgumentOutOfRangeException(nameof(index), index, "must be non-negative and less than Count"); }
            RemoveAt(index, ref this._root);
        }

        private static void RemoveAt(int index, ref TNode node)
        {
            var leftCount = node.Left?.Count ?? 0;
            if (index < leftCount)
            {
                RemoveAt(index, ref node.Left);
                --node.Count;
                Rotations<TNode>.BalanceRight(ref node);
            }
            else if (index > leftCount)
            {
                RemoveAt(index - leftCount - 1, ref node.Right);
                --node.Count;
                Rotations<TNode>.BalanceLeft(ref node);
            }
            else
            {
                node = Join(node.Left, node.Right);
            }
        }

        #region ---- Non-recursive ----
        //private static TNode EmptyRef;

        //private ref TNode GetNodeOrDefault(ref TNode root, TKey key)
        //{
        //    if (root == null) { return ref root; }

        //    var cmp = this.Comparer.Compare(key, root.Key);
        //    if (cmp == 0) { return ref root; }

        //    var parent = root;
        //    TNode current;
        //    bool isCurrentLeftChild;
            
        //    while (true)
        //    {
        //        if (cmp < 0)
        //        {
        //            current = parent.Left;
        //            isCurrentLeftChild = true;
        //        }
        //        else
        //        {
        //            current = parent.Right;
        //            isCurrentLeftChild = false;
        //        }

        //        if (current == null) { return ref EmptyRef; }

        //        cmp = this.Comparer.Compare(key, current.Key);
        //        if (cmp == 0)
        //        {
        //            if (isCurrentLeftChild) { return ref parent.Left; }
        //            else { return ref parent.Right; }
        //        }

        //        parent = current;
        //    }
        //}

        //private void Remove(ref TNode node)
        //{
        //    node = NonRecursiveJoin(node.Left, node.Right);
        //}

        //private TNode NonRecursiveJoin(TNode left, TNode right)
        //{
        //    var currentLeft = left;
        //    var currentRight = right;

        //    if (currentLeft == null) { return currentRight; }
        //    if (currentRight == null) { return currentLeft; }

        //    TNode result = null;
        //    TNode currentRoot = null;
        //    bool currentRootNeedsLeftChildAssigned = false;

        //    while (true)
        //    {
        //        TNode joinRoot;
        //        bool joinRootNeedsLeftChildAssigned;
        //        if (this.ShouldMakeNewRoot(currentLeft.Count, currentRight.Count))
        //        {
        //            joinRoot = currentLeft;
        //            joinRoot.Count += currentRight.Count + 1;
        //            currentLeft = joinRoot.Right;
        //            joinRootNeedsLeftChildAssigned = false;
        //        }
        //        else
        //        {
        //            joinRoot = currentRight;
        //            joinRoot.Count += currentLeft.Count + 1;
        //            currentRight = joinRoot.Left;
        //            joinRootNeedsLeftChildAssigned = true;
        //        }

        //        if (result == null)
        //        {
        //            result = joinRoot;
        //        }
        //        else if (currentRootNeedsLeftChildAssigned)
        //        {
        //            currentRoot.Left = joinRoot;
        //        }
        //        else
        //        {
        //            currentRoot.Right = joinRoot;
        //        }
        //        currentRoot = joinRoot;
        //        currentRootNeedsLeftChildAssigned = joinRootNeedsLeftChildAssigned;

        //        // todo need to handle nulls
        //    }
        //}

        //private bool ShouldMakeNewRoot(int n)
        //{
        //    return true;
        //}

        //private bool ShouldMakeNewRoot(int n, int m)
        //{
        //    return true;
        //}
        #endregion

        #region ---- Ideas ----
        // like a treap, but rotate up until we reach desired depth!
    
        // general idea: on the way down we can compute the exact index of the new element in the tree. That means
        // we can derive the desired depth by using the same formula we'd use to go up a heap index-wise (divide by 2).
        // We can therefore rotate the new node up the tree until we reach the desired depth, or possibly the desired depth + 1
        // if we want to avoid some rotations.

        // For delete, we traverse to the node calculating index along the way. When we get there, if it's a leaf we just remove it.
        // Otherwise, we search the subtree for a node of the right desired depth and pull that one up (is this just one of the children?)

        //public void PerfectAdd(TKey key, TValue value)
        //{
        //    if (this._root == null)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    this.PerfectAdd(key, value, ref this._root, 0);
        //}

        //private int PerfectAdd(TKey key, TValue value, ref TNode node, int outerLeftCount, int depth, out TNode splitPoint)
        //{
        //    if (node == null)
        //    {
        //        var oneBasedIndex = outerLeftCount + 1;
        //        var desiredDepth = 0;
        //        while (oneBasedIndex > 1)
        //        {
        //            ++desiredDepth;
        //            oneBasedIndex >>= 1;
        //        }

        //        splitPoint = null;
        //        if (desiredDepth == 0)
        //        {
        //            node = default(TNodeDriver).Create();
        //            return -1;
        //        }
        //        return desiredDepth;
        //    }
        //    else
        //    {
        //        var cmp = this.Comparer.Compare(key, node.Key);
        //        if (cmp <= 0)
        //        {
        //            var desiredDepth = this.PerfectAdd(key, value, ref node.Left, outerLeftCount, depth + 1, out var childSplitPoint);
        //            if (desiredDepth == depth)
        //            {
        //                var newNode = default(TNodeDriver).Create();
        //                newNode.Right = node;
        //                newNode.Left = childSplitPoint;
        //                node.RecalculateCount();
        //                newNode.RecalculateCount();
        //                splitPoint = null;
        //                return -1;
        //            }
        //            else
        //            {

        //            }
        //        }

        //        //TNode childSplitPoint;
        //        //var desiredDepth = cmp <= 0
        //        //    ? this.PerfectAdd(key, value, ref node.Left, outerLeftCount, depth + 1, out childSplitPoint)
        //        //    : this.PerfectAdd(key, value, ref node.Right, outerLeftCount + 1 + (node.Left?.Count ?? 0), depth + 1, out childSplitPoint);
        //        //if (desiredDepth == depth)
        //        //{
        //        //}
        //    }
            
        //}
        #endregion

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
