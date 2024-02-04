using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NativeCollection.UnsafeType
{
    public unsafe partial struct SortedSet<T>
{
    internal enum NodeColor : byte
    {
        Black,
        Red
    }

    internal enum TreeRotation : byte
    {
        Left,
        LeftRight,
        Right,
        RightLeft
    }
    internal struct Node : IEquatable<Node>, IPool
    {
        public Node* Left;

        public Node* Right;
        
        public NodeColor Color;
        
        public T Item;
        
        public Node* Self
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (Node*)Unsafe.AsPointer(ref this); }
        }

        public bool IsBlack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Color == NodeColor.Black; }
        }

        public bool IsRed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Color == NodeColor.Red; }
        }

        public bool Is2Node
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right); }
        }

        public bool Is4Node
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return IsNonNullRed(Left) && IsNonNullRed(Right);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ColorBlack()
        {
            Color = NodeColor.Black;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ColorRed()
        {
            Color = NodeColor.Red;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNonNullBlack(Node* node)
        {
            return node != null && node->IsBlack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNonNullRed(Node* node)
        {
            return node != null && node->IsRed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrBlack(Node* node)
        {
            return node == null || node->IsBlack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Node* Create(in T item, NodeColor nodeColor)
        {
            var node = (Node*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<Node>());
            node->Item = item;
            node->Color = nodeColor;
            node->Left = null;
            node->Right = null;
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Node* AllocFromMemoryPool(in T item, NodeColor nodeColor, FixedSizeMemoryPool* memoryPool)
        {
            Node* node = (Node*)memoryPool->Alloc();
            node->Item = item;
            node->Color = nodeColor;
            node->Left = null;
            node->Right = null;
            return node;
        }

        public struct NodeSourceTarget : IEquatable<NodeSourceTarget>
        {
            public Node* Source;
            public Node* Target;

            public NodeSourceTarget(Node* source, Node* target)
            {
                Source = source;
                Target = target;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(NodeSourceTarget other)
            {
                return Source == other.Source && Target == other.Target;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
            {
                return HashCode.Combine(unchecked((int)(long)Source), unchecked((int)(long)Target));
            }
        }

        public Node* DeepClone(int count)
        {
#if DEBUG
            Debug.Assert(count == GetCount());
#endif
            var newRoot = ShallowClone();

            var pendingNodes = UnsafeType.Stack<NodeSourceTarget>.Create(2 * Log2(count) + 2);
            pendingNodes->Push(new NodeSourceTarget(Self, newRoot));

            while (pendingNodes->TryPop(out var next))
            {
                Node* clonedNode;

                var left = next.Source->Left;
                var right = next.Source->Right;
                if (left != null)
                {
                    clonedNode = left->ShallowClone();
                    next.Target->Left = clonedNode;
                    pendingNodes->Push(new NodeSourceTarget(left, clonedNode));
                }

                if (right != null)
                {
                    clonedNode = right->ShallowClone();
                    next.Target->Right = clonedNode;
                    pendingNodes->Push(new NodeSourceTarget(right, clonedNode));
                }
            }

            pendingNodes->Dispose();
            NativeMemoryHelper.Free(pendingNodes);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.Stack<NodeSourceTarget>>());
            return newRoot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TreeRotation GetRotation(Node* current, Node* sibling)
        {
            Debug.Assert(IsNonNullRed(sibling->Left) || IsNonNullRed(sibling->Right));
#if DEBUG
            Debug.Assert(HasChildren(current, sibling));
#endif
            var currentIsLeftChild = Left == current;
            return IsNonNullRed(sibling->Left) ? currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right :
                currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* GetSibling(Node* node)
        {
            Debug.Assert(node != null);
            Debug.Assert((node == Left) ^ (node == Right));

            return node == Left ? Right : Left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* ShallowClone()
        {
            return Create(Item, Color);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Split4Node()
        {
            Debug.Assert(Left != null);
            Debug.Assert(Right != null);

            ColorRed();
            Left->ColorBlack();
            Right->ColorBlack();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* Rotate(TreeRotation rotation)
        {
            Node* removeRed;
            switch (rotation)
            {
                case TreeRotation.Right:
                    removeRed = Left == null ? Left : Left->Left;
                    Debug.Assert(removeRed->IsRed);
                    removeRed->ColorBlack();
                    return RotateRight();
                case TreeRotation.Left:
                    removeRed = Right == null ? Right : Right->Right!;
                    Debug.Assert(removeRed->IsRed);
                    removeRed->ColorBlack();
                    return RotateLeft();
                case TreeRotation.RightLeft:
                    Debug.Assert(Right->Left->IsRed);
                    return RotateRightLeft();
                case TreeRotation.LeftRight:
                    Debug.Assert(Left->Right->IsRed);
                    return RotateLeftRight();
                default:
                    Debug.Fail($"{nameof(rotation)}: {rotation} is not a defined {nameof(TreeRotation)} value.");
                    return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* RotateLeft()
        {
            var child = Right;
            Right = child->Left;
            child->Left = Self;
            return child;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* RotateLeftRight()
        {
            var child = Left;
            var grandChild = child->Right!;

            Left = grandChild->Right;
            grandChild->Right = Self;
            child->Right = grandChild->Left;
            grandChild->Left = child;
            return grandChild;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* RotateRight()
        {
            var child = Left;
            Left = child->Right;
            child->Right = Self;
            return child;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node* RotateRightLeft()
        {
            var child = Right;
            var grandChild = child->Left;

            Right = grandChild->Left;
            grandChild->Left = Self;
            child->Left = grandChild->Right;
            grandChild->Right = child;
            return grandChild;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Merge2Nodes()
        {
            Debug.Assert(IsRed);
            Debug.Assert(Left->Is2Node);
            Debug.Assert(Right->Is2Node);

            // Combine two 2-nodes into a 4-node.
            ColorBlack();
            Left->ColorRed();
            Right->ColorRed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReplaceChild(Node* child, Node* newChild)
        {
#if DEBUG
            Debug.Assert(HasChild(child));
#endif

            if (Left == child)
                Left = newChild;
            else
                Right = newChild;
        }


#if DEBUG
        private int GetCount()
        {
            var value = 1;
            if (Left != null) value += Left->GetCount();

            if (Right != null) value += Right->GetCount();
            return value;
        }

        private bool HasChild(Node* child)
        {
            return child == Left || child == Right;
        }

        private bool HasChildren(Node* child1, Node* child2)
        {
            Debug.Assert(child1 != child2);

            return (Left == child1 && Right == child2)
                   || (Left == child2 && Right == child1);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Node other)
        {
            return (Item).Equals(other.Item) && Self == other.Self && Color == other.Color && Left == other.Left &&
                   Right == other.Right;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(Item, unchecked((int)(long)Self), (int)Color, unchecked((int)(long)Left),
                unchecked((int)(long)Right));
        }

        public void Dispose()
        {
            
        }

        public void OnReturnToPool()
        {
            
        }

        public void OnGetFromPool()
        {
            
        }
    }
}
}



