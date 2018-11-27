using PF;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.Util {
	public class GraphGizmoHelper : IAstarPooledObject, System.IDisposable {
		public RetainedGizmos.Hasher hasher { get; private set; }
		Pathfinding.Util.RetainedGizmos gizmos;
		PathHandler debugData;
		ushort debugPathID;
		GraphDebugMode debugMode;
		bool showSearchTree;
		float debugFloor;
		float debugRoof;
		public RetainedGizmos.Builder builder { get; private set; }
		Vector3 drawConnectionStart;
		Color drawConnectionColor;
		readonly System.Action<GraphNode> drawConnection;

		public GraphGizmoHelper () {
			// Cache a delegate to avoid allocating memory for it every time
			drawConnection = DrawConnection;
		}

		public void Init (RetainedGizmos.Hasher hasher, RetainedGizmos gizmos) {
			debugData = PathFindHelper.debugPathData;
			debugPathID = PathFindHelper.debugPathID;
			debugMode = AstarPath.active.debugMode;
			debugFloor = AstarPath.active.debugFloor;
			debugRoof = AstarPath.active.debugRoof;
			showSearchTree = AstarPath.active.showSearchTree && debugData != null;
			this.gizmos = gizmos;
			this.hasher = hasher;
			builder = ObjectPool<RetainedGizmos.Builder>.Claim();
		}

		public void OnEnterPool () {
			// Will cause pretty much all calls to throw null ref exceptions until Init is called
			var bld = builder;

			ObjectPool<RetainedGizmos.Builder>.Release(ref bld);
			builder = null;
			debugData = null;
		}

		public void DrawConnections (GraphNode node) {
			if (showSearchTree) {
				if (InSearchTree(node, debugData, debugPathID)) {
					var pnode = debugData.GetPathNode(node);
					if (pnode.parent != null) {
						builder.DrawLine((PF.Vector3)node.position, (PF.Vector3)this.debugData.GetPathNode(node).parent.node.position, NodeColor(node));
					}
				}
			} else {
				// Calculate which color to use for drawing the node
				// based on the settings specified in the editor
				drawConnectionColor = NodeColor(node);
				// Get the node position
				// Cast it here to avoid doing it for every neighbour
				drawConnectionStart = ((PF.Vector3)node.position).ToUnityV3();
				node.GetConnections(drawConnection);
			}
		}

		void DrawConnection (GraphNode other) {
			builder.DrawLine(drawConnectionStart, Vector3.Lerp((PF.Vector3)other.position, drawConnectionStart, 0.5f), drawConnectionColor);
		}

		/** Color to use for gizmos.
		 * Returns a color to be used for the specified node with the current debug settings (editor only).
		 *
		 * \version Since 3.6.1 this method will not handle null nodes
		 */
		public Color NodeColor (GraphNode node) {
			if (showSearchTree && !InSearchTree(node, debugData, debugPathID)) return Color.clear;

			Color color;

			if (node.Walkable) {
				switch (debugMode) {
				case GraphDebugMode.Areas:
					color = AstarColor.GetAreaColor(node.Area);
					break;
				case GraphDebugMode.Penalty:
					color = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, ((float)node.Penalty-debugFloor) / (debugRoof-debugFloor));
					break;
				case GraphDebugMode.Tags:
					color = AstarColor.GetAreaColor(node.Tag);
					break;
				case GraphDebugMode.Connections:
					color = AstarColor.NodeConnection;
					break;
				default:
					if (debugData == null) {
						color = AstarColor.NodeConnection;
						break;
					}

					PathNode pathNode = debugData.GetPathNode(node);
					float value;
					if (debugMode == GraphDebugMode.G) {
						value = pathNode.G;
					} else if (debugMode == GraphDebugMode.H) {
						value = pathNode.H;
					} else {
						// mode == F
						value = pathNode.F;
					}

					color = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (value-debugFloor) / (debugRoof-debugFloor));
					break;
				}
			} else {
				color = AstarColor.UnwalkableNode;
			}

			return color;
		}

		/** Returns if the node is in the search tree of the path.
		 * Only guaranteed to be correct if \a path is the latest path calculated.
		 * Use for gizmo drawing only.
		 */
		public static bool InSearchTree (GraphNode node, PathHandler handler, ushort pathID) {
			return handler.GetPathNode(node).pathID == pathID;
		}

		public void DrawWireTriangle (Vector3 a, Vector3 b, Vector3 c, Color color) {
			builder.DrawLine(a, b, color);
			builder.DrawLine(b, c, color);
			builder.DrawLine(c, a, color);
		}

		public void DrawTriangles (Vector3[] vertices, Color[] colors, int numTriangles) {
			var triangles = ListPool<int>.Claim(numTriangles);

			for (int i = 0; i < numTriangles*3; i++) triangles.Add(i);
			builder.DrawMesh(gizmos, vertices, triangles, colors);
			ListPool<int>.Release(ref triangles);
		}

		public void DrawWireTriangles (Vector3[] vertices, Color[] colors, int numTriangles) {
			for (int i = 0; i < numTriangles; i++) {
				DrawWireTriangle(vertices[i*3+0], vertices[i*3+1], vertices[i*3+2], colors[i*3+0]);
			}
		}

		public void Submit () {
			builder.Submit(gizmos, hasher);
		}

		void System.IDisposable.Dispose () {
			var tmp = this;

			Submit();
			ObjectPool<GraphGizmoHelper>.Release(ref tmp);
		}
	}
}
