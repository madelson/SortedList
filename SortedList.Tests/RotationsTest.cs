using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections.Tests
{
    public class RotationsTest
    {
        [Test]
        public void TestSingleRotateLeft()
        {
            var root = new KeyNode<string>
            {
                Key = "a",
                Count = 8,
                Left = new KeyNode<string> { Key = "x", Count = 1 },
                Right = new KeyNode<string>
                {
                    Key = "c",
                    Count = 6,
                    Left = new KeyNode<string> { Key = "y", Count = 2 },
                    Right = new KeyNode<string> { Key = "z", Count = 3 },
                }
            };

            Rotations<KeyNode<string>>.BalanceLeft(ref root);

            Assert.AreEqual(actual: ToString(root), expected: "((x, a, y), c, z)");
            Assert.AreEqual(actual: root.Count, expected: 8);
            Assert.AreEqual(actual: root.Left.Count, expected: 4);
            Assert.AreEqual(actual: root.Right.Count, expected: 3);
        }

        [Test]
        public void TestDoubleRotateLeft()
        {
            var root = new KeyNode<string>
            {
                Key = "a",
                Count = 10,
                Left = new KeyNode<string> { Key = "x", Count = 1 },
                Right = new KeyNode<string>
                {
                    Key = "c",
                    Count = 8,
                    Left = new KeyNode<string>
                    {
                        Key = "b",
                        Count = 6,
                        Left = new KeyNode<string> { Key = "y0", Count = 2 },
                        Right = new KeyNode<string> { Key = "y1", Count = 3 },
                    },
                    Right = new KeyNode<string> { Key = "z", Count = 1 },
                }
            };

            Rotations<KeyNode<string>>.BalanceLeft(ref root);

            Assert.AreEqual(actual: ToString(root), expected: "((x, a, y0), b, (y1, c, z))");
            Assert.AreEqual(actual: root.Count, expected: 10);
            Assert.AreEqual(actual: root.Left.Count, expected: 4);
            Assert.AreEqual(actual: root.Right.Count, expected: 5);
        }

        [Test]
        public void TestSingleRotateRight()
        {
            var root = new KeyNode<string>
            {
                Key = "a",
                Count = 8,
                Right = new KeyNode<string> { Key = "x", Count = 1 },
                Left = new KeyNode<string>
                {
                    Key = "c",
                    Count = 6,
                    Right = new KeyNode<string> { Key = "y", Count = 2 },
                    Left = new KeyNode<string> { Key = "z", Count = 3 },
                }
            };

            Rotations<KeyNode<string>>.BalanceRight(ref root);

            Assert.AreEqual(actual: ToString(root), expected: "(z, c, (y, a, x))");
            Assert.AreEqual(actual: root.Count, expected: 8);
            Assert.AreEqual(actual: root.Right.Count, expected: 4);
            Assert.AreEqual(actual: root.Left.Count, expected: 3);
        }

        [Test]
        public void TestDoubleRotateRight()
        {
            var root = new KeyNode<string>
            {
                Key = "a",
                Count = 10,
                Right = new KeyNode<string> { Key = "x", Count = 1 },
                Left = new KeyNode<string>
                {
                    Key = "c",
                    Count = 8,
                    Right = new KeyNode<string>
                    {
                        Key = "b",
                        Count = 6,
                        Right = new KeyNode<string> { Key = "y0", Count = 2 },
                        Left = new KeyNode<string> { Key = "y1", Count = 3 },
                    },
                    Left = new KeyNode<string> { Key = "z", Count = 1 },
                }
            };

            Rotations<KeyNode<string>>.BalanceRight(ref root);

            Assert.AreEqual(actual: ToString(root), expected: "((z, c, y1), b, (y0, a, x))");
            Assert.AreEqual(actual: root.Count, expected: 10);
            Assert.AreEqual(actual: root.Right.Count, expected: 4);
            Assert.AreEqual(actual: root.Left.Count, expected: 5);
        }

        private string ToString(KeyNode<string> node)
        {
            return node == null ? string.Empty
                : node.Left == null && node.Right == null ? node.Key
                : $"({string.Join(", ", new[] { ToString(node.Left), node.Key, ToString(node.Right) }.Where(s => s.Length > 0))})";
        }
    }
}
