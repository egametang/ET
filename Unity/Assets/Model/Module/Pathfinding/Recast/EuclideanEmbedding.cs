#pragma warning disable 414
using System.Collections.Generic;

namespace PF {
	public enum HeuristicOptimizationMode {
		None,
		Random,
		RandomSpreadOut,
	}

	/** Implements heuristic optimizations.
	 *
	 * \see heuristic-opt
	 * \see Game AI Pro - Pathfinding Architecture Optimizations by Steve Rabin and Nathan R. Sturtevant
	 *
	 * \astarpro
	 */
	[System.Serializable]
	public class EuclideanEmbedding {
		public HeuristicOptimizationMode mode;

		public int seed;

		public int spreadOutCount = 1;

		[System.NonSerialized]
		public bool dirty;

		/**
		 * Costs laid out as n*[int],n*[int],n*[int] where n is the number of pivot points.
		 * Each node has n integers which is the cost from that node to the pivot node.
		 * They are at around the same place in the array for simplicity and for cache locality.
		 *
		 * cost(nodeIndex, pivotIndex) = costs[nodeIndex*pivotCount+pivotIndex]
		 */
		uint[] costs = new uint[8];
		int maxNodeIndex;

		int pivotCount;

		public GraphNode[] pivots;

		/*
		 * Seed for random number generator.
		 * Must not be zero
		 */
		const uint ra = 12820163;

		/*
		 * Seed for random number generator.
		 * Must not be zero
		 */
		const uint rc = 1140671485;

		/*
		 * Parameter for random number generator.
		 */
		uint rval;

		System.Object lockObj = new object ();

		/** Simple linear congruential generator.
		 * \see http://en.wikipedia.org/wiki/Linear_congruential_generator
		 */
		uint GetRandom () {
			rval = (ra*rval + rc);
			return rval;
		}

		void EnsureCapacity (int index) {
			if (index > maxNodeIndex) {
				lock (lockObj) {
					if (index > maxNodeIndex) {
						if (index >= costs.Length) {
							var newCosts = new uint[System.Math.Max(index*2, pivots.Length*2)];
							for (int i = 0; i < costs.Length; i++) newCosts[i] = costs[i];
							costs = newCosts;
						}
						maxNodeIndex = index;
					}
				}
			}
		}

		public uint GetHeuristic (int nodeIndex1, int nodeIndex2) {
			nodeIndex1 *= pivotCount;
			nodeIndex2 *= pivotCount;

			if (nodeIndex1 >= costs.Length || nodeIndex2 >= costs.Length) {
				EnsureCapacity(nodeIndex1 > nodeIndex2 ? nodeIndex1 : nodeIndex2);
			}

			uint mx = 0;
			for (int i = 0; i < pivotCount; i++) {
				uint d = (uint)System.Math.Abs((int)costs[nodeIndex1+i] - (int)costs[nodeIndex2+i]);
				if (d > mx) mx = d;
			}

			return mx;
		}

		/** Pick N random walkable nodes from all nodes in all graphs and add them to the buffer.
		 *
		 * Here we select N random nodes from a stream of nodes.
		 * Probability of choosing the first N nodes is 1
		 * Probability of choosing node i is min(N/i,1)
		 * A selected node will replace a random node of the previously
		 * selected ones.
		 *
		 * \see https://en.wikipedia.org/wiki/Reservoir_sampling
		 */
		void PickNRandomNodes (int count, List<GraphNode> buffer) {
			int n = 0;

			var graphs = PathFindHelper.GetConfig().graphs;

			// Loop through all graphs
			for (int j = 0; j < graphs.Length; j++) {
				// Loop through all nodes in the graph
				graphs[j].GetNodes(node => {
					if (!node.Destroyed && node.Walkable) {
						n++;
						if ((GetRandom() % n) < count) {
							if (buffer.Count < count) {
								buffer.Add(node);
							} else {
								buffer[(int)(GetRandom()%buffer.Count)] = node;
							}
						}
					}
				});
			}
		}

		GraphNode PickAnyWalkableNode () {
			var graphs = PathFindHelper.GetConfig().graphs;
			GraphNode first = null;

			// Find any node in the graphs
			for (int j = 0; j < graphs.Length; j++) {
				graphs[j].GetNodes(node => {
					if (node != null && node.Walkable && first == null) {
						first = node;
					}
				});
			}

			return first;
		}

		public void RecalculatePivots () {
			if (mode == HeuristicOptimizationMode.None) {
				pivotCount = 0;
				pivots = null;
				return;
			}

			// Reset the random number generator
			rval = (uint)seed;

			// Get a List<GraphNode> from a pool
			var pivotList = ListPool<GraphNode>.Claim();

			switch (mode) {
			case HeuristicOptimizationMode.Random:
				PickNRandomNodes(spreadOutCount, pivotList);
				break;
			case HeuristicOptimizationMode.RandomSpreadOut:
				// If no pivot points were found, fall back to picking arbitrary nodes
				if (pivotList.Count == 0) {
					GraphNode first = PickAnyWalkableNode();

					if (first != null) {
						pivotList.Add(first);
					} else {
#if !SERVER
						UnityEngine.Debug.LogError("Could not find any walkable node in any of the graphs.");
#endif
						ListPool<GraphNode>.Release(ref pivotList);
						return;
					}
				}

				// Fill remaining slots with null
				int toFill = spreadOutCount - pivotList.Count;
				for (int i = 0; i < toFill; i++) pivotList.Add(null);
				break;
			default:
				throw new System.Exception("Invalid HeuristicOptimizationMode: " + mode);
			}

			pivots = pivotList.ToArray();

			ListPool<GraphNode>.Release(ref pivotList);
		}

		/** Special case necessary for paths to unwalkable nodes right next to walkable nodes to be able to use good heuristics.
		 *
		 * This will find all unwalkable nodes in all grid graphs with walkable nodes as neighbours
		 * and set the cost to reach them from each of the pivots as the minimum of the cost to
		 * reach the neighbours of each node.
		 *
		 * \see ABPath.EndPointGridGraphSpecialCase
		 */
		void ApplyGridGraphEndpointSpecialCase () {
		}
	}
}
