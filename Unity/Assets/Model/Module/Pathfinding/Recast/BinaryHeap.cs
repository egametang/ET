#pragma warning disable 162
#pragma warning disable 429
#define DECREASE_KEY

namespace PF {
	/** Binary heap implementation.
	 * Binary heaps are really fast for ordering nodes in a way that
	 * makes it possible to get the node with the lowest F score.
	 * Also known as a priority queue.
	 *
	 * This has actually been rewritten as a 4-ary heap
	 * for performance, but it's the same principle.
	 *
	 * \see http://en.wikipedia.org/wiki/Binary_heap
	 * \see https://en.wikipedia.org/wiki/D-ary_heap
	 */
	public class BinaryHeap {
		/** Number of items in the tree */
		public int numberOfItems;

		/** The tree will grow by at least this factor every time it is expanded */
		public float growthFactor = 2;

		/**
		 * Number of children of each node in the tree.
		 * Different values have been tested and 4 has been empirically found to perform the best.
		 * \see https://en.wikipedia.org/wiki/D-ary_heap
		 */
		const int D = 4;

		/** Sort nodes by G score if there is a tie when comparing the F score.
		 * Disabling this will improve pathfinding performance with around 2.5%
		 * but may break ties between paths that have the same length in a less
		 * desirable manner (only relevant for grid graphs).
		 */
		const bool SortGScores = true;

		public const ushort NotInHeap = 0xFFFF;

		/** Internal backing array for the heap */
		private Tuple[] heap;

		/** True if the heap does not contain any elements */
		public bool isEmpty {
			get {
				return numberOfItems <= 0;
			}
		}

		/** Item in the heap */
		private struct Tuple {
			public PathNode node;
			public uint F;

			public Tuple (uint f, PathNode node) {
				this.F = f;
				this.node = node;
			}
		}

		/** Rounds up v so that it has remainder 1 when divided by D.
		 * I.e it is of the form n*D + 1 where n is any non-negative integer.
		 */
		static int RoundUpToNextMultipleMod1 (int v) {
			// I have a feeling there is a nicer way to do this
			return v + (4 - ((v-1) % D)) % D;
		}

		/** Create a new heap with the specified initial capacity */
		public BinaryHeap (int capacity) {
			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			capacity = RoundUpToNextMultipleMod1(capacity);

			heap = new Tuple[capacity];
			numberOfItems = 0;
		}

		/** Removes all elements from the heap */
		public void Clear () {
#if DECREASE_KEY
			// Clear all heap indices
			// This is important to avoid bugs
			for (int i = 0; i < numberOfItems; i++) {
				heap[i].node.heapIndex = NotInHeap;
			}
#endif

			numberOfItems = 0;
		}

		internal PathNode GetNode (int i) {
			return heap[i].node;
		}

		internal void SetF (int i, uint f) {
			heap[i].F = f;
		}

		/** Expands to a larger backing array when the current one is too small */
		void Expand () {
			// 65533 == 1 mod 4 and slightly smaller than 1<<16 = 65536
			int newSize = System.Math.Max(heap.Length+4, System.Math.Min(65533, (int)System.Math.Round(heap.Length*growthFactor)));

			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			newSize = RoundUpToNextMultipleMod1(newSize);

			// Check if the heap is really large
			// Also note that heaps larger than this are not supported
			// since PathNode.heapIndex is a ushort and can only store
			// values up to 65535 (NotInHeap = 65535 is reserved however)
			if (newSize > (1<<16) - 2) {
				throw new System.Exception("Binary Heap Size really large (>65534). A heap size this large is probably the cause of pathfinding running in an infinite loop. ");
			}

			var newHeap = new Tuple[newSize];
			heap.CopyTo(newHeap, 0);
			#if ASTARDEBUG
			UnityEngine.Debug.Log("Resizing binary heap to "+newSize);
			#endif
			heap = newHeap;
		}

		/** Adds a node to the heap */
		public void Add (PathNode node) {
			if (node == null) throw new System.ArgumentNullException("node");

#if DECREASE_KEY
			// Check if node is already in the heap
			if (node.heapIndex != NotInHeap) {
				DecreaseKey(heap[node.heapIndex], node.heapIndex);
				return;
			}
#endif

			if (numberOfItems == heap.Length) {
				Expand();
			}

			DecreaseKey(new Tuple(0, node), (ushort)numberOfItems);
			numberOfItems++;
		}

		void DecreaseKey (Tuple node, ushort index) {
			// This is where 'obj' is in the binary heap logically speaking
			// (for performance reasons we don't actually store it there until
			// we know the final index, that's just a waste of CPU cycles)
			int bubbleIndex = index;
			// Update F value, it might have changed since the node was originally added to the heap
			uint nodeF = node.F = node.node.F;
			uint nodeG = node.node.G;

			while (bubbleIndex != 0) {
				// Parent node of the bubble node
				int parentIndex = (bubbleIndex-1) / D;

				if (nodeF < heap[parentIndex].F || (SortGScores && nodeF == heap[parentIndex].F && nodeG > heap[parentIndex].node.G)) {
					// Swap the bubble node and parent node
					// (we don't really need to store the bubble node until we know the final index though
					// so we do that after the loop instead)
					heap[bubbleIndex] = heap[parentIndex];
#if DECREASE_KEY
					heap[bubbleIndex].node.heapIndex = (ushort)bubbleIndex;
#endif
					bubbleIndex = parentIndex;
				} else {
					break;
				}
			}

			heap[bubbleIndex] = node;
#if DECREASE_KEY
			node.node.heapIndex = (ushort)bubbleIndex;
#endif
		}

		/** Returns the node with the lowest F score from the heap */
		public PathNode Remove () {
			PathNode returnItem = heap[0].node;

#if DECREASE_KEY
			returnItem.heapIndex = NotInHeap;
#endif

			numberOfItems--;
			if (numberOfItems == 0) return returnItem;

			// Last item in the heap array
			var swapItem = heap[numberOfItems];
			var swapItemG = swapItem.node.G;

			int swapIndex = 0, parent;

			// Trickle upwards
			while (true) {
				parent = swapIndex;
				uint swapF = swapItem.F;
				int pd = parent * D + 1;

				// If this holds, then the indices used
				// below are guaranteed to not throw an index out of bounds
				// exception since we choose the size of the array in that way
				if (pd <= numberOfItems) {
					// Loading all F scores here instead of inside the if statements
					// reduces data dependencies and improves performance
					uint f0 = heap[pd+0].F;
					uint f1 = heap[pd+1].F;
					uint f2 = heap[pd+2].F;
					uint f3 = heap[pd+3].F;

					// The common case is that all children of a node are present
					// so the first comparison in each if statement below
					// will be extremely well predicted so it is essentially free
					// (I tried optimizing for the common case, but it didn't affect performance at all
					// at the expense of longer code, the CPU branch predictor is really good)

					if (pd+0 < numberOfItems && (f0 < swapF || (SortGScores && f0 == swapF && heap[pd+0].node.G < swapItemG))) {
						swapF = f0;
						swapIndex = pd+0;
					}

					if (pd+1 < numberOfItems && (f1 < swapF || (SortGScores && f1 == swapF && heap[pd+1].node.G < (swapIndex == parent ? swapItemG : heap[swapIndex].node.G)))) {
						swapF = f1;
						swapIndex = pd+1;
					}

					if (pd+2 < numberOfItems && (f2 < swapF || (SortGScores && f2 == swapF && heap[pd+2].node.G < (swapIndex == parent ? swapItemG : heap[swapIndex].node.G)))) {
						swapF = f2;
						swapIndex = pd+2;
					}

					if (pd+3 < numberOfItems && (f3 < swapF || (SortGScores && f3 == swapF && heap[pd+3].node.G < (swapIndex == parent ? swapItemG : heap[swapIndex].node.G)))) {
						swapIndex = pd+3;
					}
				}

				// One if the parent's children are smaller or equal, swap them
				// (actually we are just pretenting we swapped them, we hold the swapData
				// in local variable and only assign it once we know the final index)
				if (parent != swapIndex) {
					heap[parent] = heap[swapIndex];
#if DECREASE_KEY
					heap[parent].node.heapIndex = (ushort)parent;
#endif
				} else {
					break;
				}
			}

			// Assign element to the final position
			heap[swapIndex] = swapItem;
#if DECREASE_KEY
			swapItem.node.heapIndex = (ushort)swapIndex;
#endif

			// For debugging
			// Validate ();

			return returnItem;
		}

		void Validate () {
			for (int i = 1; i < numberOfItems; i++) {
				int parentIndex = (i-1)/D;
				if (heap[parentIndex].F > heap[i].F) {
					throw new System.Exception("Invalid state at " + i + ":" +  parentIndex + " ( " + heap[parentIndex].F + " > " + heap[i].F + " ) ");
				}
#if DECREASE_KEY
				if (heap[i].node.heapIndex != i) {
					throw new System.Exception("Invalid heap index");
				}
#endif
			}
		}

		/** Rebuilds the heap by trickeling down all items.
		 * Usually called after the hTarget on a path has been changed */
		public void Rebuild () {
			#if ASTARDEBUG
			int changes = 0;
			#endif

			for (int i = 2; i < numberOfItems; i++) {
				int bubbleIndex = i;
				var node = heap[i];
				uint nodeF = node.F;
				while (bubbleIndex != 1) {
					int parentIndex = bubbleIndex / D;

					if (nodeF < heap[parentIndex].F) {
						heap[bubbleIndex] = heap[parentIndex];
#if DECREASE_KEY
						heap[bubbleIndex].node.heapIndex = (ushort)bubbleIndex;
#endif

						heap[parentIndex] = node;
#if DECREASE_KEY
						heap[parentIndex].node.heapIndex = (ushort)parentIndex;
#endif

						bubbleIndex = parentIndex;
						#if ASTARDEBUG
						changes++;
						#endif
					} else {
						break;
					}
				}
			}

			#if ASTARDEBUG
			UnityEngine.Debug.Log("+++ Rebuilt Heap - "+changes+" changes +++");
			#endif
		}
	}
}
