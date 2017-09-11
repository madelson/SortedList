using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    internal static class BulkTreeBuilder<TNode, TKey, TKeyAndValue, TDriver>
        where TNode : Node<TKey, TNode>
        where TDriver : struct, INodeDriver<TNode, TKey, TKeyAndValue>
    {
        public static TNode BuildFrom(IEnumerable<TKeyAndValue> elements, IComparer<TKey> comparer, DuplicateHandling duplicateHandling)
        {
            if (duplicateHandling == DuplicateHandling.OverwriteValue) { throw new NotSupportedException(nameof(duplicateHandling)); }

            var keyAndValueComparer = default(TDriver).GetKeyAndValueComparer<TDriver, TNode, TKey, TKeyAndValue>(comparer);

            List<TKeyAndValue> elementsList;
            if (duplicateHandling == DuplicateHandling.RetainOriginal)
            {
                elementsList = new List<TKeyAndValue>();
                // if we're removing duplicates, do a stable sort (OrderBy) to make sure we retain the originals
                foreach (var element in elements.OrderBy(t => t, keyAndValueComparer))
                {
                    if (elementsList.Count == 0 
                        || keyAndValueComparer.Compare(elementsList[elementsList.Count - 1], element) != 0)
                    {
                        elementsList.Add(element);
                    }
                }
            }
            else
            {
                elementsList = elements.ToList();
                elementsList.Sort(keyAndValueComparer);

                if (duplicateHandling == DuplicateHandling.EnforceUnique)
                {
                    for (var i = 1; i < elementsList.Count; ++i)
                    {
                        if (keyAndValueComparer.Compare(elementsList[i - 1], elementsList[i]) == 0)
                        {
                            throw new ArgumentException("An item with the same key has already been added.");
                        }
                    }
                }
            }

            return BuildFromSortedList(elementsList);
        }

        public static TNode BuildFromSortedList(IReadOnlyList<TKeyAndValue> sortedElements) => BuildFromSortedListRange(sortedElements, 0, sortedElements.Count);

        private static TNode BuildFromSortedListRange(IReadOnlyList<TKeyAndValue> sortedElements, int startInclusive, int endExclusive)
        {
            if (startInclusive == endExclusive) { return null; }

            var median = startInclusive + ((endExclusive - startInclusive) >> 1);
            var driver = default(TDriver);
            var node = driver.Create();
            node.Left = BuildFromSortedListRange(sortedElements, startInclusive: startInclusive, endExclusive: median);
            node.Right = BuildFromSortedListRange(sortedElements, startInclusive: median + 1, endExclusive: endExclusive);
            node.Count = Node<TNode>.ComputeCount(node.Left, node.Right);
            driver.SetKeyAndValue(node, sortedElements[median]);
            return node;
        }
    }
}
