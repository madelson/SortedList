using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal sealed class KeyValueNode<TKey, TValue> : Node<TKey, KeyValueNode<TKey, TValue>>
    {
        public TValue Value;

        public struct Driver : INodeDriver<KeyValueNode<TKey, TValue>, TKey, TValue, (TKey, TValue)>
        {
            public KeyValueNode<TKey, TValue> Create() => new KeyValueNode<TKey, TValue>();

            public TKey GetKey((TKey, TValue) keyAndValue) => keyAndValue.Item1;

            public (TKey, TValue) GetKeyAndValue(KeyValueNode<TKey, TValue> node) => (node.Key, node.Value);

            public void SetKeyAndValue(KeyValueNode<TKey, TValue> node, (TKey, TValue) keyAndValue) => (node.Key, node.Value) = keyAndValue;

            public TValue GetValue(KeyValueNode<TKey, TValue> node) => node.Value;

            public void SetValue(KeyValueNode<TKey, TValue> node, TValue value) => node.Value = value;
        }
    }
}
