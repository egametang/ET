using PF;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pathfinding {
	using Pathfinding.Util;

	/** Connects two nodes with a direct connection.
	 * It is not possible to detect this link when following a path (which may be good or bad), for that you can use NodeLink2.
	 */
	[AddComponentMenu("Pathfinding/Link")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_node_link.php")]
	public class NodeLink : GraphModifier {
		/** End position of the link */
		public Transform end;

		/** The connection will be this times harder/slower to traverse.
		 * Note that values lower than one will not always make the pathfinder choose this path instead of another path even though this one should
		 * lead to a lower total cost unless you also adjust the Heuristic Scale in A* Inspector -> Settings -> Pathfinding or disable the heuristic altogether.
		 */
		public float costFactor = 1.0f;

		/** Make a one-way connection */
		public bool oneWay = false;

		/** Delete existing connection instead of adding one */
		public bool deleteConnection = false;

		public Transform Start {
			get { return transform; }
		}

		public Transform End {
			get { return end; }
		}

		public override void OnPostScan () {
			if (AstarPath.active.isScanning) {
				InternalOnPostScan();
			} else {
				AstarPath.active.AddWorkItem(new AstarWorkItem(force => {
					InternalOnPostScan();
					return true;
				}));
			}
		}

		public void InternalOnPostScan () {
			Apply();
		}

		public override void OnGraphsPostUpdate () {
			if (!AstarPath.active.isScanning) {
				AstarPath.active.AddWorkItem(new AstarWorkItem(force => {
					InternalOnPostScan();
					return true;
				}));
			}
		}

		public virtual void Apply () {
			if (Start == null || End == null || AstarPath.active == null) return;

			GraphNode startNode = PathFindHelper.GetNearest(Start.position).node;
			GraphNode endNode = PathFindHelper.GetNearest(End.position).node;

			if (startNode == null || endNode == null) return;


			if (deleteConnection) {
				startNode.RemoveConnection(endNode);
				if (!oneWay)
					endNode.RemoveConnection(startNode);
			} else {
				uint cost = (uint)System.Math.Round((startNode.position-endNode.position).costMagnitude*costFactor);

				startNode.AddConnection(endNode, cost);
				if (!oneWay)
					endNode.AddConnection(startNode, cost);
			}
		}

		public void OnDrawGizmos () {
			if (Start == null || End == null) return;

			Draw.Gizmos.Bezier(Start.position, End.position, deleteConnection ? Color.red : Color.green);
		}

	#if UNITY_EDITOR
		[UnityEditor.MenuItem("Edit/Pathfinding/Link Pair %&l")]
		public static void LinkObjects () {
			Transform[] tfs = Selection.transforms;
			if (tfs.Length == 2) {
				LinkObjects(tfs[0], tfs[1], false);
			}
			SceneView.RepaintAll();
		}

		[UnityEditor.MenuItem("Edit/Pathfinding/Unlink Pair %&u")]
		public static void UnlinkObjects () {
			Transform[] tfs = Selection.transforms;
			if (tfs.Length == 2) {
				LinkObjects(tfs[0], tfs[1], true);
			}
			SceneView.RepaintAll();
		}

		[UnityEditor.MenuItem("Edit/Pathfinding/Delete Links on Selected %&b")]
		public static void DeleteLinks () {
			Transform[] tfs = Selection.transforms;
			for (int i = 0; i < tfs.Length; i++) {
				NodeLink[] conns = tfs[i].GetComponents<NodeLink>();
				for (int j = 0; j < conns.Length; j++) DestroyImmediate(conns[j]);
			}
			SceneView.RepaintAll();
		}

		public static void LinkObjects (Transform a, Transform b, bool removeConnection) {
			NodeLink connecting = null;

			NodeLink[] conns = a.GetComponents<NodeLink>();
			for (int i = 0; i < conns.Length; i++) {
				if (conns[i].end == b) {
					connecting = conns[i];
					break;
				}
			}

			conns = b.GetComponents<NodeLink>();
			for (int i = 0; i < conns.Length; i++) {
				if (conns[i].end == a) {
					connecting = conns[i];
					break;
				}
			}

			if (removeConnection) {
				if (connecting != null) DestroyImmediate(connecting);
			} else {
				if (connecting == null) {
					connecting = a.gameObject.AddComponent<NodeLink>();
					connecting.end = b;
				} else {
					connecting.deleteConnection = !connecting.deleteConnection;
				}
			}
		}
	#endif
	}
}
