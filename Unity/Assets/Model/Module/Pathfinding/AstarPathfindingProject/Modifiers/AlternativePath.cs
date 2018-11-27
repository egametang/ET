using UnityEngine;
using System.Collections.Generic;
using PF;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Alternative Path")]
	[System.Serializable]
	/** Applies penalty to the paths it processes telling other units to avoid choosing the same path.
	 *
	 * Note that this might not work properly if penalties are modified by other actions as well (e.g graph update objects which reset the penalty to zero).
	 * It will only work when all penalty modifications are relative, i.e adding or subtracting penalties, but not when setting penalties
	 * to specific values.
	 *
	 * When destroyed, it will correctly remove any added penalty.
	 *
	 * \ingroup modifiers
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_alternative_path.php")]
	public class AlternativePath : MonoModifier {
	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Alternative Path Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(AlternativePath));
		}
	#endif

		public override int Order { get { return 10; } }

		/** How much penalty (weight) to apply to nodes */
		public int penalty = 1000;

		/** Max number of nodes to skip in a row */
		public int randomStep = 10;

		/** The previous path */
		List<GraphNode> prevNodes = new List<GraphNode>();

		/** The previous penalty used. Stored just in case it changes during operation */
		int prevPenalty;

		/** A random object */
		readonly System.Random rnd = new System.Random();

		bool destroyed;

		public override void Apply (Path p) {
			if (this == null) return;

			ApplyNow(p.path);
		}

		protected void OnDestroy () {
			destroyed = true;
			ClearOnDestroy();
		}

		void ClearOnDestroy () {
			InversePrevious();
		}

		void InversePrevious () {
			// Remove previous penalty
			if (prevNodes != null) {
				bool warnPenalties = false;
				for (int i = 0; i < prevNodes.Count; i++) {
					if (prevNodes[i].Penalty < prevPenalty) {
						warnPenalties = true;
						prevNodes[i].Penalty = 0;
					} else {
						prevNodes[i].Penalty = (uint)(prevNodes[i].Penalty-prevPenalty);
					}
				}
				if (warnPenalties) {
					Debug.LogWarning("Penalty for some nodes has been reset while the AlternativePath modifier was active (possibly because of a graph update). Some penalties might be incorrect (they may be lower than expected for the affected nodes)");
				}
			}
		}

		void ApplyNow (List<GraphNode> nodes) {
			InversePrevious();
			prevNodes.Clear();

			if (destroyed) return;

			if (nodes != null) {
				int rndStart = rnd.Next(randomStep);
				for (int i = rndStart; i < nodes.Count; i += rnd.Next(1, randomStep)) {
					nodes[i].Penalty = (uint)(nodes[i].Penalty+penalty);
					prevNodes.Add(nodes[i]);
				}
			}

			prevPenalty = penalty;
		}
	}
}
