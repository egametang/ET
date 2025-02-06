using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

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
    
#if UNITY
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
#endif
    
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public partial class SortedSet<T> : ISet<T>, ICollection<T>, ICollection, IReadOnlyCollection<T>, IReadOnlySet<T>, ISerializable, IDeserializationCallback
    {
        #region Local variables/constants

        private Node root;
        private IComparer<T> comparer = default!;
        private int count;
        private int version;

        private Stack<Node> stack;

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
                lock (this._tree)
                {
                    if (set.stack != null)
                    {
                        _stack = set.stack;
                        set.stack = null;
                    }
                    else
                    {
                        _stack = new Stack<Node>(2 * (int)Math.Log(set.Count + 1));
                    }
                }

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
                this._stack.Clear();
                lock (this._tree)
                {
                    this._tree.stack = this._stack;
                }
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
    
    
    

    
    /// <summary>
    /// The exception that is thrown when a <see langword="null"/> reference (<see langword="Nothing"/> in Visual Basic) is passed to a method that does not accept it as a valid argument.
    /// </summary>
    [Serializable]
    public class ArgumentNullException : ArgumentException
    {

        public ArgumentNullException(string paramName)
            : base(SR.ArgumentNull_Generic, paramName)
        {
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

    internal sealed class SR
    {
        internal const string InvalidOperation_EnumOpCantHappen = "InvalidOperation_EnumOpCantHappen";
        internal const string InvalidOperation_EnumFailedVersion = "InvalidOperation_EnumFailedVersion";
        internal const string Serialization_MismatchedCount = "Serialization_MismatchedCount";
        internal const string Serialization_MissingValues = "Serialization_MissingValues";
        internal const string Serialization_InvalidOnDeser = "Serialization_InvalidOnDeser";
        internal const string ArgumentOutOfRange_Index = "ArgumentOutOfRange_Index";
        internal const string Arg_NonZeroLowerBound = "Arg_NonZeroLowerBound";
        internal const string Arg_RankMultiDimNotSupported = "Arg_RankMultiDimNotSupported";
        internal const string Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";
        internal const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
        internal const string ArgumentNull_Generic = "ArgumentNull_Generic";
        internal const string Argument_IncompatibleArrayType = "Argument_IncompatibleArrayType";
        internal const string NotSupported_ValueCollectionSet = "NotSupported_ValueCollectionSet";
        internal const string NotSupported_KeyCollectionSet = "NotSupported_KeyCollectionSet";
        internal const string SortedSet_LowerValueGreaterThanUpperValue = "SortedSet_LowerValueGreaterThanUpperValue";
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