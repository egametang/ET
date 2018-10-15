#define DECREASE_KEY
using System.Collections.Generic;

namespace PF {
	/** Stores temporary node data for a single pathfinding request.
	 * Every node has one PathNode per thread used.
	 * It stores e.g G score, H score and other temporary variables needed
	 * for path calculation, but which are not part of the graph structure.
	 *
	 * \see Pathfinding.PathHandler
	 * \see https://en.wikipedia.org/wiki/A*_search_algorithm
	 */
	public class PathNode {
		/** Reference to the actual graph node */
		public GraphNode node;

		/** Parent node in the search tree */
		public PathNode parent;

		/** The path request (in this thread, if multithreading is used) which last used this node */
		public ushort pathID;

#if DECREASE_KEY
		/** Index of the node in the binary heap.
		 * The open list in the A* algorithm is backed by a binary heap.
		 * To support fast 'decrease key' operations, the index of the node
		 * is saved here.
		 */
		public ushort heapIndex = BinaryHeap.NotInHeap;
#endif

		/** Bitpacked variable which stores several fields */
		private uint flags;

		/** Cost uses the first 28 bits */
		private const uint CostMask = (1U << 28) - 1U;

		/** Flag 1 is at bit 28 */
		private const int Flag1Offset = 28;
		private const uint Flag1Mask = (uint)(1 << Flag1Offset);

		/** Flag 2 is at bit 29 */
		private const int Flag2Offset = 29;
		private const uint Flag2Mask = (uint)(1 << Flag2Offset);

		public uint cost {
			get {
				return flags & CostMask;
			}
			set {
				flags = (flags & ~CostMask) | value;
			}
		}

		/** Use as temporary flag during pathfinding.
		 * Pathfinders (only) can use this during pathfinding to mark
		 * nodes. When done, this flag should be reverted to its default state (false) to
		 * avoid messing up other pathfinding requests.
		 */
		public bool flag1 {
			get {
				return (flags & Flag1Mask) != 0;
			}
			set {
				flags = (flags & ~Flag1Mask) | (value ? Flag1Mask : 0U);
			}
		}

		/** Use as temporary flag during pathfinding.
		 * Pathfinders (only) can use this during pathfinding to mark
		 * nodes. When done, this flag should be reverted to its default state (false) to
		 * avoid messing up other pathfinding requests.
		 */
		public bool flag2 {
			get {
				return (flags & Flag2Mask) != 0;
			}
			set {
				flags = (flags & ~Flag2Mask) | (value ? Flag2Mask : 0U);
			}
		}

		/** Backing field for the G score */
		private uint g;

		/** Backing field for the H score */
		private uint h;

		/** G score, cost to get to this node */
		public uint G { get { return g; } set { g = value; } }

		/** H score, estimated cost to get to to the target */
		public uint H { get { return h; } set { h = value; } }

		/** F score. H score + G score */
		public uint F { get { return g+h; } }

		public void UpdateG (Path path) {
			#if ASTAR_NO_TRAVERSAL_COST
			g = parent.g + cost;
			#else
			g = parent.g + cost + path.GetTraversalCost(node);
			#endif
		}
	}

	/** Handles thread specific path data.
	 */
	public class PathHandler {
		/** Current PathID.
		 * \see #PathID
		 */
		private ushort pathID;

		public readonly int threadID;
		public readonly int totalThreadCount;

		/**
		 * Binary heap to keep track of nodes on the "Open list".
		 * \see https://en.wikipedia.org/wiki/A*_search_algorithm
		 */
		public readonly BinaryHeap heap = new BinaryHeap(128);

		/** ID for the path currently being calculated or last path that was calculated */
		public ushort PathID { get { return pathID; } }

		/** Array of all PathNodes */
		public PathNode[] nodes = new PathNode[0];

		/** StringBuilder that paths can use to build debug strings.
		 * Better for performance and memory usage to use a single StringBuilder instead of each path creating its own
		 */
		public readonly System.Text.StringBuilder DebugStringBuilder = new System.Text.StringBuilder();

		public PathHandler (int threadID, int totalThreadCount) {
			this.threadID = threadID;
			this.totalThreadCount = totalThreadCount;
		}

		public void InitializeForPath (Path p) {
			pathID = p.pathID;
			heap.Clear();
		}

		/** Internal method to clean up node data */
		public void DestroyNode (GraphNode node) {
			PathNode pn = GetPathNode(node);

			// Clean up references to help the GC
			pn.node = null;
			pn.parent = null;
			// This is not required for pathfinding, but not clearing it may confuse gizmo drawing for a fraction of a second.
			// Especially when 'Show Search Tree' is enabled
			pn.pathID = 0;
			pn.G = 0;
			pn.H = 0;
		}

		/** Internal method to initialize node data */
		public void InitializeNode (GraphNode node) {
			//Get the index of the node
			int ind = node.NodeIndex;

			if (ind >= nodes.Length) {
				// Grow by a factor of 2
				PathNode[] newNodes = new PathNode[System.Math.Max(128, nodes.Length*2)];
				nodes.CopyTo(newNodes, 0);
				// Initialize all PathNode instances at once
				// It is important that we do this here and don't for example leave the entries as NULL and initialize
				// them lazily. By allocating them all at once we are much more likely to allocate the PathNodes close
				// to each other in memory (most systems use some kind of bumb-allocator) and this improves cache locality
				// and reduces false sharing (which would happen if we allocated PathNodes for the different threads close
				// to each other). This has been profiled to give around a 4% difference in overall pathfinding performance.
				for (int i = nodes.Length; i < newNodes.Length; i++) newNodes[i] = new PathNode();
				nodes = newNodes;
			}

			nodes[ind].node = node;
		}

		public PathNode GetPathNode (int nodeIndex) {
			return nodes[nodeIndex];
		}

		/** Returns the PathNode corresponding to the specified node.
		 * The PathNode is specific to this PathHandler since multiple PathHandlers
		 * are used at the same time if multithreading is enabled.
		 */
		public PathNode GetPathNode (GraphNode node) {
			return nodes[node.NodeIndex];
		}

		/** Set all nodes' pathIDs to 0.
		 * \see Pathfinding.PathNode.pathID
		 */
		public void ClearPathIDs () {
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes[i] != null) nodes[i].pathID = 0;
			}
		}
	}
}
