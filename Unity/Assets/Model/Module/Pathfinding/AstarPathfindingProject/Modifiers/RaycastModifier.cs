using UnityEngine;
using System.Collections.Generic;
using PF;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	using Pathfinding.Util;

	/** Simplifies a path using raycasting.
	 * \ingroup modifiers
	 * This modifier will try to remove as many nodes as possible from the path using raycasting (linecasting) to validate the node removal.
	 * You can use either graph raycasting or Physics.Raycast.
	 * When using graph raycasting, the graph will be traversed and checked for obstacles. When physics raycasting is used, the Unity physics system
	 * will be asked if there are any colliders which intersect the line that is currently being checked.
	 *
	 * \see https://docs.unity3d.com/ScriptReference/Physics.html
	 * \see #Pathfinding.IRaycastableGraph
	 *
	 * This modifier is primarily intended for grid graphs and layered grid graphs. Though depending on your game it may also be
	 * useful for point graphs. However note that point graphs do not have any built-in raycasting so you need to use physics raycasting for that graph.
	 *
	 * For navmesh/recast graphs the #Pathfinding.FunnelModifier is a much better and faster alternative.
	 *
	 * On grid graphs you can combine the FunnelModifier with this modifier by simply attaching both of them to a GameObject with a Seeker.
	 * This may or may not give you better results. It will usually follow the border of the graph more closely when they are both used
	 * however it more often misses some simplification opportunities.
	 * When both modifiers are used then the funnel modifier will run first and simplify the path, and then this modifier will take
	 * the output from the funnel modifier and try to simplify that even more.
	 *
	 * This modifier has several different quality levels. The highest quality is significantly slower than the
	 * lowest quality level (10 times slower is not unusual). So make sure you pick the lowest quality that your game can get away with.
	 * You can use the Unity profiler to see if it takes up any significant amount of time. It will show up under the heading "Running Path Modifiers".
	 *
	 * \shadowimage{raycast_modifier_quality.gif}
	 *
	 * \see \ref modifiers
	 */
	[AddComponentMenu("Pathfinding/Modifiers/Raycast Modifier")]
	[RequireComponent(typeof(Seeker))]
	[System.Serializable]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_raycast_modifier.php")]
	public class RaycastModifier : MonoModifier {
	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Raycast Simplifier Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(RaycastModifier));
		}
	#endif

		public override int Order { get { return 40; } }

		/** Use Physics.Raycast to simplify the path */
		public bool useRaycasting = true;

		/** Layer mask used for physics raycasting.
		 * All objects with layers that are included in this mask will be treated as obstacles.
		 * If you are using a grid graph you usually want this to be the same as the mask in the grid graph's 'Collision Testing' settings.
		  */
		public LayerMask mask = -1;

		/** Checks around the line between two points, not just the exact line.
		 * Make sure the ground is either too far below or is not inside the mask since otherwise the raycast might always hit the ground.
		 *
		 * \see https://docs.unity3d.com/ScriptReference/Physics.SphereCast.html
		 */
		[Tooltip("Checks around the line between two points, not just the exact line.\nMake sure the ground is either too far below or is not inside the mask since otherwise the raycast might always hit the ground.")]
		public bool thickRaycast;

		/** Distance from the ray which will be checked for colliders */
		[Tooltip("Distance from the ray which will be checked for colliders")]
		public float thickRaycastRadius;

		/** Check for intersections with 2D colliders instead of 3D colliders.
		 * Useful for 2D games.
		 *
		 * \see https://docs.unity3d.com/ScriptReference/Physics2D.html
		 */
		[Tooltip("Check for intersections with 2D colliders instead of 3D colliders.")]
		public bool use2DPhysics;

		/** Offset from the original positions to perform the raycast.
		 * Can be useful to avoid the raycast intersecting the ground or similar things you do not want to it intersect
		 */
		[Tooltip("Offset from the original positions to perform the raycast.\nCan be useful to avoid the raycast intersecting the ground or similar things you do not want to it intersect")]
		public Vector3 raycastOffset = Vector3.zero;

		/** Use raycasting on the graphs. Only currently works with GridGraph and NavmeshGraph and RecastGraph. \astarpro */
		[Tooltip("Use raycasting on the graphs. Only currently works with GridGraph and NavmeshGraph and RecastGraph. This is a pro version feature.")]
		public bool useGraphRaycasting;

		/** Higher quality modes will try harder to find a shorter path.
		 * Higher qualities may be significantly slower than low quality.
		 * \shadowimage{raycast_modifier_quality.gif}
		 */
		[Tooltip("When using the high quality mode the script will try harder to find a shorter path. This is significantly slower than the greedy low quality approach.")]
		public Quality quality = Quality.Medium;

		public enum Quality {
			/** One iteration using a greedy algorithm */
			Low,
			/** Two iterations using a greedy algorithm */
			Medium,
			/** One iteration using a dynamic programming algorithm */
			High,
			/** Three iterations using a dynamic programming algorithm */
			Highest
		}

		static readonly int[] iterationsByQuality = new [] { 1, 2, 1, 3 };
		static List<PF.Vector3> buffer = new List<PF.Vector3>();
		static float[] DPCosts = new float[16];
		static int[] DPParents = new int[16];

		public override void Apply (Path p) {
			if (!useRaycasting && !useGraphRaycasting) return;

			var points = p.vectorPath;

			if (ValidateLine(null, null, p.vectorPath[0], p.vectorPath[p.vectorPath.Count-1])) {
				// A very common case is that there is a straight line to the target.
				var s = p.vectorPath[0];
				var e = p.vectorPath[p.vectorPath.Count-1];
				points.ClearFast();
				points.Add(s);
				points.Add(e);
			} else {
				int iterations = iterationsByQuality[(int)quality];
				for (int it = 0; it < iterations; it++) {
					if (it != 0) {
						Polygon.Subdivide(points, buffer, 3);
						Memory.Swap(ref buffer, ref points);
						buffer.ClearFast();
						points.Reverse();
					}

					points = quality >= Quality.High ? ApplyDP(p, points) : ApplyGreedy(p, points);
				}
				if ((iterations % 2) == 0) points.Reverse();
			}

			p.vectorPath = points;
		}

		List<PF.Vector3> ApplyGreedy (Path p, List<PF.Vector3> points) {
			bool canBeOriginalNodes = points.Count == p.path.Count;
			int startIndex = 0;

			while (startIndex < points.Count) {
				Vector3 start = points[startIndex];
				var startNode = canBeOriginalNodes && points[startIndex] == (PF.Vector3)p.path[startIndex].position ? p.path[startIndex] : null;
				buffer.Add(start);

				// Do a binary search to find the furthest node we can see from this node
				int mn = 1, mx = 2;
				while (true) {
					int endIndex = startIndex + mx;
					if (endIndex >= points.Count) {
						mx = points.Count - startIndex;
						break;
					}
					Vector3 end = points[endIndex];
					var endNode = canBeOriginalNodes && end == (Vector3)p.path[endIndex].position ? p.path[endIndex] : null;
					if (!ValidateLine(startNode, endNode, start, end)) break;
					mn = mx;
					mx *= 2;
				}

				while (mn + 1 < mx) {
					int mid = (mn + mx)/2;
					int endIndex = startIndex + mid;
					Vector3 end = points[endIndex];
					var endNode = canBeOriginalNodes && end == (Vector3)p.path[endIndex].position ? p.path[endIndex] : null;

					if (ValidateLine(startNode, endNode, start, end)) {
						mn = mid;
					} else {
						mx = mid;
					}
				}
				startIndex += mn;
			}

			Memory.Swap(ref buffer, ref points);
			buffer.ClearFast();
			return points;
		}

		List<PF.Vector3> ApplyDP (Path p, List<PF.Vector3> points) {
			if (DPCosts.Length < points.Count) {
				DPCosts = new float[points.Count];
				DPParents = new int[points.Count];
			}
			for (int i = 0; i < DPParents.Length; i++) DPCosts[i] = DPParents[i] = -1;
			bool canBeOriginalNodes = points.Count == p.path.Count;

			for (int i = 0; i < points.Count; i++) {
				float d = DPCosts[i];
				PF.Vector3 start = points[i];
				var startIsOriginalNode = canBeOriginalNodes && start == (PF.Vector3)p.path[i].position;
				for (int j = i+1; j < points.Count; j++) {
					// Total distance from the start to this point using the best simplified path
					// The small additive constant is to make sure that the number of points is kept as small as possible
					// even when the total distance is the same (which can happen with e.g multiple colinear points).
					float d2 = d + (points[j] - start).magnitude + 0.0001f;
					if (DPParents[j] == -1 || d2 < DPCosts[j]) {
						var endIsOriginalNode = canBeOriginalNodes && points[j] == (PF.Vector3)p.path[j].position;
						if (j == i+1 || ValidateLine(startIsOriginalNode ? p.path[i] : null, endIsOriginalNode ? p.path[j] : null, start, points[j])) {
							DPCosts[j] = d2;
							DPParents[j] = i;
						} else {
							break;
						}
					}
				}
			}

			int c = points.Count - 1;
			while (c != -1) {
				buffer.Add(points[c]);
				c = DPParents[c];
			}
			buffer.Reverse();
			Memory.Swap(ref buffer, ref points);
			buffer.ClearFast();
			return points;
		}

		/** Check if a straight path between v1 and v2 is valid.
		 * If both \a n1 and \a n2 are supplied it is assumed that the line goes from the center of \a n1 to the center of \a n2 and a more optimized graph linecast may be done.
		 */
		protected bool ValidateLine (GraphNode n1, GraphNode n2, Vector3 v1, Vector3 v2) {
			if (useRaycasting) {
				// Use raycasting to check if a straight path between v1 and v2 is valid
				if (use2DPhysics) {
					if (thickRaycast && thickRaycastRadius > 0 && Physics2D.CircleCast(v1 + raycastOffset, thickRaycastRadius, v2 - v1, (v2 - v1).magnitude, mask)) {
						return false;
					}

					if (Physics2D.Linecast(v1+raycastOffset, v2+raycastOffset, mask)) {
						return false;
					}
				} else {
					// Perform a thick raycast (if enabled)
					if (thickRaycast && thickRaycastRadius > 0 && Physics.SphereCast(new Ray(v1+raycastOffset, v2-v1), thickRaycastRadius, (v2-v1).magnitude, mask)) {
						return false;
					}

					// Perform a normal raycast
					// This is done even if a thick raycast is also done because thick raycasts do not report collisions for
					// colliders that overlapped the (imaginary) sphere at the origin of the thick raycast.
					// If this raycast was not done then some obstacles could be missed.
					if (Physics.Linecast(v1+raycastOffset, v2+raycastOffset, mask)) {
						return false;
					}
				}
			}

			if (useGraphRaycasting) {
#if !AstarFree && !ASTAR_NO_GRID_GRAPH
				bool betweenNodeCenters = n1 != null && n2 != null;
#endif
				if (n1 == null) n1 = PathFindHelper.GetNearest(v1).node;
				if (n2 == null) n2 = PathFindHelper.GetNearest(v2).node;

				if (n1 != null && n2 != null) {
					// Use graph raycasting to check if a straight path between v1 and v2 is valid
					NavGraph graph = UnityHelper.GetGraph(n1);
					NavGraph graph2 = UnityHelper.GetGraph(n2);

					if (graph != graph2) {
						return false;
					}

					var rayGraph = graph as IRaycastableGraph;
					
					if (rayGraph != null) {
						return !rayGraph.Linecast(v1, v2, n1);
					}
				}
			}
			return true;
		}
	
	}
}
