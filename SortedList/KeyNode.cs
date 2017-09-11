using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal sealed class KeyNode<TKey> : Node<TKey, KeyNode<TKey>>
    {
        public struct Driver : INodeDriver<KeyNode<TKey>, TKey, TKey, TKey>
        {
            public KeyNode<TKey> Create() => new KeyNode<TKey>();

            public TKey GetKey(TKey keyAndValue) => keyAndValue;

            public TKey GetKeyAndValue(KeyNode<TKey> node) => node.Key;

            public void SetKeyAndValue(KeyNode<TKey> node, TKey keyAndValue) => node.Key = keyAndValue;

            public TKey GetValue(KeyNode<TKey> node) => node.Key;

            public void SetValue(KeyNode<TKey> node, TKey value) => throw new NotSupportedException();
        }
    }
}
