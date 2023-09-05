using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace NativeCollection.UnsafeType
{
    public unsafe partial struct SortedSet<T> : ICollection<T>, IDisposable where T : unmanaged, IEquatable<T>,IComparable<T>
{
    private SortedSet<T>* _self;
    private int _count;
    private Node* _root;
    private FixedSizeMemoryPool* _nodeMemoryPool;
    private NativeStackPool<Stack<IntPtr>>* _stackPool;
    private int _version;
    private const int _defaultNodePoolBlockSize = 64;
    public static SortedSet<T>* Create(int nodePoolBlockSize = _defaultNodePoolBlockSize)
    {
        var sortedSet = (SortedSet<T>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<SortedSet<T>>());
        sortedSet->_self = sortedSet;
        sortedSet->_root = null;
        sortedSet->_count = 0;
        sortedSet->_version = 0;
        sortedSet->_stackPool = NativeStackPool<Stack<IntPtr>>.Create(2);
        sortedSet->_nodeMemoryPool = FixedSizeMemoryPool.Create(nodePoolBlockSize, Unsafe.SizeOf<Node>());
        return sortedSet;
    }

    public T? Min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return MinInternal;
        }
    }

    internal T? MinInternal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_root == null) return default;

            var current = _root;
            while (current->Left != null) current = current->Left;

            return current->Item;
        }
    }

    public T? Max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return MaxInternal; }
    }

    internal T? MaxInternal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_root == null) return default;

            var current = _root;
            while (current->Right != null) current = current->Right;

            return current->Item;
        }
    }


    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            VersionCheck(true);
            return _count;
        }
    }

    bool ICollection<T>.IsReadOnly => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int index)
    {
        CopyTo(array, index, Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        return DoRemove(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRef(in T item)
    {
        return DoRemove(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    public void Clear()
    {
        using var enumerator = GetEnumerator();
        do
        {
            if (enumerator.CurrentPointer != null)
            {
                _nodeMemoryPool->Free(enumerator.CurrentPointer);
                
            }
        } while (enumerator.MoveNext());
        
        _root = null;
        _count = 0;
        ++_version;
        _stackPool->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        return FindNode(item) != null;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsRef(in T item)
    {
        return FindNode(item) != null;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        Clear();
        if (_stackPool!=null)
        {
            _stackPool->Dispose();
            NativeMemoryHelper.Free(_stackPool);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<NativeStackPool<Stack<IntPtr>>>());
        }

        if (_nodeMemoryPool!=null)
        {
            _nodeMemoryPool->Dispose();
            _nodeMemoryPool = null;
        }
        _version = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array)
    {
        CopyTo(array, 0, Count);
    }

    public void CopyTo(T[] array, int index, int count)
    {
        //ArgumentNullException.ThrowIfNull(array);

        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, "ArgumentOutOfRange_NeedNonNegNum");

        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "ArgumentOutOfRange_NeedNonNegNum");

        if (count > array.Length - index) throw new ArgumentException("Arg_ArrayPlusOffTooSmall");

        count += index; // Make `count` the upper bound.

        InOrderTreeWalk(node =>
        {
            if (index >= count) return false;

            array[index++] = node->Item;
            return true;
        });
    }

    /// <summary>
    ///     Does an in-order tree walk and calls the delegate for each node.
    /// </summary>
    /// <param name="action">
    ///     The delegate to invoke on each node.
    ///     If the delegate returns <c>false</c>, the walk is stopped.
    /// </param>
    /// <returns><c>true</c> if the entire tree has been walked; otherwise, <c>false</c>.</returns>
    internal bool InOrderTreeWalk(TreeWalkPredicate action)
    {
        if (_root == null) return true;

        // The maximum height of a red-black tree is 2 * log2(n+1).
        // See page 264 of "Introduction to algorithms" by Thomas H. Cormen
        // Note: It's not strictly necessary to provide the stack capacity, but we don't
        // want the stack to unnecessarily allocate arrays as it grows.

        var stack = UnsafeType.Stack<IntPtr>.Create(2 * Log2(Count + 1));
        var current = _root;

        while (current != null)
        {
            stack->Push((IntPtr)current);
            current = current->Left;
        }

        while (stack->Count != 0)
        {
            current = (Node*)stack->Pop();
            if (!action(current)) return false;

            var node = current->Right;
            while (node != null)
            {
                stack->Push((IntPtr)node);
                node = node->Left;
            }
        }
        stack->Dispose();
        NativeMemoryHelper.Free(stack);
        NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.Stack<IntPtr>>());
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void VersionCheck(bool updateCount = false)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int TotalCount()
    {
        return Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsWithinRange(in T item)
    {
        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        return AddIfNotPresent(item);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AddRef(in T item)
    {
        return AddIfNotPresent(item);
    }

    internal bool AddIfNotPresent(in T item)
    {
        if (_root == null)
        {
            // The tree is empty and this is the first item.
            _root = Node.AllocFromMemoryPool(item, NodeColor.Black,_nodeMemoryPool);
            _count = 1;
            _version++;
            return true;
        }

        // Search for a node at bottom to insert the new node.
        // If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
        // We split 4-nodes along the search path.
        var current = _root;
        Node* parent = null;
        Node* grandParent = null;
        Node* greatGrandParent = null;

        // Even if we don't actually add to the set, we may be altering its structure (by doing rotations and such).
        // So update `_version` to disable any instances of Enumerator/TreeSubSet from working on it.
        _version++;

        var order = 0;
        while (current != null)
        {
            order = item.CompareTo(current->Item);
            if (order == 0)
            {
                // We could have changed root node to red during the search process.
                // We need to set it to black before we return.
                _root->ColorBlack();
                return false;
            }

            // Split a 4-node into two 2-nodes.
            if (current->Is4Node)
            {
                current->Split4Node();
                // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                if (Node.IsNonNullRed(parent)) InsertionBalance(current, parent, grandParent, greatGrandParent);
            }

            greatGrandParent = grandParent;
            grandParent = parent;
            parent = current;
            current = order < 0 ? current->Left : current->Right;
        }

        Debug.Assert(parent != null);
        // We're ready to insert the new node.
        var node = Node.AllocFromMemoryPool(item, NodeColor.Red,_nodeMemoryPool);
        if (order > 0)
            parent->Right = node;
        else
            parent->Left = node;

        // The new node will be red, so we will need to adjust colors if its parent is also red.
        if (parent->IsRed) InsertionBalance(node, parent, grandParent, greatGrandParent);

        // The root node is always black.
        _root->ColorBlack();
        ++_count;
        return true;
    }

    internal bool DoRemove(in T item)
    {
        if (_root == null) return false;

        // Search for a node and then find its successor.
        // Then copy the item from the successor to the matching node, and delete the successor.
        // If a node doesn't have a successor, we can replace it with its left child (if not empty),
        // or delete the matching node.
        //
        // In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
        // Following code will make sure the node on the path is not a 2-node.

        // Even if we don't actually remove from the set, we may be altering its structure (by doing rotations
        // and such). So update our version to disable any enumerators/subsets working on it.
        _version++;

        var current = _root;
        Node* parent = null;
        Node* grandParent = null;
        Node* match = null;
        Node* parentOfMatch = null;
        var foundMatch = false;
        while (current != null)
        {
            if (current->Is2Node)
            {
                // Fix up 2-node
                if (parent == null)
                {
                    // `current` is the root. Mark it red.
                    current->ColorRed();
                }
                else
                {
                    var sibling = parent->GetSibling(current);
                    if (sibling->IsRed)
                    {
                        // If parent is a 3-node, flip the orientation of the red link.
                        // We can achieve this by a single rotation.
                        // This case is converted to one of the other cases below.
                        Debug.Assert(parent->IsBlack);
                        if (parent->Right == sibling)
                            parent->RotateLeft();
                        else
                            parent->RotateRight();

                        parent->ColorRed();
                        sibling->ColorBlack(); // The red parent can't have black children.
                        // `sibling` becomes the child of `grandParent` or `root` after rotation. Update the link from that node.
                        ReplaceChildOrRoot(grandParent, parent, sibling);
                        // `sibling` will become the grandparent of `current`.
                        grandParent = sibling;
                        if (parent == match) parentOfMatch = sibling;

                        sibling = parent->GetSibling(current);
                    }

                    Debug.Assert(Node.IsNonNullBlack(sibling));

                    if (sibling->Is2Node)
                    {
                        parent->Merge2Nodes();
                    }
                    else
                    {
                        // `current` is a 2-node and `sibling` is either a 3-node or a 4-node.
                        // We can change the color of `current` to red by some rotation.
                        var newGrandParent = parent->Rotate(parent->GetRotation(current, sibling))!;

                        newGrandParent->Color = parent->Color;
                        parent->ColorBlack();
                        current->ColorRed();

                        ReplaceChildOrRoot(grandParent, parent, newGrandParent);
                        if (parent == match) parentOfMatch = newGrandParent;
                    }
                }
            }

            // We don't need to compare after we find the match.
            var order = foundMatch ? -1 : item.CompareTo(current->Item);
            if (order == 0)
            {
                // Save the matching node.
                foundMatch = true;
                match = current;
                parentOfMatch = parent;
            }

            grandParent = parent;
            parent = current;
            // If we found a match, continue the search in the right sub-tree.
            current = order < 0 ? current->Left : current->Right;
        }

        // Move successor to the matching node position and replace links.
        if (match != null)
        {
            ReplaceNode(match, parentOfMatch!, parent!, grandParent!);
            --_count;
            //_nodePool->Return(match);
            _nodeMemoryPool->Free(match);
        }

        if (_root != null) _root->ColorBlack();


        return foundMatch;
    }

    // After calling InsertionBalance, we need to make sure `current` and `parent` are up-to-date.
    // It doesn't matter if we keep `grandParent` and `greatGrandParent` up-to-date, because we won't
    // need to split again in the next node.
    // By the time we need to split again, everything will be correctly set.
    private void InsertionBalance(Node* current, Node* parent, Node* grandParent, Node* greatGrandParent)
    {
        Debug.Assert(parent != null);
        Debug.Assert(grandParent != null);

        var parentIsOnRight = grandParent->Right == parent;
        var currentIsOnRight = parent->Right == current;

        Node* newChildOfGreatGrandParent;
        if (parentIsOnRight == currentIsOnRight)
        {
            // Same orientation, single rotation
            newChildOfGreatGrandParent = currentIsOnRight ? grandParent->RotateLeft() : grandParent->RotateRight();
        }
        else
        {
            // Different orientation, double rotation
            newChildOfGreatGrandParent =
                currentIsOnRight ? grandParent->RotateLeftRight() : grandParent->RotateRightLeft();
            // Current node now becomes the child of `greatGrandParent`
            parent = greatGrandParent;
        }

        // `grandParent` will become a child of either `parent` of `current`.
        grandParent->ColorRed();
        newChildOfGreatGrandParent->ColorBlack();

        ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
    }

    /// <summary>
    ///     Replaces the child of a parent node, or replaces the root if the parent is <c>null</c>.
    /// </summary>
    /// <param name="parent">The (possibly <c>null</c>) parent.</param>
    /// <param name="child">The child node to replace.</param>
    /// <param name="newChild">The node to replace <paramref name="child" /> with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReplaceChildOrRoot(Node* parent, Node* child, Node* newChild)
    {
        if (parent != null)
            parent->ReplaceChild(child, newChild);
        else
            _root = newChild;
    }

    /// <summary>
    ///     Replaces the matching node with its successor.
    /// </summary>
    private void ReplaceNode(Node* match, Node* parentOfMatch, Node* successor, Node* parentOfSuccessor)
    {
        Debug.Assert(match != null);

        if (successor == match)
        {
            // This node has no successor. This can only happen if the right child of the match is null.
            Debug.Assert(match->Right == null);
            successor = match->Left!;
        }
        else
        {
            Debug.Assert(parentOfSuccessor != null);
            Debug.Assert(successor->Left == null);
            Debug.Assert((successor->Right == null && successor->IsRed) ||
                         (successor->Right!->IsRed && successor->IsBlack));

            if (successor->Right != null) successor->Right->ColorBlack();

            if (parentOfSuccessor != match)
            {
                // Detach the successor from its parent and set its right child.
                parentOfSuccessor->Left = successor->Right;
                successor->Right = match->Right;
            }

            successor->Left = match->Left;
        }

        if (successor != null) successor->Color = match->Color;

        ReplaceChildOrRoot(parentOfMatch, match, successor!);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Node* FindNode(in T item)
    {
        var current = _root;
        while (current != null)
        {
            var order = item.CompareTo(current->Item);
            if (order == 0) return current;

            current = order < 0 ? current->Left : current->Right;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UpdateVersion()
    {
        ++_version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Log2(int value)
    {
#if NET6_0_OR_GREATER
        return BitOperations.Log2((uint)value);
#else
        int num = 0;
        for (; value > 0; value >>= 1)
            ++num;
        return num;
#endif
    }

    
    
    public Enumerator GetEnumerator()
    {
        return new Enumerator(_self);
    }

    internal delegate bool TreeWalkPredicate(Node* node);

    public struct Enumerator : IEnumerator<T>
    {
        private readonly SortedSet<T>* _tree;
        private readonly int _version;

        private readonly UnsafeType.Stack<IntPtr>* _stack;
        private readonly bool _reverse;

        internal Enumerator(SortedSet<T>* set)
            : this(set, false)
        {
        }

        internal Enumerator(SortedSet<T>* set, bool reverse)
        {
            _tree = set;
            set->VersionCheck();
            _version = set->_version;

            // 2 log(n + 1) is the maximum height.

            _stack = set->_stackPool->Alloc();
            if (_stack==null)
            {
                _stack = UnsafeType.Stack<IntPtr>.Create(2 * Log2(set->TotalCount() + 1));
            }
            CurrentPointer = null;
            _reverse = reverse;
            Initialize();
            
        }

        private void Initialize()
        {
            CurrentPointer = null;
            var node = _tree->_root;
            while (node != null)
            {
                var next = _reverse ? node->Right : node->Left;
                var other = _reverse ? node->Left : node->Right;
                if (_tree->IsWithinRange(node->Item))
                {
                    _stack->Push((IntPtr)node);
                    node = next;
                }
                else if (next == null || !_tree->IsWithinRange(next->Item))
                {
                    node = other;
                }
                else
                {
                    node = next;
                }
            }
        }

        public bool MoveNext()
        {
            // Make sure that the underlying subset has not been changed since
            //_tree->VersionCheck();

            if (_version != _tree->_version)
            {
                ThrowHelper.SortedSetVersionChanged();
            }

            if (_stack->Count == 0)
            {
                CurrentPointer = null;
                return false;
            }

            CurrentPointer = (Node*)_stack->Pop();
            var node = _reverse ? CurrentPointer->Left : CurrentPointer->Right;
            while (node != null)
            {
                var next = _reverse ? node->Right : node->Left;
                var other = _reverse ? node->Left : node->Right;
                if (_tree->IsWithinRange(node->Item))
                {
                    _stack->Push((IntPtr)node);
                    node = next;
                }
                else if (other == null || !_tree->IsWithinRange(other->Item))
                {
                    node = next;
                }
                else
                {
                    node = other;
                }
            }

            return true;
        }


        public void Dispose()
        {
            //Console.WriteLine("Enumerator Dispose");
            // _stack->Dispose();
            //
            // NativeMemoryHelper.Free(_stack);
            // GC.RemoveMemoryPressure(Unsafe.SizeOf<NativeCollection.Stack<IntPtr>>());
            _tree->_stackPool->Return(_stack);
        }

        public T Current
        {
            get
            {
                if (CurrentPointer != null) return CurrentPointer->Item;
                return default!; // Should only happen when accessing Current is undefined behavior
            }
        }

        internal Node* CurrentPointer { get; private set; }

        internal bool NotStartedOrEnded => CurrentPointer == null;

        internal void Reset()
        {
            if (_version != _tree->_version) throw new InvalidOperationException("_version != _tree.version");

            _stack->Clear();
            Initialize();
        }

        object IEnumerator.Current
        {
            get
            {
                if (CurrentPointer == null) throw new InvalidOperationException("_current == null");

                return CurrentPointer->Item;
            }
        }

        void IEnumerator.Reset()
        {
            Reset();
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var value in this) sb.Append($"{value} ");

        return sb.ToString();
    }
}
}


