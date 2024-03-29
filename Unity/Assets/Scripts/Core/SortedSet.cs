using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

#nullable disable
namespace ET
{
    // A binary search tree is a red-black tree if it satisfies the following red-black properties:
    // 1. Every node is either red or black
    // 2. Every leaf (nil node) is black
    // 3. If a node is red, the both its children are black
    // 4. Every simple path from a node to a descendant leaf contains the same number of black nodes
    //
    // The basic idea of a red-black tree is to represent 2-3-4 trees as standard BSTs but to add one extra bit of information
    // per node to encode 3-nodes and 4-nodes.
    // 4-nodes will be represented as:   B
    //                                 R   R
    //
    // 3 -node will be represented as:   B     or     B
    //                                 R   B        B   R
    //
    // For a detailed description of the algorithm, take a look at "Algorithms" by Robert Sedgewick.

    public enum NodeColor : byte
    {
        Black,
        Red
    }

    internal delegate bool TreeWalkPredicate<T>(SortedSet<T>.Node node);

    public enum TreeRotation : byte
    {
        Left,
        LeftRight,
        Right,
        RightLeft
    }
    
    internal sealed class ICollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public ICollectionDebugView(ICollection<T> collection)
        {
            ArgumentNullException.ThrowIfNull(collection);

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
    
    /// <summary>
    /// Provides a readonly abstraction of a set.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        /// <summary>
        /// Determines if the set contains a specific item
        /// </summary>
        /// <param name="item">The item to check if the set contains.</param>
        /// <returns><see langword="true" /> if found; otherwise <see langword="false" />.</returns>
        bool Contains(T item);
        /// <summary>
        /// Determines whether the current set is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true" /> if the current set is a proper subset of other; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool IsProperSubsetOf(IEnumerable<T> other);
        /// <summary>
        /// Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true" /> if the collection is a proper superset of other; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool IsProperSupersetOf(IEnumerable<T> other);
        /// <summary>
        /// Determine whether the current set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true" /> if the current set is a subset of other; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool IsSubsetOf(IEnumerable<T> other);
        /// <summary>
        /// Determine whether the current set is a super set of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set</param>
        /// <returns><see langword="true" /> if the current set is a subset of other; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool IsSupersetOf(IEnumerable<T> other);
        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true" /> if the current set and other share at least one common element; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool Overlaps(IEnumerable<T> other);
        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true" /> if the current set is equal to other; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">other is <see langword="null" />.</exception>
        bool SetEquals(IEnumerable<T> other);
    }

    [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public partial class SortedSet<T> : ISet<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IReadOnlySet<T>, ISerializable, IDeserializationCallback
    {
        #region Local variables/constants

        private Node root;
        private IComparer<T> comparer = default!;
        private int count;
        private int version;
        
        public Stack<Node> Stack = new Stack<Node>(10);

        private SerializationInfo siInfo; // A temporary variable which we need during deserialization.

        private const string ComparerName = "Comparer"; // Do not rename (binary serialization)
        private const string CountName = "Count"; // Do not rename (binary serialization)
        private const string ItemsName = "Items"; // Do not rename (binary serialization)
        private const string VersionName = "Version"; // Do not rename (binary serialization)

        internal const int StackAllocThreshold = 100;

        #endregion

        #region Constructors

        public SortedSet()
        {
            comparer = Comparer<T>.Default;
        }

        public SortedSet(IComparer<T> comparer)
        {
            this.comparer = comparer ?? Comparer<T>.Default;
        }


        public SortedSet(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) { }

        public SortedSet(IEnumerable<T> collection, IComparer<T> comparer)
            : this(comparer)
        {
            ArgumentNullException.ThrowIfNull(collection);

            // These are explicit type checks in the mold of HashSet. It would have worked better with
            // something like an ISorted<T> interface. (We could make this work for SortedList.Keys, etc.)
            SortedSet<T> sortedSet = collection as SortedSet<T>;
            if (sortedSet != null && !(sortedSet is TreeSubSet) && HasEqualComparer(sortedSet))
            {
                if (sortedSet.Count > 0)
                {
                    Debug.Assert(sortedSet.root != null);
                    this.count = sortedSet.count;
                    root = sortedSet.root.DeepClone(this.count);
                }
                return;
            }

            int count;
            T[] elements = EnumerableHelpers.ToArray(collection, out count);
            if (count > 0)
            {
                // If `comparer` is null, sets it to Comparer<T>.Default. We checked for this condition in the IComparer<T> constructor.
                // Array.Sort handles null comparers, but we need this later when we use `comparer.Compare` directly.
                comparer = this.comparer;
                Array.Sort(elements, 0, count, comparer);

                // Overwrite duplicates while shifting the distinct elements towards
                // the front of the array.
                int index = 1;
                for (int i = 1; i < count; i++)
                {
                    if (comparer.Compare(elements[i], elements[i - 1]) != 0)
                    {
                        elements[index++] = elements[i];
                    }
                }

                count = index;
                root = ConstructRootFromSortedArray(elements, 0, count - 1, null);
                this.count = count;
            }
        }

        protected SortedSet(SerializationInfo info, StreamingContext context) => siInfo = info;

        #endregion

        #region Bulk operation helpers

        private void AddAllElements(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                if (!Contains(item))
                {
                    Add(item);
                }
            }
        }

        private void RemoveAllElements(IEnumerable<T> collection)
        {
            T min = Min;
            T max = Max;
            foreach (T item in collection)
            {
                if (!(comparer.Compare(item, min) < 0 || comparer.Compare(item, max) > 0) && Contains(item))
                {
                    Remove(item);
                }
            }
        }

        private bool ContainsAllElements(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                if (!Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Does an in-order tree walk and calls the delegate for each node.
        /// </summary>
        /// <param name="action">
        /// The delegate to invoke on each node.
        /// If the delegate returns <c>false</c>, the walk is stopped.
        /// </param>
        /// <returns><c>true</c> if the entire tree has been walked; otherwise, <c>false</c>.</returns>
        internal virtual bool InOrderTreeWalk(TreeWalkPredicate<T> action)
        {
            if (root == null)
            {
                return true;
            }

            // The maximum height of a red-black tree is 2 * log2(n+1).
            // See page 264 of "Introduction to algorithms" by Thomas H. Cormen
            // Note: It's not strictly necessary to provide the stack capacity, but we don't
            // want the stack to unnecessarily allocate arrays as it grows.

            var stack = new Stack<Node>(2 * (int)Log2(Count + 1));
            Node current = root;

            while (current != null)
            {
                stack.Push(current);
                current = current.Left;
            }

            while (stack.Count != 0)
            {
                current = stack.Pop();
                if (!action(current))
                {
                    return false;
                }

                Node node = current.Right;
                while (node != null)
                {
                    stack.Push(node);
                    node = node.Left;
                }
            }

            return true;
        }

        /// <summary>
        /// Does a left-to-right breadth-first tree walk and calls the delegate for each node.
        /// </summary>
        /// <param name="action">
        /// The delegate to invoke on each node.
        /// If the delegate returns <c>false</c>, the walk is stopped.
        /// </param>
        /// <returns><c>true</c> if the entire tree has been walked; otherwise, <c>false</c>.</returns>
        internal virtual bool BreadthFirstTreeWalk(TreeWalkPredicate<T> action)
        {
            if (root == null)
            {
                return true;
            }

            var processQueue = new Queue<Node>();
            processQueue.Enqueue(root);

            Node current;
            while (processQueue.Count != 0)
            {
                current = processQueue.Dequeue();
                if (!action(current))
                {
                    return false;
                }

                if (current.Left != null)
                {
                    processQueue.Enqueue(current.Left);
                }
                if (current.Right != null)
                {
                    processQueue.Enqueue(current.Right);
                }
            }

            return true;
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                VersionCheck(updateCount: true);
                return count;
            }
        }

        public IComparer<T> Comparer => comparer;

        bool ICollection<T>.IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        #endregion

        #region Subclass helpers

        // Virtual function for TreeSubSet, which may need to update its count.
        internal virtual void VersionCheck(bool updateCount = false) { }
        // Virtual function for TreeSubSet, which may need the count variable of the parent set.
        internal virtual int TotalCount() { return Count; }

        // Virtual function for TreeSubSet, which may need to do range checks.
        internal virtual bool IsWithinRange(T item) => true;

        #endregion

        #region ICollection<T> members

        public bool Add(T item) => AddIfNotPresent(item); // Hack so the implementation can be made virtual

        void ICollection<T>.Add(T item) => Add(item);

        internal virtual bool AddIfNotPresent(T item)
        {
            if (root == null)
            {
                // The tree is empty and this is the first item.
                root = new Node(item, NodeColor.Black);
                count = 1;
                version++;
                return true;
            }

            // Search for a node at bottom to insert the new node.
            // If we can guarantee the node we found is not a 4-node, it would be easy to do insertion.
            // We split 4-nodes along the search path.
            Node current = root;
            Node parent = null;
            Node grandParent = null;
            Node greatGrandParent = null;

            // Even if we don't actually add to the set, we may be altering its structure (by doing rotations and such).
            // So update `_version` to disable any instances of Enumerator/TreeSubSet from working on it.
            version++;

            int order = 0;
            while (current != null)
            {
                order = comparer.Compare(item, current.Item);
                if (order == 0)
                {
                    // We could have changed root node to red during the search process.
                    // We need to set it to black before we return.
                    root.ColorBlack();
                    return false;
                }

                // Split a 4-node into two 2-nodes.
                if (current.Is4Node)
                {
                    current.Split4Node();
                    // We could have introduced two consecutive red nodes after split. Fix that by rotation.
                    if (Node.IsNonNullRed(parent))
                    {
                        InsertionBalance(current, ref parent!, grandParent!, greatGrandParent!);
                    }
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = (order < 0) ? current.Left : current.Right;
            }

            Debug.Assert(parent != null);
            // We're ready to insert the new node.
            Node node = new Node(item, NodeColor.Red);
            if (order > 0)
            {
                parent.Right = node;
            }
            else
            {
                parent.Left = node;
            }

            // The new node will be red, so we will need to adjust colors if its parent is also red.
            if (parent.IsRed)
            {
                InsertionBalance(node, ref parent!, grandParent!, greatGrandParent!);
            }

            // The root node is always black.
            root.ColorBlack();
            ++count;
            return true;
        }

        public bool Remove(T item) => DoRemove(item); // Hack so the implementation can be made virtual

        internal virtual bool DoRemove(T item)
        {
            if (root == null)
            {
                return false;
            }

            // Search for a node and then find its successor.
            // Then copy the item from the successor to the matching node, and delete the successor.
            // If a node doesn't have a successor, we can replace it with its left child (if not empty),
            // or delete the matching node.
            //
            // In top-down implementation, it is important to make sure the node to be deleted is not a 2-node.
            // Following code will make sure the node on the path is not a 2-node.

            // Even if we don't actually remove from the set, we may be altering its structure (by doing rotations
            // and such). So update our version to disable any enumerators/subsets working on it.
            version++;

            Node current = root;
            Node parent = null;
            Node grandParent = null;
            Node match = null;
            Node parentOfMatch = null;
            bool foundMatch = false;
            while (current != null)
            {
                if (current.Is2Node)
                {
                    // Fix up 2-node
                    if (parent == null)
                    {
                        // `current` is the root. Mark it red.
                        current.ColorRed();
                    }
                    else
                    {
                        Node sibling = parent.GetSibling(current);
                        if (sibling.IsRed)
                        {
                            // If parent is a 3-node, flip the orientation of the red link.
                            // We can achieve this by a single rotation.
                            // This case is converted to one of the other cases below.
                            Debug.Assert(parent.IsBlack);
                            if (parent.Right == sibling)
                            {
                                parent.RotateLeft();
                            }
                            else
                            {
                                parent.RotateRight();
                            }

                            parent.ColorRed();
                            sibling.ColorBlack(); // The red parent can't have black children.
                            // `sibling` becomes the child of `grandParent` or `root` after rotation. Update the link from that node.
                            ReplaceChildOrRoot(grandParent, parent, sibling);
                            // `sibling` will become the grandparent of `current`.
                            grandParent = sibling;
                            if (parent == match)
                            {
                                parentOfMatch = sibling;
                            }

                            sibling = parent.GetSibling(current);
                        }

                        Debug.Assert(Node.IsNonNullBlack(sibling));

                        if (sibling.Is2Node)
                        {
                            parent.Merge2Nodes();
                        }
                        else
                        {
                            // `current` is a 2-node and `sibling` is either a 3-node or a 4-node.
                            // We can change the color of `current` to red by some rotation.
                            Node newGrandParent = parent.Rotate(parent.GetRotation(current, sibling))!;

                            newGrandParent.Color = parent.Color;
                            parent.ColorBlack();
                            current.ColorRed();

                            ReplaceChildOrRoot(grandParent, parent, newGrandParent);
                            if (parent == match)
                            {
                                parentOfMatch = newGrandParent;
                            }
                        }
                    }
                }

                // We don't need to compare after we find the match.
                int order = foundMatch ? -1 : comparer.Compare(item, current.Item);
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
                current = order < 0 ? current.Left : current.Right;
            }

            // Move successor to the matching node position and replace links.
            if (match != null)
            {
                ReplaceNode(match, parentOfMatch!, parent!, grandParent!);
                --count;
            }

            root?.ColorBlack();
            return foundMatch;
        }

        public virtual void Clear()
        {
            root = null;
            count = 0;
            ++version;
        }

        public virtual bool Contains(T item) => FindNode(item) != null;

        public void CopyTo(T[] array) => CopyTo(array, 0, Count);

        public void CopyTo(T[] array, int index) => CopyTo(array, index, Count);

        public void CopyTo(T[] array, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, SR.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (count > array.Length - index)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }

            count += index; // Make `count` the upper bound.

            InOrderTreeWalk(node =>
            {
                if (index >= count)
                {
                    return false;
                }

                array[index++] = node.Item;
                return true;
            });
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.Arg_RankMultiDimNotSupported, nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(SR.Arg_NonZeroLowerBound, nameof(array));
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_Index);
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }

            T[] tarray = array as T[];
            if (tarray != null)
            {
                CopyTo(tarray, index);
            }
            else
            {
                object[] objects = array as object[];
                if (objects == null)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }

                try
                {
                    InOrderTreeWalk(node =>
                    {
                        objects[index++] = node.Item;
                        return true;
                    });
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException(SR.Argument_IncompatibleArrayType, nameof(array));
                }
            }
        }

        #endregion

        #region IEnumerable<T> members

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        #endregion

        #region Tree-specific operations

        // After calling InsertionBalance, we need to make sure `current` and `parent` are up-to-date.
        // It doesn't matter if we keep `grandParent` and `greatGrandParent` up-to-date, because we won't
        // need to split again in the next node.
        // By the time we need to split again, everything will be correctly set.
        private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(grandParent != null);

            bool parentIsOnRight = grandParent.Right == parent;
            bool currentIsOnRight = parent.Right == current;

            Node newChildOfGreatGrandParent;
            if (parentIsOnRight == currentIsOnRight)
            {
                // Same orientation, single rotation
                newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeft() : grandParent.RotateRight();
            }
            else
            {
                // Different orientation, double rotation
                newChildOfGreatGrandParent = currentIsOnRight ? grandParent.RotateLeftRight() : grandParent.RotateRightLeft();
                // Current node now becomes the child of `greatGrandParent`
                parent = greatGrandParent;
            }

            // `grandParent` will become a child of either `parent` of `current`.
            grandParent.ColorRed();
            newChildOfGreatGrandParent.ColorBlack();

            ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
        }

        /// <summary>
        /// Replaces the child of a parent node, or replaces the root if the parent is <c>null</c>.
        /// </summary>
        /// <param name="parent">The (possibly <c>null</c>) parent.</param>
        /// <param name="child">The child node to replace.</param>
        /// <param name="newChild">The node to replace <paramref name="child"/> with.</param>
        private void ReplaceChildOrRoot(Node parent, Node child, Node newChild)
        {
            if (parent != null)
            {
                parent.ReplaceChild(child, newChild);
            }
            else
            {
                root = newChild;
            }
        }

        /// <summary>
        /// Replaces the matching node with its successor.
        /// </summary>
        private void ReplaceNode(Node match, Node parentOfMatch, Node successor, Node parentOfSuccessor)
        {
            Debug.Assert(match != null);

            if (successor == match)
            {
                // This node has no successor. This can only happen if the right child of the match is null.
                Debug.Assert(match.Right == null);
                successor = match.Left!;
            }
            else
            {
                Debug.Assert(parentOfSuccessor != null);
                Debug.Assert(successor.Left == null);
                Debug.Assert((successor.Right == null && successor.IsRed) || (successor.Right!.IsRed && successor.IsBlack));

                successor.Right?.ColorBlack();

                if (parentOfSuccessor != match)
                {
                    // Detach the successor from its parent and set its right child.
                    parentOfSuccessor.Left = successor.Right;
                    successor.Right = match.Right;
                }

                successor.Left = match.Left;
            }

            if (successor != null)
            {
                successor.Color = match.Color;
            }

            ReplaceChildOrRoot(parentOfMatch, match, successor!);
        }

        internal virtual Node FindNode(T item)
        {
            Node current = root;
            while (current != null)
            {
                int order = comparer.Compare(item, current.Item);
                if (order == 0)
                {
                    return current;
                }

                current = order < 0 ? current.Left : current.Right;
            }

            return null;
        }

        /// <summary>
        /// Searches for an item and returns its zero-based index in this set.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The item's zero-based index in this set, or -1 if it isn't found.</returns>
        /// <remarks>
        /// <para>
        /// This implementation is based off of http://en.wikipedia.org/wiki/Binary_Tree#Methods_for_storing_binary_trees.
        /// </para>
        /// <para>
        /// This method is used with the <see cref="BitHelper"/> class. Note that this implementation is
        /// completely different from <see cref="TreeSubSet"/>'s, and that the two should not be mixed.
        /// </para>
        /// </remarks>
        internal virtual int InternalIndexOf(T item)
        {
            Node current = root;
            int count = 0;
            while (current != null)
            {
                int order = comparer.Compare(item, current.Item);
                if (order == 0)
                {
                    return count;
                }

                current = order < 0 ? current.Left : current.Right;
                count = order < 0 ? (2 * count + 1) : (2 * count + 2);
            }

            return -1;
        }

        internal Node FindRange(T from, T to) => FindRange(from, to, lowerBoundActive: true, upperBoundActive: true);

        internal Node FindRange(T from, T to, bool lowerBoundActive, bool upperBoundActive)
        {
            Node current = root;
            while (current != null)
            {
                if (lowerBoundActive && comparer.Compare(from, current.Item) > 0)
                {
                    current = current.Right;
                }
                else
                {
                    if (upperBoundActive && comparer.Compare(to, current.Item) < 0)
                    {
                        current = current.Left;
                    }
                    else
                    {
                        return current;
                    }
                }
            }

            return null;
        }

        internal void UpdateVersion() => ++version;

        /// <summary>
        /// Returns an <see cref="IEqualityComparer{T}"/> object that can be used to create a collection that contains individual sets.
        /// </summary>
        public static IEqualityComparer<SortedSet<T>> CreateSetComparer() => CreateSetComparer(memberEqualityComparer: null);

        /// <summary>
        /// Returns an <see cref="IEqualityComparer{T}"/> object, according to a specified comparer, that can be used to create a collection that contains individual sets.
        /// </summary>
        public static IEqualityComparer<SortedSet<T>> CreateSetComparer(IEqualityComparer<T> memberEqualityComparer)
        {
            return new SortedSetEqualityComparer<T>(memberEqualityComparer);
        }

        /// <summary>
        /// Decides whether two sets have equal contents, using a fallback comparer if the sets do not have equivalent equality comparers.
        /// </summary>
        /// <param name="set1">The first set.</param>
        /// <param name="set2">The second set.</param>
        /// <param name="comparer">The fallback comparer to use if the sets do not have equal comparers.</param>
        /// <returns><c>true</c> if the sets have equal contents; otherwise, <c>false</c>.</returns>
        internal static bool SortedSetEquals(SortedSet<T> set1, SortedSet<T> set2, IComparer<T> comparer)
        {
            if (set1 == null)
            {
                return set2 == null;
            }

            if (set2 == null)
            {
                Debug.Assert(set1 != null);
                return false;
            }

            if (set1.HasEqualComparer(set2))
            {
                return set1.Count == set2.Count && set1.SetEquals(set2);
            }

            bool found;
            foreach (T item1 in set1)
            {
                found = false;
                foreach (T item2 in set2)
                {
                    if (comparer.Compare(item1, item2) == 0)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two <see cref="SortedSet{T}"/> instances have the same comparer.
        /// </summary>
        /// <param name="other">The other <see cref="SortedSet{T}"/>.</param>
        /// <returns>A value indicating whether both sets have the same comparer.</returns>
        private bool HasEqualComparer(SortedSet<T> other)
        {
            // Commonly, both comparers will be the default comparer (and reference-equal). Avoid a virtual method call to Equals() in that case.
            return Comparer == other.Comparer || Comparer.Equals(other.Comparer);
        }

        #endregion

        #region ISet members

        public void UnionWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            SortedSet<T> asSorted = other as SortedSet<T>;
            TreeSubSet treeSubset = this as TreeSubSet;

            if (treeSubset != null)
                VersionCheck();

            if (asSorted != null && treeSubset == null && Count == 0)
            {
                SortedSet<T> dummy = new SortedSet<T>(asSorted, comparer);
                root = dummy.root;
                count = dummy.count;
                version++;
                return;
            }

            // This actually hurts if N is much greater than M. The / 2 is arbitrary.
            if (asSorted != null && treeSubset == null && HasEqualComparer(asSorted) && (asSorted.Count > this.Count / 2))
            {
                // First do a merge sort to an array.
                T[] merged = new T[asSorted.Count + this.Count];
                int c = 0;
                Enumerator mine = this.GetEnumerator();
                Enumerator theirs = asSorted.GetEnumerator();
                bool mineEnded = !mine.MoveNext(), theirsEnded = !theirs.MoveNext();
                while (!mineEnded && !theirsEnded)
                {
                    int comp = Comparer.Compare(mine.Current, theirs.Current);
                    if (comp < 0)
                    {
                        merged[c++] = mine.Current;
                        mineEnded = !mine.MoveNext();
                    }
                    else if (comp == 0)
                    {
                        merged[c++] = theirs.Current;
                        mineEnded = !mine.MoveNext();
                        theirsEnded = !theirs.MoveNext();
                    }
                    else
                    {
                        merged[c++] = theirs.Current;
                        theirsEnded = !theirs.MoveNext();
                    }
                }

                if (!mineEnded || !theirsEnded)
                {
                    Enumerator remaining = (mineEnded ? theirs : mine);
                    do
                    {
                        merged[c++] = remaining.Current;
                    }
                    while (remaining.MoveNext());
                }

                // now merged has all c elements

                // safe to gc the root, we  have all the elements
                root = null;

                root = ConstructRootFromSortedArray(merged, 0, c - 1, null);
                count = c;
                version++;
            }
            else
            {
                AddAllElements(other);
            }
        }

        private static Node ConstructRootFromSortedArray(T[] arr, int startIndex, int endIndex, Node redNode)
        {
            // You're given a sorted array... say 1 2 3 4 5 6
            // There are 2 cases:
            // -  If there are odd # of elements, pick the middle element (in this case 4), and compute
            //    its left and right branches
            // -  If there are even # of elements, pick the left middle element, save the right middle element
            //    and call the function on the rest
            //    1 2 3 4 5 6 -> pick 3, save 4 and call the fn on 1,2 and 5,6
            //    now add 4 as a red node to the lowest element on the right branch
            //             3                       3
            //         1       5       ->     1        5
            //           2       6             2     4   6
            //    As we're adding to the leftmost of the right branch, nesting will not hurt the red-black properties
            //    Leaf nodes are red if they have no sibling (if there are 2 nodes or if a node trickles
            //    down to the bottom

            // This is done recursively because the iterative way to do this ends up wasting more space than it saves in stack frames
            // Only some base cases are handled below.

            int size = endIndex - startIndex + 1;
            Node root;

            switch (size)
            {
                case 0:
                    return null;
                case 1:
                    root = new Node(arr[startIndex], NodeColor.Black);
                    if (redNode != null)
                    {
                        root.Left = redNode;
                    }
                    break;
                case 2:
                    root = new Node(arr[startIndex], NodeColor.Black);
                    root.Right = new Node(arr[endIndex], NodeColor.Black);
                    root.Right.ColorRed();
                    if (redNode != null)
                    {
                        root.Left = redNode;
                    }
                    break;
                case 3:
                    root = new Node(arr[startIndex + 1], NodeColor.Black);
                    root.Left = new Node(arr[startIndex], NodeColor.Black);
                    root.Right = new Node(arr[endIndex], NodeColor.Black);
                    if (redNode != null)
                    {
                        root.Left.Left = redNode;
                    }
                    break;
                default:
                    int midpt = ((startIndex + endIndex) / 2);
                    root = new Node(arr[midpt], NodeColor.Black);
                    root.Left = ConstructRootFromSortedArray(arr, startIndex, midpt - 1, redNode);
                    root.Right = size % 2 == 0 ?
                        ConstructRootFromSortedArray(arr, midpt + 2, endIndex, new Node(arr[midpt + 1], NodeColor.Red)) :
                        ConstructRootFromSortedArray(arr, midpt + 1, endIndex, null);
                    break;

            }

            return root;
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Count == 0)
                return;

            if (other == this)
                return;

            // HashSet<T> optimizations can't be done until equality comparers and comparers are related

            // Technically, this would work as well with an ISorted<T>
            SortedSet<T> asSorted = other as SortedSet<T>;
            TreeSubSet treeSubset = this as TreeSubSet;

            if (treeSubset != null)
                VersionCheck();

            if (asSorted != null && treeSubset == null && HasEqualComparer(asSorted))
            {
                // First do a merge sort to an array.
                T[] merged = new T[this.Count];
                int c = 0;
                Enumerator mine = this.GetEnumerator();
                Enumerator theirs = asSorted.GetEnumerator();
                bool mineEnded = !mine.MoveNext(), theirsEnded = !theirs.MoveNext();
                T max = Max;

                while (!mineEnded && !theirsEnded && Comparer.Compare(theirs.Current, max) <= 0)
                {
                    int comp = Comparer.Compare(mine.Current, theirs.Current);
                    if (comp < 0)
                    {
                        mineEnded = !mine.MoveNext();
                    }
                    else if (comp == 0)
                    {
                        merged[c++] = theirs.Current;
                        mineEnded = !mine.MoveNext();
                        theirsEnded = !theirs.MoveNext();
                    }
                    else
                    {
                        theirsEnded = !theirs.MoveNext();
                    }
                }

                // now merged has all c elements

                // safe to gc the root, we  have all the elements
                root = null;

                root = ConstructRootFromSortedArray(merged, 0, c - 1, null);
                count = c;
                version++;
            }
            else
            {
                IntersectWithEnumerable(other);
            }
        }

        internal virtual void IntersectWithEnumerable(IEnumerable<T> other)
        {
            // TODO: Perhaps a more space-conservative way to do this
            List<T> toSave = new List<T>(Count);
            foreach (T item in other)
            {
                if (Contains(item))
                {
                    toSave.Add(item);
                }
            }

            Clear();
            foreach (T item in toSave)
            {
                Add(item);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (count == 0)
                return;

            if (other == this)
            {
                Clear();
                return;
            }

            SortedSet<T> asSorted = other as SortedSet<T>;

            if (asSorted != null && HasEqualComparer(asSorted))
            {
                // Outside range, no point in doing anything
                if (comparer.Compare(asSorted.Max, Min) >= 0 && comparer.Compare(asSorted.Min, Max) <= 0)
                {
                    T min = Min;
                    T max = Max;
                    foreach (T item in other)
                    {
                        if (comparer.Compare(item, min) < 0)
                            continue;
                        if (comparer.Compare(item, max) > 0)
                            break;
                        Remove(item);
                    }
                }
            }
            else
            {
                RemoveAllElements(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Count == 0)
            {
                UnionWith(other);
                return;
            }

            if (other == this)
            {
                Clear();
                return;
            }

            SortedSet<T> asSorted = other as SortedSet<T>;

            if (asSorted != null && HasEqualComparer(asSorted))
            {
                SymmetricExceptWithSameComparer(asSorted);
            }
            else
            {
                int length;
                T[] elements = EnumerableHelpers.ToArray(other, out length);
                Array.Sort(elements, 0, length, Comparer);
                SymmetricExceptWithSameComparer(elements, length);
            }
        }

        private void SymmetricExceptWithSameComparer(SortedSet<T> other)
        {
            Debug.Assert(other != null);
            Debug.Assert(HasEqualComparer(other));

            foreach (T item in other)
            {
                bool result = Contains(item) ? Remove(item) : Add(item);
                Debug.Assert(result);
            }
        }

        private void SymmetricExceptWithSameComparer(T[] other, int count)
        {
            Debug.Assert(other != null);
            Debug.Assert(count >= 0 && count <= other.Length);

            if (count == 0)
            {
                return;
            }

            T previous = other[0];
            for (int i = 0; i < count; i++)
            {
                while (i < count && i != 0 && comparer.Compare(other[i], previous) == 0)
                    i++;
                if (i >= count)
                    break;
                T current = other[i];
                bool result = Contains(current) ? Remove(current) : Add(current);
                Debug.Assert(result);
                previous = current;
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Count == 0)
            {
                return true;
            }

            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted))
            {
                if (Count > asSorted.Count)
                    return false;
                return IsSubsetOfSortedSetWithSameComparer(asSorted);
            }
            else
            {
                // Worst case: I mark every element in my set and see if I've counted all of them. O(M log N).
                ElementCount result = CheckUniqueAndUnfoundElements(other, false);
                return result.UniqueCount == Count && result.UnfoundCount >= 0;
            }
        }

        private bool IsSubsetOfSortedSetWithSameComparer(SortedSet<T> asSorted)
        {
            SortedSet<T> prunedOther = asSorted.GetViewBetween(Min, Max);
            foreach (T item in this)
            {
                if (!prunedOther.Contains(item))
                    return false;
            }
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other is ICollection c)
            {
                if (Count == 0)
                    return c.Count > 0;
            }

            // another for sorted sets with the same comparer
            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted))
            {
                if (Count >= asSorted.Count)
                    return false;
                return IsSubsetOfSortedSetWithSameComparer(asSorted);
            }

            // Worst case: I mark every element in my set and see if I've counted all of them. O(M log N).
            ElementCount result = CheckUniqueAndUnfoundElements(other, false);
            return result.UniqueCount == Count && result.UnfoundCount > 0;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (other is ICollection c && c.Count == 0)
                return true;

            // do it one way for HashSets
            // another for sorted sets with the same comparer
            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted))
            {
                if (Count < asSorted.Count)
                    return false;
                SortedSet<T> pruned = GetViewBetween(asSorted.Min, asSorted.Max);
                foreach (T item in asSorted)
                {
                    if (!pruned.Contains(item))
                        return false;
                }
                return true;
            }

            // and a third for everything else
            return ContainsAllElements(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Count == 0)
                return false;

            if (other is ICollection c && c.Count == 0)
                return true;

            // another way for sorted sets
            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted))
            {
                if (asSorted.Count >= Count)
                    return false;
                SortedSet<T> pruned = GetViewBetween(asSorted.Min, asSorted.Max);
                foreach (T item in asSorted)
                {
                    if (!pruned.Contains(item))
                        return false;
                }
                return true;
            }

            // Worst case: I mark every element in my set and see if I've counted all of them. O(M log N).
            // slight optimization, put it into a HashSet and then check can do it in O(N+M)
            // but slower in better cases + wastes space
            ElementCount result = CheckUniqueAndUnfoundElements(other, true);
            return result.UniqueCount < Count && result.UnfoundCount == 0;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted))
            {
                Enumerator mine = GetEnumerator();
                Enumerator theirs = asSorted.GetEnumerator();
                bool mineEnded = !mine.MoveNext();
                bool theirsEnded = !theirs.MoveNext();
                while (!mineEnded && !theirsEnded)
                {
                    if (Comparer.Compare(mine.Current, theirs.Current) != 0)
                    {
                        return false;
                    }
                    mineEnded = !mine.MoveNext();
                    theirsEnded = !theirs.MoveNext();
                }
                return mineEnded && theirsEnded;
            }

            // Worst case: I mark every element in my set and see if I've counted all of them. O(size of the other collection).
            ElementCount result = CheckUniqueAndUnfoundElements(other, true);
            return result.UniqueCount == Count && result.UnfoundCount == 0;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Count == 0)
                return false;

            if (other is ICollection<T> c && c.Count == 0)
                return false;

            SortedSet<T> asSorted = other as SortedSet<T>;
            if (asSorted != null && HasEqualComparer(asSorted) && (comparer.Compare(Min, asSorted.Max) > 0 || comparer.Compare(Max, asSorted.Min) < 0))
            {
                return false;
            }

            foreach (T item in other)
            {
                if (Contains(item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This works similar to HashSet's CheckUniqueAndUnfound (description below), except that the bit
        /// array maps differently than in the HashSet. We can only use this for the bulk boolean checks.
        ///
        /// Determines counts that can be used to determine equality, subset, and superset. This
        /// is only used when other is an IEnumerable and not a HashSet. If other is a HashSet
        /// these properties can be checked faster without use of marking because we can assume
        /// other has no duplicates.
        ///
        /// The following count checks are performed by callers:
        /// 1. Equals: checks if UnfoundCount = 0 and uniqueFoundCount = Count; i.e. everything
        /// in other is in this and everything in this is in other
        /// 2. Subset: checks if UnfoundCount >= 0 and uniqueFoundCount = Count; i.e. other may
        /// have elements not in this and everything in this is in other
        /// 3. Proper subset: checks if UnfoundCount > 0 and uniqueFoundCount = Count; i.e
        /// other must have at least one element not in this and everything in this is in other
        /// 4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
        /// than Count; i.e. everything in other was in this and this had at least one element
        /// not contained in other.
        ///
        /// An earlier implementation used delegates to perform these checks rather than returning
        /// an ElementCount struct; however this was changed due to the perf overhead of delegates.
        /// </summary>
        private unsafe ElementCount CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
        {
            ElementCount result;

            // need special case in case this has no elements.
            if (Count == 0)
            {
                int numElementsInOther = 0;
                foreach (T item in other)
                {
                    numElementsInOther++;
                    // break right away, all we want to know is whether other has 0 or 1 elements
                    break;
                }
                result.UniqueCount = 0;
                result.UnfoundCount = numElementsInOther;
                return result;
            }

            int originalLastIndex = Count;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            Span<int> span = stackalloc int[StackAllocThreshold];
            BitHelper bitHelper = intArrayLength <= StackAllocThreshold ?
                new BitHelper(span.Slice(0, intArrayLength), clear: true) :
                new BitHelper(new int[intArrayLength], clear: false);

            // count of items in other not found in this
            int UnfoundCount = 0;
            // count of unique items in other found in this
            int uniqueFoundCount = 0;

            foreach (T item in other)
            {
                int index = InternalIndexOf(item);
                if (index >= 0)
                {
                    if (!bitHelper.IsMarked(index))
                    {
                        // item hasn't been seen yet
                        bitHelper.MarkBit(index);
                        uniqueFoundCount++;
                    }
                }
                else
                {
                    UnfoundCount++;
                    if (returnIfUnfound)
                    {
                        break;
                    }
                }
            }

            result.UniqueCount = uniqueFoundCount;
            result.UnfoundCount = UnfoundCount;
            return result;
        }

        public int RemoveWhere(Predicate<T> match)
        {
            ArgumentNullException.ThrowIfNull(match);

            List<T> matches = new List<T>(this.Count);

            BreadthFirstTreeWalk(n =>
            {
                if (match(n.Item))
                {
                    matches.Add(n.Item);
                }
                return true;
            });

            // Enumerate the results of the breadth-first walk in reverse in an attempt to lower cost.
            int actuallyRemoved = 0;
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                if (Remove(matches[i]))
                {
                    actuallyRemoved++;
                }
            }

            return actuallyRemoved;
        }

        #endregion

        #region ISorted members

        public T Min => MinInternal;

        internal virtual T MinInternal
        {
            get
            {
                if (root == null)
                {
                    return default;
                }

                Node current = root;
                while (current.Left != null)
                {
                    current = current.Left;
                }

                return current.Item;
            }
        }

        public T Max => MaxInternal;

        internal virtual T MaxInternal
        {
            get
            {
                if (root == null)
                {
                    return default;
                }

                Node current = root;
                while (current.Right != null)
                {
                    current = current.Right;
                }

                return current.Item;
            }
        }

        public IEnumerable<T> Reverse()
        {
            Enumerator e = new Enumerator(this, reverse: true);
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }

        public virtual SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
        {
            if (Comparer.Compare(lowerValue, upperValue) > 0)
            {
                throw new ArgumentException(SR.SortedSet_LowerValueGreaterThanUpperValue, nameof(lowerValue));
            }
            return new TreeSubSet(this, lowerValue, upperValue, true, true);
        }

#if DEBUG
        /// <summary>
        /// debug status to be checked whenever any operation is called
        /// </summary>
        /// <returns></returns>
        internal virtual bool versionUpToDate()
        {
            return true;
        }
#endif

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => GetObjectData(info, context);

        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);

            info.AddValue(CountName, count); // This is the length of the bucket array.
            info.AddValue(ComparerName, comparer, typeof(IComparer<T>));
            info.AddValue(VersionName, version);

            if (root != null)
            {
                T[] items = new T[Count];
                CopyTo(items, 0);
                info.AddValue(ItemsName, items, typeof(T[]));
            }
        }

        void IDeserializationCallback.OnDeserialization(object sender) => OnDeserialization(sender);

        protected virtual void OnDeserialization(object sender)
        {
            if (comparer != null)
            {
                return; // Somebody had a dependency on this class and fixed us up before the ObjectManager got to it.
            }

            if (siInfo == null)
            {
                throw new SerializationException(SR.Serialization_InvalidOnDeser);
            }

            comparer = (IComparer<T>)siInfo.GetValue(ComparerName, typeof(IComparer<T>))!;
            int savedCount = siInfo.GetInt32(CountName);

            if (savedCount != 0)
            {
                T[] items = (T[])siInfo.GetValue(ItemsName, typeof(T[]));

                if (items == null)
                {
                    throw new SerializationException(SR.Serialization_MissingValues);
                }

                for (int i = 0; i < items.Length; i++)
                {
                    Add(items[i]);
                }
            }

            version = siInfo.GetInt32(VersionName);
            if (count != savedCount)
            {
                throw new SerializationException(SR.Serialization_MismatchedCount);
            }

            siInfo = null;
        }

        #endregion

        #region Helper classes

        public sealed class Node
        {
            public Node(T item, NodeColor color)
            {
                Item = item;
                Color = color;
            }

            public static bool IsNonNullBlack(Node node) => node != null && node.IsBlack;

            public static bool IsNonNullRed(Node node) => node != null && node.IsRed;

            public static bool IsNullOrBlack(Node node) => node == null || node.IsBlack;

            public T Item { get; set; }

            public Node Left { get; set; }

            public Node Right { get; set; }

            public NodeColor Color { get; set; }

            public bool IsBlack => Color == NodeColor.Black;

            public bool IsRed => Color == NodeColor.Red;

            public bool Is2Node => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);

            public bool Is4Node => IsNonNullRed(Left) && IsNonNullRed(Right);

            public void ColorBlack() => Color = NodeColor.Black;

            public void ColorRed() => Color = NodeColor.Red;

            public Node DeepClone(int count)
            {
#if DEBUG
                Debug.Assert(count == GetCount());
#endif
                Node newRoot = ShallowClone();

                var pendingNodes = new Stack<(Node source, Node target)>(2 * Log2(count) + 2);
                pendingNodes.Push((this, newRoot));

                while (pendingNodes.TryPop(out var next))
                {
                    Node clonedNode;

                    if (next.source.Left is Node left)
                    {
                        clonedNode = left.ShallowClone();
                        next.target.Left = clonedNode;
                        pendingNodes.Push((left, clonedNode));
                    }

                    if (next.source.Right is Node right)
                    {
                        clonedNode = right.ShallowClone();
                        next.target.Right = clonedNode;
                        pendingNodes.Push((right, clonedNode));
                    }
                }

                return newRoot;
            }

            /// <summary>
            /// Gets the rotation this node should undergo during a removal.
            /// </summary>
            public TreeRotation GetRotation(Node current, Node sibling)
            {
                Debug.Assert(IsNonNullRed(sibling.Left) || IsNonNullRed(sibling.Right));
#if DEBUG
                Debug.Assert(HasChildren(current, sibling));
#endif

                bool currentIsLeftChild = Left == current;
                return IsNonNullRed(sibling.Left) ?
                    (currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right) :
                    (currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight);
            }

            /// <summary>
            /// Gets the sibling of one of this node's children.
            /// </summary>
            public Node GetSibling(Node node)
            {
                Debug.Assert(node != null);
                Debug.Assert(node == Left ^ node == Right);

                return node == Left ? Right! : Left!;
            }

            public Node ShallowClone() => new Node(Item, Color);

            public void Split4Node()
            {
                Debug.Assert(Left != null);
                Debug.Assert(Right != null);

                ColorRed();
                Left.ColorBlack();
                Right.ColorBlack();
            }

            /// <summary>
            /// Does a rotation on this tree. May change the color of a grandchild from red to black.
            /// </summary>
            public Node Rotate(TreeRotation rotation)
            {
                Node removeRed;
                switch (rotation)
                {
                    case TreeRotation.Right:
                        removeRed = Left!.Left!;
                        Debug.Assert(removeRed.IsRed);
                        removeRed.ColorBlack();
                        return RotateRight();
                    case TreeRotation.Left:
                        removeRed = Right!.Right!;
                        Debug.Assert(removeRed.IsRed);
                        removeRed.ColorBlack();
                        return RotateLeft();
                    case TreeRotation.RightLeft:
                        Debug.Assert(Right!.Left!.IsRed);
                        return RotateRightLeft();
                    case TreeRotation.LeftRight:
                        Debug.Assert(Left!.Right!.IsRed);
                        return RotateLeftRight();
                    default:
                        Debug.Fail($"{nameof(rotation)}: {rotation} is not a defined {nameof(TreeRotation)} value.");
                        return null;
                }
            }

            /// <summary>
            /// Does a left rotation on this tree, making this node the new left child of the current right child.
            /// </summary>
            public Node RotateLeft()
            {
                Node child = Right!;
                Right = child.Left;
                child.Left = this;
                return child;
            }

            /// <summary>
            /// Does a left-right rotation on this tree. The left child is rotated left, then this node is rotated right.
            /// </summary>
            public Node RotateLeftRight()
            {
                Node child = Left!;
                Node grandChild = child.Right!;

                Left = grandChild.Right;
                grandChild.Right = this;
                child.Right = grandChild.Left;
                grandChild.Left = child;
                return grandChild;
            }

            /// <summary>
            /// Does a right rotation on this tree, making this node the new right child of the current left child.
            /// </summary>
            public Node RotateRight()
            {
                Node child = Left!;
                Left = child.Right;
                child.Right = this;
                return child;
            }

            /// <summary>
            /// Does a right-left rotation on this tree. The right child is rotated right, then this node is rotated left.
            /// </summary>
            public Node RotateRightLeft()
            {
                Node child = Right!;
                Node grandChild = child.Left!;

                Right = grandChild.Left;
                grandChild.Left = this;
                child.Left = grandChild.Right;
                grandChild.Right = child;
                return grandChild;
            }

            /// <summary>
            /// Combines two 2-nodes into a 4-node.
            /// </summary>
            public void Merge2Nodes()
            {
                Debug.Assert(IsRed);
                Debug.Assert(Left!.Is2Node);
                Debug.Assert(Right!.Is2Node);

                // Combine two 2-nodes into a 4-node.
                ColorBlack();
                Left.ColorRed();
                Right.ColorRed();
            }

            /// <summary>
            /// Replaces a child of this node with a new node.
            /// </summary>
            /// <param name="child">The child to replace.</param>
            /// <param name="newChild">The node to replace <paramref name="child"/> with.</param>
            public void ReplaceChild(Node child, Node newChild)
            {
#if DEBUG
                Debug.Assert(HasChild(child));
#endif

                if (Left == child)
                {
                    Left = newChild;
                }
                else
                {
                    Right = newChild;
                }
            }

#if DEBUG
            private int GetCount() => 1 + (Left?.GetCount() ?? 0) + (Right?.GetCount() ?? 0);

            private bool HasChild(Node child) => child == Left || child == Right;

            private bool HasChildren(Node child1, Node child2)
            {
                Debug.Assert(child1 != child2);

                return (Left == child1 && Right == child2)
                    || (Left == child2 && Right == child1);
            }
#endif
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, ISerializable, IDeserializationCallback
        {
            private readonly SortedSet<T> _tree;
            private readonly int _version;

            private readonly Stack<Node> _stack;
            private Node _current;

            private readonly bool _reverse;

            internal Enumerator(SortedSet<T> set)
                : this(set, reverse: false)
            {
            }

            internal Enumerator(SortedSet<T> set, bool reverse)
            {
                _tree = set;
                set.VersionCheck();
                _version = set.version;

                // 2 log(n + 1) is the maximum height.
                // 这里缓存了Stack，防止GC
                _stack = set.Stack;
                _stack.Clear();
                _current = null;
                _reverse = reverse;

                Initialize();
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new PlatformNotSupportedException();
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                throw new PlatformNotSupportedException();
            }

            private void Initialize()
            {
                _current = null;
                Node node = _tree.root;
                Node next, other;
                while (node != null)
                {
                    next = (_reverse ? node.Right : node.Left);
                    other = (_reverse ? node.Left : node.Right);
                    if (_tree.IsWithinRange(node.Item))
                    {
                        _stack.Push(node);
                        node = next;
                    }
                    else if (next == null || !_tree.IsWithinRange(next.Item))
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
                _tree.VersionCheck();

                if (_version != _tree.version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }

                if (_stack.Count == 0)
                {
                    _current = null;
                    return false;
                }

                _current = _stack.Pop();
                Node node = (_reverse ? _current.Left : _current.Right);
                Node next, other;
                while (node != null)
                {
                    next = (_reverse ? node.Right : node.Left);
                    other = (_reverse ? node.Left : node.Right);
                    if (_tree.IsWithinRange(node.Item))
                    {
                        _stack.Push(node);
                        node = next;
                    }
                    else if (other == null || !_tree.IsWithinRange(other.Item))
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
                _stack.Clear();
            }

            public T Current
            {
                get
                {
                    if (_current != null)
                    {
                        return _current.Item;
                    }
                    return default(T)!; // Should only happen when accessing Current is undefined behavior
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }

                    return _current.Item;
                }
            }

            internal bool NotStartedOrEnded => _current == null;

            internal void Reset()
            {
                if (_version != _tree.version)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                }

                if (_stack.Count > 32)
                {
                    this._tree.Stack = new Stack<Node>(10);
                }
                _stack.Clear();
                Initialize();
            }

            void IEnumerator.Reset() => Reset();
        }

        internal struct ElementCount
        {
            internal int UniqueCount;
            internal int UnfoundCount;
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">The value from the set that the search found, or the default value of <typeparamref name="T"/> when the search yielded no match.</param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        /// This can be useful when you want to reuse a previously stored reference instead of
        /// a newly constructed one (so that more sharing of references can occur) or to look up
        /// a value that has more complete data than the value you currently have, although their
        /// comparer functions indicate they are equal.
        /// </remarks>
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            Node node = FindNode(equalValue);
            if (node != null)
            {
                actualValue = node.Item;
                return true;
            }
            actualValue = default;
            return false;
        }

        // Used for set checking operations (using enumerables) that rely on counting
        private static int Log2(int value) => (int)Math.Log(value, 2);

        #endregion
    }
    
    
    internal static partial class HResults
    {
        internal const int S_OK = unchecked((int)0x00000000);
        internal const int S_FALSE = unchecked((int)0x1);
        internal const int COR_E_ABANDONEDMUTEX = unchecked((int)0x8013152D);
        internal const int COR_E_AMBIGUOUSIMPLEMENTATION = unchecked((int)0x8013106A);
        internal const int COR_E_AMBIGUOUSMATCH = unchecked((int)0x8000211D);
        internal const int COR_E_APPDOMAINUNLOADED = unchecked((int)0x80131014);
        internal const int COR_E_APPLICATION = unchecked((int)0x80131600);
        internal const int COR_E_ARGUMENT = unchecked((int)0x80070057);
        internal const int COR_E_ARGUMENTOUTOFRANGE = unchecked((int)0x80131502);
        internal const int COR_E_ARITHMETIC = unchecked((int)0x80070216);
        internal const int COR_E_ARRAYTYPEMISMATCH = unchecked((int)0x80131503);
        internal const int COR_E_BADEXEFORMAT = unchecked((int)0x800700C1);
        internal const int COR_E_BADIMAGEFORMAT = unchecked((int)0x8007000B);
        internal const int COR_E_CANNOTUNLOADAPPDOMAIN = unchecked((int)0x80131015);
        internal const int COR_E_CODECONTRACTFAILED = unchecked((int)0x80131542);
        internal const int COR_E_CONTEXTMARSHAL = unchecked((int)0x80131504);
        internal const int COR_E_CUSTOMATTRIBUTEFORMAT = unchecked((int)0x80131605);
        internal const int COR_E_DATAMISALIGNED = unchecked((int)0x80131541);
        internal const int COR_E_DIRECTORYNOTFOUND = unchecked((int)0x80070003);
        internal const int COR_E_DIVIDEBYZERO = unchecked((int)0x80020012); // DISP_E_DIVBYZERO
        internal const int COR_E_DLLNOTFOUND = unchecked((int)0x80131524);
        internal const int COR_E_DUPLICATEWAITOBJECT = unchecked((int)0x80131529);
        internal const int COR_E_ENDOFSTREAM = unchecked((int)0x80070026);
        internal const int COR_E_ENTRYPOINTNOTFOUND = unchecked((int)0x80131523);
        internal const int COR_E_EXCEPTION = unchecked((int)0x80131500);
        internal const int COR_E_EXECUTIONENGINE = unchecked((int)0x80131506);
        internal const int COR_E_FAILFAST = unchecked((int)0x80131623);
        internal const int COR_E_FIELDACCESS = unchecked((int)0x80131507);
        internal const int COR_E_FILELOAD = unchecked((int)0x80131621);
        internal const int COR_E_FILENOTFOUND = unchecked((int)0x80070002);
        internal const int COR_E_FORMAT = unchecked((int)0x80131537);
        internal const int COR_E_INDEXOUTOFRANGE = unchecked((int)0x80131508);
        internal const int COR_E_INSUFFICIENTEXECUTIONSTACK = unchecked((int)0x80131578);
        internal const int COR_E_INSUFFICIENTMEMORY = unchecked((int)0x8013153D);
        internal const int COR_E_INVALIDCAST = unchecked((int)0x80004002);
        internal const int COR_E_INVALIDCOMOBJECT = unchecked((int)0x80131527);
        internal const int COR_E_INVALIDFILTERCRITERIA = unchecked((int)0x80131601);
        internal const int COR_E_INVALIDOLEVARIANTTYPE = unchecked((int)0x80131531);
        internal const int COR_E_INVALIDOPERATION = unchecked((int)0x80131509);
        internal const int COR_E_INVALIDPROGRAM = unchecked((int)0x8013153A);
        internal const int COR_E_IO = unchecked((int)0x80131620);
        internal const int COR_E_KEYNOTFOUND = unchecked((int)0x80131577);
        internal const int COR_E_MARSHALDIRECTIVE = unchecked((int)0x80131535);
        internal const int COR_E_MEMBERACCESS = unchecked((int)0x8013151A);
        internal const int COR_E_METHODACCESS = unchecked((int)0x80131510);
        internal const int COR_E_MISSINGFIELD = unchecked((int)0x80131511);
        internal const int COR_E_MISSINGMANIFESTRESOURCE = unchecked((int)0x80131532);
        internal const int COR_E_MISSINGMEMBER = unchecked((int)0x80131512);
        internal const int COR_E_MISSINGMETHOD = unchecked((int)0x80131513);
        internal const int COR_E_MISSINGSATELLITEASSEMBLY = unchecked((int)0x80131536);
        internal const int COR_E_MULTICASTNOTSUPPORTED = unchecked((int)0x80131514);
        internal const int COR_E_NOTFINITENUMBER = unchecked((int)0x80131528);
        internal const int COR_E_NOTSUPPORTED = unchecked((int)0x80131515);
        internal const int COR_E_OBJECTDISPOSED = unchecked((int)0x80131622);
        internal const int COR_E_OPERATIONCANCELED = unchecked((int)0x8013153B);
        internal const int COR_E_OUTOFMEMORY = unchecked((int)0x8007000E);
        internal const int COR_E_OVERFLOW = unchecked((int)0x80131516);
        internal const int COR_E_PATHTOOLONG = unchecked((int)0x800700CE);
        internal const int COR_E_PLATFORMNOTSUPPORTED = unchecked((int)0x80131539);
        internal const int COR_E_RANK = unchecked((int)0x80131517);
        internal const int COR_E_REFLECTIONTYPELOAD = unchecked((int)0x80131602);
        internal const int COR_E_RUNTIMEWRAPPED = unchecked((int)0x8013153E);
        internal const int COR_E_SAFEARRAYRANKMISMATCH = unchecked((int)0x80131538);
        internal const int COR_E_SAFEARRAYTYPEMISMATCH = unchecked((int)0x80131533);
        internal const int COR_E_SECURITY = unchecked((int)0x8013150A);
        internal const int COR_E_SERIALIZATION = unchecked((int)0x8013150C);
        internal const int COR_E_STACKOVERFLOW = unchecked((int)0x800703E9);
        internal const int COR_E_SYNCHRONIZATIONLOCK = unchecked((int)0x80131518);
        internal const int COR_E_SYSTEM = unchecked((int)0x80131501);
        internal const int COR_E_TARGET = unchecked((int)0x80131603);
        internal const int COR_E_TARGETINVOCATION = unchecked((int)0x80131604);
        internal const int COR_E_TARGETPARAMCOUNT = unchecked((int)0x8002000E);
        internal const int COR_E_THREADABORTED = unchecked((int)0x80131530);
        internal const int COR_E_THREADINTERRUPTED = unchecked((int)0x80131519);
        internal const int COR_E_THREADSTART = unchecked((int)0x80131525);
        internal const int COR_E_THREADSTATE = unchecked((int)0x80131520);
        internal const int COR_E_TIMEOUT = unchecked((int)0x80131505);
        internal const int COR_E_TYPEACCESS = unchecked((int)0x80131543);
        internal const int COR_E_TYPEINITIALIZATION = unchecked((int)0x80131534);
        internal const int COR_E_TYPELOAD = unchecked((int)0x80131522);
        internal const int COR_E_TYPEUNLOADED = unchecked((int)0x80131013);
        internal const int COR_E_UNAUTHORIZEDACCESS = unchecked((int)0x80070005);
        internal const int COR_E_VERIFICATION = unchecked((int)0x8013150D);
        internal const int COR_E_WAITHANDLECANNOTBEOPENED = unchecked((int)0x8013152C);
        internal const int CO_E_NOTINITIALIZED = unchecked((int)0x800401F0);
        internal const int DISP_E_OVERFLOW = unchecked((int)0x8002000A);
        internal const int E_BOUNDS = unchecked((int)0x8000000B);
        internal const int E_CHANGED_STATE = unchecked((int)0x8000000C);
        internal const int E_FILENOTFOUND = unchecked((int)0x80070002);
        internal const int E_FAIL = unchecked((int)0x80004005);
        internal const int E_HANDLE = unchecked((int)0x80070006);
        internal const int E_INVALIDARG = unchecked((int)0x80070057);
        internal const int E_NOTIMPL = unchecked((int)0x80004001);
        internal const int E_POINTER = unchecked((int)0x80004003);
        internal const int ERROR_MRM_MAP_NOT_FOUND = unchecked((int)0x80073B1F);
        internal const int ERROR_TIMEOUT = unchecked((int)0x800705B4);
        internal const int RO_E_CLOSED = unchecked((int)0x80000013);
        internal const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
        internal const int TYPE_E_TYPEMISMATCH = unchecked((int)0x80028CA0);
        internal const int STG_E_PATHNOTFOUND = unchecked((int)0x80030003);
        internal const int CTL_E_PATHNOTFOUND = unchecked((int)0x800A004C);
        internal const int CTL_E_FILENOTFOUND = unchecked((int)0x800A0035);
        internal const int FUSION_E_INVALID_NAME = unchecked((int)0x80131047);
        internal const int FUSION_E_PRIVATE_ASM_DISALLOWED = unchecked((int)0x80131044);
        internal const int FUSION_E_REF_DEF_MISMATCH = unchecked((int)0x80131040);
        internal const int ERROR_TOO_MANY_OPEN_FILES = unchecked((int)0x80070004);
        internal const int ERROR_SHARING_VIOLATION = unchecked((int)0x80070020);
        internal const int ERROR_LOCK_VIOLATION = unchecked((int)0x80070021);
        internal const int ERROR_OPEN_FAILED = unchecked((int)0x8007006E);
        internal const int ERROR_DISK_CORRUPT = unchecked((int)0x80070571);
        internal const int ERROR_UNRECOGNIZED_VOLUME = unchecked((int)0x800703ED);
        internal const int ERROR_DLL_INIT_FAILED = unchecked((int)0x8007045A);
        internal const int MSEE_E_ASSEMBLYLOADINPROGRESS = unchecked((int)0x80131016);
        internal const int ERROR_FILE_INVALID = unchecked((int)0x800703EE);
    }

    
    /// <summary>
    /// The exception that is thrown when a <see langword="null"/> reference (<see langword="Nothing"/> in Visual Basic) is passed to a method that does not accept it as a valid argument.
    /// </summary>
    [Serializable]
    public class ArgumentNullException : ArgumentException
    {
        // Creates a new ArgumentNullException with its message
        // string set to a default message explaining an argument was null.
        public ArgumentNullException()
             : base(SR.ArgumentNull_Generic)
        {
            // Use E_POINTER - COM used that for null pointers.  Description is "invalid pointer"
            HResult = HResults.E_POINTER;
        }

        public ArgumentNullException(string paramName)
            : base(SR.ArgumentNull_Generic, paramName)
        {
            HResult = HResults.E_POINTER;
        }

        public ArgumentNullException(string message, Exception innerException)
            : base(message ?? SR.ArgumentNull_Generic, innerException)
        {
            HResult = HResults.E_POINTER;
        }

        public ArgumentNullException(string paramName, string message)
            : base(message ?? SR.ArgumentNull_Generic, paramName)
        {
            HResult = HResults.E_POINTER;
        }

        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull(object argument, string paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }

        internal static void ThrowIfNull(object argument, ExceptionArgument paramName)
        {
            if (argument is null)
            {
                ThrowHelper.ThrowArgumentNullException(paramName);
            }
        }

        public static unsafe void ThrowIfNull(void* argument, string paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }

        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The pointer argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        internal static unsafe void ThrowIfNull(IntPtr argument, string paramName = null)
        {
            if (argument == IntPtr.Zero)
            {
                Throw(paramName);
            }
        }

        internal static void Throw(string paramName) =>
            throw new ArgumentNullException(paramName);
    }

    internal static class ThrowHelper
    {
        internal static void ThrowWrongKeyTypeArgumentException(object key, Type targetType)
        {
            throw new ArgumentException(SR.GetString("Arg_WrongType", key, (object)targetType), nameof(key));
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type targetType)
        {
            throw new ArgumentException(SR.GetString("Arg_WrongType", value, (object)targetType), nameof(value));
        }

        internal static void ThrowKeyNotFoundException() => throw new KeyNotFoundException();

        internal static void ThrowArgumentException(ExceptionResource resource)
        {
            throw new ArgumentException(SR.GetString(ThrowHelper.GetResourceName(resource)));
        }

        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw new ArgumentNullException(ThrowHelper.GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(ThrowHelper.GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(
        ExceptionArgument argument,
        ExceptionResource resource)
        {
            throw new ArgumentOutOfRangeException(ThrowHelper.GetArgumentName(argument), SR.GetString(ThrowHelper.GetResourceName(resource)));
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw new InvalidOperationException(SR.GetString(ThrowHelper.GetResourceName(resource)));
        }

        internal static void ThrowSerializationException(ExceptionResource resource)
        {
            throw new SerializationException(SR.GetString(ThrowHelper.GetResourceName(resource)));
        }

        internal static void ThrowNotSupportedException(ExceptionResource resource)
        {
            throw new NotSupportedException(SR.GetString(ThrowHelper.GetResourceName(resource)));
        }

        internal static void IfNullAndNullsAreIllegalThenThrow<T>(
        object value,
        ExceptionArgument argName)
        {
            if (value != null || (object)default(T) == null)
                return;
            ThrowHelper.ThrowArgumentNullException(argName);
        }

        internal static string GetArgumentName(ExceptionArgument argument)
        {
            switch (argument)
            {
                case ExceptionArgument.obj:
                    return "obj";
                case ExceptionArgument.dictionary:
                    return "dictionary";
                case ExceptionArgument.array:
                    return "array";
                case ExceptionArgument.info:
                    return "info";
                case ExceptionArgument.key:
                    return "key";
                case ExceptionArgument.collection:
                    return "collection";
                case ExceptionArgument.match:
                    return "match";
                case ExceptionArgument.converter:
                    return "converter";
                case ExceptionArgument.queue:
                    return "queue";
                case ExceptionArgument.stack:
                    return "stack";
                case ExceptionArgument.capacity:
                    return "capacity";
                case ExceptionArgument.index:
                    return "index";
                case ExceptionArgument.startIndex:
                    return "startIndex";
                case ExceptionArgument.value:
                    return "value";
                case ExceptionArgument.count:
                    return "count";
                case ExceptionArgument.arrayIndex:
                    return "arrayIndex";
                case ExceptionArgument.item:
                    return "item";
                default:
                    return string.Empty;
            }
        }

        internal static string GetResourceName(ExceptionResource resource)
        {
            switch (resource)
            {
                case ExceptionResource.Argument_ImplementIComparable:
                    return "Argument_ImplementIComparable";
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    return "ArgumentOutOfRange_NeedNonNegNum";
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired:
                    return "ArgumentOutOfRange_NeedNonNegNumRequired";
                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    return "Arg_ArrayPlusOffTooSmall";
                case ExceptionResource.Argument_AddingDuplicate:
                    return "Argument_AddingDuplicate";
                case ExceptionResource.Serialization_InvalidOnDeser:
                    return "Serialization_InvalidOnDeser";
                case ExceptionResource.Serialization_MismatchedCount:
                    return "Serialization_MismatchedCount";
                case ExceptionResource.Serialization_MissingValues:
                    return "Serialization_MissingValues";
                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    return "Arg_MultiRank";
                case ExceptionResource.Arg_NonZeroLowerBound:
                    return "Arg_NonZeroLowerBound";
                case ExceptionResource.Argument_InvalidArrayType:
                    return "Invalid_Array_Type";
                case ExceptionResource.NotSupported_KeyCollectionSet:
                    return "NotSupported_KeyCollectionSet";
                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    return "ArgumentOutOfRange_SmallCapacity";
                case ExceptionResource.ArgumentOutOfRange_Index:
                    return "ArgumentOutOfRange_Index";
                case ExceptionResource.Argument_InvalidOffLen:
                    return "Argument_InvalidOffLen";
                case ExceptionResource.InvalidOperation_CannotRemoveFromStackOrQueue:
                    return "InvalidOperation_CannotRemoveFromStackOrQueue";
                case ExceptionResource.InvalidOperation_EmptyCollection:
                    return "InvalidOperation_EmptyCollection";
                case ExceptionResource.InvalidOperation_EmptyQueue:
                    return "InvalidOperation_EmptyQueue";
                case ExceptionResource.InvalidOperation_EnumOpCantHappen:
                    return "InvalidOperation_EnumOpCantHappen";
                case ExceptionResource.InvalidOperation_EnumFailedVersion:
                    return "InvalidOperation_EnumFailedVersion";
                case ExceptionResource.InvalidOperation_EmptyStack:
                    return "InvalidOperation_EmptyStack";
                case ExceptionResource.InvalidOperation_EnumNotStarted:
                    return "InvalidOperation_EnumNotStarted";
                case ExceptionResource.InvalidOperation_EnumEnded:
                    return "InvalidOperation_EnumEnded";
                case ExceptionResource.NotSupported_SortedListNestedWrite:
                    return "NotSupported_SortedListNestedWrite";
                case ExceptionResource.NotSupported_ValueCollectionSet:
                    return "NotSupported_ValueCollectionSet";
                default:
                    return string.Empty;
            }
        }
    }

    internal enum ExceptionResource
    {
        Argument_ImplementIComparable,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_NeedNonNegNumRequired,
        Arg_ArrayPlusOffTooSmall,
        Argument_AddingDuplicate,
        Serialization_InvalidOnDeser,
        Serialization_MismatchedCount,
        Serialization_MissingValues,
        Arg_RankMultiDimNotSupported,
        Arg_NonZeroLowerBound,
        Argument_InvalidArrayType,
        NotSupported_KeyCollectionSet,
        ArgumentOutOfRange_SmallCapacity,
        ArgumentOutOfRange_Index,
        Argument_InvalidOffLen,
        NotSupported_ReadOnlyCollection,
        InvalidOperation_CannotRemoveFromStackOrQueue,
        InvalidOperation_EmptyCollection,
        InvalidOperation_EmptyQueue,
        InvalidOperation_EnumOpCantHappen,
        InvalidOperation_EnumFailedVersion,
        InvalidOperation_EmptyStack,
        InvalidOperation_EnumNotStarted,
        InvalidOperation_EnumEnded,
        NotSupported_SortedListNestedWrite,
        NotSupported_ValueCollectionSet,
    }

    internal enum ExceptionArgument
    {
        obj,
        dictionary,
        array,
        info,
        key,
        collection,
        match,
        converter,
        queue,
        stack,
        capacity,
        index,
        startIndex,
        value,
        count,
        arrayIndex,
        item,
    }

    internal sealed class SR
    {
        internal const string ArgumentOutOfRange_IndexMustBeLessOrEqual = "ArgumentOutOfRange_IndexMustBeLessOrEqual";
        internal const string Argument_IncompatibleArrayType = "Argument_IncompatibleArrayType";
        internal const string SortedSet_LowerValueGreaterThanUpperValue = "SortedSet_LowerValueGreaterThanUpperValue";
        internal const string ArgumentNull_Generic = "ArgumentNull_Generic";
        internal const string RTL = "RTL";
        internal const string ContinueButtonText = "ContinueButtonText";
        internal const string DebugAssertBanner = "DebugAssertBanner";
        internal const string DebugAssertShortMessage = "DebugAssertShortMessage";
        internal const string DebugAssertLongMessage = "DebugAssertLongMessage";
        internal const string DebugMessageTruncated = "DebugMessageTruncated";
        internal const string DebugAssertTitle = "DebugAssertTitle";
        internal const string NotSupported = "NotSupported";
        internal const string DebugLaunchFailed = "DebugLaunchFailed";
        internal const string DebugLaunchFailedTitle = "DebugLaunchFailedTitle";
        internal const string ObjectDisposed = "ObjectDisposed";
        internal const string ExceptionOccurred = "ExceptionOccurred";
        internal const string MustAddListener = "MustAddListener";
        internal const string ToStringNull = "ToStringNull";
        internal const string EnumConverterInvalidValue = "EnumConverterInvalidValue";
        internal const string ConvertFromException = "ConvertFromException";
        internal const string ConvertToException = "ConvertToException";
        internal const string ConvertInvalidPrimitive = "ConvertInvalidPrimitive";
        internal const string ErrorMissingPropertyAccessors = "ErrorMissingPropertyAccessors";
        internal const string ErrorInvalidPropertyType = "ErrorInvalidPropertyType";
        internal const string ErrorMissingEventAccessors = "ErrorMissingEventAccessors";
        internal const string ErrorInvalidEventHandler = "ErrorInvalidEventHandler";
        internal const string ErrorInvalidEventType = "ErrorInvalidEventType";
        internal const string InvalidMemberName = "InvalidMemberName";
        internal const string ErrorBadExtenderType = "ErrorBadExtenderType";
        internal const string NullableConverterBadCtorArg = "NullableConverterBadCtorArg";
        internal const string TypeDescriptorExpectedElementType = "TypeDescriptorExpectedElementType";
        internal const string TypeDescriptorSameAssociation = "TypeDescriptorSameAssociation";
        internal const string TypeDescriptorAlreadyAssociated = "TypeDescriptorAlreadyAssociated";
        internal const string TypeDescriptorProviderError = "TypeDescriptorProviderError";
        internal const string TypeDescriptorUnsupportedRemoteObject = "TypeDescriptorUnsupportedRemoteObject";
        internal const string TypeDescriptorArgsCountMismatch = "TypeDescriptorArgsCountMismatch";
        internal const string ErrorCreateSystemEvents = "ErrorCreateSystemEvents";
        internal const string ErrorCreateTimer = "ErrorCreateTimer";
        internal const string ErrorKillTimer = "ErrorKillTimer";
        internal const string ErrorSystemEventsNotSupported = "ErrorSystemEventsNotSupported";
        internal const string ErrorGetTempPath = "ErrorGetTempPath";
        internal const string CHECKOUTCanceled = "CHECKOUTCanceled";
        internal const string ErrorInvalidServiceInstance = "ErrorInvalidServiceInstance";
        internal const string ErrorServiceExists = "ErrorServiceExists";
        internal const string Argument_InvalidNumberStyles = "Argument_InvalidNumberStyles";
        internal const string Argument_InvalidHexStyle = "Argument_InvalidHexStyle";
        internal const string Argument_ByteArrayLengthMustBeAMultipleOf4 = "Argument_ByteArrayLengthMustBeAMultipleOf4";
        internal const string Argument_InvalidCharactersInString = "Argument_InvalidCharactersInString";
        internal const string Argument_ParsedStringWasInvalid = "Argument_ParsedStringWasInvalid";
        internal const string Argument_MustBeBigInt = "Argument_MustBeBigInt";
        internal const string Format_InvalidFormatSpecifier = "Format_InvalidFormatSpecifier";
        internal const string Format_TooLarge = "Format_TooLarge";
        internal const string ArgumentOutOfRange_MustBeLessThanUInt32MaxValue = "ArgumentOutOfRange_MustBeLessThanUInt32MaxValue";
        internal const string ArgumentOutOfRange_MustBeNonNeg = "ArgumentOutOfRange_MustBeNonNeg";
        internal const string NotSupported_NumberStyle = "NotSupported_NumberStyle";
        internal const string Overflow_BigIntInfinity = "Overflow_BigIntInfinity";
        internal const string Overflow_NotANumber = "Overflow_NotANumber";
        internal const string Overflow_ParseBigInteger = "Overflow_ParseBigInteger";
        internal const string Overflow_Int32 = "Overflow_Int32";
        internal const string Overflow_Int64 = "Overflow_Int64";
        internal const string Overflow_UInt32 = "Overflow_UInt32";
        internal const string Overflow_UInt64 = "Overflow_UInt64";
        internal const string Overflow_Decimal = "Overflow_Decimal";
        internal const string Argument_FrameworkNameTooShort = "Argument_FrameworkNameTooShort";
        internal const string Argument_FrameworkNameInvalid = "Argument_FrameworkNameInvalid";
        internal const string Argument_FrameworkNameInvalidVersion = "Argument_FrameworkNameInvalidVersion";
        internal const string Argument_FrameworkNameMissingVersion = "Argument_FrameworkNameMissingVersion";
        internal const string ArgumentNull_Key = "ArgumentNull_Key";
        internal const string Argument_AddingDuplicate = "Argument_AddingDuplicate";
        internal const string Argument_InvalidValue = "Argument_InvalidValue";
        internal const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
        internal const string ArgumentOutOfRange_InvalidThreshold = "ArgumentOutOfRange_InvalidThreshold";
        internal const string InvalidOperation_EnumFailedVersion = "InvalidOperation_EnumFailedVersion";
        internal const string InvalidOperation_EnumOpCantHappen = "InvalidOperation_EnumOpCantHappen";
        internal const string Arg_MultiRank = "Arg_MultiRank";
        internal const string Arg_NonZeroLowerBound = "Arg_NonZeroLowerBound";
        internal const string Arg_InsufficientSpace = "Arg_InsufficientSpace";
        internal const string NotSupported_EnumeratorReset = "NotSupported_EnumeratorReset";
        internal const string Invalid_Array_Type = "Invalid_Array_Type";
        internal const string Serialization_InvalidOnDeser = "Serialization_InvalidOnDeser";
        internal const string Serialization_MissingValues = "Serialization_MissingValues";
        internal const string Serialization_MismatchedCount = "Serialization_MismatchedCount";
        internal const string ExternalLinkedListNode = "ExternalLinkedListNode";
        internal const string LinkedListNodeIsAttached = "LinkedListNodeIsAttached";
        internal const string LinkedListEmpty = "LinkedListEmpty";
        internal const string Arg_WrongType = "Arg_WrongType";
        internal const string Argument_ItemNotExist = "Argument_ItemNotExist";
        internal const string Argument_ImplementIComparable = "Argument_ImplementIComparable";
        internal const string InvalidOperation_EmptyCollection = "InvalidOperation_EmptyCollection";
        internal const string InvalidOperation_EmptyQueue = "InvalidOperation_EmptyQueue";
        internal const string InvalidOperation_EmptyStack = "InvalidOperation_EmptyStack";
        internal const string InvalidOperation_CannotRemoveFromStackOrQueue = "InvalidOperation_CannotRemoveFromStackOrQueue";
        internal const string ArgumentOutOfRange_Index = "ArgumentOutOfRange_Index";
        internal const string ArgumentOutOfRange_SmallCapacity = "ArgumentOutOfRange_SmallCapacity";
        internal const string Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";
        internal const string NotSupported_KeyCollectionSet = "NotSupported_KeyCollectionSet";
        internal const string NotSupported_ValueCollectionSet = "NotSupported_ValueCollectionSet";
        internal const string NotSupported_ReadOnlyCollection = "NotSupported_ReadOnlyCollection";
        internal const string NotSupported_SortedListNestedWrite = "NotSupported_SortedListNestedWrite";
        internal const string BlockingCollection_ctor_BoundedCapacityRange = "BlockingCollection_ctor_BoundedCapacityRange";
        internal const string BlockingCollection_ctor_CountMoreThanCapacity = "BlockingCollection_ctor_CountMoreThanCapacity";
        internal const string BlockingCollection_Add_ConcurrentCompleteAdd = "BlockingCollection_Add_ConcurrentCompleteAdd";
        internal const string BlockingCollection_Add_Failed = "BlockingCollection_Add_Failed";
        internal const string BlockingCollection_Take_CollectionModified = "BlockingCollection_Take_CollectionModified";
        internal const string BlockingCollection_Completed = "BlockingCollection_Completed";
        internal const string BlockingCollection_Disposed = "BlockingCollection_Disposed";
        internal const string BlockingCollection_TimeoutInvalid = "BlockingCollection_TimeoutInvalid";
        internal const string BlockingCollection_CantTakeWhenDone = "BlockingCollection_CantTakeWhenDone";
        internal const string BlockingCollection_CantAddAnyWhenCompleted = "BlockingCollection_CantAddAnyWhenCompleted";
        internal const string BlockingCollection_CantTakeAnyWhenAllDone = "BlockingCollection_CantTakeAnyWhenAllDone";
        internal const string BlockingCollection_ValidateCollectionsArray_ZeroSize = "BlockingCollection_ValidateCollectionsArray_ZeroSize";
        internal const string BlockingCollection_ValidateCollectionsArray_LargeSize = "BlockingCollection_ValidateCollectionsArray_LargeSize";
        internal const string BlockingCollection_ValidateCollectionsArray_NullElems = "BlockingCollection_ValidateCollectionsArray_NullElems";
        internal const string BlockingCollection_ValidateCollectionsArray_DispElems = "BlockingCollection_ValidateCollectionsArray_DispElems";
        internal const string BlockingCollection_CompleteAdding_AlreadyDone = "BlockingCollection_CompleteAdding_AlreadyDone";
        internal const string BlockingCollection_CopyTo_NonNegative = "BlockingCollection_CopyTo_NonNegative";
        internal const string BlockingCollection_CopyTo_TooManyElems = "BlockingCollection_CopyTo_TooManyElems";
        internal const string BlockingCollection_CopyTo_MultiDim = "BlockingCollection_CopyTo_MultiDim";
        internal const string BlockingCollection_CopyTo_IncorrectType = "BlockingCollection_CopyTo_IncorrectType";
        internal const string ConcurrentBag_Ctor_ArgumentNullException = "ConcurrentBag_Ctor_ArgumentNullException";
        internal const string ConcurrentBag_CopyTo_ArgumentNullException = "ConcurrentBag_CopyTo_ArgumentNullException";
        internal const string ConcurrentBag_CopyTo_ArgumentOutOfRangeException = "ConcurrentBag_CopyTo_ArgumentOutOfRangeException";

        internal const string ConcurrentBag_CopyTo_ArgumentException_IndexGreaterThanLength =
                "ConcurrentBag_CopyTo_ArgumentException_IndexGreaterThanLength";

        internal const string ConcurrentBag_CopyTo_ArgumentException_NoEnoughSpace = "ConcurrentBag_CopyTo_ArgumentException_NoEnoughSpace";
        internal const string ConcurrentBag_CopyTo_ArgumentException_InvalidArrayType = "ConcurrentBag_CopyTo_ArgumentException_InvalidArrayType";
        internal const string ConcurrentCollection_SyncRoot_NotSupported = "ConcurrentCollection_SyncRoot_NotSupported";
        internal const string Common_OperationCanceled = "Common_OperationCanceled";
        internal const string Barrier_ctor_ArgumentOutOfRange = "Barrier_ctor_ArgumentOutOfRange";
        internal const string Barrier_AddParticipants_NonPositive_ArgumentOutOfRange = "Barrier_AddParticipants_NonPositive_ArgumentOutOfRange";
        internal const string Barrier_AddParticipants_Overflow_ArgumentOutOfRange = "Barrier_AddParticipants_Overflow_ArgumentOutOfRange";
        internal const string Barrier_InvalidOperation_CalledFromPHA = "Barrier_InvalidOperation_CalledFromPHA";

        internal const string Barrier_RemoveParticipants_NonPositive_ArgumentOutOfRange =
                "Barrier_RemoveParticipants_NonPositive_ArgumentOutOfRange";

        internal const string Barrier_RemoveParticipants_ArgumentOutOfRange = "Barrier_RemoveParticipants_ArgumentOutOfRange";
        internal const string Barrier_RemoveParticipants_InvalidOperation = "Barrier_RemoveParticipants_InvalidOperation";
        internal const string Barrier_SignalAndWait_ArgumentOutOfRange = "Barrier_SignalAndWait_ArgumentOutOfRange";
        internal const string Barrier_SignalAndWait_InvalidOperation_ZeroTotal = "Barrier_SignalAndWait_InvalidOperation_ZeroTotal";
        internal const string Barrier_SignalAndWait_InvalidOperation_ThreadsExceeded = "Barrier_SignalAndWait_InvalidOperation_ThreadsExceeded";
        internal const string Barrier_Dispose = "Barrier_Dispose";
        internal const string BarrierPostPhaseException = "BarrierPostPhaseException";
        internal const string UriTypeConverter_ConvertFrom_CannotConvert = "UriTypeConverter_ConvertFrom_CannotConvert";
        internal const string UriTypeConverter_ConvertTo_CannotConvert = "UriTypeConverter_ConvertTo_CannotConvert";
        internal const string ISupportInitializeDescr = "ISupportInitializeDescr";
        internal const string CantModifyListSortDescriptionCollection = "CantModifyListSortDescriptionCollection";
        internal const string Argument_NullComment = "Argument_NullComment";
        internal const string InvalidPrimitiveType = "InvalidPrimitiveType";
        internal const string Cannot_Specify_Both_Compiler_Path_And_Version = "Cannot_Specify_Both_Compiler_Path_And_Version";
        internal const string CodeGenOutputWriter = "CodeGenOutputWriter";
        internal const string CodeGenReentrance = "CodeGenReentrance";
        internal const string InvalidLanguageIdentifier = "InvalidLanguageIdentifier";
        internal const string InvalidTypeName = "InvalidTypeName";
        internal const string Empty_attribute = "Empty_attribute";
        internal const string Invalid_nonnegative_integer_attribute = "Invalid_nonnegative_integer_attribute";
        internal const string CodeDomProvider_NotDefined = "CodeDomProvider_NotDefined";
        internal const string Language_Names_Cannot_Be_Empty = "Language_Names_Cannot_Be_Empty";
        internal const string Extension_Names_Cannot_Be_Empty_Or_Non_Period_Based = "Extension_Names_Cannot_Be_Empty_Or_Non_Period_Based";
        internal const string Unable_To_Locate_Type = "Unable_To_Locate_Type";
        internal const string NotSupported_CodeDomAPI = "NotSupported_CodeDomAPI";
        internal const string ArityDoesntMatch = "ArityDoesntMatch";
        internal const string PartialTrustErrorTextReplacement = "PartialTrustErrorTextReplacement";
        internal const string PartialTrustIllegalProvider = "PartialTrustIllegalProvider";
        internal const string IllegalAssemblyReference = "IllegalAssemblyReference";
        internal const string NullOrEmpty_Value_in_Property = "NullOrEmpty_Value_in_Property";
        internal const string AutoGen_Comment_Line1 = "AutoGen_Comment_Line1";
        internal const string AutoGen_Comment_Line2 = "AutoGen_Comment_Line2";
        internal const string AutoGen_Comment_Line3 = "AutoGen_Comment_Line3";
        internal const string AutoGen_Comment_Line4 = "AutoGen_Comment_Line4";
        internal const string AutoGen_Comment_Line5 = "AutoGen_Comment_Line5";
        internal const string CantContainNullEntries = "CantContainNullEntries";
        internal const string InvalidPathCharsInChecksum = "InvalidPathCharsInChecksum";
        internal const string InvalidRegion = "InvalidRegion";
        internal const string Provider_does_not_support_options = "Provider_does_not_support_options";
        internal const string FileIntegrityCheckFailed = "FileIntegrityCheckFailed";
        internal const string MetaExtenderName = "MetaExtenderName";
        internal const string InvalidEnumArgument = "InvalidEnumArgument";
        internal const string InvalidArgument = "InvalidArgument";
        internal const string InvalidNullArgument = "InvalidNullArgument";
        internal const string InvalidNullEmptyArgument = "InvalidNullEmptyArgument";
        internal const string LicExceptionTypeOnly = "LicExceptionTypeOnly";
        internal const string LicExceptionTypeAndInstance = "LicExceptionTypeAndInstance";
        internal const string LicMgrContextCannotBeChanged = "LicMgrContextCannotBeChanged";
        internal const string LicMgrAlreadyLocked = "LicMgrAlreadyLocked";
        internal const string LicMgrDifferentUser = "LicMgrDifferentUser";
        internal const string InvalidElementType = "InvalidElementType";
        internal const string InvalidIdentifier = "InvalidIdentifier";
        internal const string ExecFailedToCreate = "ExecFailedToCreate";
        internal const string ExecTimeout = "ExecTimeout";
        internal const string ExecBadreturn = "ExecBadreturn";
        internal const string ExecCantGetRetCode = "ExecCantGetRetCode";
        internal const string ExecCantExec = "ExecCantExec";
        internal const string ExecCantRevert = "ExecCantRevert";
        internal const string CompilerNotFound = "CompilerNotFound";
        internal const string DuplicateFileName = "DuplicateFileName";
        internal const string CollectionReadOnly = "CollectionReadOnly";
        internal const string BitVectorFull = "BitVectorFull";
        internal const string ArrayConverterText = "ArrayConverterText";
        internal const string CollectionConverterText = "CollectionConverterText";
        internal const string MultilineStringConverterText = "MultilineStringConverterText";
        internal const string CultureInfoConverterDefaultCultureString = "CultureInfoConverterDefaultCultureString";
        internal const string CultureInfoConverterInvalidCulture = "CultureInfoConverterInvalidCulture";
        internal const string InvalidPrimitive = "InvalidPrimitive";
        internal const string TimerInvalidInterval = "TimerInvalidInterval";
        internal const string TraceSwitchLevelTooHigh = "TraceSwitchLevelTooHigh";
        internal const string TraceSwitchLevelTooLow = "TraceSwitchLevelTooLow";
        internal const string TraceSwitchInvalidLevel = "TraceSwitchInvalidLevel";
        internal const string TraceListenerIndentSize = "TraceListenerIndentSize";
        internal const string TraceListenerFail = "TraceListenerFail";
        internal const string TraceAsTraceSource = "TraceAsTraceSource";
        internal const string InvalidLowBoundArgument = "InvalidLowBoundArgument";
        internal const string DuplicateComponentName = "DuplicateComponentName";
        internal const string NotImplemented = "NotImplemented";
        internal const string OutOfMemory = "OutOfMemory";
        internal const string EOF = "EOF";
        internal const string IOError = "IOError";
        internal const string BadChar = "BadChar";
        internal const string toStringNone = "toStringNone";
        internal const string toStringUnknown = "toStringUnknown";
        internal const string InvalidEnum = "InvalidEnum";
        internal const string IndexOutOfRange = "IndexOutOfRange";
        internal const string ErrorPropertyAccessorException = "ErrorPropertyAccessorException";
        internal const string InvalidOperation = "InvalidOperation";
        internal const string EmptyStack = "EmptyStack";
        internal const string PerformanceCounterDesc = "PerformanceCounterDesc";
        internal const string PCCategoryName = "PCCategoryName";
        internal const string PCCounterName = "PCCounterName";
        internal const string PCInstanceName = "PCInstanceName";
        internal const string PCMachineName = "PCMachineName";
        internal const string PCInstanceLifetime = "PCInstanceLifetime";
        internal const string PropertyCategoryAction = "PropertyCategoryAction";
        internal const string PropertyCategoryAppearance = "PropertyCategoryAppearance";
        internal const string PropertyCategoryAsynchronous = "PropertyCategoryAsynchronous";
        internal const string PropertyCategoryBehavior = "PropertyCategoryBehavior";
        internal const string PropertyCategoryData = "PropertyCategoryData";
        internal const string PropertyCategoryDDE = "PropertyCategoryDDE";
        internal const string PropertyCategoryDesign = "PropertyCategoryDesign";
        internal const string PropertyCategoryDragDrop = "PropertyCategoryDragDrop";
        internal const string PropertyCategoryFocus = "PropertyCategoryFocus";
        internal const string PropertyCategoryFont = "PropertyCategoryFont";
        internal const string PropertyCategoryFormat = "PropertyCategoryFormat";
        internal const string PropertyCategoryKey = "PropertyCategoryKey";
        internal const string PropertyCategoryList = "PropertyCategoryList";
        internal const string PropertyCategoryLayout = "PropertyCategoryLayout";
        internal const string PropertyCategoryDefault = "PropertyCategoryDefault";
        internal const string PropertyCategoryMouse = "PropertyCategoryMouse";
        internal const string PropertyCategoryPosition = "PropertyCategoryPosition";
        internal const string PropertyCategoryText = "PropertyCategoryText";
        internal const string PropertyCategoryScale = "PropertyCategoryScale";
        internal const string PropertyCategoryWindowStyle = "PropertyCategoryWindowStyle";
        internal const string PropertyCategoryConfig = "PropertyCategoryConfig";
        internal const string ArgumentNull_ArrayWithNullElements = "ArgumentNull_ArrayWithNullElements";
        internal const string OnlyAllowedOnce = "OnlyAllowedOnce";
        internal const string BeginIndexNotNegative = "BeginIndexNotNegative";
        internal const string LengthNotNegative = "LengthNotNegative";
        internal const string UnimplementedState = "UnimplementedState";
        internal const string UnexpectedOpcode = "UnexpectedOpcode";
        internal const string NoResultOnFailed = "NoResultOnFailed";
        internal const string UnterminatedBracket = "UnterminatedBracket";
        internal const string TooManyParens = "TooManyParens";
        internal const string NestedQuantify = "NestedQuantify";
        internal const string QuantifyAfterNothing = "QuantifyAfterNothing";
        internal const string InternalError = "InternalError";
        internal const string IllegalRange = "IllegalRange";
        internal const string NotEnoughParens = "NotEnoughParens";
        internal const string BadClassInCharRange = "BadClassInCharRange";
        internal const string ReversedCharRange = "ReversedCharRange";
        internal const string UndefinedReference = "UndefinedReference";
        internal const string MalformedReference = "MalformedReference";
        internal const string UnrecognizedGrouping = "UnrecognizedGrouping";
        internal const string UnterminatedComment = "UnterminatedComment";
        internal const string IllegalEndEscape = "IllegalEndEscape";
        internal const string MalformedNameRef = "MalformedNameRef";
        internal const string UndefinedBackref = "UndefinedBackref";
        internal const string UndefinedNameRef = "UndefinedNameRef";
        internal const string TooFewHex = "TooFewHex";
        internal const string MissingControl = "MissingControl";
        internal const string UnrecognizedControl = "UnrecognizedControl";
        internal const string UnrecognizedEscape = "UnrecognizedEscape";
        internal const string IllegalCondition = "IllegalCondition";
        internal const string TooManyAlternates = "TooManyAlternates";
        internal const string MakeException = "MakeException";
        internal const string IncompleteSlashP = "IncompleteSlashP";
        internal const string MalformedSlashP = "MalformedSlashP";
        internal const string InvalidGroupName = "InvalidGroupName";
        internal const string CapnumNotZero = "CapnumNotZero";
        internal const string AlternationCantCapture = "AlternationCantCapture";
        internal const string AlternationCantHaveComment = "AlternationCantHaveComment";
        internal const string CaptureGroupOutOfRange = "CaptureGroupOutOfRange";
        internal const string SubtractionMustBeLast = "SubtractionMustBeLast";
        internal const string UnknownProperty = "UnknownProperty";
        internal const string ReplacementError = "ReplacementError";
        internal const string CountTooSmall = "CountTooSmall";
        internal const string EnumNotStarted = "EnumNotStarted";
        internal const string Arg_InvalidArrayType = "Arg_InvalidArrayType";
        internal const string Arg_RankMultiDimNotSupported = "Arg_RankMultiDimNotSupported";
        internal const string RegexMatchTimeoutException_Occurred = "RegexMatchTimeoutException_Occurred";
        internal const string IllegalDefaultRegexMatchTimeoutInAppDomain = "IllegalDefaultRegexMatchTimeoutInAppDomain";
        internal const string FileObject_AlreadyOpen = "FileObject_AlreadyOpen";
        internal const string FileObject_Closed = "FileObject_Closed";
        internal const string FileObject_NotWhileWriting = "FileObject_NotWhileWriting";
        internal const string FileObject_FileDoesNotExist = "FileObject_FileDoesNotExist";
        internal const string FileObject_MustBeClosed = "FileObject_MustBeClosed";
        internal const string FileObject_MustBeFileName = "FileObject_MustBeFileName";
        internal const string FileObject_InvalidInternalState = "FileObject_InvalidInternalState";
        internal const string FileObject_PathNotSet = "FileObject_PathNotSet";
        internal const string FileObject_Reading = "FileObject_Reading";
        internal const string FileObject_Writing = "FileObject_Writing";
        internal const string FileObject_InvalidEnumeration = "FileObject_InvalidEnumeration";
        internal const string FileObject_NoReset = "FileObject_NoReset";
        internal const string DirectoryObject_MustBeDirName = "DirectoryObject_MustBeDirName";
        internal const string DirectoryObjectPathDescr = "DirectoryObjectPathDescr";
        internal const string FileObjectDetectEncodingDescr = "FileObjectDetectEncodingDescr";
        internal const string FileObjectEncodingDescr = "FileObjectEncodingDescr";
        internal const string FileObjectPathDescr = "FileObjectPathDescr";
        internal const string Arg_EnumIllegalVal = "Arg_EnumIllegalVal";
        internal const string Arg_OutOfRange_NeedNonNegNum = "Arg_OutOfRange_NeedNonNegNum";
        internal const string Argument_InvalidPermissionState = "Argument_InvalidPermissionState";
        internal const string Argument_InvalidOidValue = "Argument_InvalidOidValue";
        internal const string Argument_WrongType = "Argument_WrongType";
        internal const string Arg_EmptyOrNullString = "Arg_EmptyOrNullString";
        internal const string Arg_EmptyOrNullArray = "Arg_EmptyOrNullArray";
        internal const string Argument_InvalidClassAttribute = "Argument_InvalidClassAttribute";
        internal const string Argument_InvalidNameType = "Argument_InvalidNameType";
        internal const string InvalidOperation_EnumNotStarted = "InvalidOperation_EnumNotStarted";
        internal const string InvalidOperation_DuplicateItemNotAllowed = "InvalidOperation_DuplicateItemNotAllowed";
        internal const string Cryptography_Asn_MismatchedOidInCollection = "Cryptography_Asn_MismatchedOidInCollection";
        internal const string Cryptography_Cms_Envelope_Empty_Content = "Cryptography_Cms_Envelope_Empty_Content";
        internal const string Cryptography_Cms_Invalid_Recipient_Info_Type = "Cryptography_Cms_Invalid_Recipient_Info_Type";
        internal const string Cryptography_Cms_Invalid_Subject_Identifier_Type = "Cryptography_Cms_Invalid_Subject_Identifier_Type";

        internal const string Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch =
                "Cryptography_Cms_Invalid_Subject_Identifier_Type_Value_Mismatch";

        internal const string Cryptography_Cms_Key_Agree_Date_Not_Available = "Cryptography_Cms_Key_Agree_Date_Not_Available";

        internal const string Cryptography_Cms_Key_Agree_Other_Key_Attribute_Not_Available =
                "Cryptography_Cms_Key_Agree_Other_Key_Attribute_Not_Available";

        internal const string Cryptography_Cms_MessageNotSigned = "Cryptography_Cms_MessageNotSigned";
        internal const string Cryptography_Cms_MessageNotSignedByNoSignature = "Cryptography_Cms_MessageNotSignedByNoSignature";
        internal const string Cryptography_Cms_MessageNotEncrypted = "Cryptography_Cms_MessageNotEncrypted";
        internal const string Cryptography_Cms_Not_Supported = "Cryptography_Cms_Not_Supported";
        internal const string Cryptography_Cms_RecipientCertificateNotFound = "Cryptography_Cms_RecipientCertificateNotFound";
        internal const string Cryptography_Cms_Sign_Empty_Content = "Cryptography_Cms_Sign_Empty_Content";
        internal const string Cryptography_Cms_Sign_No_Signature_First_Signer = "Cryptography_Cms_Sign_No_Signature_First_Signer";
        internal const string Cryptography_DpApi_InvalidMemoryLength = "Cryptography_DpApi_InvalidMemoryLength";
        internal const string Cryptography_InvalidHandle = "Cryptography_InvalidHandle";
        internal const string Cryptography_InvalidContextHandle = "Cryptography_InvalidContextHandle";
        internal const string Cryptography_InvalidStoreHandle = "Cryptography_InvalidStoreHandle";
        internal const string Cryptography_Oid_InvalidValue = "Cryptography_Oid_InvalidValue";
        internal const string Cryptography_Pkcs9_ExplicitAddNotAllowed = "Cryptography_Pkcs9_ExplicitAddNotAllowed";
        internal const string Cryptography_Pkcs9_InvalidOid = "Cryptography_Pkcs9_InvalidOid";
        internal const string Cryptography_Pkcs9_MultipleSigningTimeNotAllowed = "Cryptography_Pkcs9_MultipleSigningTimeNotAllowed";
        internal const string Cryptography_Pkcs9_AttributeMismatch = "Cryptography_Pkcs9_AttributeMismatch";
        internal const string Cryptography_X509_AddFailed = "Cryptography_X509_AddFailed";
        internal const string Cryptography_X509_BadEncoding = "Cryptography_X509_BadEncoding";
        internal const string Cryptography_X509_ExportFailed = "Cryptography_X509_ExportFailed";
        internal const string Cryptography_X509_ExtensionMismatch = "Cryptography_X509_ExtensionMismatch";
        internal const string Cryptography_X509_InvalidFindType = "Cryptography_X509_InvalidFindType";
        internal const string Cryptography_X509_InvalidFindValue = "Cryptography_X509_InvalidFindValue";
        internal const string Cryptography_X509_InvalidEncodingFormat = "Cryptography_X509_InvalidEncodingFormat";
        internal const string Cryptography_X509_InvalidContentType = "Cryptography_X509_InvalidContentType";
        internal const string Cryptography_X509_KeyMismatch = "Cryptography_X509_KeyMismatch";
        internal const string Cryptography_X509_RemoveFailed = "Cryptography_X509_RemoveFailed";
        internal const string Cryptography_X509_StoreNotOpen = "Cryptography_X509_StoreNotOpen";
        internal const string Environment_NotInteractive = "Environment_NotInteractive";
        internal const string NotSupported_InvalidKeyImpl = "NotSupported_InvalidKeyImpl";
        internal const string NotSupported_KeyAlgorithm = "NotSupported_KeyAlgorithm";
        internal const string NotSupported_PlatformRequiresNT = "NotSupported_PlatformRequiresNT";
        internal const string NotSupported_UnreadableStream = "NotSupported_UnreadableStream";
        internal const string Security_InvalidValue = "Security_InvalidValue";
        internal const string Unknown_Error = "Unknown_Error";
        internal const string security_ServiceNameCollection_EmptyServiceName = "security_ServiceNameCollection_EmptyServiceName";

        internal const string security_ExtendedProtectionPolicy_UseDifferentConstructorForNever =
                "security_ExtendedProtectionPolicy_UseDifferentConstructorForNever";

        internal const string security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection =
                "security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection";

        internal const string security_ExtendedProtection_NoOSSupport = "security_ExtendedProtection_NoOSSupport";
        internal const string net_nonClsCompliantException = "net_nonClsCompliantException";
        internal const string net_illegalConfigWith = "net_illegalConfigWith";
        internal const string net_illegalConfigWithout = "net_illegalConfigWithout";
        internal const string net_baddate = "net_baddate";
        internal const string net_writestarted = "net_writestarted";
        internal const string net_clsmall = "net_clsmall";
        internal const string net_reqsubmitted = "net_reqsubmitted";
        internal const string net_rspsubmitted = "net_rspsubmitted";
        internal const string net_ftp_no_http_cmd = "net_ftp_no_http_cmd";
        internal const string net_ftp_invalid_method_name = "net_ftp_invalid_method_name";
        internal const string net_ftp_invalid_renameto = "net_ftp_invalid_renameto";
        internal const string net_ftp_no_defaultcreds = "net_ftp_no_defaultcreds";
        internal const string net_ftpnoresponse = "net_ftpnoresponse";
        internal const string net_ftp_response_invalid_format = "net_ftp_response_invalid_format";
        internal const string net_ftp_no_offsetforhttp = "net_ftp_no_offsetforhttp";
        internal const string net_ftp_invalid_uri = "net_ftp_invalid_uri";
        internal const string net_ftp_invalid_status_response = "net_ftp_invalid_status_response";
        internal const string net_ftp_server_failed_passive = "net_ftp_server_failed_passive";
        internal const string net_ftp_active_address_different = "net_ftp_active_address_different";
        internal const string net_ftp_proxy_does_not_support_ssl = "net_ftp_proxy_does_not_support_ssl";
        internal const string net_ftp_invalid_response_filename = "net_ftp_invalid_response_filename";
        internal const string net_ftp_unsupported_method = "net_ftp_unsupported_method";
        internal const string net_resubmitcanceled = "net_resubmitcanceled";
        internal const string net_redirect_perm = "net_redirect_perm";
        internal const string net_resubmitprotofailed = "net_resubmitprotofailed";
        internal const string net_needchunked = "net_needchunked";
        internal const string net_nochunked = "net_nochunked";
        internal const string net_nochunkuploadonhttp10 = "net_nochunkuploadonhttp10";
        internal const string net_connarg = "net_connarg";
        internal const string net_no100 = "net_no100";
        internal const string net_fromto = "net_fromto";
        internal const string net_rangetoosmall = "net_rangetoosmall";
        internal const string net_entitytoobig = "net_entitytoobig";
        internal const string net_invalidversion = "net_invalidversion";
        internal const string net_invalidstatus = "net_invalidstatus";
        internal const string net_toosmall = "net_toosmall";
        internal const string net_toolong = "net_toolong";
        internal const string net_connclosed = "net_connclosed";
        internal const string net_noseek = "net_noseek";
        internal const string net_servererror = "net_servererror";
        internal const string net_nouploadonget = "net_nouploadonget";
        internal const string net_mutualauthfailed = "net_mutualauthfailed";
        internal const string net_invasync = "net_invasync";
        internal const string net_inasync = "net_inasync";
        internal const string net_mustbeuri = "net_mustbeuri";
        internal const string net_format_shexp = "net_format_shexp";
        internal const string net_cannot_load_proxy_helper = "net_cannot_load_proxy_helper";
        internal const string net_invalid_host = "net_invalid_host";
        internal const string net_repcall = "net_repcall";
        internal const string net_wrongversion = "net_wrongversion";
        internal const string net_badmethod = "net_badmethod";
        internal const string net_io_notenoughbyteswritten = "net_io_notenoughbyteswritten";
        internal const string net_io_timeout_use_ge_zero = "net_io_timeout_use_ge_zero";
        internal const string net_io_timeout_use_gt_zero = "net_io_timeout_use_gt_zero";
        internal const string net_io_no_0timeouts = "net_io_no_0timeouts";
        internal const string net_requestaborted = "net_requestaborted";
        internal const string net_tooManyRedirections = "net_tooManyRedirections";
        internal const string net_authmodulenotregistered = "net_authmodulenotregistered";
        internal const string net_authschemenotregistered = "net_authschemenotregistered";
        internal const string net_proxyschemenotsupported = "net_proxyschemenotsupported";
        internal const string net_maxsrvpoints = "net_maxsrvpoints";
        internal const string net_unknown_prefix = "net_unknown_prefix";
        internal const string net_notconnected = "net_notconnected";
        internal const string net_notstream = "net_notstream";
        internal const string net_timeout = "net_timeout";
        internal const string net_nocontentlengthonget = "net_nocontentlengthonget";
        internal const string net_contentlengthmissing = "net_contentlengthmissing";
        internal const string net_nonhttpproxynotallowed = "net_nonhttpproxynotallowed";
        internal const string net_nottoken = "net_nottoken";
        internal const string net_rangetype = "net_rangetype";
        internal const string net_need_writebuffering = "net_need_writebuffering";
        internal const string net_securitypackagesupport = "net_securitypackagesupport";
        internal const string net_securityprotocolnotsupported = "net_securityprotocolnotsupported";
        internal const string net_nodefaultcreds = "net_nodefaultcreds";
        internal const string net_stopped = "net_stopped";
        internal const string net_udpconnected = "net_udpconnected";
        internal const string net_readonlystream = "net_readonlystream";
        internal const string net_writeonlystream = "net_writeonlystream";
        internal const string net_no_concurrent_io_allowed = "net_no_concurrent_io_allowed";
        internal const string net_needmorethreads = "net_needmorethreads";
        internal const string net_MethodNotImplementedException = "net_MethodNotImplementedException";
        internal const string net_PropertyNotImplementedException = "net_PropertyNotImplementedException";
        internal const string net_MethodNotSupportedException = "net_MethodNotSupportedException";
        internal const string net_PropertyNotSupportedException = "net_PropertyNotSupportedException";
        internal const string net_ProtocolNotSupportedException = "net_ProtocolNotSupportedException";
        internal const string net_SelectModeNotSupportedException = "net_SelectModeNotSupportedException";
        internal const string net_InvalidSocketHandle = "net_InvalidSocketHandle";
        internal const string net_InvalidAddressFamily = "net_InvalidAddressFamily";
        internal const string net_InvalidEndPointAddressFamily = "net_InvalidEndPointAddressFamily";
        internal const string net_InvalidSocketAddressSize = "net_InvalidSocketAddressSize";
        internal const string net_invalidAddressList = "net_invalidAddressList";
        internal const string net_invalidPingBufferSize = "net_invalidPingBufferSize";
        internal const string net_cant_perform_during_shutdown = "net_cant_perform_during_shutdown";
        internal const string net_cant_create_environment = "net_cant_create_environment";
        internal const string net_completed_result = "net_completed_result";
        internal const string net_protocol_invalid_family = "net_protocol_invalid_family";
        internal const string net_protocol_invalid_multicast_family = "net_protocol_invalid_multicast_family";
        internal const string net_empty_osinstalltype = "net_empty_osinstalltype";
        internal const string net_unknown_osinstalltype = "net_unknown_osinstalltype";
        internal const string net_cant_determine_osinstalltype = "net_cant_determine_osinstalltype";
        internal const string net_osinstalltype = "net_osinstalltype";
        internal const string net_entire_body_not_written = "net_entire_body_not_written";
        internal const string net_must_provide_request_body = "net_must_provide_request_body";
        internal const string net_ssp_dont_support_cbt = "net_ssp_dont_support_cbt";
        internal const string net_sockets_zerolist = "net_sockets_zerolist";
        internal const string net_sockets_blocking = "net_sockets_blocking";
        internal const string net_sockets_useblocking = "net_sockets_useblocking";
        internal const string net_sockets_select = "net_sockets_select";
        internal const string net_sockets_toolarge_select = "net_sockets_toolarge_select";
        internal const string net_sockets_empty_select = "net_sockets_empty_select";
        internal const string net_sockets_mustbind = "net_sockets_mustbind";
        internal const string net_sockets_mustlisten = "net_sockets_mustlisten";
        internal const string net_sockets_mustnotlisten = "net_sockets_mustnotlisten";
        internal const string net_sockets_mustnotbebound = "net_sockets_mustnotbebound";
        internal const string net_sockets_namedmustnotbebound = "net_sockets_namedmustnotbebound";
        internal const string net_sockets_invalid_socketinformation = "net_sockets_invalid_socketinformation";
        internal const string net_sockets_invalid_ipaddress_length = "net_sockets_invalid_ipaddress_length";
        internal const string net_sockets_invalid_optionValue = "net_sockets_invalid_optionValue";
        internal const string net_sockets_invalid_optionValue_all = "net_sockets_invalid_optionValue_all";
        internal const string net_sockets_invalid_dnsendpoint = "net_sockets_invalid_dnsendpoint";
        internal const string net_sockets_disconnectedConnect = "net_sockets_disconnectedConnect";
        internal const string net_sockets_disconnectedAccept = "net_sockets_disconnectedAccept";
        internal const string net_tcplistener_mustbestopped = "net_tcplistener_mustbestopped";
        internal const string net_sockets_no_duplicate_async = "net_sockets_no_duplicate_async";
        internal const string net_socketopinprogress = "net_socketopinprogress";
        internal const string net_buffercounttoosmall = "net_buffercounttoosmall";
        internal const string net_multibuffernotsupported = "net_multibuffernotsupported";
        internal const string net_ambiguousbuffers = "net_ambiguousbuffers";
        internal const string net_sockets_ipv6only = "net_sockets_ipv6only";
        internal const string net_perfcounter_initialized_success = "net_perfcounter_initialized_success";
        internal const string net_perfcounter_initialized_error = "net_perfcounter_initialized_error";
        internal const string net_perfcounter_nocategory = "net_perfcounter_nocategory";
        internal const string net_perfcounter_initialization_started = "net_perfcounter_initialization_started";
        internal const string net_perfcounter_cant_queue_workitem = "net_perfcounter_cant_queue_workitem";
        internal const string net_config_proxy = "net_config_proxy";
        internal const string net_config_proxy_module_not_public = "net_config_proxy_module_not_public";
        internal const string net_config_authenticationmodules = "net_config_authenticationmodules";
        internal const string net_config_webrequestmodules = "net_config_webrequestmodules";
        internal const string net_config_requestcaching = "net_config_requestcaching";
        internal const string net_config_section_permission = "net_config_section_permission";
        internal const string net_config_element_permission = "net_config_element_permission";
        internal const string net_config_property_permission = "net_config_property_permission";
        internal const string net_WebResponseParseError_InvalidHeaderName = "net_WebResponseParseError_InvalidHeaderName";
        internal const string net_WebResponseParseError_InvalidContentLength = "net_WebResponseParseError_InvalidContentLength";
        internal const string net_WebResponseParseError_IncompleteHeaderLine = "net_WebResponseParseError_IncompleteHeaderLine";
        internal const string net_WebResponseParseError_CrLfError = "net_WebResponseParseError_CrLfError";
        internal const string net_WebResponseParseError_InvalidChunkFormat = "net_WebResponseParseError_InvalidChunkFormat";
        internal const string net_WebResponseParseError_UnexpectedServerResponse = "net_WebResponseParseError_UnexpectedServerResponse";
        internal const string net_webstatus_Success = "net_webstatus_Success";
        internal const string net_webstatus_NameResolutionFailure = "net_webstatus_NameResolutionFailure";
        internal const string net_webstatus_ConnectFailure = "net_webstatus_ConnectFailure";
        internal const string net_webstatus_ReceiveFailure = "net_webstatus_ReceiveFailure";
        internal const string net_webstatus_SendFailure = "net_webstatus_SendFailure";
        internal const string net_webstatus_PipelineFailure = "net_webstatus_PipelineFailure";
        internal const string net_webstatus_RequestCanceled = "net_webstatus_RequestCanceled";
        internal const string net_webstatus_ConnectionClosed = "net_webstatus_ConnectionClosed";
        internal const string net_webstatus_TrustFailure = "net_webstatus_TrustFailure";
        internal const string net_webstatus_SecureChannelFailure = "net_webstatus_SecureChannelFailure";
        internal const string net_webstatus_ServerProtocolViolation = "net_webstatus_ServerProtocolViolation";
        internal const string net_webstatus_KeepAliveFailure = "net_webstatus_KeepAliveFailure";
        internal const string net_webstatus_ProxyNameResolutionFailure = "net_webstatus_ProxyNameResolutionFailure";
        internal const string net_webstatus_MessageLengthLimitExceeded = "net_webstatus_MessageLengthLimitExceeded";
        internal const string net_webstatus_CacheEntryNotFound = "net_webstatus_CacheEntryNotFound";
        internal const string net_webstatus_RequestProhibitedByCachePolicy = "net_webstatus_RequestProhibitedByCachePolicy";
        internal const string net_webstatus_Timeout = "net_webstatus_Timeout";
        internal const string net_webstatus_RequestProhibitedByProxy = "net_webstatus_RequestProhibitedByProxy";
        internal const string net_InvalidStatusCode = "net_InvalidStatusCode";
        internal const string net_ftpstatuscode_ServiceNotAvailable = "net_ftpstatuscode_ServiceNotAvailable";
        internal const string net_ftpstatuscode_CantOpenData = "net_ftpstatuscode_CantOpenData";
        internal const string net_ftpstatuscode_ConnectionClosed = "net_ftpstatuscode_ConnectionClosed";
        internal const string net_ftpstatuscode_ActionNotTakenFileUnavailableOrBusy = "net_ftpstatuscode_ActionNotTakenFileUnavailableOrBusy";
        internal const string net_ftpstatuscode_ActionAbortedLocalProcessingError = "net_ftpstatuscode_ActionAbortedLocalProcessingError";
        internal const string net_ftpstatuscode_ActionNotTakenInsufficentSpace = "net_ftpstatuscode_ActionNotTakenInsufficentSpace";
        internal const string net_ftpstatuscode_CommandSyntaxError = "net_ftpstatuscode_CommandSyntaxError";
        internal const string net_ftpstatuscode_ArgumentSyntaxError = "net_ftpstatuscode_ArgumentSyntaxError";
        internal const string net_ftpstatuscode_CommandNotImplemented = "net_ftpstatuscode_CommandNotImplemented";
        internal const string net_ftpstatuscode_BadCommandSequence = "net_ftpstatuscode_BadCommandSequence";
        internal const string net_ftpstatuscode_NotLoggedIn = "net_ftpstatuscode_NotLoggedIn";
        internal const string net_ftpstatuscode_AccountNeeded = "net_ftpstatuscode_AccountNeeded";
        internal const string net_ftpstatuscode_ActionNotTakenFileUnavailable = "net_ftpstatuscode_ActionNotTakenFileUnavailable";
        internal const string net_ftpstatuscode_ActionAbortedUnknownPageType = "net_ftpstatuscode_ActionAbortedUnknownPageType";
        internal const string net_ftpstatuscode_FileActionAborted = "net_ftpstatuscode_FileActionAborted";
        internal const string net_ftpstatuscode_ActionNotTakenFilenameNotAllowed = "net_ftpstatuscode_ActionNotTakenFilenameNotAllowed";
        internal const string net_httpstatuscode_NoContent = "net_httpstatuscode_NoContent";
        internal const string net_httpstatuscode_NonAuthoritativeInformation = "net_httpstatuscode_NonAuthoritativeInformation";
        internal const string net_httpstatuscode_ResetContent = "net_httpstatuscode_ResetContent";
        internal const string net_httpstatuscode_PartialContent = "net_httpstatuscode_PartialContent";
        internal const string net_httpstatuscode_MultipleChoices = "net_httpstatuscode_MultipleChoices";
        internal const string net_httpstatuscode_Ambiguous = "net_httpstatuscode_Ambiguous";
        internal const string net_httpstatuscode_MovedPermanently = "net_httpstatuscode_MovedPermanently";
        internal const string net_httpstatuscode_Moved = "net_httpstatuscode_Moved";
        internal const string net_httpstatuscode_Found = "net_httpstatuscode_Found";
        internal const string net_httpstatuscode_Redirect = "net_httpstatuscode_Redirect";
        internal const string net_httpstatuscode_SeeOther = "net_httpstatuscode_SeeOther";
        internal const string net_httpstatuscode_RedirectMethod = "net_httpstatuscode_RedirectMethod";
        internal const string net_httpstatuscode_NotModified = "net_httpstatuscode_NotModified";
        internal const string net_httpstatuscode_UseProxy = "net_httpstatuscode_UseProxy";
        internal const string net_httpstatuscode_TemporaryRedirect = "net_httpstatuscode_TemporaryRedirect";
        internal const string net_httpstatuscode_RedirectKeepVerb = "net_httpstatuscode_RedirectKeepVerb";
        internal const string net_httpstatuscode_BadRequest = "net_httpstatuscode_BadRequest";
        internal const string net_httpstatuscode_Unauthorized = "net_httpstatuscode_Unauthorized";
        internal const string net_httpstatuscode_PaymentRequired = "net_httpstatuscode_PaymentRequired";
        internal const string net_httpstatuscode_Forbidden = "net_httpstatuscode_Forbidden";
        internal const string net_httpstatuscode_NotFound = "net_httpstatuscode_NotFound";
        internal const string net_httpstatuscode_MethodNotAllowed = "net_httpstatuscode_MethodNotAllowed";
        internal const string net_httpstatuscode_NotAcceptable = "net_httpstatuscode_NotAcceptable";
        internal const string net_httpstatuscode_ProxyAuthenticationRequired = "net_httpstatuscode_ProxyAuthenticationRequired";
        internal const string net_httpstatuscode_RequestTimeout = "net_httpstatuscode_RequestTimeout";
        internal const string net_httpstatuscode_Conflict = "net_httpstatuscode_Conflict";
        internal const string net_httpstatuscode_Gone = "net_httpstatuscode_Gone";
        internal const string net_httpstatuscode_LengthRequired = "net_httpstatuscode_LengthRequired";
        internal const string net_httpstatuscode_InternalServerError = "net_httpstatuscode_InternalServerError";
        internal const string net_httpstatuscode_NotImplemented = "net_httpstatuscode_NotImplemented";
        internal const string net_httpstatuscode_BadGateway = "net_httpstatuscode_BadGateway";
        internal const string net_httpstatuscode_ServiceUnavailable = "net_httpstatuscode_ServiceUnavailable";
        internal const string net_httpstatuscode_GatewayTimeout = "net_httpstatuscode_GatewayTimeout";
        internal const string net_httpstatuscode_HttpVersionNotSupported = "net_httpstatuscode_HttpVersionNotSupported";
        internal const string net_uri_BadScheme = "net_uri_BadScheme";
        internal const string net_uri_BadFormat = "net_uri_BadFormat";
        internal const string net_uri_BadUserPassword = "net_uri_BadUserPassword";
        internal const string net_uri_BadHostName = "net_uri_BadHostName";
        internal const string net_uri_BadAuthority = "net_uri_BadAuthority";
        internal const string net_uri_BadAuthorityTerminator = "net_uri_BadAuthorityTerminator";
        internal const string net_uri_EmptyUri = "net_uri_EmptyUri";
        internal const string net_uri_BadString = "net_uri_BadString";
        internal const string net_uri_MustRootedPath = "net_uri_MustRootedPath";
        internal const string net_uri_BadPort = "net_uri_BadPort";
        internal const string net_uri_SizeLimit = "net_uri_SizeLimit";
        internal const string net_uri_SchemeLimit = "net_uri_SchemeLimit";
        internal const string net_uri_NotAbsolute = "net_uri_NotAbsolute";
        internal const string net_uri_PortOutOfRange = "net_uri_PortOutOfRange";
        internal const string net_uri_UserDrivenParsing = "net_uri_UserDrivenParsing";
        internal const string net_uri_AlreadyRegistered = "net_uri_AlreadyRegistered";
        internal const string net_uri_NeedFreshParser = "net_uri_NeedFreshParser";
        internal const string net_uri_CannotCreateRelative = "net_uri_CannotCreateRelative";
        internal const string net_uri_InvalidUriKind = "net_uri_InvalidUriKind";
        internal const string net_uri_BadUnicodeHostForIdn = "net_uri_BadUnicodeHostForIdn";
        internal const string net_uri_GenericAuthorityNotDnsSafe = "net_uri_GenericAuthorityNotDnsSafe";
        internal const string net_uri_NotJustSerialization = "net_uri_NotJustSerialization";
        internal const string net_emptystringcall = "net_emptystringcall";
        internal const string net_emptystringset = "net_emptystringset";
        internal const string net_headers_req = "net_headers_req";
        internal const string net_headers_rsp = "net_headers_rsp";
        internal const string net_headers_toolong = "net_headers_toolong";
        internal const string net_WebHeaderInvalidControlChars = "net_WebHeaderInvalidControlChars";
        internal const string net_WebHeaderInvalidCRLFChars = "net_WebHeaderInvalidCRLFChars";
        internal const string net_WebHeaderInvalidHeaderChars = "net_WebHeaderInvalidHeaderChars";
        internal const string net_WebHeaderInvalidNonAsciiChars = "net_WebHeaderInvalidNonAsciiChars";
        internal const string net_WebHeaderMissingColon = "net_WebHeaderMissingColon";
        internal const string net_headerrestrict = "net_headerrestrict";
        internal const string net_io_completionportwasbound = "net_io_completionportwasbound";
        internal const string net_io_writefailure = "net_io_writefailure";
        internal const string net_io_readfailure = "net_io_readfailure";
        internal const string net_io_connectionclosed = "net_io_connectionclosed";
        internal const string net_io_transportfailure = "net_io_transportfailure";
        internal const string net_io_internal_bind = "net_io_internal_bind";
        internal const string net_io_invalidasyncresult = "net_io_invalidasyncresult";
        internal const string net_io_invalidnestedcall = "net_io_invalidnestedcall";
        internal const string net_io_invalidendcall = "net_io_invalidendcall";
        internal const string net_io_must_be_rw_stream = "net_io_must_be_rw_stream";
        internal const string net_io_header_id = "net_io_header_id";
        internal const string net_io_out_range = "net_io_out_range";
        internal const string net_io_encrypt = "net_io_encrypt";
        internal const string net_io_decrypt = "net_io_decrypt";
        internal const string net_io_read = "net_io_read";
        internal const string net_io_write = "net_io_write";
        internal const string net_io_eof = "net_io_eof";
        internal const string net_io_async_result = "net_io_async_result";
        internal const string net_listener_mustcall = "net_listener_mustcall";
        internal const string net_listener_mustcompletecall = "net_listener_mustcompletecall";
        internal const string net_listener_callinprogress = "net_listener_callinprogress";
        internal const string net_listener_scheme = "net_listener_scheme";
        internal const string net_listener_host = "net_listener_host";
        internal const string net_listener_slash = "net_listener_slash";
        internal const string net_listener_repcall = "net_listener_repcall";
        internal const string net_listener_invalid_cbt_type = "net_listener_invalid_cbt_type";
        internal const string net_listener_no_spns = "net_listener_no_spns";
        internal const string net_listener_cannot_set_custom_cbt = "net_listener_cannot_set_custom_cbt";
        internal const string net_listener_cbt_not_supported = "net_listener_cbt_not_supported";
        internal const string net_listener_detach_error = "net_listener_detach_error";
        internal const string net_listener_close_urlgroup_error = "net_listener_close_urlgroup_error";
        internal const string net_tls_version = "net_tls_version";
        internal const string net_perm_target = "net_perm_target";
        internal const string net_perm_both_regex = "net_perm_both_regex";
        internal const string net_perm_none = "net_perm_none";
        internal const string net_perm_attrib_count = "net_perm_attrib_count";
        internal const string net_perm_invalid_val = "net_perm_invalid_val";
        internal const string net_perm_attrib_multi = "net_perm_attrib_multi";
        internal const string net_perm_epname = "net_perm_epname";
        internal const string net_perm_invalid_val_in_element = "net_perm_invalid_val_in_element";
        internal const string net_invalid_ip_addr = "net_invalid_ip_addr";
        internal const string dns_bad_ip_address = "dns_bad_ip_address";
        internal const string net_bad_mac_address = "net_bad_mac_address";
        internal const string net_ping = "net_ping";
        internal const string net_bad_ip_address_prefix = "net_bad_ip_address_prefix";
        internal const string net_max_ip_address_list_length_exceeded = "net_max_ip_address_list_length_exceeded";
        internal const string net_ipv4_not_installed = "net_ipv4_not_installed";
        internal const string net_ipv6_not_installed = "net_ipv6_not_installed";
        internal const string net_webclient = "net_webclient";
        internal const string net_webclient_ContentType = "net_webclient_ContentType";
        internal const string net_webclient_Multipart = "net_webclient_Multipart";
        internal const string net_webclient_no_concurrent_io_allowed = "net_webclient_no_concurrent_io_allowed";
        internal const string net_webclient_invalid_baseaddress = "net_webclient_invalid_baseaddress";
        internal const string net_container_add_cookie = "net_container_add_cookie";
        internal const string net_cookie_invalid = "net_cookie_invalid";
        internal const string net_cookie_size = "net_cookie_size";
        internal const string net_cookie_parse_header = "net_cookie_parse_header";
        internal const string net_cookie_attribute = "net_cookie_attribute";
        internal const string net_cookie_format = "net_cookie_format";
        internal const string net_cookie_exists = "net_cookie_exists";
        internal const string net_cookie_capacity_range = "net_cookie_capacity_range";
        internal const string net_set_token = "net_set_token";
        internal const string net_revert_token = "net_revert_token";
        internal const string net_ssl_io_async_context = "net_ssl_io_async_context";
        internal const string net_ssl_io_encrypt = "net_ssl_io_encrypt";
        internal const string net_ssl_io_decrypt = "net_ssl_io_decrypt";
        internal const string net_ssl_io_context_expired = "net_ssl_io_context_expired";
        internal const string net_ssl_io_handshake_start = "net_ssl_io_handshake_start";
        internal const string net_ssl_io_handshake = "net_ssl_io_handshake";
        internal const string net_ssl_io_frame = "net_ssl_io_frame";
        internal const string net_ssl_io_corrupted = "net_ssl_io_corrupted";
        internal const string net_ssl_io_cert_validation = "net_ssl_io_cert_validation";
        internal const string net_ssl_io_invalid_end_call = "net_ssl_io_invalid_end_call";
        internal const string net_ssl_io_invalid_begin_call = "net_ssl_io_invalid_begin_call";
        internal const string net_ssl_io_no_server_cert = "net_ssl_io_no_server_cert";
        internal const string net_auth_bad_client_creds = "net_auth_bad_client_creds";
        internal const string net_auth_bad_client_creds_or_target_mismatch = "net_auth_bad_client_creds_or_target_mismatch";
        internal const string net_auth_context_expectation = "net_auth_context_expectation";
        internal const string net_auth_context_expectation_remote = "net_auth_context_expectation_remote";
        internal const string net_auth_supported_impl_levels = "net_auth_supported_impl_levels";
        internal const string net_auth_no_anonymous_support = "net_auth_no_anonymous_support";
        internal const string net_auth_reauth = "net_auth_reauth";
        internal const string net_auth_noauth = "net_auth_noauth";
        internal const string net_auth_client_server = "net_auth_client_server";
        internal const string net_auth_noencryption = "net_auth_noencryption";
        internal const string net_auth_SSPI = "net_auth_SSPI";
        internal const string net_auth_failure = "net_auth_failure";
        internal const string net_auth_eof = "net_auth_eof";
        internal const string net_auth_alert = "net_auth_alert";
        internal const string net_auth_ignored_reauth = "net_auth_ignored_reauth";
        internal const string net_auth_empty_read = "net_auth_empty_read";
        internal const string net_auth_message_not_encrypted = "net_auth_message_not_encrypted";
        internal const string net_auth_must_specify_extended_protection_scheme = "net_auth_must_specify_extended_protection_scheme";
        internal const string net_frame_size = "net_frame_size";
        internal const string net_frame_read_io = "net_frame_read_io";
        internal const string net_frame_read_size = "net_frame_read_size";
        internal const string net_frame_max_size = "net_frame_max_size";
        internal const string net_jscript_load = "net_jscript_load";
        internal const string net_proxy_not_gmt = "net_proxy_not_gmt";
        internal const string net_proxy_invalid_dayofweek = "net_proxy_invalid_dayofweek";
        internal const string net_proxy_invalid_url_format = "net_proxy_invalid_url_format";
        internal const string net_param_not_string = "net_param_not_string";
        internal const string net_value_cannot_be_negative = "net_value_cannot_be_negative";
        internal const string net_invalid_offset = "net_invalid_offset";
        internal const string net_offset_plus_count = "net_offset_plus_count";
        internal const string net_cannot_be_false = "net_cannot_be_false";
        internal const string net_invalid_enum = "net_invalid_enum";
        internal const string net_listener_already = "net_listener_already";
        internal const string net_cache_shadowstream_not_writable = "net_cache_shadowstream_not_writable";
        internal const string net_cache_validator_fail = "net_cache_validator_fail";
        internal const string net_cache_access_denied = "net_cache_access_denied";
        internal const string net_cache_validator_result = "net_cache_validator_result";
        internal const string net_cache_retrieve_failure = "net_cache_retrieve_failure";
        internal const string net_cache_not_supported_body = "net_cache_not_supported_body";
        internal const string net_cache_not_supported_command = "net_cache_not_supported_command";
        internal const string net_cache_not_accept_response = "net_cache_not_accept_response";
        internal const string net_cache_method_failed = "net_cache_method_failed";
        internal const string net_cache_key_failed = "net_cache_key_failed";
        internal const string net_cache_no_stream = "net_cache_no_stream";
        internal const string net_cache_unsupported_partial_stream = "net_cache_unsupported_partial_stream";
        internal const string net_cache_not_configured = "net_cache_not_configured";
        internal const string net_cache_non_seekable_stream_not_supported = "net_cache_non_seekable_stream_not_supported";
        internal const string net_invalid_cast = "net_invalid_cast";
        internal const string net_collection_readonly = "net_collection_readonly";
        internal const string net_not_ipermission = "net_not_ipermission";
        internal const string net_no_classname = "net_no_classname";
        internal const string net_no_typename = "net_no_typename";
        internal const string net_array_too_small = "net_array_too_small";
        internal const string net_servicePointAddressNotSupportedInHostMode = "net_servicePointAddressNotSupportedInHostMode";
        internal const string net_Websockets_AlreadyOneOutstandingOperation = "net_Websockets_AlreadyOneOutstandingOperation";
        internal const string net_Websockets_WebSocketBaseFaulted = "net_Websockets_WebSocketBaseFaulted";
        internal const string net_WebSockets_NativeSendResponseHeaders = "net_WebSockets_NativeSendResponseHeaders";
        internal const string net_WebSockets_Generic = "net_WebSockets_Generic";
        internal const string net_WebSockets_NotAWebSocket_Generic = "net_WebSockets_NotAWebSocket_Generic";
        internal const string net_WebSockets_UnsupportedWebSocketVersion_Generic = "net_WebSockets_UnsupportedWebSocketVersion_Generic";
        internal const string net_WebSockets_HeaderError_Generic = "net_WebSockets_HeaderError_Generic";
        internal const string net_WebSockets_UnsupportedProtocol_Generic = "net_WebSockets_UnsupportedProtocol_Generic";
        internal const string net_WebSockets_UnsupportedPlatform = "net_WebSockets_UnsupportedPlatform";
        internal const string net_WebSockets_AcceptNotAWebSocket = "net_WebSockets_AcceptNotAWebSocket";
        internal const string net_WebSockets_AcceptUnsupportedWebSocketVersion = "net_WebSockets_AcceptUnsupportedWebSocketVersion";
        internal const string net_WebSockets_AcceptHeaderNotFound = "net_WebSockets_AcceptHeaderNotFound";
        internal const string net_WebSockets_AcceptUnsupportedProtocol = "net_WebSockets_AcceptUnsupportedProtocol";
        internal const string net_WebSockets_ClientAcceptingNoProtocols = "net_WebSockets_ClientAcceptingNoProtocols";
        internal const string net_WebSockets_ClientSecWebSocketProtocolsBlank = "net_WebSockets_ClientSecWebSocketProtocolsBlank";
        internal const string net_WebSockets_ArgumentOutOfRange_TooSmall = "net_WebSockets_ArgumentOutOfRange_TooSmall";
        internal const string net_WebSockets_ArgumentOutOfRange_InternalBuffer = "net_WebSockets_ArgumentOutOfRange_InternalBuffer";
        internal const string net_WebSockets_ArgumentOutOfRange_TooBig = "net_WebSockets_ArgumentOutOfRange_TooBig";
        internal const string net_WebSockets_InvalidState_Generic = "net_WebSockets_InvalidState_Generic";
        internal const string net_WebSockets_InvalidState_ClosedOrAborted = "net_WebSockets_InvalidState_ClosedOrAborted";
        internal const string net_WebSockets_InvalidState = "net_WebSockets_InvalidState";
        internal const string net_WebSockets_ReceiveAsyncDisallowedAfterCloseAsync = "net_WebSockets_ReceiveAsyncDisallowedAfterCloseAsync";
        internal const string net_WebSockets_InvalidMessageType = "net_WebSockets_InvalidMessageType";
        internal const string net_WebSockets_InvalidBufferType = "net_WebSockets_InvalidBufferType";
        internal const string net_WebSockets_InvalidMessageType_Generic = "net_WebSockets_InvalidMessageType_Generic";
        internal const string net_WebSockets_Argument_InvalidMessageType = "net_WebSockets_Argument_InvalidMessageType";
        internal const string net_WebSockets_ConnectionClosedPrematurely_Generic = "net_WebSockets_ConnectionClosedPrematurely_Generic";
        internal const string net_WebSockets_InvalidCharInProtocolString = "net_WebSockets_InvalidCharInProtocolString";
        internal const string net_WebSockets_InvalidEmptySubProtocol = "net_WebSockets_InvalidEmptySubProtocol";
        internal const string net_WebSockets_ReasonNotNull = "net_WebSockets_ReasonNotNull";
        internal const string net_WebSockets_InvalidCloseStatusCode = "net_WebSockets_InvalidCloseStatusCode";
        internal const string net_WebSockets_InvalidCloseStatusDescription = "net_WebSockets_InvalidCloseStatusDescription";
        internal const string net_WebSockets_Scheme = "net_WebSockets_Scheme";
        internal const string net_WebSockets_AlreadyStarted = "net_WebSockets_AlreadyStarted";
        internal const string net_WebSockets_Connect101Expected = "net_WebSockets_Connect101Expected";
        internal const string net_WebSockets_InvalidResponseHeader = "net_WebSockets_InvalidResponseHeader";
        internal const string net_WebSockets_NotConnected = "net_WebSockets_NotConnected";
        internal const string net_WebSockets_InvalidRegistration = "net_WebSockets_InvalidRegistration";
        internal const string net_WebSockets_NoDuplicateProtocol = "net_WebSockets_NoDuplicateProtocol";
        internal const string net_log_exception = "net_log_exception";
        internal const string net_log_listener_delegate_exception = "net_log_listener_delegate_exception";
        internal const string net_log_listener_unsupported_authentication_scheme = "net_log_listener_unsupported_authentication_scheme";
        internal const string net_log_listener_unmatched_authentication_scheme = "net_log_listener_unmatched_authentication_scheme";
        internal const string net_log_listener_create_valid_identity_failed = "net_log_listener_create_valid_identity_failed";
        internal const string net_log_listener_httpsys_registry_null = "net_log_listener_httpsys_registry_null";
        internal const string net_log_listener_httpsys_registry_error = "net_log_listener_httpsys_registry_error";
        internal const string net_log_listener_cant_convert_raw_path = "net_log_listener_cant_convert_raw_path";
        internal const string net_log_listener_cant_convert_percent_value = "net_log_listener_cant_convert_percent_value";
        internal const string net_log_listener_cant_convert_bytes = "net_log_listener_cant_convert_bytes";
        internal const string net_log_listener_cant_convert_to_utf8 = "net_log_listener_cant_convert_to_utf8";
        internal const string net_log_listener_cant_create_uri = "net_log_listener_cant_create_uri";
        internal const string net_log_listener_no_cbt_disabled = "net_log_listener_no_cbt_disabled";
        internal const string net_log_listener_no_cbt_http = "net_log_listener_no_cbt_http";
        internal const string net_log_listener_no_cbt_platform = "net_log_listener_no_cbt_platform";
        internal const string net_log_listener_no_cbt_trustedproxy = "net_log_listener_no_cbt_trustedproxy";
        internal const string net_log_listener_cbt = "net_log_listener_cbt";
        internal const string net_log_listener_no_spn_kerberos = "net_log_listener_no_spn_kerberos";
        internal const string net_log_listener_no_spn_disabled = "net_log_listener_no_spn_disabled";
        internal const string net_log_listener_no_spn_cbt = "net_log_listener_no_spn_cbt";
        internal const string net_log_listener_no_spn_platform = "net_log_listener_no_spn_platform";
        internal const string net_log_listener_no_spn_whensupported = "net_log_listener_no_spn_whensupported";
        internal const string net_log_listener_no_spn_loopback = "net_log_listener_no_spn_loopback";
        internal const string net_log_listener_spn = "net_log_listener_spn";
        internal const string net_log_listener_spn_passed = "net_log_listener_spn_passed";
        internal const string net_log_listener_spn_failed = "net_log_listener_spn_failed";
        internal const string net_log_listener_spn_failed_always = "net_log_listener_spn_failed_always";
        internal const string net_log_listener_spn_failed_empty = "net_log_listener_spn_failed_empty";
        internal const string net_log_listener_spn_failed_dump = "net_log_listener_spn_failed_dump";
        internal const string net_log_listener_spn_add = "net_log_listener_spn_add";
        internal const string net_log_listener_spn_not_add = "net_log_listener_spn_not_add";
        internal const string net_log_listener_spn_remove = "net_log_listener_spn_remove";
        internal const string net_log_listener_spn_not_remove = "net_log_listener_spn_not_remove";
        internal const string net_log_sspi_enumerating_security_packages = "net_log_sspi_enumerating_security_packages";
        internal const string net_log_sspi_security_package_not_found = "net_log_sspi_security_package_not_found";
        internal const string net_log_sspi_security_context_input_buffer = "net_log_sspi_security_context_input_buffer";
        internal const string net_log_sspi_security_context_input_buffers = "net_log_sspi_security_context_input_buffers";
        internal const string net_log_sspi_selected_cipher_suite = "net_log_sspi_selected_cipher_suite";
        internal const string net_log_remote_certificate = "net_log_remote_certificate";
        internal const string net_log_locating_private_key_for_certificate = "net_log_locating_private_key_for_certificate";
        internal const string net_log_cert_is_of_type_2 = "net_log_cert_is_of_type_2";
        internal const string net_log_found_cert_in_store = "net_log_found_cert_in_store";
        internal const string net_log_did_not_find_cert_in_store = "net_log_did_not_find_cert_in_store";
        internal const string net_log_open_store_failed = "net_log_open_store_failed";
        internal const string net_log_got_certificate_from_delegate = "net_log_got_certificate_from_delegate";
        internal const string net_log_no_delegate_and_have_no_client_cert = "net_log_no_delegate_and_have_no_client_cert";
        internal const string net_log_no_delegate_but_have_client_cert = "net_log_no_delegate_but_have_client_cert";
        internal const string net_log_attempting_restart_using_cert = "net_log_attempting_restart_using_cert";
        internal const string net_log_no_issuers_try_all_certs = "net_log_no_issuers_try_all_certs";
        internal const string net_log_server_issuers_look_for_matching_certs = "net_log_server_issuers_look_for_matching_certs";
        internal const string net_log_selected_cert = "net_log_selected_cert";
        internal const string net_log_n_certs_after_filtering = "net_log_n_certs_after_filtering";
        internal const string net_log_finding_matching_certs = "net_log_finding_matching_certs";
        internal const string net_log_using_cached_credential = "net_log_using_cached_credential";
        internal const string net_log_remote_cert_user_declared_valid = "net_log_remote_cert_user_declared_valid";
        internal const string net_log_remote_cert_user_declared_invalid = "net_log_remote_cert_user_declared_invalid";
        internal const string net_log_remote_cert_has_no_errors = "net_log_remote_cert_has_no_errors";
        internal const string net_log_remote_cert_has_errors = "net_log_remote_cert_has_errors";
        internal const string net_log_remote_cert_not_available = "net_log_remote_cert_not_available";
        internal const string net_log_remote_cert_name_mismatch = "net_log_remote_cert_name_mismatch";
        internal const string net_log_proxy_autodetect_script_location_parse_error = "net_log_proxy_autodetect_script_location_parse_error";
        internal const string net_log_proxy_autodetect_failed = "net_log_proxy_autodetect_failed";
        internal const string net_log_proxy_script_execution_error = "net_log_proxy_script_execution_error";
        internal const string net_log_proxy_script_download_compile_error = "net_log_proxy_script_download_compile_error";
        internal const string net_log_proxy_system_setting_update = "net_log_proxy_system_setting_update";
        internal const string net_log_proxy_update_due_to_ip_config_change = "net_log_proxy_update_due_to_ip_config_change";
        internal const string net_log_proxy_called_with_null_parameter = "net_log_proxy_called_with_null_parameter";
        internal const string net_log_proxy_called_with_invalid_parameter = "net_log_proxy_called_with_invalid_parameter";
        internal const string net_log_proxy_ras_supported = "net_log_proxy_ras_supported";
        internal const string net_log_proxy_ras_notsupported_exception = "net_log_proxy_ras_notsupported_exception";
        internal const string net_log_proxy_winhttp_cant_open_session = "net_log_proxy_winhttp_cant_open_session";
        internal const string net_log_proxy_winhttp_getproxy_failed = "net_log_proxy_winhttp_getproxy_failed";
        internal const string net_log_proxy_winhttp_timeout_error = "net_log_proxy_winhttp_timeout_error";
        internal const string net_log_cache_validation_failed_resubmit = "net_log_cache_validation_failed_resubmit";
        internal const string net_log_cache_refused_server_response = "net_log_cache_refused_server_response";
        internal const string net_log_cache_ftp_proxy_doesnt_support_partial = "net_log_cache_ftp_proxy_doesnt_support_partial";
        internal const string net_log_cache_ftp_method = "net_log_cache_ftp_method";
        internal const string net_log_cache_ftp_supports_bin_only = "net_log_cache_ftp_supports_bin_only";
        internal const string net_log_cache_replacing_entry_with_HTTP_200 = "net_log_cache_replacing_entry_with_HTTP_200";
        internal const string net_log_cache_now_time = "net_log_cache_now_time";
        internal const string net_log_cache_max_age_absolute = "net_log_cache_max_age_absolute";
        internal const string net_log_cache_age1 = "net_log_cache_age1";
        internal const string net_log_cache_age1_date_header = "net_log_cache_age1_date_header";
        internal const string net_log_cache_age1_last_synchronized = "net_log_cache_age1_last_synchronized";
        internal const string net_log_cache_age1_last_synchronized_age_header = "net_log_cache_age1_last_synchronized_age_header";
        internal const string net_log_cache_age2 = "net_log_cache_age2";
        internal const string net_log_cache_max_age_cache_s_max_age = "net_log_cache_max_age_cache_s_max_age";
        internal const string net_log_cache_max_age_expires_date = "net_log_cache_max_age_expires_date";
        internal const string net_log_cache_max_age_cache_max_age = "net_log_cache_max_age_cache_max_age";
        internal const string net_log_cache_no_max_age_use_10_percent = "net_log_cache_no_max_age_use_10_percent";
        internal const string net_log_cache_no_max_age_use_default = "net_log_cache_no_max_age_use_default";
        internal const string net_log_cache_validator_invalid_for_policy = "net_log_cache_validator_invalid_for_policy";
        internal const string net_log_cache_response_last_modified = "net_log_cache_response_last_modified";
        internal const string net_log_cache_cache_last_modified = "net_log_cache_cache_last_modified";
        internal const string net_log_cache_partial_and_non_zero_content_offset = "net_log_cache_partial_and_non_zero_content_offset";
        internal const string net_log_cache_response_valid_based_on_policy = "net_log_cache_response_valid_based_on_policy";
        internal const string net_log_cache_null_response_failure = "net_log_cache_null_response_failure";
        internal const string net_log_cache_ftp_response_status = "net_log_cache_ftp_response_status";
        internal const string net_log_cache_resp_valid_based_on_retry = "net_log_cache_resp_valid_based_on_retry";
        internal const string net_log_cache_no_update_based_on_method = "net_log_cache_no_update_based_on_method";
        internal const string net_log_cache_removed_existing_invalid_entry = "net_log_cache_removed_existing_invalid_entry";
        internal const string net_log_cache_not_updated_based_on_policy = "net_log_cache_not_updated_based_on_policy";
        internal const string net_log_cache_not_updated_because_no_response = "net_log_cache_not_updated_because_no_response";
        internal const string net_log_cache_removed_existing_based_on_method = "net_log_cache_removed_existing_based_on_method";

        internal const string net_log_cache_existing_not_removed_because_unexpected_response_status =
                "net_log_cache_existing_not_removed_because_unexpected_response_status";

        internal const string net_log_cache_removed_existing_based_on_policy = "net_log_cache_removed_existing_based_on_policy";
        internal const string net_log_cache_not_updated_based_on_ftp_response_status = "net_log_cache_not_updated_based_on_ftp_response_status";
        internal const string net_log_cache_update_not_supported_for_ftp_restart = "net_log_cache_update_not_supported_for_ftp_restart";

        internal const string net_log_cache_removed_entry_because_ftp_restart_response_changed =
                "net_log_cache_removed_entry_because_ftp_restart_response_changed";

        internal const string net_log_cache_last_synchronized = "net_log_cache_last_synchronized";

        internal const string net_log_cache_suppress_update_because_synched_last_minute =
                "net_log_cache_suppress_update_because_synched_last_minute";

        internal const string net_log_cache_updating_last_synchronized = "net_log_cache_updating_last_synchronized";
        internal const string net_log_cache_cannot_remove = "net_log_cache_cannot_remove";
        internal const string net_log_cache_key_status = "net_log_cache_key_status";
        internal const string net_log_cache_key_remove_failed_status = "net_log_cache_key_remove_failed_status";
        internal const string net_log_cache_usecount_file = "net_log_cache_usecount_file";
        internal const string net_log_cache_stream = "net_log_cache_stream";
        internal const string net_log_cache_filename = "net_log_cache_filename";
        internal const string net_log_cache_lookup_failed = "net_log_cache_lookup_failed";
        internal const string net_log_cache_exception = "net_log_cache_exception";
        internal const string net_log_cache_expected_length = "net_log_cache_expected_length";
        internal const string net_log_cache_last_modified = "net_log_cache_last_modified";
        internal const string net_log_cache_expires = "net_log_cache_expires";
        internal const string net_log_cache_max_stale = "net_log_cache_max_stale";
        internal const string net_log_cache_dumping_metadata = "net_log_cache_dumping_metadata";
        internal const string net_log_cache_create_failed = "net_log_cache_create_failed";
        internal const string net_log_cache_set_expires = "net_log_cache_set_expires";
        internal const string net_log_cache_set_last_modified = "net_log_cache_set_last_modified";
        internal const string net_log_cache_set_last_synchronized = "net_log_cache_set_last_synchronized";
        internal const string net_log_cache_enable_max_stale = "net_log_cache_enable_max_stale";
        internal const string net_log_cache_disable_max_stale = "net_log_cache_disable_max_stale";
        internal const string net_log_cache_set_new_metadata = "net_log_cache_set_new_metadata";
        internal const string net_log_cache_dumping = "net_log_cache_dumping";
        internal const string net_log_cache_key = "net_log_cache_key";
        internal const string net_log_cache_no_commit = "net_log_cache_no_commit";
        internal const string net_log_cache_error_deleting_filename = "net_log_cache_error_deleting_filename";
        internal const string net_log_cache_update_failed = "net_log_cache_update_failed";
        internal const string net_log_cache_delete_failed = "net_log_cache_delete_failed";
        internal const string net_log_cache_commit_failed = "net_log_cache_commit_failed";
        internal const string net_log_cache_committed_as_partial = "net_log_cache_committed_as_partial";
        internal const string net_log_cache_max_stale_and_update_status = "net_log_cache_max_stale_and_update_status";
        internal const string net_log_cache_failing_request_with_exception = "net_log_cache_failing_request_with_exception";
        internal const string net_log_cache_request_method = "net_log_cache_request_method";
        internal const string net_log_cache_http_status_parse_failure = "net_log_cache_http_status_parse_failure";
        internal const string net_log_cache_http_status_line = "net_log_cache_http_status_line";
        internal const string net_log_cache_cache_control = "net_log_cache_cache_control";
        internal const string net_log_cache_invalid_http_version = "net_log_cache_invalid_http_version";
        internal const string net_log_cache_no_http_response_header = "net_log_cache_no_http_response_header";
        internal const string net_log_cache_http_header_parse_error = "net_log_cache_http_header_parse_error";
        internal const string net_log_cache_metadata_name_value_parse_error = "net_log_cache_metadata_name_value_parse_error";
        internal const string net_log_cache_content_range_error = "net_log_cache_content_range_error";
        internal const string net_log_cache_cache_control_error = "net_log_cache_cache_control_error";
        internal const string net_log_cache_unexpected_status = "net_log_cache_unexpected_status";
        internal const string net_log_cache_object_and_exception = "net_log_cache_object_and_exception";
        internal const string net_log_cache_revalidation_not_needed = "net_log_cache_revalidation_not_needed";

        internal const string net_log_cache_not_updated_based_on_cache_protocol_status =
                "net_log_cache_not_updated_based_on_cache_protocol_status";

        internal const string net_log_cache_closing_cache_stream = "net_log_cache_closing_cache_stream";
        internal const string net_log_cache_exception_ignored = "net_log_cache_exception_ignored";
        internal const string net_log_cache_no_cache_entry = "net_log_cache_no_cache_entry";
        internal const string net_log_cache_null_cached_stream = "net_log_cache_null_cached_stream";
        internal const string net_log_cache_requested_combined_but_null_cached_stream = "net_log_cache_requested_combined_but_null_cached_stream";
        internal const string net_log_cache_returned_range_cache = "net_log_cache_returned_range_cache";
        internal const string net_log_cache_entry_not_found_freshness_undefined = "net_log_cache_entry_not_found_freshness_undefined";
        internal const string net_log_cache_dumping_cache_context = "net_log_cache_dumping_cache_context";
        internal const string net_log_cache_result = "net_log_cache_result";
        internal const string net_log_cache_uri_with_query_has_no_expiration = "net_log_cache_uri_with_query_has_no_expiration";

        internal const string net_log_cache_uri_with_query_and_cached_resp_from_http_10 =
                "net_log_cache_uri_with_query_and_cached_resp_from_http_10";

        internal const string net_log_cache_valid_as_fresh_or_because_policy = "net_log_cache_valid_as_fresh_or_because_policy";
        internal const string net_log_cache_accept_based_on_retry_count = "net_log_cache_accept_based_on_retry_count";
        internal const string net_log_cache_date_header_older_than_cache_entry = "net_log_cache_date_header_older_than_cache_entry";
        internal const string net_log_cache_server_didnt_satisfy_range = "net_log_cache_server_didnt_satisfy_range";
        internal const string net_log_cache_304_received_on_unconditional_request = "net_log_cache_304_received_on_unconditional_request";

        internal const string net_log_cache_304_received_on_unconditional_request_expected_200_206 =
                "net_log_cache_304_received_on_unconditional_request_expected_200_206";

        internal const string net_log_cache_last_modified_header_older_than_cache_entry =
                "net_log_cache_last_modified_header_older_than_cache_entry";

        internal const string net_log_cache_freshness_outside_policy_limits = "net_log_cache_freshness_outside_policy_limits";
        internal const string net_log_cache_need_to_remove_invalid_cache_entry_304 = "net_log_cache_need_to_remove_invalid_cache_entry_304";
        internal const string net_log_cache_resp_status = "net_log_cache_resp_status";
        internal const string net_log_cache_resp_304_or_request_head = "net_log_cache_resp_304_or_request_head";
        internal const string net_log_cache_dont_update_cached_headers = "net_log_cache_dont_update_cached_headers";
        internal const string net_log_cache_update_cached_headers = "net_log_cache_update_cached_headers";

        internal const string net_log_cache_partial_resp_not_combined_with_existing_entry =
                "net_log_cache_partial_resp_not_combined_with_existing_entry";

        internal const string net_log_cache_request_contains_conditional_header = "net_log_cache_request_contains_conditional_header";
        internal const string net_log_cache_not_a_get_head_post = "net_log_cache_not_a_get_head_post";
        internal const string net_log_cache_cannot_update_cache_if_304 = "net_log_cache_cannot_update_cache_if_304";
        internal const string net_log_cache_cannot_update_cache_with_head_resp = "net_log_cache_cannot_update_cache_with_head_resp";
        internal const string net_log_cache_http_resp_is_null = "net_log_cache_http_resp_is_null";
        internal const string net_log_cache_resp_cache_control_is_no_store = "net_log_cache_resp_cache_control_is_no_store";
        internal const string net_log_cache_resp_cache_control_is_public = "net_log_cache_resp_cache_control_is_public";
        internal const string net_log_cache_resp_cache_control_is_private = "net_log_cache_resp_cache_control_is_private";

        internal const string net_log_cache_resp_cache_control_is_private_plus_headers =
                "net_log_cache_resp_cache_control_is_private_plus_headers";

        internal const string net_log_cache_resp_older_than_cache = "net_log_cache_resp_older_than_cache";
        internal const string net_log_cache_revalidation_required = "net_log_cache_revalidation_required";
        internal const string net_log_cache_needs_revalidation = "net_log_cache_needs_revalidation";
        internal const string net_log_cache_resp_allows_caching = "net_log_cache_resp_allows_caching";
        internal const string net_log_cache_auth_header_and_no_s_max_age = "net_log_cache_auth_header_and_no_s_max_age";

        internal const string net_log_cache_post_resp_without_cache_control_or_expires =
                "net_log_cache_post_resp_without_cache_control_or_expires";

        internal const string net_log_cache_valid_based_on_status_code = "net_log_cache_valid_based_on_status_code";
        internal const string net_log_cache_resp_no_cache_control = "net_log_cache_resp_no_cache_control";
        internal const string net_log_cache_age = "net_log_cache_age";
        internal const string net_log_cache_policy_min_fresh = "net_log_cache_policy_min_fresh";
        internal const string net_log_cache_policy_max_age = "net_log_cache_policy_max_age";
        internal const string net_log_cache_policy_cache_sync_date = "net_log_cache_policy_cache_sync_date";
        internal const string net_log_cache_policy_max_stale = "net_log_cache_policy_max_stale";
        internal const string net_log_cache_control_no_cache = "net_log_cache_control_no_cache";
        internal const string net_log_cache_control_no_cache_removing_some_headers = "net_log_cache_control_no_cache_removing_some_headers";
        internal const string net_log_cache_control_must_revalidate = "net_log_cache_control_must_revalidate";
        internal const string net_log_cache_cached_auth_header = "net_log_cache_cached_auth_header";
        internal const string net_log_cache_cached_auth_header_no_control_directive = "net_log_cache_cached_auth_header_no_control_directive";
        internal const string net_log_cache_after_validation = "net_log_cache_after_validation";
        internal const string net_log_cache_resp_status_304 = "net_log_cache_resp_status_304";
        internal const string net_log_cache_head_resp_has_different_content_length = "net_log_cache_head_resp_has_different_content_length";
        internal const string net_log_cache_head_resp_has_different_content_md5 = "net_log_cache_head_resp_has_different_content_md5";
        internal const string net_log_cache_head_resp_has_different_etag = "net_log_cache_head_resp_has_different_etag";
        internal const string net_log_cache_304_head_resp_has_different_last_modified = "net_log_cache_304_head_resp_has_different_last_modified";
        internal const string net_log_cache_existing_entry_has_to_be_discarded = "net_log_cache_existing_entry_has_to_be_discarded";
        internal const string net_log_cache_existing_entry_should_be_discarded = "net_log_cache_existing_entry_should_be_discarded";
        internal const string net_log_cache_206_resp_non_matching_entry = "net_log_cache_206_resp_non_matching_entry";
        internal const string net_log_cache_206_resp_starting_position_not_adjusted = "net_log_cache_206_resp_starting_position_not_adjusted";
        internal const string net_log_cache_combined_resp_requested = "net_log_cache_combined_resp_requested";
        internal const string net_log_cache_updating_headers_on_304 = "net_log_cache_updating_headers_on_304";
        internal const string net_log_cache_suppressing_headers_update_on_304 = "net_log_cache_suppressing_headers_update_on_304";
        internal const string net_log_cache_status_code_not_304_206 = "net_log_cache_status_code_not_304_206";
        internal const string net_log_cache_sxx_resp_cache_only = "net_log_cache_sxx_resp_cache_only";
        internal const string net_log_cache_sxx_resp_can_be_replaced = "net_log_cache_sxx_resp_can_be_replaced";
        internal const string net_log_cache_vary_header_empty = "net_log_cache_vary_header_empty";
        internal const string net_log_cache_vary_header_contains_asterisks = "net_log_cache_vary_header_contains_asterisks";
        internal const string net_log_cache_no_headers_in_metadata = "net_log_cache_no_headers_in_metadata";
        internal const string net_log_cache_vary_header_mismatched_count = "net_log_cache_vary_header_mismatched_count";
        internal const string net_log_cache_vary_header_mismatched_field = "net_log_cache_vary_header_mismatched_field";
        internal const string net_log_cache_vary_header_match = "net_log_cache_vary_header_match";
        internal const string net_log_cache_range = "net_log_cache_range";
        internal const string net_log_cache_range_invalid_format = "net_log_cache_range_invalid_format";
        internal const string net_log_cache_range_not_in_cache = "net_log_cache_range_not_in_cache";
        internal const string net_log_cache_range_in_cache = "net_log_cache_range_in_cache";
        internal const string net_log_cache_partial_resp = "net_log_cache_partial_resp";
        internal const string net_log_cache_range_request_range = "net_log_cache_range_request_range";
        internal const string net_log_cache_could_be_partial = "net_log_cache_could_be_partial";
        internal const string net_log_cache_condition_if_none_match = "net_log_cache_condition_if_none_match";
        internal const string net_log_cache_condition_if_modified_since = "net_log_cache_condition_if_modified_since";
        internal const string net_log_cache_cannot_construct_conditional_request = "net_log_cache_cannot_construct_conditional_request";

        internal const string net_log_cache_cannot_construct_conditional_range_request =
                "net_log_cache_cannot_construct_conditional_range_request";

        internal const string net_log_cache_entry_size_too_big = "net_log_cache_entry_size_too_big";
        internal const string net_log_cache_condition_if_range = "net_log_cache_condition_if_range";

        internal const string net_log_cache_conditional_range_not_implemented_on_http_10 =
                "net_log_cache_conditional_range_not_implemented_on_http_10";

        internal const string net_log_cache_saving_request_headers = "net_log_cache_saving_request_headers";
        internal const string net_log_cache_only_byte_range_implemented = "net_log_cache_only_byte_range_implemented";
        internal const string net_log_cache_multiple_complex_range_not_implemented = "net_log_cache_multiple_complex_range_not_implemented";
        internal const string net_log_digest_hash_algorithm_not_supported = "net_log_digest_hash_algorithm_not_supported";
        internal const string net_log_digest_qop_not_supported = "net_log_digest_qop_not_supported";
        internal const string net_log_digest_requires_nonce = "net_log_digest_requires_nonce";
        internal const string net_log_auth_invalid_challenge = "net_log_auth_invalid_challenge";
        internal const string net_log_unknown = "net_log_unknown";
        internal const string net_log_operation_returned_something = "net_log_operation_returned_something";
        internal const string net_log_operation_failed_with_error = "net_log_operation_failed_with_error";
        internal const string net_log_buffered_n_bytes = "net_log_buffered_n_bytes";
        internal const string net_log_method_equal = "net_log_method_equal";
        internal const string net_log_releasing_connection = "net_log_releasing_connection";
        internal const string net_log_unexpected_exception = "net_log_unexpected_exception";
        internal const string net_log_server_response_error_code = "net_log_server_response_error_code";
        internal const string net_log_resubmitting_request = "net_log_resubmitting_request";
        internal const string net_log_retrieving_localhost_exception = "net_log_retrieving_localhost_exception";
        internal const string net_log_resolved_servicepoint_may_not_be_remote_server = "net_log_resolved_servicepoint_may_not_be_remote_server";
        internal const string net_log_closed_idle = "net_log_closed_idle";
        internal const string net_log_received_status_line = "net_log_received_status_line";
        internal const string net_log_sending_headers = "net_log_sending_headers";
        internal const string net_log_received_headers = "net_log_received_headers";
        internal const string net_log_shell_expression_pattern_format_warning = "net_log_shell_expression_pattern_format_warning";
        internal const string net_log_exception_in_callback = "net_log_exception_in_callback";
        internal const string net_log_sending_command = "net_log_sending_command";
        internal const string net_log_received_response = "net_log_received_response";
        internal const string net_log_socket_connected = "net_log_socket_connected";
        internal const string net_log_socket_accepted = "net_log_socket_accepted";
        internal const string net_log_socket_not_logged_file = "net_log_socket_not_logged_file";
        internal const string net_log_socket_connect_dnsendpoint = "net_log_socket_connect_dnsendpoint";
        internal const string net_log_set_socketoption_reuseport = "net_log_set_socketoption_reuseport";
        internal const string net_log_set_socketoption_reuseport_not_supported = "net_log_set_socketoption_reuseport_not_supported";
        internal const string net_log_set_socketoption_reuseport_default_on = "net_log_set_socketoption_reuseport_default_on";
        internal const string MailAddressInvalidFormat = "MailAddressInvalidFormat";
        internal const string MailSubjectInvalidFormat = "MailSubjectInvalidFormat";
        internal const string MailBase64InvalidCharacter = "MailBase64InvalidCharacter";
        internal const string MailCollectionIsReadOnly = "MailCollectionIsReadOnly";
        internal const string MailDateInvalidFormat = "MailDateInvalidFormat";
        internal const string MailHeaderFieldAlreadyExists = "MailHeaderFieldAlreadyExists";
        internal const string MailHeaderFieldInvalidCharacter = "MailHeaderFieldInvalidCharacter";
        internal const string MailHeaderFieldMalformedHeader = "MailHeaderFieldMalformedHeader";
        internal const string MailHeaderFieldMismatchedName = "MailHeaderFieldMismatchedName";
        internal const string MailHeaderIndexOutOfBounds = "MailHeaderIndexOutOfBounds";
        internal const string MailHeaderItemAccessorOnlySingleton = "MailHeaderItemAccessorOnlySingleton";
        internal const string MailHeaderListHasChanged = "MailHeaderListHasChanged";
        internal const string MailHeaderResetCalledBeforeEOF = "MailHeaderResetCalledBeforeEOF";
        internal const string MailHeaderTargetArrayTooSmall = "MailHeaderTargetArrayTooSmall";
        internal const string MailHeaderInvalidCID = "MailHeaderInvalidCID";
        internal const string MailHostNotFound = "MailHostNotFound";
        internal const string MailReaderGetContentStreamAlreadyCalled = "MailReaderGetContentStreamAlreadyCalled";
        internal const string MailReaderTruncated = "MailReaderTruncated";
        internal const string MailWriterIsInContent = "MailWriterIsInContent";
        internal const string MailServerDoesNotSupportStartTls = "MailServerDoesNotSupportStartTls";
        internal const string MailServerResponse = "MailServerResponse";
        internal const string SSPIAuthenticationOrSPNNull = "SSPIAuthenticationOrSPNNull";
        internal const string SSPIPInvokeError = "SSPIPInvokeError";
        internal const string SSPIInvalidHandleType = "SSPIInvalidHandleType";
        internal const string SmtpAlreadyConnected = "SmtpAlreadyConnected";
        internal const string SmtpAuthenticationFailed = "SmtpAuthenticationFailed";
        internal const string SmtpAuthenticationFailedNoCreds = "SmtpAuthenticationFailedNoCreds";
        internal const string SmtpDataStreamOpen = "SmtpDataStreamOpen";
        internal const string SmtpDefaultMimePreamble = "SmtpDefaultMimePreamble";
        internal const string SmtpDefaultSubject = "SmtpDefaultSubject";
        internal const string SmtpInvalidResponse = "SmtpInvalidResponse";
        internal const string SmtpNotConnected = "SmtpNotConnected";
        internal const string SmtpSystemStatus = "SmtpSystemStatus";
        internal const string SmtpHelpMessage = "SmtpHelpMessage";
        internal const string SmtpServiceReady = "SmtpServiceReady";
        internal const string SmtpServiceClosingTransmissionChannel = "SmtpServiceClosingTransmissionChannel";
        internal const string SmtpOK = "SmtpOK";
        internal const string SmtpUserNotLocalWillForward = "SmtpUserNotLocalWillForward";
        internal const string SmtpStartMailInput = "SmtpStartMailInput";
        internal const string SmtpServiceNotAvailable = "SmtpServiceNotAvailable";
        internal const string SmtpMailboxBusy = "SmtpMailboxBusy";
        internal const string SmtpLocalErrorInProcessing = "SmtpLocalErrorInProcessing";
        internal const string SmtpInsufficientStorage = "SmtpInsufficientStorage";
        internal const string SmtpPermissionDenied = "SmtpPermissionDenied";
        internal const string SmtpCommandUnrecognized = "SmtpCommandUnrecognized";
        internal const string SmtpSyntaxError = "SmtpSyntaxError";
        internal const string SmtpCommandNotImplemented = "SmtpCommandNotImplemented";
        internal const string SmtpBadCommandSequence = "SmtpBadCommandSequence";
        internal const string SmtpCommandParameterNotImplemented = "SmtpCommandParameterNotImplemented";
        internal const string SmtpMailboxUnavailable = "SmtpMailboxUnavailable";
        internal const string SmtpUserNotLocalTryAlternatePath = "SmtpUserNotLocalTryAlternatePath";
        internal const string SmtpExceededStorageAllocation = "SmtpExceededStorageAllocation";
        internal const string SmtpMailboxNameNotAllowed = "SmtpMailboxNameNotAllowed";
        internal const string SmtpTransactionFailed = "SmtpTransactionFailed";
        internal const string SmtpSendMailFailure = "SmtpSendMailFailure";
        internal const string SmtpRecipientFailed = "SmtpRecipientFailed";
        internal const string SmtpRecipientRequired = "SmtpRecipientRequired";
        internal const string SmtpFromRequired = "SmtpFromRequired";
        internal const string SmtpAllRecipientsFailed = "SmtpAllRecipientsFailed";
        internal const string SmtpClientNotPermitted = "SmtpClientNotPermitted";
        internal const string SmtpMustIssueStartTlsFirst = "SmtpMustIssueStartTlsFirst";
        internal const string SmtpNeedAbsolutePickupDirectory = "SmtpNeedAbsolutePickupDirectory";
        internal const string SmtpGetIisPickupDirectoryFailed = "SmtpGetIisPickupDirectoryFailed";
        internal const string SmtpPickupDirectoryDoesnotSupportSsl = "SmtpPickupDirectoryDoesnotSupportSsl";
        internal const string SmtpOperationInProgress = "SmtpOperationInProgress";
        internal const string SmtpAuthResponseInvalid = "SmtpAuthResponseInvalid";
        internal const string SmtpEhloResponseInvalid = "SmtpEhloResponseInvalid";
        internal const string SmtpNonAsciiUserNotSupported = "SmtpNonAsciiUserNotSupported";
        internal const string SmtpInvalidHostName = "SmtpInvalidHostName";
        internal const string MimeTransferEncodingNotSupported = "MimeTransferEncodingNotSupported";
        internal const string SeekNotSupported = "SeekNotSupported";
        internal const string WriteNotSupported = "WriteNotSupported";
        internal const string InvalidHexDigit = "InvalidHexDigit";
        internal const string InvalidSSPIContext = "InvalidSSPIContext";
        internal const string InvalidSSPIContextKey = "InvalidSSPIContextKey";
        internal const string InvalidSSPINegotiationElement = "InvalidSSPINegotiationElement";
        internal const string InvalidHeaderName = "InvalidHeaderName";
        internal const string InvalidHeaderValue = "InvalidHeaderValue";
        internal const string CannotGetEffectiveTimeOfSSPIContext = "CannotGetEffectiveTimeOfSSPIContext";
        internal const string CannotGetExpiryTimeOfSSPIContext = "CannotGetExpiryTimeOfSSPIContext";
        internal const string ReadNotSupported = "ReadNotSupported";
        internal const string InvalidAsyncResult = "InvalidAsyncResult";
        internal const string UnspecifiedHost = "UnspecifiedHost";
        internal const string InvalidPort = "InvalidPort";
        internal const string SmtpInvalidOperationDuringSend = "SmtpInvalidOperationDuringSend";
        internal const string MimePartCantResetStream = "MimePartCantResetStream";
        internal const string MediaTypeInvalid = "MediaTypeInvalid";
        internal const string ContentTypeInvalid = "ContentTypeInvalid";
        internal const string ContentDispositionInvalid = "ContentDispositionInvalid";
        internal const string AttributeNotSupported = "AttributeNotSupported";
        internal const string Cannot_remove_with_null = "Cannot_remove_with_null";
        internal const string Config_base_elements_only = "Config_base_elements_only";
        internal const string Config_base_no_child_nodes = "Config_base_no_child_nodes";
        internal const string Config_base_required_attribute_empty = "Config_base_required_attribute_empty";
        internal const string Config_base_required_attribute_missing = "Config_base_required_attribute_missing";
        internal const string Config_base_time_overflow = "Config_base_time_overflow";
        internal const string Config_base_type_must_be_configurationvalidation = "Config_base_type_must_be_configurationvalidation";
        internal const string Config_base_type_must_be_typeconverter = "Config_base_type_must_be_typeconverter";
        internal const string Config_base_unknown_format = "Config_base_unknown_format";
        internal const string Config_base_unrecognized_attribute = "Config_base_unrecognized_attribute";
        internal const string Config_base_unrecognized_element = "Config_base_unrecognized_element";
        internal const string Config_invalid_boolean_attribute = "Config_invalid_boolean_attribute";
        internal const string Config_invalid_integer_attribute = "Config_invalid_integer_attribute";
        internal const string Config_invalid_positive_integer_attribute = "Config_invalid_positive_integer_attribute";
        internal const string Config_invalid_type_attribute = "Config_invalid_type_attribute";
        internal const string Config_missing_required_attribute = "Config_missing_required_attribute";
        internal const string Config_name_value_file_section_file_invalid_root = "Config_name_value_file_section_file_invalid_root";
        internal const string Config_provider_must_implement_type = "Config_provider_must_implement_type";
        internal const string Config_provider_name_null_or_empty = "Config_provider_name_null_or_empty";
        internal const string Config_provider_not_found = "Config_provider_not_found";
        internal const string Config_property_name_cannot_be_empty = "Config_property_name_cannot_be_empty";
        internal const string Config_section_cannot_clear_locked_section = "Config_section_cannot_clear_locked_section";
        internal const string Config_section_record_not_found = "Config_section_record_not_found";
        internal const string Config_source_cannot_contain_file = "Config_source_cannot_contain_file";
        internal const string Config_system_already_set = "Config_system_already_set";
        internal const string Config_unable_to_read_security_policy = "Config_unable_to_read_security_policy";
        internal const string Config_write_xml_returned_null = "Config_write_xml_returned_null";
        internal const string Cannot_clear_sections_within_group = "Cannot_clear_sections_within_group";
        internal const string Cannot_exit_up_top_directory = "Cannot_exit_up_top_directory";
        internal const string Could_not_create_listener = "Could_not_create_listener";
        internal const string TL_InitializeData_NotSpecified = "TL_InitializeData_NotSpecified";
        internal const string Could_not_create_type_instance = "Could_not_create_type_instance";
        internal const string Could_not_find_type = "Could_not_find_type";
        internal const string Could_not_get_constructor = "Could_not_get_constructor";
        internal const string EmptyTypeName_NotAllowed = "EmptyTypeName_NotAllowed";
        internal const string Incorrect_base_type = "Incorrect_base_type";
        internal const string Only_specify_one = "Only_specify_one";
        internal const string Provider_Already_Initialized = "Provider_Already_Initialized";
        internal const string Reference_listener_cant_have_properties = "Reference_listener_cant_have_properties";
        internal const string Reference_to_nonexistent_listener = "Reference_to_nonexistent_listener";
        internal const string SettingsPropertyNotFound = "SettingsPropertyNotFound";
        internal const string SettingsPropertyReadOnly = "SettingsPropertyReadOnly";
        internal const string SettingsPropertyWrongType = "SettingsPropertyWrongType";
        internal const string Type_isnt_tracelistener = "Type_isnt_tracelistener";
        internal const string Unable_to_convert_type_from_string = "Unable_to_convert_type_from_string";
        internal const string Unable_to_convert_type_to_string = "Unable_to_convert_type_to_string";
        internal const string Value_must_be_numeric = "Value_must_be_numeric";
        internal const string Could_not_create_from_default_value = "Could_not_create_from_default_value";
        internal const string Could_not_create_from_default_value_2 = "Could_not_create_from_default_value_2";
        internal const string InvalidDirName = "InvalidDirName";
        internal const string FSW_IOError = "FSW_IOError";
        internal const string PatternInvalidChar = "PatternInvalidChar";
        internal const string BufferSizeTooLarge = "BufferSizeTooLarge";
        internal const string FSW_ChangedFilter = "FSW_ChangedFilter";
        internal const string FSW_Enabled = "FSW_Enabled";
        internal const string FSW_Filter = "FSW_Filter";
        internal const string FSW_IncludeSubdirectories = "FSW_IncludeSubdirectories";
        internal const string FSW_Path = "FSW_Path";
        internal const string FSW_SynchronizingObject = "FSW_SynchronizingObject";
        internal const string FSW_Changed = "FSW_Changed";
        internal const string FSW_Created = "FSW_Created";
        internal const string FSW_Deleted = "FSW_Deleted";
        internal const string FSW_Renamed = "FSW_Renamed";
        internal const string FSW_BufferOverflow = "FSW_BufferOverflow";
        internal const string FileSystemWatcherDesc = "FileSystemWatcherDesc";
        internal const string NotSet = "NotSet";
        internal const string TimerAutoReset = "TimerAutoReset";
        internal const string TimerEnabled = "TimerEnabled";
        internal const string TimerInterval = "TimerInterval";
        internal const string TimerIntervalElapsed = "TimerIntervalElapsed";
        internal const string TimerSynchronizingObject = "TimerSynchronizingObject";
        internal const string MismatchedCounterTypes = "MismatchedCounterTypes";
        internal const string NoPropertyForAttribute = "NoPropertyForAttribute";
        internal const string InvalidAttributeType = "InvalidAttributeType";
        internal const string Generic_ArgCantBeEmptyString = "Generic_ArgCantBeEmptyString";
        internal const string BadLogName = "BadLogName";
        internal const string InvalidProperty = "InvalidProperty";
        internal const string CantMonitorEventLog = "CantMonitorEventLog";
        internal const string InitTwice = "InitTwice";
        internal const string InvalidParameter = "InvalidParameter";
        internal const string MissingParameter = "MissingParameter";
        internal const string ParameterTooLong = "ParameterTooLong";
        internal const string LocalSourceAlreadyExists = "LocalSourceAlreadyExists";
        internal const string SourceAlreadyExists = "SourceAlreadyExists";
        internal const string LocalLogAlreadyExistsAsSource = "LocalLogAlreadyExistsAsSource";
        internal const string LogAlreadyExistsAsSource = "LogAlreadyExistsAsSource";
        internal const string DuplicateLogName = "DuplicateLogName";
        internal const string RegKeyMissing = "RegKeyMissing";
        internal const string LocalRegKeyMissing = "LocalRegKeyMissing";
        internal const string RegKeyMissingShort = "RegKeyMissingShort";
        internal const string InvalidParameterFormat = "InvalidParameterFormat";
        internal const string NoLogName = "NoLogName";
        internal const string RegKeyNoAccess = "RegKeyNoAccess";
        internal const string MissingLog = "MissingLog";
        internal const string SourceNotRegistered = "SourceNotRegistered";
        internal const string LocalSourceNotRegistered = "LocalSourceNotRegistered";
        internal const string CantRetrieveEntries = "CantRetrieveEntries";
        internal const string IndexOutOfBounds = "IndexOutOfBounds";
        internal const string CantReadLogEntryAt = "CantReadLogEntryAt";
        internal const string MissingLogProperty = "MissingLogProperty";
        internal const string CantOpenLog = "CantOpenLog";
        internal const string NeedSourceToOpen = "NeedSourceToOpen";
        internal const string NeedSourceToWrite = "NeedSourceToWrite";
        internal const string CantOpenLogAccess = "CantOpenLogAccess";
        internal const string LogEntryTooLong = "LogEntryTooLong";
        internal const string TooManyReplacementStrings = "TooManyReplacementStrings";
        internal const string LogSourceMismatch = "LogSourceMismatch";
        internal const string NoAccountInfo = "NoAccountInfo";
        internal const string NoCurrentEntry = "NoCurrentEntry";
        internal const string MessageNotFormatted = "MessageNotFormatted";
        internal const string EventID = "EventID";
        internal const string LogDoesNotExists = "LogDoesNotExists";
        internal const string InvalidCustomerLogName = "InvalidCustomerLogName";
        internal const string CannotDeleteEqualSource = "CannotDeleteEqualSource";
        internal const string RentionDaysOutOfRange = "RentionDaysOutOfRange";
        internal const string MaximumKilobytesOutOfRange = "MaximumKilobytesOutOfRange";
        internal const string SomeLogsInaccessible = "SomeLogsInaccessible";
        internal const string SomeLogsInaccessibleToCreate = "SomeLogsInaccessibleToCreate";
        internal const string BadConfigSwitchValue = "BadConfigSwitchValue";
        internal const string ConfigSectionsUnique = "ConfigSectionsUnique";
        internal const string ConfigSectionsUniquePerSection = "ConfigSectionsUniquePerSection";
        internal const string SourceListenerDoesntExist = "SourceListenerDoesntExist";
        internal const string SourceSwitchDoesntExist = "SourceSwitchDoesntExist";
        internal const string CategoryHelpCorrupt = "CategoryHelpCorrupt";
        internal const string CounterNameCorrupt = "CounterNameCorrupt";
        internal const string CounterDataCorrupt = "CounterDataCorrupt";
        internal const string ReadOnlyCounter = "ReadOnlyCounter";
        internal const string ReadOnlyRemoveInstance = "ReadOnlyRemoveInstance";
        internal const string NotCustomCounter = "NotCustomCounter";
        internal const string CategoryNameMissing = "CategoryNameMissing";
        internal const string CounterNameMissing = "CounterNameMissing";
        internal const string InstanceNameProhibited = "InstanceNameProhibited";
        internal const string InstanceNameRequired = "InstanceNameRequired";
        internal const string MissingInstance = "MissingInstance";
        internal const string PerformanceCategoryExists = "PerformanceCategoryExists";
        internal const string InvalidCounterName = "InvalidCounterName";
        internal const string DuplicateCounterName = "DuplicateCounterName";
        internal const string CantChangeCategoryRegistration = "CantChangeCategoryRegistration";
        internal const string CantDeleteCategory = "CantDeleteCategory";
        internal const string MissingCategory = "MissingCategory";
        internal const string MissingCategoryDetail = "MissingCategoryDetail";
        internal const string CantReadCategory = "CantReadCategory";
        internal const string MissingCounter = "MissingCounter";
        internal const string CategoryNameNotSet = "CategoryNameNotSet";
        internal const string CounterExists = "CounterExists";
        internal const string CantReadCategoryIndex = "CantReadCategoryIndex";
        internal const string CantReadCounter = "CantReadCounter";
        internal const string CantReadInstance = "CantReadInstance";
        internal const string RemoteWriting = "RemoteWriting";
        internal const string CounterLayout = "CounterLayout";
        internal const string PossibleDeadlock = "PossibleDeadlock";
        internal const string SharedMemoryGhosted = "SharedMemoryGhosted";
        internal const string HelpNotAvailable = "HelpNotAvailable";
        internal const string PerfInvalidHelp = "PerfInvalidHelp";
        internal const string PerfInvalidCounterName = "PerfInvalidCounterName";
        internal const string PerfInvalidCategoryName = "PerfInvalidCategoryName";
        internal const string MustAddCounterCreationData = "MustAddCounterCreationData";
        internal const string RemoteCounterAdmin = "RemoteCounterAdmin";
        internal const string NoInstanceInformation = "NoInstanceInformation";
        internal const string PerfCounterPdhError = "PerfCounterPdhError";
        internal const string MultiInstanceOnly = "MultiInstanceOnly";
        internal const string SingleInstanceOnly = "SingleInstanceOnly";
        internal const string InstanceNameTooLong = "InstanceNameTooLong";
        internal const string CategoryNameTooLong = "CategoryNameTooLong";
        internal const string InstanceLifetimeProcessonReadOnly = "InstanceLifetimeProcessonReadOnly";
        internal const string InstanceLifetimeProcessforSingleInstance = "InstanceLifetimeProcessforSingleInstance";
        internal const string InstanceAlreadyExists = "InstanceAlreadyExists";
        internal const string CantSetLifetimeAfterInitialized = "CantSetLifetimeAfterInitialized";
        internal const string ProcessLifetimeNotValidInGlobal = "ProcessLifetimeNotValidInGlobal";
        internal const string CantConvertProcessToGlobal = "CantConvertProcessToGlobal";
        internal const string CantConvertGlobalToProcess = "CantConvertGlobalToProcess";
        internal const string PCNotSupportedUnderAppContainer = "PCNotSupportedUnderAppContainer";
        internal const string PriorityClassNotSupported = "PriorityClassNotSupported";
        internal const string WinNTRequired = "WinNTRequired";
        internal const string Win2kRequired = "Win2kRequired";
        internal const string NoAssociatedProcess = "NoAssociatedProcess";
        internal const string ProcessIdRequired = "ProcessIdRequired";
        internal const string NotSupportedRemote = "NotSupportedRemote";
        internal const string NoProcessInfo = "NoProcessInfo";
        internal const string WaitTillExit = "WaitTillExit";
        internal const string NoProcessHandle = "NoProcessHandle";
        internal const string MissingProccess = "MissingProccess";
        internal const string BadMinWorkset = "BadMinWorkset";
        internal const string BadMaxWorkset = "BadMaxWorkset";
        internal const string WinNTRequiredForRemote = "WinNTRequiredForRemote";
        internal const string ProcessHasExited = "ProcessHasExited";
        internal const string ProcessHasExitedNoId = "ProcessHasExitedNoId";
        internal const string ThreadExited = "ThreadExited";
        internal const string Win2000Required = "Win2000Required";
        internal const string ProcessNotFound = "ProcessNotFound";
        internal const string CantGetProcessId = "CantGetProcessId";
        internal const string ProcessDisabled = "ProcessDisabled";
        internal const string WaitReasonUnavailable = "WaitReasonUnavailable";
        internal const string NotSupportedRemoteThread = "NotSupportedRemoteThread";
        internal const string UseShellExecuteRequiresSTA = "UseShellExecuteRequiresSTA";
        internal const string CantRedirectStreams = "CantRedirectStreams";
        internal const string CantUseEnvVars = "CantUseEnvVars";
        internal const string CantStartAsUser = "CantStartAsUser";
        internal const string CouldntConnectToRemoteMachine = "CouldntConnectToRemoteMachine";
        internal const string CouldntGetProcessInfos = "CouldntGetProcessInfos";
        internal const string InputIdleUnkownError = "InputIdleUnkownError";
        internal const string FileNameMissing = "FileNameMissing";
        internal const string EnvironmentBlock = "EnvironmentBlock";
        internal const string EnumProcessModuleFailed = "EnumProcessModuleFailed";
        internal const string EnumProcessModuleFailedDueToWow = "EnumProcessModuleFailedDueToWow";
        internal const string PendingAsyncOperation = "PendingAsyncOperation";
        internal const string NoAsyncOperation = "NoAsyncOperation";
        internal const string InvalidApplication = "InvalidApplication";
        internal const string StandardOutputEncodingNotAllowed = "StandardOutputEncodingNotAllowed";
        internal const string StandardErrorEncodingNotAllowed = "StandardErrorEncodingNotAllowed";
        internal const string CountersOOM = "CountersOOM";
        internal const string MappingCorrupted = "MappingCorrupted";
        internal const string SetSecurityDescriptorFailed = "SetSecurityDescriptorFailed";
        internal const string CantCreateFileMapping = "CantCreateFileMapping";
        internal const string CantMapFileView = "CantMapFileView";
        internal const string CantGetMappingSize = "CantGetMappingSize";
        internal const string CantGetStandardOut = "CantGetStandardOut";
        internal const string CantGetStandardIn = "CantGetStandardIn";
        internal const string CantGetStandardError = "CantGetStandardError";
        internal const string CantMixSyncAsyncOperation = "CantMixSyncAsyncOperation";
        internal const string NoFileMappingSize = "NoFileMappingSize";
        internal const string EnvironmentBlockTooLong = "EnvironmentBlockTooLong";
        internal const string CantSetDuplicatePassword = "CantSetDuplicatePassword";
        internal const string Arg_InvalidSerialPort = "Arg_InvalidSerialPort";
        internal const string Arg_InvalidSerialPortExtended = "Arg_InvalidSerialPortExtended";
        internal const string Arg_SecurityException = "Arg_SecurityException";
        internal const string Argument_InvalidOffLen = "Argument_InvalidOffLen";
        internal const string ArgumentNull_Array = "ArgumentNull_Array";
        internal const string ArgumentNull_Buffer = "ArgumentNull_Buffer";
        internal const string ArgumentOutOfRange_Bounds_Lower_Upper = "ArgumentOutOfRange_Bounds_Lower_Upper";
        internal const string ArgumentOutOfRange_Enum = "ArgumentOutOfRange_Enum";
        internal const string ArgumentOutOfRange_NeedNonNegNumRequired = "ArgumentOutOfRange_NeedNonNegNumRequired";
        internal const string ArgumentOutOfRange_NeedPosNum = "ArgumentOutOfRange_NeedPosNum";
        internal const string ArgumentOutOfRange_Timeout = "ArgumentOutOfRange_Timeout";
        internal const string ArgumentOutOfRange_WriteTimeout = "ArgumentOutOfRange_WriteTimeout";
        internal const string ArgumentOutOfRange_OffsetOut = "ArgumentOutOfRange_OffsetOut";
        internal const string IndexOutOfRange_IORaceCondition = "IndexOutOfRange_IORaceCondition";
        internal const string IO_BindHandleFailed = "IO_BindHandleFailed";
        internal const string IO_OperationAborted = "IO_OperationAborted";
        internal const string NotSupported_UnseekableStream = "NotSupported_UnseekableStream";
        internal const string IO_EOF_ReadBeyondEOF = "IO_EOF_ReadBeyondEOF";
        internal const string ObjectDisposed_StreamClosed = "ObjectDisposed_StreamClosed";
        internal const string UnauthorizedAccess_IODenied_Path = "UnauthorizedAccess_IODenied_Path";
        internal const string IO_UnknownError = "IO_UnknownError";
        internal const string Arg_WrongAsyncResult = "Arg_WrongAsyncResult";
        internal const string InvalidOperation_EndReadCalledMultiple = "InvalidOperation_EndReadCalledMultiple";
        internal const string InvalidOperation_EndWriteCalledMultiple = "InvalidOperation_EndWriteCalledMultiple";
        internal const string IO_PortNotFound = "IO_PortNotFound";
        internal const string IO_PortNotFoundFileName = "IO_PortNotFoundFileName";
        internal const string UnauthorizedAccess_IODenied_NoPathName = "UnauthorizedAccess_IODenied_NoPathName";
        internal const string IO_PathTooLong = "IO_PathTooLong";
        internal const string IO_SharingViolation_NoFileName = "IO_SharingViolation_NoFileName";
        internal const string IO_SharingViolation_File = "IO_SharingViolation_File";
        internal const string NotSupported_UnwritableStream = "NotSupported_UnwritableStream";
        internal const string ObjectDisposed_WriterClosed = "ObjectDisposed_WriterClosed";
        internal const string BaseStream_Invalid_Not_Open = "BaseStream_Invalid_Not_Open";
        internal const string PortNameEmpty_String = "PortNameEmpty_String";
        internal const string Port_not_open = "Port_not_open";
        internal const string Port_already_open = "Port_already_open";
        internal const string Cant_be_set_when_open = "Cant_be_set_when_open";
        internal const string Max_Baud = "Max_Baud";
        internal const string In_Break_State = "In_Break_State";
        internal const string Write_timed_out = "Write_timed_out";
        internal const string CantSetRtsWithHandshaking = "CantSetRtsWithHandshaking";
        internal const string NotSupportedOS = "NotSupportedOS";
        internal const string NotSupportedEncoding = "NotSupportedEncoding";
        internal const string BaudRate = "BaudRate";
        internal const string DataBits = "DataBits";
        internal const string DiscardNull = "DiscardNull";
        internal const string DtrEnable = "DtrEnable";
        internal const string Encoding = "Encoding";
        internal const string Handshake = "Handshake";
        internal const string NewLine = "NewLine";
        internal const string Parity = "Parity";
        internal const string ParityReplace = "ParityReplace";
        internal const string PortName = "PortName";
        internal const string ReadBufferSize = "ReadBufferSize";
        internal const string ReadTimeout = "ReadTimeout";
        internal const string ReceivedBytesThreshold = "ReceivedBytesThreshold";
        internal const string RtsEnable = "RtsEnable";
        internal const string SerialPortDesc = "SerialPortDesc";
        internal const string StopBits = "StopBits";
        internal const string WriteBufferSize = "WriteBufferSize";
        internal const string WriteTimeout = "WriteTimeout";
        internal const string SerialErrorReceived = "SerialErrorReceived";
        internal const string SerialPinChanged = "SerialPinChanged";
        internal const string SerialDataReceived = "SerialDataReceived";
        internal const string CounterType = "CounterType";
        internal const string CounterName = "CounterName";
        internal const string CounterHelp = "CounterHelp";
        internal const string EventLogDesc = "EventLogDesc";
        internal const string ErrorDataReceived = "ErrorDataReceived";
        internal const string LogEntries = "LogEntries";
        internal const string LogLog = "LogLog";
        internal const string LogMachineName = "LogMachineName";
        internal const string LogMonitoring = "LogMonitoring";
        internal const string LogSynchronizingObject = "LogSynchronizingObject";
        internal const string LogSource = "LogSource";
        internal const string LogEntryWritten = "LogEntryWritten";
        internal const string LogEntryMachineName = "LogEntryMachineName";
        internal const string LogEntryData = "LogEntryData";
        internal const string LogEntryIndex = "LogEntryIndex";
        internal const string LogEntryCategory = "LogEntryCategory";
        internal const string LogEntryCategoryNumber = "LogEntryCategoryNumber";
        internal const string LogEntryEventID = "LogEntryEventID";
        internal const string LogEntryEntryType = "LogEntryEntryType";
        internal const string LogEntryMessage = "LogEntryMessage";
        internal const string LogEntrySource = "LogEntrySource";
        internal const string LogEntryReplacementStrings = "LogEntryReplacementStrings";
        internal const string LogEntryResourceId = "LogEntryResourceId";
        internal const string LogEntryTimeGenerated = "LogEntryTimeGenerated";
        internal const string LogEntryTimeWritten = "LogEntryTimeWritten";
        internal const string LogEntryUserName = "LogEntryUserName";
        internal const string OutputDataReceived = "OutputDataReceived";
        internal const string PC_CounterHelp = "PC_CounterHelp";
        internal const string PC_CounterType = "PC_CounterType";
        internal const string PC_ReadOnly = "PC_ReadOnly";
        internal const string PC_RawValue = "PC_RawValue";
        internal const string ProcessAssociated = "ProcessAssociated";
        internal const string ProcessDesc = "ProcessDesc";
        internal const string ProcessExitCode = "ProcessExitCode";
        internal const string ProcessTerminated = "ProcessTerminated";
        internal const string ProcessExitTime = "ProcessExitTime";
        internal const string ProcessHandle = "ProcessHandle";
        internal const string ProcessHandleCount = "ProcessHandleCount";
        internal const string ProcessId = "ProcessId";
        internal const string ProcessMachineName = "ProcessMachineName";
        internal const string ProcessMainModule = "ProcessMainModule";
        internal const string ProcessModules = "ProcessModules";
        internal const string ProcessSynchronizingObject = "ProcessSynchronizingObject";
        internal const string ProcessSessionId = "ProcessSessionId";
        internal const string ProcessThreads = "ProcessThreads";
        internal const string ProcessEnableRaisingEvents = "ProcessEnableRaisingEvents";
        internal const string ProcessExited = "ProcessExited";
        internal const string ProcessFileName = "ProcessFileName";
        internal const string ProcessWorkingDirectory = "ProcessWorkingDirectory";
        internal const string ProcessBasePriority = "ProcessBasePriority";
        internal const string ProcessMainWindowHandle = "ProcessMainWindowHandle";
        internal const string ProcessMainWindowTitle = "ProcessMainWindowTitle";
        internal const string ProcessMaxWorkingSet = "ProcessMaxWorkingSet";
        internal const string ProcessMinWorkingSet = "ProcessMinWorkingSet";
        internal const string ProcessNonpagedSystemMemorySize = "ProcessNonpagedSystemMemorySize";
        internal const string ProcessPagedMemorySize = "ProcessPagedMemorySize";
        internal const string ProcessPagedSystemMemorySize = "ProcessPagedSystemMemorySize";
        internal const string ProcessPeakPagedMemorySize = "ProcessPeakPagedMemorySize";
        internal const string ProcessPeakWorkingSet = "ProcessPeakWorkingSet";
        internal const string ProcessPeakVirtualMemorySize = "ProcessPeakVirtualMemorySize";
        internal const string ProcessPriorityBoostEnabled = "ProcessPriorityBoostEnabled";
        internal const string ProcessPriorityClass = "ProcessPriorityClass";
        internal const string ProcessPrivateMemorySize = "ProcessPrivateMemorySize";
        internal const string ProcessPrivilegedProcessorTime = "ProcessPrivilegedProcessorTime";
        internal const string ProcessProcessName = "ProcessProcessName";
        internal const string ProcessProcessorAffinity = "ProcessProcessorAffinity";
        internal const string ProcessResponding = "ProcessResponding";
        internal const string ProcessStandardError = "ProcessStandardError";
        internal const string ProcessStandardInput = "ProcessStandardInput";
        internal const string ProcessStandardOutput = "ProcessStandardOutput";
        internal const string ProcessStartInfo = "ProcessStartInfo";
        internal const string ProcessStartTime = "ProcessStartTime";
        internal const string ProcessTotalProcessorTime = "ProcessTotalProcessorTime";
        internal const string ProcessUserProcessorTime = "ProcessUserProcessorTime";
        internal const string ProcessVirtualMemorySize = "ProcessVirtualMemorySize";
        internal const string ProcessWorkingSet = "ProcessWorkingSet";
        internal const string ProcModModuleName = "ProcModModuleName";
        internal const string ProcModFileName = "ProcModFileName";
        internal const string ProcModBaseAddress = "ProcModBaseAddress";
        internal const string ProcModModuleMemorySize = "ProcModModuleMemorySize";
        internal const string ProcModEntryPointAddress = "ProcModEntryPointAddress";
        internal const string ProcessVerb = "ProcessVerb";
        internal const string ProcessArguments = "ProcessArguments";
        internal const string ProcessErrorDialog = "ProcessErrorDialog";
        internal const string ProcessWindowStyle = "ProcessWindowStyle";
        internal const string ProcessCreateNoWindow = "ProcessCreateNoWindow";
        internal const string ProcessEnvironmentVariables = "ProcessEnvironmentVariables";
        internal const string ProcessRedirectStandardInput = "ProcessRedirectStandardInput";
        internal const string ProcessRedirectStandardOutput = "ProcessRedirectStandardOutput";
        internal const string ProcessRedirectStandardError = "ProcessRedirectStandardError";
        internal const string ProcessUseShellExecute = "ProcessUseShellExecute";
        internal const string ThreadBasePriority = "ThreadBasePriority";
        internal const string ThreadCurrentPriority = "ThreadCurrentPriority";
        internal const string ThreadId = "ThreadId";
        internal const string ThreadPriorityBoostEnabled = "ThreadPriorityBoostEnabled";
        internal const string ThreadPriorityLevel = "ThreadPriorityLevel";
        internal const string ThreadPrivilegedProcessorTime = "ThreadPrivilegedProcessorTime";
        internal const string ThreadStartAddress = "ThreadStartAddress";
        internal const string ThreadStartTime = "ThreadStartTime";
        internal const string ThreadThreadState = "ThreadThreadState";
        internal const string ThreadTotalProcessorTime = "ThreadTotalProcessorTime";
        internal const string ThreadUserProcessorTime = "ThreadUserProcessorTime";
        internal const string ThreadWaitReason = "ThreadWaitReason";
        internal const string VerbEditorDefault = "VerbEditorDefault";
        internal const string AppSettingsReaderNoKey = "AppSettingsReaderNoKey";
        internal const string AppSettingsReaderNoParser = "AppSettingsReaderNoParser";
        internal const string AppSettingsReaderCantParse = "AppSettingsReaderCantParse";
        internal const string AppSettingsReaderEmptyString = "AppSettingsReaderEmptyString";
        internal const string InvalidPermissionState = "InvalidPermissionState";
        internal const string PermissionNumberOfElements = "PermissionNumberOfElements";
        internal const string PermissionItemExists = "PermissionItemExists";
        internal const string PermissionItemDoesntExist = "PermissionItemDoesntExist";
        internal const string PermissionBadParameterEnum = "PermissionBadParameterEnum";
        internal const string PermissionInvalidLength = "PermissionInvalidLength";
        internal const string PermissionTypeMismatch = "PermissionTypeMismatch";
        internal const string Argument_NotAPermissionElement = "Argument_NotAPermissionElement";
        internal const string Argument_InvalidXMLBadVersion = "Argument_InvalidXMLBadVersion";
        internal const string InvalidPermissionLevel = "InvalidPermissionLevel";
        internal const string TargetNotWebBrowserPermissionLevel = "TargetNotWebBrowserPermissionLevel";
        internal const string WebBrowserBadXml = "WebBrowserBadXml";
        internal const string KeyedCollNeedNonNegativeNum = "KeyedCollNeedNonNegativeNum";
        internal const string KeyedCollDuplicateKey = "KeyedCollDuplicateKey";
        internal const string KeyedCollReferenceKeyNotFound = "KeyedCollReferenceKeyNotFound";
        internal const string KeyedCollKeyNotFound = "KeyedCollKeyNotFound";
        internal const string KeyedCollInvalidKey = "KeyedCollInvalidKey";
        internal const string KeyedCollCapacityOverflow = "KeyedCollCapacityOverflow";
        internal const string InvalidOperation_EnumEnded = "InvalidOperation_EnumEnded";
        internal const string OrderedDictionary_ReadOnly = "OrderedDictionary_ReadOnly";
        internal const string OrderedDictionary_SerializationMismatch = "OrderedDictionary_SerializationMismatch";
        internal const string Async_ExceptionOccurred = "Async_ExceptionOccurred";
        internal const string Async_QueueingFailed = "Async_QueueingFailed";
        internal const string Async_OperationCancelled = "Async_OperationCancelled";
        internal const string Async_OperationAlreadyCompleted = "Async_OperationAlreadyCompleted";
        internal const string Async_NullDelegate = "Async_NullDelegate";
        internal const string BackgroundWorker_AlreadyRunning = "BackgroundWorker_AlreadyRunning";
        internal const string BackgroundWorker_CancellationNotSupported = "BackgroundWorker_CancellationNotSupported";
        internal const string BackgroundWorker_OperationCompleted = "BackgroundWorker_OperationCompleted";
        internal const string BackgroundWorker_ProgressNotSupported = "BackgroundWorker_ProgressNotSupported";
        internal const string BackgroundWorker_WorkerAlreadyRunning = "BackgroundWorker_WorkerAlreadyRunning";
        internal const string BackgroundWorker_WorkerDoesntReportProgress = "BackgroundWorker_WorkerDoesntReportProgress";
        internal const string BackgroundWorker_WorkerDoesntSupportCancellation = "BackgroundWorker_WorkerDoesntSupportCancellation";
        internal const string Async_ProgressChangedEventArgs_ProgressPercentage = "Async_ProgressChangedEventArgs_ProgressPercentage";
        internal const string Async_ProgressChangedEventArgs_UserState = "Async_ProgressChangedEventArgs_UserState";
        internal const string Async_AsyncEventArgs_Cancelled = "Async_AsyncEventArgs_Cancelled";
        internal const string Async_AsyncEventArgs_Error = "Async_AsyncEventArgs_Error";
        internal const string Async_AsyncEventArgs_UserState = "Async_AsyncEventArgs_UserState";
        internal const string BackgroundWorker_CancellationPending = "BackgroundWorker_CancellationPending";
        internal const string BackgroundWorker_DoWork = "BackgroundWorker_DoWork";
        internal const string BackgroundWorker_IsBusy = "BackgroundWorker_IsBusy";
        internal const string BackgroundWorker_ProgressChanged = "BackgroundWorker_ProgressChanged";
        internal const string BackgroundWorker_RunWorkerCompleted = "BackgroundWorker_RunWorkerCompleted";
        internal const string BackgroundWorker_WorkerReportsProgress = "BackgroundWorker_WorkerReportsProgress";
        internal const string BackgroundWorker_WorkerSupportsCancellation = "BackgroundWorker_WorkerSupportsCancellation";
        internal const string BackgroundWorker_DoWorkEventArgs_Argument = "BackgroundWorker_DoWorkEventArgs_Argument";
        internal const string BackgroundWorker_DoWorkEventArgs_Result = "BackgroundWorker_DoWorkEventArgs_Result";
        internal const string BackgroundWorker_Desc = "BackgroundWorker_Desc";
        internal const string InstanceCreationEditorDefaultText = "InstanceCreationEditorDefaultText";
        internal const string PropertyTabAttributeBadPropertyTabScope = "PropertyTabAttributeBadPropertyTabScope";
        internal const string PropertyTabAttributeTypeLoadException = "PropertyTabAttributeTypeLoadException";
        internal const string PropertyTabAttributeArrayLengthMismatch = "PropertyTabAttributeArrayLengthMismatch";
        internal const string PropertyTabAttributeParamsBothNull = "PropertyTabAttributeParamsBothNull";
        internal const string InstanceDescriptorCannotBeStatic = "InstanceDescriptorCannotBeStatic";
        internal const string InstanceDescriptorMustBeStatic = "InstanceDescriptorMustBeStatic";
        internal const string InstanceDescriptorMustBeReadable = "InstanceDescriptorMustBeReadable";
        internal const string InstanceDescriptorLengthMismatch = "InstanceDescriptorLengthMismatch";
        internal const string ToolboxItemAttributeFailedGetType = "ToolboxItemAttributeFailedGetType";
        internal const string PropertyDescriptorCollectionBadValue = "PropertyDescriptorCollectionBadValue";
        internal const string PropertyDescriptorCollectionBadKey = "PropertyDescriptorCollectionBadKey";
        internal const string AspNetHostingPermissionBadXml = "AspNetHostingPermissionBadXml";
        internal const string CorruptedGZipHeader = "CorruptedGZipHeader";
        internal const string UnknownCompressionMode = "UnknownCompressionMode";
        internal const string UnknownState = "UnknownState";
        internal const string InvalidHuffmanData = "InvalidHuffmanData";
        internal const string InvalidCRC = "InvalidCRC";
        internal const string InvalidStreamSize = "InvalidStreamSize";
        internal const string UnknownBlockType = "UnknownBlockType";
        internal const string InvalidBlockLength = "InvalidBlockLength";
        internal const string GenericInvalidData = "GenericInvalidData";
        internal const string CannotReadFromDeflateStream = "CannotReadFromDeflateStream";
        internal const string CannotWriteToDeflateStream = "CannotWriteToDeflateStream";
        internal const string NotReadableStream = "NotReadableStream";
        internal const string NotWriteableStream = "NotWriteableStream";
        internal const string InvalidArgumentOffsetCount = "InvalidArgumentOffsetCount";
        internal const string InvalidBeginCall = "InvalidBeginCall";
        internal const string InvalidEndCall = "InvalidEndCall";
        internal const string StreamSizeOverflow = "StreamSizeOverflow";
        internal const string ZLibErrorDLLLoadError = "ZLibErrorDLLLoadError";
        internal const string ZLibErrorUnexpected = "ZLibErrorUnexpected";
        internal const string ZLibErrorInconsistentStream = "ZLibErrorInconsistentStream";
        internal const string ZLibErrorSteamFreedPrematurely = "ZLibErrorSteamFreedPrematurely";
        internal const string ZLibErrorNotEnoughMemory = "ZLibErrorNotEnoughMemory";
        internal const string ZLibErrorIncorrectInitParameters = "ZLibErrorIncorrectInitParameters";
        internal const string ZLibErrorVersionMismatch = "ZLibErrorVersionMismatch";
        internal const string InvalidOperation_HCCountOverflow = "InvalidOperation_HCCountOverflow";
        internal const string Argument_InvalidThreshold = "Argument_InvalidThreshold";
        internal const string Argument_SemaphoreInitialMaximum = "Argument_SemaphoreInitialMaximum";
        internal const string Argument_WaitHandleNameTooLong = "Argument_WaitHandleNameTooLong";
        internal const string WaitHandleCannotBeOpenedException_InvalidHandle = "WaitHandleCannotBeOpenedException_InvalidHandle";
        internal const string ArgumentNotAPermissionElement = "ArgumentNotAPermissionElement";
        internal const string ArgumentWrongType = "ArgumentWrongType";
        internal const string BadXmlVersion = "BadXmlVersion";
        internal const string BinarySerializationNotSupported = "BinarySerializationNotSupported";
        internal const string BothScopeAttributes = "BothScopeAttributes";
        internal const string NoScopeAttributes = "NoScopeAttributes";
        internal const string PositionOutOfRange = "PositionOutOfRange";
        internal const string ProviderInstantiationFailed = "ProviderInstantiationFailed";
        internal const string ProviderTypeLoadFailed = "ProviderTypeLoadFailed";
        internal const string SaveAppScopedNotSupported = "SaveAppScopedNotSupported";
        internal const string SettingsResetFailed = "SettingsResetFailed";
        internal const string SettingsSaveFailed = "SettingsSaveFailed";
        internal const string SettingsSaveFailedNoSection = "SettingsSaveFailedNoSection";
        internal const string StringDeserializationFailed = "StringDeserializationFailed";
        internal const string StringSerializationFailed = "StringSerializationFailed";
        internal const string UnknownSerializationFormat = "UnknownSerializationFormat";
        internal const string UnknownSeekOrigin = "UnknownSeekOrigin";
        internal const string UnknownUserLevel = "UnknownUserLevel";
        internal const string UserSettingsNotSupported = "UserSettingsNotSupported";
        internal const string XmlDeserializationFailed = "XmlDeserializationFailed";
        internal const string XmlSerializationFailed = "XmlSerializationFailed";
        internal const string MemberRelationshipService_RelationshipNotSupported = "MemberRelationshipService_RelationshipNotSupported";
        internal const string MaskedTextProviderPasswordAndPromptCharError = "MaskedTextProviderPasswordAndPromptCharError";
        internal const string MaskedTextProviderInvalidCharError = "MaskedTextProviderInvalidCharError";
        internal const string MaskedTextProviderMaskNullOrEmpty = "MaskedTextProviderMaskNullOrEmpty";
        internal const string MaskedTextProviderMaskInvalidChar = "MaskedTextProviderMaskInvalidChar";
        internal const string StandardOleMarshalObjectGetMarshalerFailed = "StandardOleMarshalObjectGetMarshalerFailed";
        internal const string SoundAPIBadSoundLocation = "SoundAPIBadSoundLocation";
        internal const string SoundAPIFileDoesNotExist = "SoundAPIFileDoesNotExist";
        internal const string SoundAPIFormatNotSupported = "SoundAPIFormatNotSupported";
        internal const string SoundAPIInvalidWaveFile = "SoundAPIInvalidWaveFile";
        internal const string SoundAPIInvalidWaveHeader = "SoundAPIInvalidWaveHeader";
        internal const string SoundAPILoadTimedOut = "SoundAPILoadTimedOut";
        internal const string SoundAPILoadTimeout = "SoundAPILoadTimeout";
        internal const string SoundAPIReadError = "SoundAPIReadError";
        internal const string WrongActionForCtor = "WrongActionForCtor";
        internal const string MustBeResetAddOrRemoveActionForCtor = "MustBeResetAddOrRemoveActionForCtor";
        internal const string ResetActionRequiresNullItem = "ResetActionRequiresNullItem";
        internal const string ResetActionRequiresIndexMinus1 = "ResetActionRequiresIndexMinus1";
        internal const string IndexCannotBeNegative = "IndexCannotBeNegative";
        internal const string ObservableCollectionReentrancyNotAllowed = "ObservableCollectionReentrancyNotAllowed";
        internal const string net_ssl_io_already_shutdown = "net_ssl_io_already_shutdown";
        internal const string net_io_readwritefailure = "net_io_readwritefailure";
        internal const string Cryptography_X509_InvalidFlagCombination = "Cryptography_X509_InvalidFlagCombination";
        [StaticField]
        private static SR loader;
        private ResourceManager resources;

        internal SR() => this.resources = new ResourceManager("System", this.GetType().Assembly);

        private static SR GetLoader()
        {
            if (SR.loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange<SR>(ref SR.loader, sr, (SR)null);
            }

            return SR.loader;
        }

        [StaticField]
        private static CultureInfo Culture => (CultureInfo)null;

        [StaticField]
        public static ResourceManager Resources => SR.GetLoader().resources;

        public static string GetString(string name, params object[] args)
        {
            SR loader = SR.GetLoader();
            if (loader == null)
                return (string)null;
            string format = loader.resources.GetString(name, SR.Culture);
            if (args == null || args.Length == 0)
                return format;
            for (int index = 0; index < args.Length; ++index)
            {
                if (args[index] is string str && str.Length > 1024)
                    args[index] = (object)(str.Substring(0, 1021) + "...");
            }

            return string.Format((IFormatProvider)CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name)
        {
            return SR.GetLoader()?.resources.GetString(name, SR.Culture);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return SR.GetString(name);
        }

        public static object GetObject(string name)
        {
            return SR.GetLoader()?.resources.GetObject(name, SR.Culture);
        }
    }

    internal ref struct BitHelper
    {
        private const int IntSize = sizeof(int) * 8;
        private readonly Span<int> _span;

        internal BitHelper(Span<int> span, bool clear)
        {
            if (clear)
            {
                span.Clear();
            }
            _span = span;
        }

        internal void MarkBit(int bitPosition)
        {
            Debug.Assert(bitPosition >= 0);

            uint bitArrayIndex = (uint)bitPosition / IntSize;

            // Workaround for https://github.com/dotnet/runtime/issues/72004
            Span<int> span = _span;
            if (bitArrayIndex < (uint)span.Length)
            {
                span[(int)bitArrayIndex] |= (1 << (int)((uint)bitPosition % IntSize));
            }
        }

        internal bool IsMarked(int bitPosition)
        {
            Debug.Assert(bitPosition >= 0);

            uint bitArrayIndex = (uint)bitPosition / IntSize;

            // Workaround for https://github.com/dotnet/runtime/issues/72004
            Span<int> span = _span;
            return
                    bitArrayIndex < (uint)span.Length &&
                    (span[(int)bitArrayIndex] & (1 << ((int)((uint)bitPosition % IntSize)))) != 0;
        }

        /// <summary>How many ints must be allocated to represent n bits. Returns (n+31)/32, but avoids overflow.</summary>
        internal static int ToIntArrayLength(int n) => n > 0 ? ((n - 1) / IntSize + 1) : 0;
    }
    
    internal static partial class EnumerableHelpers
    {
        private const int MaxLength = 2147483591;
        
        /// <summary>Gets an enumerator singleton for an empty collection.</summary>
        internal static IEnumerator<T> GetEmptyEnumerator<T>() =>
            ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator();

        /// <summary>Converts an enumerable to an array using the same logic as List{T}.</summary>
        /// <param name="source">The enumerable to convert.</param>
        /// <param name="length">The number of items stored in the resulting array, 0-indexed.</param>
        /// <returns>
        /// The resulting array.  The length of the array may be greater than <paramref name="length"/>,
        /// which is the actual number of elements in the array.
        /// </returns>
        internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
        {
            if (source is ICollection<T> ic)
            {
                int count = ic.Count;
                if (count != 0)
                {
                    // Allocate an array of the desired size, then copy the elements into it. Note that this has the same
                    // issue regarding concurrency as other existing collections like List<T>. If the collection size
                    // concurrently changes between the array allocation and the CopyTo, we could end up either getting an
                    // exception from overrunning the array (if the size went up) or we could end up not filling as many
                    // items as 'count' suggests (if the size went down).  This is only an issue for concurrent collections
                    // that implement ICollection<T>, which as of .NET 4.6 is just ConcurrentDictionary<TKey, TValue>.
                    T[] arr = new T[count];
                    ic.CopyTo(arr, 0);
                    length = count;
                    return arr;
                }
            }
            else
            {
                using (var en = source.GetEnumerator())
                {
                    if (en.MoveNext())
                    {
                        const int DefaultCapacity = 4;
                        T[] arr = new T[DefaultCapacity];
                        arr[0] = en.Current;
                        int count = 1;

                        while (en.MoveNext())
                        {
                            if (count == arr.Length)
                            {
                                // This is the same growth logic as in List<T>:
                                // If the array is currently empty, we make it a default size.  Otherwise, we attempt to
                                // double the size of the array.  Doubling will overflow once the size of the array reaches
                                // 2^30, since doubling to 2^31 is 1 larger than Int32.MaxValue.  In that case, we instead
                                // constrain the length to be Array.MaxLength (this overflow check works because of the
                                // cast to uint).
                                int newLength = count << 1;
                                if ((uint)newLength > MaxLength)
                                {
                                    newLength = MaxLength <= count ? count + 1 : MaxLength;
                                }

                                Array.Resize(ref arr, newLength);
                            }

                            arr[count++] = en.Current;
                        }

                        length = count;
                        return arr;
                    }
                }
            }

            length = 0;
            return Array.Empty<T>();
        }
    }
    
    
    public partial class SortedSet<T>
    {
        /// <summary>
        /// This class represents a subset view into the tree. Any changes to this view
        /// are reflected in the actual tree. It uses the comparer of the underlying tree.
        /// </summary>
        internal sealed class TreeSubSet : SortedSet<T>, ISerializable, IDeserializationCallback
        {
            private readonly SortedSet<T> _underlying;
            private readonly T _min;
            private readonly T _max;
            // keeps track of whether the count variable is up to date
            // up to date -> _countVersion = _underlying.version
            // not up to date -> _countVersion < _underlying.version
            private int _countVersion;
            // these exist for unbounded collections
            // for instance, you could allow this subset to be defined for i > 10. The set will throw if
            // anything <= 10 is added, but there is no upper bound. These features Head(), Tail(), were punted
            // in the spec, and are not available, but the framework is there to make them available at some point.
            private readonly bool _lBoundActive, _uBoundActive;
            // used to see if the count is out of date

#if DEBUG
            internal override bool versionUpToDate()
            {
                return (version == _underlying.version);
            }
#endif

            public TreeSubSet(SortedSet<T> Underlying, T Min, T Max, bool lowerBoundActive, bool upperBoundActive)
                : base(Underlying.Comparer)
            {
                _underlying = Underlying;
                _min = Min;
                _max = Max;
                _lBoundActive = lowerBoundActive;
                _uBoundActive = upperBoundActive;
                root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive); // root is first element within range
                count = 0;
                version = -1;
                _countVersion = -1;
            }

            internal override bool AddIfNotPresent(T item)
            {
                if (!IsWithinRange(item))
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }

                bool ret = _underlying.AddIfNotPresent(item);
                VersionCheck();
#if DEBUG
                Debug.Assert(this.versionUpToDate() && root == _underlying.FindRange(_min, _max));
#endif

                return ret;
            }

            public override bool Contains(T item)
            {
                VersionCheck();
#if DEBUG
                Debug.Assert(versionUpToDate() && root == _underlying.FindRange(_min, _max));
#endif
                return base.Contains(item);
            }

            internal override bool DoRemove(T item)
            {
                if (!IsWithinRange(item))
                {
                    return false;
                }

                bool ret = _underlying.Remove(item);
                VersionCheck();
#if DEBUG
                Debug.Assert(versionUpToDate() && root == _underlying.FindRange(_min, _max));
#endif
                return ret;
            }

            public override void Clear()
            {
                if (Count == 0)
                {
                    return;
                }

                List<T> toRemove = new List<T>();
                BreadthFirstTreeWalk(n => { toRemove.Add(n.Item); return true; });
                while (toRemove.Count != 0)
                {
                    _underlying.Remove(toRemove[^1]);
                    toRemove.RemoveAt(toRemove.Count - 1);
                }

                root = null;
                count = 0;
                version = _underlying.version;
            }

            internal override bool IsWithinRange(T item)
            {
                int comp = _lBoundActive ? Comparer.Compare(_min, item) : -1;
                if (comp > 0)
                {
                    return false;
                }

                comp = _uBoundActive ? Comparer.Compare(_max, item) : 1;
                return comp >= 0;
            }

            internal override T MinInternal
            {
                get
                {
                    VersionCheck();
                    Node current = root;
                    T result = default;

                    while (current != null)
                    {

                        int comp = _lBoundActive ? Comparer.Compare(_min, current.Item) : -1;
                        if (comp > 0)
                        {
                            current = current.Right;
                        }
                        else
                        {
                            result = current.Item;
                            if (comp == 0)
                            {
                                break;
                            }
                            current = current.Left;
                        }
                    }

                    return result!;
                }
            }

            internal override T MaxInternal
            {
                get
                {
                    VersionCheck();
                    Node current = root;
                    T result = default;

                    while (current != null)
                    {
                        int comp = _uBoundActive ? Comparer.Compare(_max, current.Item) : 1;
                        if (comp < 0)
                        {
                            current = current.Left;
                        }
                        else
                        {
                            result = current.Item;
                            if (comp == 0)
                            {
                                break;
                            }
                            current = current.Right;
                        }
                    }

                    return result!;
                }
            }

            internal override bool InOrderTreeWalk(TreeWalkPredicate<T> action)
            {
                VersionCheck();

                if (root == null)
                {
                    return true;
                }

                // The maximum height of a red-black tree is 2*lg(n+1).
                // See page 264 of "Introduction to algorithms" by Thomas H. Cormen
                Stack<Node> stack = new Stack<Node>(2 * (int)SortedSet<T>.Log2(count + 1)); // this is not exactly right if count is out of date, but the stack can grow
                Node current = root;
                while (current != null)
                {
                    if (IsWithinRange(current.Item))
                    {
                        stack.Push(current);
                        current = current.Left;
                    }
                    else if (_lBoundActive && Comparer.Compare(_min, current.Item) > 0)
                    {
                        current = current.Right;
                    }
                    else
                    {
                        current = current.Left;
                    }
                }

                while (stack.Count != 0)
                {
                    current = stack.Pop();
                    if (!action(current))
                    {
                        return false;
                    }

                    Node node = current.Right;
                    while (node != null)
                    {
                        if (IsWithinRange(node.Item))
                        {
                            stack.Push(node);
                            node = node.Left;
                        }
                        else if (_lBoundActive && Comparer.Compare(_min, node.Item) > 0)
                        {
                            node = node.Right;
                        }
                        else
                        {
                            node = node.Left;
                        }
                    }
                }
                return true;
            }

            internal override bool BreadthFirstTreeWalk(TreeWalkPredicate<T> action)
            {
                VersionCheck();

                if (root == null)
                {
                    return true;
                }

                Queue<Node> processQueue = new Queue<Node>();
                processQueue.Enqueue(root);
                Node current;

                while (processQueue.Count != 0)
                {
                    current = processQueue.Dequeue();
                    if (IsWithinRange(current.Item) && !action(current))
                    {
                        return false;
                    }
                    if (current.Left != null && (!_lBoundActive || Comparer.Compare(_min, current.Item) < 0))
                    {
                        processQueue.Enqueue(current.Left);
                    }
                    if (current.Right != null && (!_uBoundActive || Comparer.Compare(_max, current.Item) > 0))
                    {
                        processQueue.Enqueue(current.Right);
                    }
                }
                return true;
            }

            internal override SortedSet<T>.Node FindNode(T item)
            {
                if (!IsWithinRange(item))
                {
                    return null;
                }

                VersionCheck();
#if DEBUG
                Debug.Assert(this.versionUpToDate() && root == _underlying.FindRange(_min, _max));
#endif
                return base.FindNode(item);
            }

            // this does indexing in an inefficient way compared to the actual sortedset, but it saves a
            // lot of space
            internal override int InternalIndexOf(T item)
            {
                int count = -1;
                foreach (T i in this)
                {
                    count++;
                    if (Comparer.Compare(item, i) == 0)
                        return count;
                }
#if DEBUG
                Debug.Assert(this.versionUpToDate() && root == _underlying.FindRange(_min, _max));
#endif
                return -1;
            }

            /// <summary>
            /// Checks whether this subset is out of date, and updates it if necessary.
            /// </summary>
            /// <param name="updateCount">Updates the count variable if necessary.</param>
            internal override void VersionCheck(bool updateCount = false) => VersionCheckImpl(updateCount);

            private void VersionCheckImpl(bool updateCount)
            {
                Debug.Assert(_underlying != null);
                if (version != _underlying.version)
                {
                    root = _underlying.FindRange(_min, _max, _lBoundActive, _uBoundActive);
                    version = _underlying.version;
                }

                if (updateCount && _countVersion != _underlying.version)
                {
                    count = 0;
                    InOrderTreeWalk(n => { count++; return true; });
                    _countVersion = _underlying.version;
                }
            }

            /// <summary>
            /// Returns the number of elements <c>count</c> of the parent set.
            /// </summary>
            internal override int TotalCount()
            {
                Debug.Assert(_underlying != null);
                return _underlying.Count;
            }

            // This passes functionality down to the underlying tree, clipping edges if necessary
            // There's nothing gained by having a nested subset. May as well draw it from the base
            // Cannot increase the bounds of the subset, can only decrease it
            public override SortedSet<T> GetViewBetween(T lowerValue, T upperValue)
            {
                if (_lBoundActive && Comparer.Compare(_min, lowerValue) > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lowerValue));
                }
                if (_uBoundActive && Comparer.Compare(_max, upperValue) < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(upperValue));
                }
                return (TreeSubSet)_underlying.GetViewBetween(lowerValue, upperValue);
            }

#if DEBUG
            internal override void IntersectWithEnumerable(IEnumerable<T> other)
            {
                base.IntersectWithEnumerable(other);
                Debug.Assert(versionUpToDate() && root == _underlying.FindRange(_min, _max));
            }
#endif

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) => GetObjectData(info, context);

            protected override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new PlatformNotSupportedException();
            }

            void IDeserializationCallback.OnDeserialization(object sender)
            {
                throw new PlatformNotSupportedException();
            }

            protected override void OnDeserialization(object sender) => throw new PlatformNotSupportedException();
        }
    }
    
    internal sealed class SortedSetEqualityComparer<T> : IEqualityComparer<SortedSet<T>>
    {
        private readonly IComparer<T> _comparer;
        private readonly IEqualityComparer<T> _memberEqualityComparer;

        public SortedSetEqualityComparer(IEqualityComparer<T> memberEqualityComparer)
            : this(comparer: null, memberEqualityComparer: memberEqualityComparer)
        { }

        /// <summary>
        /// Create a new SetEqualityComparer, given a comparer for member order and another for member equality (these
        /// must be consistent in their definition of equality)
        /// </summary>
        private SortedSetEqualityComparer(IComparer<T> comparer, IEqualityComparer<T> memberEqualityComparer)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _memberEqualityComparer = memberEqualityComparer ?? EqualityComparer<T>.Default;
        }

        // Use _comparer to keep equals properties intact; don't want to choose one of the comparers.
        public bool Equals(SortedSet<T> x, SortedSet<T> y) => SortedSet<T>.SortedSetEquals(x, y, _comparer);

        // IMPORTANT: this part uses the fact that GetHashCode() is consistent with the notion of equality in the set.
        public int GetHashCode(SortedSet<T> obj)
        {
            int hashCode = 0;
            if (obj != null)
            {
                foreach (T t in obj)
                {
                    if (t != null)
                    {
                        hashCode ^= (_memberEqualityComparer.GetHashCode(t) & 0x7FFFFFFF);
                    }
                }
            }
            // Returns 0 for null sets.
            return hashCode;
        }

        // Equals method for the comparer itself.
        public override bool Equals(object obj)
        {
            SortedSetEqualityComparer<T> comparer = obj as SortedSetEqualityComparer<T>;
            return comparer != null && _comparer == comparer._comparer;
        }

        public override int GetHashCode() => _comparer.GetHashCode() ^ _memberEqualityComparer.GetHashCode();
    }
}