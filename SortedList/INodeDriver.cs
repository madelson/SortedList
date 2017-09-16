using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal interface INodeDriver<TKey, TKeyAndValue>
    {
        TKey GetKey(TKeyAndValue keyAndValue);
    }

    internal interface INodeDriver<TNode, TKey, TKeyAndValue> : INodeDriver<TKey, TKeyAndValue>
        where TNode : Node<TKey, TNode>
    {
        TNode Create();
        TKeyAndValue GetKeyAndValue(TNode node);
        void SetKeyAndValue(TNode node, TKeyAndValue keyAndValue);
    }

    internal interface INodeDriver<TNode, TKey, TValue, TKeyAndValue> : INodeDriver<TNode, TKey, TKeyAndValue>
        where TNode : Node<TKey, TNode>
    {
        TKeyAndValue CreateKeyAndValue(TKey key, TValue value);
        TValue GetValue(TKeyAndValue keyAndValue);
        TValue GetValue(TNode node);
        void SetValue(TNode node, TValue value);
    }

    internal static class NodeDriverHelpers
    {
        public static IComparer<TKeyAndValue> GetKeyAndValueComparer<TDriver, TNode, TKey, TKeyAndValue>(this TDriver driver, IComparer<TKey> comparer)
            where TDriver : struct, INodeDriver<TNode, TKey, TKeyAndValue>
            where TNode : Node<TKey, TNode>
        {
            return comparer is IComparer<TKeyAndValue> keyAndValueComparer ? keyAndValueComparer
                : Comparer<TKey>.Default.Equals(comparer) ? KeyAndValueComparer<TDriver, TKey, TKeyAndValue>.Default
                : new KeyAndValueComparer<TDriver, TKey, TKeyAndValue>(comparer);
        }

        private sealed class KeyAndValueComparer<TDriver, TKey, TKeyAndValue> : IComparer<TKeyAndValue>
            where TDriver : struct, INodeDriver<TKey, TKeyAndValue>
        {
            private static IComparer<TKeyAndValue> _default;

            public static IComparer<TKeyAndValue> Default => _default ?? (_default = new KeyAndValueComparer<TDriver, TKey, TKeyAndValue>(Comparer<TKey>.Default));

            private readonly IComparer<TKey> _keyComparer;

            public KeyAndValueComparer(IComparer<TKey> keyComparer)
            {
                this._keyComparer = keyComparer;
            }

            public int Compare(TKeyAndValue x, TKeyAndValue y)
            {
                var driver = default(TDriver);
                return this._keyComparer.Compare(driver.GetKey(x), driver.GetKey(y));
            }
        }
    }
}
