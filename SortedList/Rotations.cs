using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    internal static class Rotations<TNode> where TNode : Node<TNode>
    {
        public static void BalanceLeft(ref TNode node) => Chiral<NormalChirality>.BalanceLeft(ref node);
        public static void BalanceRight(ref TNode node) => Chiral<ReverseChirality>.BalanceLeft(ref node);

        private static class Chiral<TChirality> where TChirality : struct, IChirality
        {
            public static void BalanceLeft(ref TNode node)
            {
                var chirality = default(TChirality);
                ref var left = ref chirality.Left(node);
                ref var right = ref chirality.Right(node);
                if (IsBalanced(left, right)) { return; }

                ref var rightsLeft = ref chirality.Left(right);
                var rightsRight = chirality.Right(right);
                TNode newRoot;
                if (NeedsSingleRotation(rightsLeft, rightsRight))
                {
                    newRoot = right;
                    ref var newRootLeft = ref chirality.Left(newRoot);
                    right = newRootLeft;
                    node.RecalculateCount();
                    newRootLeft = node;
                }
                else // double rotation
                {
                    newRoot = rightsLeft;
                    rightsLeft = chirality.Right(newRoot);
                    right.RecalculateCount();
                    chirality.Right(newRoot) = right;
                    ref var newRootLeft = ref chirality.Left(newRoot);
                    right = newRootLeft;
                    node.RecalculateCount();
                    newRootLeft = node;
                }

                newRoot.RecalculateCount();
                node = newRoot;
            }

            // 2 parameters: delta=3 and gamma=2 per https://yoichihirai.com/bst.pdf
            // we don't need to rotate at all if delta * (size left + 1) >= (size right + 1)
            // we can do a single rotation if (size rightsLeft + 1) < gamma * (size rightsRight + 1)

            private static bool IsBalanced(TNode left, TNode right)
            {
                // 3 * (leftCount + 1) >= rightCount + 1
                // <=> (3 * leftCount) + 3 >= rightCount + 1
                // <=> (3 * leftCount) + 2 >= rightCount

                // note: uint cast prevents overflow
                // note: right won't be null if we're balancing left
                return (3 * (uint)(left?.Count ?? 0)) + 2 >= (uint)right.Count;
            }

            private static bool NeedsSingleRotation(TNode rightsLeft, TNode rightsRight)
            {
                // rlCount + 1 < 2 * (rrCount + 1)
                // <=> rlCount < (2 * rrCount) + 1

                // note: uint cast prevents overflow
                var rightsRightCount = (uint)(rightsRight?.Count ?? 0);
                return (uint)(rightsLeft?.Count ?? 0) < (2 * (uint)(rightsRight?.Count ?? 0)) + 1;
            }
        }

        private interface IChirality
        {
            ref TNode Left(TNode node);
            ref TNode Right(TNode node);
        }

        private struct NormalChirality : IChirality
        {
            public ref TNode Left(TNode node) => ref node.Left;
            public ref TNode Right(TNode node) => ref node.Right;
        }

        private struct ReverseChirality : IChirality
        {
            public ref TNode Left(TNode node) => ref node.Right;
            public ref TNode Right(TNode node) => ref node.Left;
        }
    }
}
