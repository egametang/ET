//#define ASTARDEBUG   //"BBTree Debug" If enables, some queries to the tree will show debug lines. Turn off multithreading when using this since DrawLine calls cannot be called from a different thread

using System;
using PF;

namespace PF {

	/** Axis Aligned Bounding Box Tree.
	 * Holds a bounding box tree of triangles.
	 *
	 * \astarpro
	 */
	public class BBTree : IAstarPooledObject {
		/** Holds all tree nodes */
		BBTreeBox[] tree = null;
		TriangleMeshNode[] nodeLookup = null;
		int count;
		int leafNodes;

		const int MaximumLeafSize = 4;

		/** Clear the tree.
		 * Note that references to old nodes will still be intact so the GC cannot immediately collect them.
		 */
		public void Clear () {
			count = 0;
			leafNodes = 0;
			if (tree != null) ArrayPool<BBTreeBox>.Release(ref tree);
			if (nodeLookup != null) {
				// Prevent memory leaks as the pool does not clear the array
				for (int i = 0; i < nodeLookup.Length; i++) nodeLookup[i] = null;
				ArrayPool<TriangleMeshNode>.Release(ref nodeLookup);
			}
			tree = ArrayPool<BBTreeBox>.Claim(0);
			nodeLookup = ArrayPool<TriangleMeshNode>.Claim(0);
		}

		void IAstarPooledObject.OnEnterPool () {
			Clear();
		}

		void EnsureCapacity (int c) {
			if (c > tree.Length) {
				var newArr = ArrayPool<BBTreeBox>.Claim(c);
				tree.CopyTo(newArr, 0);
				ArrayPool<BBTreeBox>.Release(ref tree);
				tree = newArr;
			}
		}

		void EnsureNodeCapacity (int c) {
			if (c > nodeLookup.Length) {
				var newArr = ArrayPool<TriangleMeshNode>.Claim(c);
				nodeLookup.CopyTo(newArr, 0);
				ArrayPool<TriangleMeshNode>.Release(ref nodeLookup);
				nodeLookup = newArr;
			}
		}

		int GetBox (IntRect rect) {
			if (count >= tree.Length) EnsureCapacity(count+1);

			tree[count] = new BBTreeBox(rect);
			count++;
			return count-1;
		}

		/** Rebuilds the tree using the specified nodes */
		public void RebuildFrom (TriangleMeshNode[] nodes) {
			Clear();

			if (nodes.Length == 0) return;

			// We will use approximately 2N tree nodes
			EnsureCapacity(Mathf.CeilToInt(nodes.Length * 2.1f));
			// We will use approximately N node references
			EnsureNodeCapacity(Mathf.CeilToInt(nodes.Length * 1.1f));

			// This will store the order of the nodes while the tree is being built
			// It turns out that it is a lot faster to do this than to actually modify
			// the nodes and nodeBounds arrays (presumably since that involves shuffling
			// around 20 bytes of memory (sizeof(pointer) + sizeof(IntRect)) per node
			// instead of 4 bytes (sizeof(int)).
			// It also means we don't have to make a copy of the nodes array since
			// we do not modify it
			var permutation = ArrayPool<int>.Claim(nodes.Length);
			for (int i = 0; i < nodes.Length; i++) {
				permutation[i] = i;
			}

			// Precalculate the bounds of the nodes in XZ space.
			// It turns out that calculating the bounds is a bottleneck and precalculating
			// the bounds makes it around 3 times faster to build a tree
			var nodeBounds = ArrayPool<IntRect>.Claim(nodes.Length);
			for (int i = 0; i < nodes.Length; i++) {
				Int3 v0, v1, v2;
				nodes[i].GetVertices(out v0, out v1, out v2);

				var rect = new IntRect(v0.x, v0.z, v0.x, v0.z);
				rect = rect.ExpandToContain(v1.x, v1.z);
				rect = rect.ExpandToContain(v2.x, v2.z);
				nodeBounds[i] = rect;
			}

			RebuildFromInternal(nodes, permutation, nodeBounds, 0, nodes.Length, false);

			ArrayPool<int>.Release(ref permutation);
			ArrayPool<IntRect>.Release(ref nodeBounds);
		}

		static int SplitByX (TriangleMeshNode[] nodes, int[] permutation, int from, int to, int divider) {
			int mx = to;

			for (int i = from; i < mx; i++) {
				if (nodes[permutation[i]].position.x > divider) {
					mx--;
					// Swap items i and mx
					var tmp = permutation[mx];
					permutation[mx] = permutation[i];
					permutation[i] = tmp;
					i--;
				}
			}
			return mx;
		}

		static int SplitByZ (TriangleMeshNode[] nodes, int[] permutation, int from, int to, int divider) {
			int mx = to;

			for (int i = from; i < mx; i++) {
				if (nodes[permutation[i]].position.z > divider) {
					mx--;
					// Swap items i and mx
					var tmp = permutation[mx];
					permutation[mx] = permutation[i];
					permutation[i] = tmp;
					i--;
				}
			}
			return mx;
		}

		int RebuildFromInternal (TriangleMeshNode[] nodes, int[] permutation, IntRect[] nodeBounds, int from, int to, bool odd) {
			var rect = NodeBounds(permutation, nodeBounds, from, to);
			int box = GetBox(rect);

			if (to - from <= MaximumLeafSize) {
				var nodeOffset = tree[box].nodeOffset = leafNodes*MaximumLeafSize;
				EnsureNodeCapacity(nodeOffset + MaximumLeafSize);
				leafNodes++;
				// Assign all nodes to the array. Note that we also need clear unused slots as the array from the pool may contain any information
				for (int i = 0; i < MaximumLeafSize; i++) {
					nodeLookup[nodeOffset + i] = i < to - from ? nodes[permutation[from + i]] : null;
				}
				return box;
			}

			int splitIndex;
			if (odd) {
				// X
				int divider = (rect.xmin + rect.xmax)/2;
				splitIndex = SplitByX(nodes, permutation, from, to, divider);
			} else {
				// Y/Z
				int divider = (rect.ymin + rect.ymax)/2;
				splitIndex = SplitByZ(nodes, permutation, from, to, divider);
			}

			if (splitIndex == from || splitIndex == to) {
				// All nodes were on one side of the divider
				// Try to split along the other axis

				if (!odd) {
					// X
					int divider = (rect.xmin + rect.xmax)/2;
					splitIndex = SplitByX(nodes, permutation, from, to, divider);
				} else {
					// Y/Z
					int divider = (rect.ymin + rect.ymax)/2;
					splitIndex = SplitByZ(nodes, permutation, from, to, divider);
				}

				if (splitIndex == from || splitIndex == to) {
					// All nodes were on one side of the divider
					// Just pick one half
					splitIndex = (from+to)/2;
				}
			}

			tree[box].left = RebuildFromInternal(nodes, permutation, nodeBounds, from, splitIndex, !odd);
			tree[box].right = RebuildFromInternal(nodes, permutation, nodeBounds, splitIndex, to, !odd);

			return box;
		}

		/** Calculates the bounding box in XZ space of all nodes between \a from (inclusive) and \a to (exclusive) */
		static IntRect NodeBounds (int[] permutation, IntRect[] nodeBounds, int from, int to) {
			var rect = nodeBounds[permutation[from]];

			for (int j = from + 1; j < to; j++) {
				var otherRect = nodeBounds[permutation[j]];

				// Equivalent to rect = IntRect.Union(rect, otherRect)
				// but manually inlining is approximately
				// 25% faster when building an entire tree.
				// This code is hot when using navmesh cutting.
				rect.xmin = Math.Min(rect.xmin, otherRect.xmin);
				rect.ymin = Math.Min(rect.ymin, otherRect.ymin);
				rect.xmax = Math.Max(rect.xmax, otherRect.xmax);
				rect.ymax = Math.Max(rect.ymax, otherRect.ymax);
			}

			return rect;
		}
#if !SERVER
		[System.Diagnostics.Conditional("ASTARDEBUG")]
		static void DrawDebugRect (IntRect rect) {
			UnityEngine.Debug.DrawLine(new Vector3(rect.xmin, 0, rect.ymin), new Vector3(rect.xmax, 0, rect.ymin), UnityEngine.Color.white);
			UnityEngine.Debug.DrawLine(new Vector3(rect.xmin, 0, rect.ymax), new Vector3(rect.xmax, 0, rect.ymax), UnityEngine.Color.white);
			UnityEngine.Debug.DrawLine(new Vector3(rect.xmin, 0, rect.ymin), new Vector3(rect.xmin, 0, rect.ymax), UnityEngine.Color.white);
			UnityEngine.Debug.DrawLine(new Vector3(rect.xmax, 0, rect.ymin), new Vector3(rect.xmax, 0, rect.ymax), UnityEngine.Color.white);
		}

		[System.Diagnostics.Conditional("ASTARDEBUG")]
		static void DrawDebugNode (TriangleMeshNode node, float yoffset, UnityEngine.Color color) {
			UnityEngine.Debug.DrawLine(((Vector3)node.GetVertex(1) + Vector3.up*yoffset), ((Vector3)node.GetVertex(2) + Vector3.up*yoffset), color);
			UnityEngine.Debug.DrawLine(((Vector3)node.GetVertex(0) + Vector3.up*yoffset), ((Vector3)node.GetVertex(1) + Vector3.up*yoffset), color);
			UnityEngine.Debug.DrawLine(((Vector3)node.GetVertex(2) + Vector3.up*yoffset), ((Vector3)node.GetVertex(0) + Vector3.up*yoffset), color);
		}
#endif

		/** Queries the tree for the closest node to \a p constrained by the NNConstraint.
		 * Note that this function will only fill in the constrained node.
		 * If you want a node not constrained by any NNConstraint, do an additional search with constraint = NNConstraint.None
		 */
		public NNInfoInternal QueryClosest (Vector3 p, NNConstraint constraint, out float distance) {
			distance = float.PositiveInfinity;
			return QueryClosest(p, constraint, ref distance, new NNInfoInternal(null));
		}

		/** Queries the tree for the closest node to \a p constrained by the NNConstraint trying to improve an existing solution.
		 * Note that this function will only fill in the constrained node.
		 * If you want a node not constrained by any NNConstraint, do an additional search with constraint = NNConstraint.None
		 *
		 * This method will completely ignore any Y-axis differences in positions.
		 *
		 * \param p Point to search around
		 * \param constraint Optionally set to constrain which nodes to return
		 * \param distance The best distance for the \a previous solution. Will be updated with the best distance
		 * after this search. Will be positive infinity if no node could be found.
		 * Set to positive infinity if there was no previous solution.
		 * \param previous This search will start from the \a previous NNInfo and improve it if possible.
		 * Even if the search fails on this call, the solution will never be worse than \a previous.
		 * Note that the \a distance parameter need to be configured with the distance for the previous result
		 * otherwise it may get overwritten even though it was actually closer.
		 */
		public NNInfoInternal QueryClosestXZ (Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous) {
			var sqrDistance = distance*distance;
			var origSqrDistance = sqrDistance;

			if (count > 0 && SquaredRectPointDistance(tree[0].rect, p) < sqrDistance) {
				SearchBoxClosestXZ(0, p, ref sqrDistance, constraint, ref previous);
				// Only update the distance if the squared distance changed as otherwise #distance
				// might change due to rounding errors even if no better solution was found
				if (sqrDistance < origSqrDistance) distance = Mathf.Sqrt(sqrDistance);
			}
			return previous;
		}

		void SearchBoxClosestXZ (int boxi, Vector3 p, ref float closestSqrDist, NNConstraint constraint, ref NNInfoInternal nnInfo) {
			BBTreeBox box = tree[boxi];

			if (box.IsLeaf) {
				var nodes = nodeLookup;
				for (int i = 0; i < MaximumLeafSize && nodes[box.nodeOffset+i] != null; i++) {
					var node = nodes[box.nodeOffset+i];
					// Update the NNInfo
#if !SERVER
					DrawDebugNode(node, 0.2f, UnityEngine.Color.red);
#endif

					if (constraint == null || constraint.Suitable(node)) {
						Vector3 closest = node.ClosestPointOnNodeXZ(p);
						// XZ squared distance
						float dist = (closest.x-p.x)*(closest.x-p.x)+(closest.z-p.z)*(closest.z-p.z);

						// There's a theoretical case when the closest point is on the edge of a node which may cause the
						// closest point's xz coordinates to not line up perfectly with p's xz coordinates even though they should
						// (because floating point errors are annoying). So use a tiny margin to cover most of those cases.
						const float fuzziness = 0.000001f;
						if (nnInfo.constrainedNode == null || dist < closestSqrDist - fuzziness || (dist <= closestSqrDist + fuzziness && Mathf.Abs(closest.y - p.y) < Mathf.Abs(nnInfo.constClampedPosition.y - p.y))) {
							nnInfo.constrainedNode = node;
							nnInfo.constClampedPosition = closest;
							closestSqrDist = dist;
						}
					}
				}
			} else {
#if !SERVER
				DrawDebugRect(box.rect);
#endif

				int first = box.left, second = box.right;
				float firstDist, secondDist;
				GetOrderedChildren(ref first, ref second, out firstDist, out secondDist, p);

				// Search children (closest box first to improve performance)
				if (firstDist <= closestSqrDist) {
					SearchBoxClosestXZ(first, p, ref closestSqrDist, constraint, ref nnInfo);
				}

				if (secondDist <= closestSqrDist) {
					SearchBoxClosestXZ(second, p, ref closestSqrDist, constraint, ref nnInfo);
				}
			}
		}

		/** Queries the tree for the closest node to \a p constrained by the NNConstraint trying to improve an existing solution.
		 * Note that this function will only fill in the constrained node.
		 * If you want a node not constrained by any NNConstraint, do an additional search with constraint = NNConstraint.None
		 *
		 * \param p Point to search around
		 * \param constraint Optionally set to constrain which nodes to return
		 * \param distance The best distance for the \a previous solution. Will be updated with the best distance
		 * after this search. Will be positive infinity if no node could be found.
		 * Set to positive infinity if there was no previous solution.
		 * \param previous This search will start from the \a previous NNInfo and improve it if possible.
		 * Even if the search fails on this call, the solution will never be worse than \a previous.
		 */
		public NNInfoInternal QueryClosest (Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous) {
			var sqrDistance = distance*distance;
			var origSqrDistance = sqrDistance;

			if (count > 0 && SquaredRectPointDistance(tree[0].rect, p) < sqrDistance) {
				SearchBoxClosest(0, p, ref sqrDistance, constraint, ref previous);
				// Only update the distance if the squared distance changed as otherwise #distance
				// might change due to rounding errors even if no better solution was found
				if (sqrDistance < origSqrDistance) distance = Mathf.Sqrt(sqrDistance);
			}
			return previous;
		}

		void SearchBoxClosest (int boxi, Vector3 p, ref float closestSqrDist, NNConstraint constraint, ref NNInfoInternal nnInfo) {
			BBTreeBox box = tree[boxi];

			if (box.IsLeaf) {
				var nodes = nodeLookup;
				for (int i = 0; i < MaximumLeafSize && nodes[box.nodeOffset+i] != null; i++) {
					var node = nodes[box.nodeOffset+i];
					Vector3 closest = node.ClosestPointOnNode(p);
					float dist = (closest-p).sqrMagnitude;
					if (dist < closestSqrDist) {
#if !SERVER
						DrawDebugNode(node, 0.2f, UnityEngine.Color.red);
#endif

						if (constraint == null || constraint.Suitable(node)) {
							// Update the NNInfo
							nnInfo.constrainedNode = node;
							nnInfo.constClampedPosition = closest;
							closestSqrDist = dist;
						}
					} else {
#if !SERVER
						DrawDebugNode(node, 0.0f, UnityEngine.Color.blue);
#endif
					}
				}
			} else {
#if !SERVER
				DrawDebugRect(box.rect);
#endif
				int first = box.left, second = box.right;
				float firstDist, secondDist;
				GetOrderedChildren(ref first, ref second, out firstDist, out secondDist, p);

				// Search children (closest box first to improve performance)
				if (firstDist < closestSqrDist) {
					SearchBoxClosest(first, p, ref closestSqrDist, constraint, ref nnInfo);
				}

				if (secondDist < closestSqrDist) {
					SearchBoxClosest(second, p, ref closestSqrDist, constraint, ref nnInfo);
				}
			}
		}

		/** Orders the box indices first and second by the approximate distance to the point p */
		void GetOrderedChildren (ref int first, ref int second, out float firstDist, out float secondDist, Vector3 p) {
			firstDist = SquaredRectPointDistance(tree[first].rect, p);
			secondDist = SquaredRectPointDistance(tree[second].rect, p);

			if (secondDist < firstDist) {
				// Swap
				var tmp = first;
				first = second;
				second = tmp;
				var tmp2 = firstDist;
				firstDist = secondDist;
				secondDist = tmp2;
			}
		}

		/** Searches for a node which contains the specified point.
		 * If there are multiple nodes that contain the point any one of them
		 * may be returned.
		 *
		 * \see TriangleMeshNode.ContainsPoint
		 */
		public TriangleMeshNode QueryInside (Vector3 p, NNConstraint constraint) {
			return count != 0 && tree[0].Contains(p) ? SearchBoxInside(0, p, constraint) : null;
		}

		TriangleMeshNode SearchBoxInside (int boxi, Vector3 p, NNConstraint constraint) {
			BBTreeBox box = tree[boxi];

			if (box.IsLeaf) {
				var nodes = nodeLookup;
				for (int i = 0; i < MaximumLeafSize && nodes[box.nodeOffset+i] != null; i++) {
					var node = nodes[box.nodeOffset+i];
					if (node.ContainsPoint((Int3)p)) {
#if !SERVER
						DrawDebugNode(node, 0.2f, UnityEngine.Color.red);
#endif

						if (constraint == null || constraint.Suitable(node)) {
							return node;
						}
					} else {
#if !SERVER
						DrawDebugNode(node, 0.0f, UnityEngine.Color.blue);
#endif
					}
				}
			} else {
#if !SERVER
				DrawDebugRect(box.rect);
#endif

				//Search children
				if (tree[box.left].Contains(p)) {
					var result = SearchBoxInside(box.left, p, constraint);
					if (result != null) return result;
				}

				if (tree[box.right].Contains(p)) {
					var result = SearchBoxInside(box.right, p, constraint);
					if (result != null) return result;
				}
			}

			return null;
		}

		struct BBTreeBox {
			public IntRect rect;

			public int nodeOffset;
			public int left, right;

			public bool IsLeaf {
				get {
					return nodeOffset >= 0;
				}
			}

			public BBTreeBox (IntRect rect) {
				nodeOffset = -1;
				this.rect = rect;
				left = right = -1;
			}

			public bool Contains (Vector3 point) {
				var pi = (Int3)point;

				return rect.Contains(pi.x, pi.z);
			}
		}

		/** Returns distance from \a p to the rectangle \a r */
		static float SquaredRectPointDistance (IntRect r, Vector3 p) {
			Vector3 po = p;

			p.x = Math.Max(p.x, r.xmin*Int3.PrecisionFactor);
			p.x = Math.Min(p.x, r.xmax*Int3.PrecisionFactor);
			p.z = Math.Max(p.z, r.ymin*Int3.PrecisionFactor);
			p.z = Math.Min(p.z, r.ymax*Int3.PrecisionFactor);

			// XZ squared magnitude comparison
			return (p.x-po.x)*(p.x-po.x) + (p.z-po.z)*(p.z-po.z);
		}
	}
}
