using System.Collections.Generic;
using System.Threading;
using PF;
using UnityEngine;

namespace Pathfinding {
	using UnityEngine.Assertions;

#if NETFX_CORE
	using Thread = Pathfinding.WindowsStore.Thread;
#else
	using Thread = System.Threading.Thread;
#endif

	class GraphUpdateProcessor {
		public event System.Action OnGraphsUpdated;

#if !UNITY_WEBGL
		/**
		 * Reference to the thread which handles async graph updates.
		 * \see ProcessGraphUpdatesAsync
		 */
		Thread graphUpdateThread;
#endif

		/** Used for IsAnyGraphUpdateInProgress */
		bool anyGraphUpdateInProgress;

		/**
		 * Queue containing all waiting graph update queries. Add to this queue by using \link AddToQueue \endlink.
		 * \see AddToQueue
		 */
		readonly Queue<GraphUpdateObject> graphUpdateQueue = new Queue<GraphUpdateObject>();

		/** Queue of all async graph updates waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueueAsync = new Queue<GUOSingle>();

		/** Queue of all non-async graph update post events waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueuePost = new Queue<GUOSingle>();

		/** Queue of all non-async graph updates waiting to be executed */
		readonly Queue<GUOSingle> graphUpdateQueueRegular = new Queue<GUOSingle>();

		readonly System.Threading.ManualResetEvent asyncGraphUpdatesComplete = new System.Threading.ManualResetEvent(true);

#if !UNITY_WEBGL
		readonly System.Threading.AutoResetEvent graphUpdateAsyncEvent = new System.Threading.AutoResetEvent(false);
		readonly System.Threading.AutoResetEvent exitAsyncThread = new System.Threading.AutoResetEvent(false);
#endif

		/** Returns if any graph updates are waiting to be applied */
		public bool IsAnyGraphUpdateQueued { get { return graphUpdateQueue.Count > 0; } }

		/** Returns if any graph updates are in progress */
		public bool IsAnyGraphUpdateInProgress { get { return anyGraphUpdateInProgress; } }

		/** The last area index which was used.
		 * Used for the \link FloodFill(GraphNode node) FloodFill \endlink function to start flood filling with an unused area.
		 * \see FloodFill(Node node)
		 */
		uint lastUniqueAreaIndex = 0;

		/** Order type for updating graphs */
		enum GraphUpdateOrder {
			GraphUpdate,
			FloodFill
		}

		/** Holds a single update that needs to be performed on a graph */
		struct GUOSingle {
			public GraphUpdateOrder order;
			public GraphUpdateObject obj;
		}

		public void DisableMultithreading () {
#if !UNITY_WEBGL
			if (graphUpdateThread != null && graphUpdateThread.IsAlive) {
				// Resume graph update thread, will cause it to terminate
				exitAsyncThread.Set();

				if (!graphUpdateThread.Join(5*1000)) {
					Debug.LogError("Graph update thread did not exit in 5 seconds");
				}

				graphUpdateThread = null;
			}
#endif
		}

		/** Update all graphs using the GraphUpdateObject.
		 * This can be used to, e.g make all nodes in an area unwalkable, or set them to a higher penalty.
		 * The graphs will be updated as soon as possible (with respect to AstarPath.batchGraphUpdates)
		 *
		 * \see FlushGraphUpdates
		 */
		public void AddToQueue (GraphUpdateObject ob) {
			// Put the GUO in the queue
			graphUpdateQueue.Enqueue(ob);
		}

		/** Floodfills starting from the specified node.
		 * \see https://en.wikipedia.org/wiki/Flood_fill
		 */
		public void FloodFill (GraphNode seed) {
			FloodFill(seed, lastUniqueAreaIndex+1);
			lastUniqueAreaIndex++;
		}

		/** Floodfills starting from 'seed' using the specified area.
		 * \see https://en.wikipedia.org/wiki/Flood_fill
		 */
		public void FloodFill (GraphNode seed, uint area) {
			if (area > GraphNode.MaxAreaIndex) {
				Debug.LogError("Too high area index - The maximum area index is " + GraphNode.MaxAreaIndex);
				return;
			}

			if (area < 0) {
				Debug.LogError("Too low area index - The minimum area index is 0");
				return;
			}

			var stack = Pathfinding.Util.StackPool<GraphNode>.Claim();

			stack.Push(seed);
			seed.Area = (uint)area;

			while (stack.Count > 0) {
				stack.Pop().FloodFill(stack, (uint)area);
			}

			Pathfinding.Util.StackPool<GraphNode>.Release(stack);
		}

		/** Floodfills all graphs and updates areas for every node.
		 * The different colored areas that you see in the scene view when looking at graphs
		 * are called just 'areas', this method calculates which nodes are in what areas.
		 * \see Pathfinding.Node.area
		 */
		public void FloodFill () {
			#if ASTARDEBUG
			System.DateTime startTime = System.DateTime.UtcNow;
			#endif

			var graphs = PathFindHelper.graphs;

			if (graphs == null) {
				return;
			}

			// Iterate through all nodes in all graphs
			// and reset their Area field
			for (int i = 0; i < graphs.Length; i++) {
				var graph = graphs[i];

				if (graph != null) {
					graph.GetNodes(node => node.Area = 0);
				}
			}

			lastUniqueAreaIndex = 0;
			uint area = 0;
			int forcedSmallAreas = 0;

			// Get a temporary stack from a pool
			var stack = Pathfinding.Util.StackPool<GraphNode>.Claim();

			for (int i = 0; i < graphs.Length; i++) {
				NavGraph graph = graphs[i];

				if (graph == null) continue;

				graph.GetNodes(node => {
					if (node.Walkable && node.Area == 0) {
						area++;

						uint thisArea = area;

						if (area > GraphNode.MaxAreaIndex) {
					        // Forced to consider this a small area
							area--;
							thisArea = area;

					        // Make sure the first small area is also counted
							if (forcedSmallAreas == 0) forcedSmallAreas = 1;

							forcedSmallAreas++;
						}

						stack.Clear();
						stack.Push(node);

						int counter = 1;
						node.Area = thisArea;

						while (stack.Count > 0) {
							counter++;
							stack.Pop().FloodFill(stack, thisArea);
						}
					}
				});
			}

			lastUniqueAreaIndex = area;

			if (forcedSmallAreas > 0) {
				Debug.LogError(forcedSmallAreas +" areas had to share IDs. " +
					"This usually doesn't affect pathfinding in any significant way (you might get 'Searched whole area but could not find target' as a reason for path failure) " +
					"however some path requests may take longer to calculate (specifically those that fail with the 'Searched whole area' error)." +
					"The maximum number of areas is " + GraphNode.MaxAreaIndex +".");
			}

			// Put back into the pool
			Pathfinding.Util.StackPool<GraphNode>.Release(stack);

			#if ASTARDEBUG
			Debug.Log("Flood fill complete, "+area+" area"+(area > 1 ? "s" : "")+" found - "+((System.DateTime.UtcNow.Ticks-startTime.Ticks)*0.0001).ToString("0.00")+" ms");
			#endif
		}
	}
}
