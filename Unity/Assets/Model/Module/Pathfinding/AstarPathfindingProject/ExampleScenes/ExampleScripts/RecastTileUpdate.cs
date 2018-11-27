using UnityEngine;

namespace Pathfinding {
	/** Updates the recast tile(s) it is in at start, needs RecastTileUpdateHandler.
	 *
	 * If there is a collider attached to the same GameObject, the bounds
	 * of that collider will be used for updating, otherwise
	 * only the position of the object will be used.
	 *
	 * \note This class needs a RecastTileUpdateHandler somewhere in the scene.
	 * See the documentation for that class, it contains more information.
	 *
	 * \note This does not use navmesh cutting. If you only ever add
	 * obstacles, but never add any new walkable surfaces then you might
	 * want to use navmesh cutting instead. See \ref navmeshcutting.
	 *
	 * \see RecastTileUpdateHandler
	 */
	[AddComponentMenu("Pathfinding/Navmesh/RecastTileUpdate")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_recast_tile_update.php")]
	public class RecastTileUpdate : MonoBehaviour {
		public static event System.Action<Bounds> OnNeedUpdates;

		void Start () {
			ScheduleUpdate();
		}

		void OnDestroy () {
			ScheduleUpdate();
		}

		/** Schedule a tile update for all tiles that contain this object */
		public void ScheduleUpdate () {
			var collider = GetComponent<Collider>();

			if (collider != null) {
				if (OnNeedUpdates != null) {
					OnNeedUpdates(collider.bounds);
				}
			} else {
				if (OnNeedUpdates != null) {
					OnNeedUpdates(new Bounds(transform.position, Vector3.zero));
				}
			}
		}
	}
}
