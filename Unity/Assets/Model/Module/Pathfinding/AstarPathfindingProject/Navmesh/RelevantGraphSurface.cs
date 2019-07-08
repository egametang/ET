using UnityEngine;

namespace Pathfinding {
	/** Pruning of recast navmesh regions.
	 * A RelevantGraphSurface component placed in the scene specifies that
	 * the navmesh region it is inside should be included in the navmesh.
	 *
	 * \see Pathfinding.RecastGraph.relevantGraphSurfaceMode
	 *
	 */
	[AddComponentMenu("Pathfinding/Navmesh/RelevantGraphSurface")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_relevant_graph_surface.php")]
	public class RelevantGraphSurface : VersionedMonoBehaviour {
		private static RelevantGraphSurface root;

		public float maxRange = 1;

		private RelevantGraphSurface prev;
		private RelevantGraphSurface next;
		private Vector3 position;

		public Vector3 Position {
			get { return position; }
		}

		public RelevantGraphSurface Next {
			get { return next; }
		}

		public RelevantGraphSurface Prev {
			get { return prev; }
		}

		public static RelevantGraphSurface Root {
			get { return root; }
		}

		public void UpdatePosition () {
			position = transform.position;
		}

		void OnEnable () {
			UpdatePosition();
			if (root == null) {
				root = this;
			} else {
				next = root;
				root.prev = this;
				root = this;
			}
		}

		void OnDisable () {
			if (root == this) {
				root = next;
				if (root != null) root.prev = null;
			} else {
				if (prev != null) prev.next = next;
				if (next != null) next.prev = prev;
			}
			prev = null;
			next = null;
		}

		/** Updates the positions of all relevant graph surface components.
		 * Required to be able to use the position property reliably.
		 */
		public static void UpdateAllPositions () {
			RelevantGraphSurface c = root;

			while (c != null) { c.UpdatePosition(); c = c.Next; }
		}

		public static void FindAllGraphSurfaces () {
			var srf = GameObject.FindObjectsOfType(typeof(RelevantGraphSurface)) as RelevantGraphSurface[];

			for (int i = 0; i < srf.Length; i++) {
				srf[i].OnDisable();
				srf[i].OnEnable();
			}
		}

		public void OnDrawGizmos () {
			Gizmos.color = new Color(57/255f, 211/255f, 46/255f, 0.4f);
			Gizmos.DrawLine(transform.position - Vector3.up*maxRange, transform.position + Vector3.up*maxRange);
		}

		public void OnDrawGizmosSelected () {
			Gizmos.color = new Color(57/255f, 211/255f, 46/255f);
			Gizmos.DrawLine(transform.position - Vector3.up*maxRange, transform.position + Vector3.up*maxRange);
		}
	}
}
