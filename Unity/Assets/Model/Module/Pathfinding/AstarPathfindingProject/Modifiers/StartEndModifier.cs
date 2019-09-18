using UnityEngine;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	[System.Serializable]
	/** Adjusts start and end points of a path.
	 *
	 * This modifier is included in the \link Pathfinding.Seeker Seeker\endlink component and is always used if you are using a Seeker.
	 * When a path is calculated the resulting path will only be the positions of the nodes it passes through.
	 * However often you may not want to navigate to the center of a specific node but instead to a point on the surface of a node.
	 * This modifier will adjust the endpoints of the path.
	 *
	 * \shadowimage{startendmodifier.png}
	 *
	 * \ingroup modifiers
	 *
	 */
	public class StartEndModifier : PathModifier {
		public override int Order { get { return 0; } }

		/** Add points to the path instead of replacing them.
		 * If for example #exactEndPoint is set to ClosestOnNode then the path will be modified so that
		 * the path goes first to the center of the last node in the path and then goes to the closest point
		 * on the node to the end point in the path request.
		 *
		 * If this is false however then the relevant points in the path will simply be replaced.
		 * In the above example the path would go directly to the closest point on the node without passing
		 * through the center of the node.
		 */
		public bool addPoints;

		/** How the start point of the path will be determined.
		 * \see #Exactness
		 */
		public Exactness exactStartPoint = Exactness.ClosestOnNode;

		/** How the end point of the path will be determined.
		 * \see #Exactness
		 */
		public Exactness exactEndPoint = Exactness.ClosestOnNode;

		/** Will be called when a path is processed.
		 * The value which is returned will be used as the start point of the path
		 * and potentially clamped depending on the value of the #exactStartPoint field.
		 * Only used for the Original, Interpolate and NodeConnection modes.
		 */
		public System.Func<Vector3> adjustStartPoint;

		/** Sets where the start and end points of a path should be placed.
		 *
		 * Here is a legend showing what the different items in the above images represent.
		 * The images above show a path coming in from the top left corner and ending at a node next to an obstacle as well as 2 different possible end points of the path and how they would be modified.
		 * \shadowimage{startend/exactness_legend.png}
		 */
		public enum Exactness {
			/** The point is snapped to the position of the first/last node in the path.
			 * Use this if your game is very tile based and you want your agents to stop precisely at the center of the nodes.
			 * If you recalculate the path while the agent is moving you may want the start point snapping to be ClosestOnNode and the end point snapping to be SnapToNode however
			 * as while an agent is moving it will likely not be right at the center of a node.
			 *
			 * \shadowimage{startend/exactness_snap_to_node.png}
			 */
			SnapToNode,
			/** The point is set to the exact point which was passed when creating the path request.
			 * Note that if a path was for example requested to a point inside an obstacle, then the last point of the path will be inside that obstacle, which is usually not what you want.
			 * Consider using the #ClosestOnNode option instead.
			 *
			 * \shadowimage{startend/exactness_original.png}
			 */
			Original,
			/** The point is set to the closest point on the line between either the two first points or the two last points.
			 * Usually you will want to use the NodeConnection mode instead since that is usually the behaviour that you really want.
			 * This mode exists mostly for compatibility reasons.
			 * \shadowimage{startend/exactness_interpolate.png}
			 * \deprecated Use NodeConnection instead.
			 */
			Interpolate,
			/** The point is set to the closest point on the surface of the node. Note that some node types (point nodes) do not have a surface, so the "closest point" is simply the node's position which makes this identical to #Exactness.SnapToNode.
			 * This is the mode that you almost always want to use in a free movement 3D world.
			 * \shadowimage{startend/exactness_closest_on_node.png}
			 */
			ClosestOnNode,
			/** The point is set to the closest point on one of the connections from the start/end node.
			 * This mode may be useful in a grid based or point graph based world when using the AILerp script.
			 *
			 * \shadowimage{startend/exactness_connection.png}
			 */
			NodeConnection,
		}

		/** Do a straight line check from the node's center to the point determined by the #Exactness.
		 * There are very few cases where you will want to use this. It is mostly here for
		 * backwards compatibility reasons.
		 *
		 * \version Since 4.1 this field only has an effect for the #Exactness mode Original because that's the only one where it makes sense.
		 */
		public bool useRaycasting;
		public LayerMask mask = -1;

		/** Do a straight line check from the node's center to the point determined by the #Exactness.
		 * \see #useRaycasting
		 *
		 * \version Since 4.1 this field only has an effect for the #Exactness mode Original because that's the only one where it makes sense.
		 */
		public bool useGraphRaycasting;

		List<GraphNode> connectionBuffer;
		System.Action<GraphNode> connectionBufferAddDelegate;

		public override void Apply (Path _p) {
			var p = _p as ABPath;

			// This modifier only supports ABPaths (doesn't make much sense for other paths anyway)
			if (p == null || p.vectorPath.Count == 0) return;

			if (p.vectorPath.Count == 1 && !addPoints) {
				// Duplicate first point
				p.vectorPath.Add(p.vectorPath[0]);
			}

			// Add instead of replacing points
			bool forceAddStartPoint, forceAddEndPoint;

			Vector3 pStart = Snap(p, exactStartPoint, true, out forceAddStartPoint);
			Vector3 pEnd = Snap(p, exactEndPoint, false, out forceAddEndPoint);

			// Add or replace the start point
			// Disable adding of points if the mode is SnapToNode since then
			// the first item in vectorPath will very likely be the same as the
			// position of the first node
			if ((forceAddStartPoint || addPoints) && exactStartPoint != Exactness.SnapToNode) {
				p.vectorPath.Insert(0, pStart);
			} else {
				p.vectorPath[0] = pStart;
			}

			if ((forceAddEndPoint || addPoints) && exactEndPoint != Exactness.SnapToNode) {
				p.vectorPath.Add(pEnd);
			} else {
				p.vectorPath[p.vectorPath.Count-1] = pEnd;
			}
		}

		Vector3 Snap (ABPath path, Exactness mode, bool start, out bool forceAddPoint) {
			var index = start ? 0 : path.path.Count - 1;
			var node = path.path[index];
			var nodePos = (Vector3)node.position;

			forceAddPoint = false;

			switch (mode) {
			case Exactness.ClosestOnNode:
				return start ? path.startPoint : path.endPoint;
			case Exactness.SnapToNode:
				return nodePos;
			case Exactness.Original:
			case Exactness.Interpolate:
			case Exactness.NodeConnection:
				Vector3 relevantPoint;
				if (start) {
					relevantPoint = adjustStartPoint != null ? adjustStartPoint().ToPFV3() : path.originalStartPoint;
				} else {
					relevantPoint = path.originalEndPoint;
				}

				switch (mode) {
				case Exactness.Original:
					return GetClampedPoint(nodePos, relevantPoint, node);
				case Exactness.Interpolate:
					// Adjacent node to either the start node or the end node in the path
					var adjacentNode = path.path[Mathf.Clamp(index + (start ? 1 : -1), 0, path.path.Count-1)];
					return VectorMath.ClosestPointOnSegment(nodePos, (Vector3)adjacentNode.position, relevantPoint);
				case Exactness.NodeConnection:
					// This code uses some tricks to avoid allocations
					// even though it uses delegates heavily
					// The connectionBufferAddDelegate delegate simply adds whatever node
					// it is called with to the connectionBuffer
					connectionBuffer = connectionBuffer ?? new List<GraphNode>();
					connectionBufferAddDelegate = connectionBufferAddDelegate ?? (System.Action<GraphNode>)connectionBuffer.Add;

					// Adjacent node to either the start node or the end node in the path
					adjacentNode = path.path[Mathf.Clamp(index + (start ? 1 : -1), 0, path.path.Count-1)];

					// Add all neighbours of #node to the connectionBuffer
					node.GetConnections(connectionBufferAddDelegate);
					var bestPos = nodePos;
					var bestDist = float.PositiveInfinity;

					// Loop through all neighbours
					// Do it in reverse order because the length of the connectionBuffer
					// will change during iteration
					for (int i = connectionBuffer.Count - 1; i >= 0; i--) {
						var neighbour = connectionBuffer[i];

						// Find the closest point on the connection between the nodes
						// and check if the distance to that point is lower than the previous best
						var closest = VectorMath.ClosestPointOnSegment(nodePos, (Vector3)neighbour.position, relevantPoint);

						var dist = (closest.ToUnityV3() - relevantPoint).sqrMagnitude;
						if (dist < bestDist) {
							bestPos = closest;
							bestDist = dist;

							// If this node is not the adjacent node
							// then the path should go through the start node as well
							forceAddPoint = neighbour != adjacentNode;
						}
					}

					connectionBuffer.Clear();
					return bestPos;
				default:
					throw new System.ArgumentException("Cannot reach this point, but the compiler is not smart enough to realize that.");
				}
			default:
				throw new System.ArgumentException("Invalid mode");
			}
		}

		protected Vector3 GetClampedPoint (Vector3 from, Vector3 to, GraphNode hint) {
			Vector3 point = to;
			RaycastHit hit;

			if (useRaycasting && Physics.Linecast(from, to, out hit, mask)) {
				point = hit.point;
			}

			if (useGraphRaycasting && hint != null) {
				var rayGraph = UnityHelper.GetGraph(hint) as IRaycastableGraph;

				if (rayGraph != null) {
					GraphHitInfo graphHit;
					if (rayGraph.Linecast(from, point, hint, out graphHit)) {
						point = graphHit.point;
					}
				}
			}

			return point;
		}
	}
}
