using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	/** Explicit mesh object for recast graphs.
	 * Adding this component to an object will make sure it is included in any recast graphs.
	 * It will be included even though the Rasterize Meshes toggle is set to false.
	 *
	 * Using RecastMeshObjs instead of relying on the Rasterize Meshes option is good for several reasons.
	 * - Rasterize Meshes is slow. If you are using a tiled graph and you are updaing it, every time something is recalculated
	 * the graph will have to search all meshes in your scene for ones to rasterize, in contrast, RecastMeshObjs are stored
	 * in a tree for extreamly fast lookup (O(log n + k) compared to O(n) where \a n is the number of meshes in your scene and \a k is the number of meshes
	 * which should be rasterized, if you know Big-O notation).
	 * - The RecastMeshObj exposes some options which can not be accessed using the Rasterize Meshes toggle. See member documentation for more info.
	 *      This can for example be used to include meshes in the recast graph rasterization, but make sure that the character cannot walk on them.
	 *
	 * Since the objects are stored in a tree, and trees are slow to update, there is an enforcement that objects are not allowed to move
	 * unless the #dynamic option is enabled. When the dynamic option is enabled, the object will be stored in an array instead of in the tree.
	 * This will reduce the performance improvement over 'Rasterize Meshes' but is still faster.
	 *
	 * If a mesh filter and a mesh renderer is attached to this GameObject, those will be used in the rasterization
	 * otherwise if a collider is attached, that will be used.
	 */
	[AddComponentMenu("Pathfinding/Navmesh/RecastMeshObj")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_recast_mesh_obj.php")]
	public class RecastMeshObj : VersionedMonoBehaviour {
		/** Static objects are stored in a tree for fast bounds lookups */
		protected static RecastBBTree tree = new RecastBBTree();

		/** Dynamic objects are stored in a list since it is costly to update the tree every time they move */
		protected static List<RecastMeshObj> dynamicMeshObjs = new List<RecastMeshObj>();

		/** Fills the buffer with all RecastMeshObjs which intersect the specified bounds */
		public static void GetAllInBounds (List<RecastMeshObj> buffer, Bounds bounds) {
			if (!Application.isPlaying) {
				var objs = FindObjectsOfType(typeof(RecastMeshObj)) as RecastMeshObj[];
				for (int i = 0; i < objs.Length; i++) {
					objs[i].RecalculateBounds();
					if (objs[i].GetBounds().Intersects(bounds)) {
						buffer.Add(objs[i]);
					}
				}
				return;
			} else if (Time.timeSinceLevelLoad == 0) {
				// Is is not guaranteed that all RecastMeshObj OnEnable functions have been called, so if it is the first frame since loading a new level
				// try to initialize all RecastMeshObj objects.
				var objs = FindObjectsOfType(typeof(RecastMeshObj)) as RecastMeshObj[];
				for (int i = 0; i < objs.Length; i++) objs[i].Register();
			}

			for (int q = 0; q < dynamicMeshObjs.Count; q++) {
				if (dynamicMeshObjs[q].GetBounds().Intersects(bounds)) {
					buffer.Add(dynamicMeshObjs[q]);
				}
			}

			Rect r = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);

			tree.QueryInBounds(r, buffer);
		}

		[HideInInspector]
		public Bounds bounds;

		/** Check if the object will move.
		 * Recalculation of bounding box trees is expensive so if this is true, the object
		 * will simply be stored in an array. Easier to move, but slower lookup, so use wisely.
		 * If you for some reason want to move it, but don't want it dynamic (maybe you will only move it veery seldom and have lots of similar
		 * objects so they would add overhead by being dynamic). You can enable and disable the component every time you move it.
		 * Disabling it will remove it from the bounding box tree and enabling it will add it to the bounding box tree again.
		 *
		 * The object should never move unless being dynamic or disabling/enabling it as described above.
		 */
		public bool dynamic = true;

		/** Voxel area for mesh.
		 * This area (not to be confused with pathfinding areas, this is only used when rasterizing meshes for the recast graph) field
		 * can be used to explicitly insert edges in the navmesh geometry or to make some parts of the mesh unwalkable.
		 * If the area is set to -1, it will be removed from the resulting navmesh. This is useful if you have some object that you want to be included in the rasterization,
		 * but you don't want to let the character walk on it.
		 *
		 * When rasterizing the world and two objects with different area values are adjacent to each other, a split in the navmesh geometry
		 * will be added between them, characters will still be able to walk between them, but this can be useful when working with navmesh updates.
		 *
		 * Navmesh updates which recalculate a whole tile (updatePhysics=True) are very slow So if there are special places
		 * which you know are going to be updated quite often, for example at a door opening (opened/closed door) you
		 * can use areas to create splits on the navmesh for easier updating using normal graph updates (updatePhysics=False).
		 * See the below video for more information.
		 *
		 * \youtube{CS6UypuEMwM}
		 *
		 */
		public int area = 0;

		bool _dynamic;
		bool registered;

		void OnEnable () {
			Register();
		}

		void Register () {
			if (registered) return;

			registered = true;

			//Clamp area, upper limit isn't really a hard limit, but if it gets much higher it will start to interfere with other stuff
			area = Mathf.Clamp(area, -1, 1 << 25);

			Renderer rend = GetComponent<Renderer>();

			Collider coll = GetComponent<Collider>();
			if (rend == null && coll == null) throw new System.Exception("A renderer or a collider should be attached to the GameObject");

			MeshFilter filter = GetComponent<MeshFilter>();

			if (rend != null && filter == null) throw new System.Exception("A renderer was attached but no mesh filter");

			// Default to renderer
			bounds = rend != null ? rend.bounds : coll.bounds;

			_dynamic = dynamic;
			if (_dynamic) {
				dynamicMeshObjs.Add(this);
			} else {
				tree.Insert(this);
			}
		}

		/** Recalculates the internally stored bounds of the object */
		private void RecalculateBounds () {
			Renderer rend = GetComponent<Renderer>();

			Collider coll = GetCollider();

			if (rend == null && coll == null) throw new System.Exception("A renderer or a collider should be attached to the GameObject");

			MeshFilter filter = GetComponent<MeshFilter>();

			if (rend != null && filter == null) throw new System.Exception("A renderer was attached but no mesh filter");

			// Default to renderer
			bounds = rend != null ? rend.bounds : coll.bounds;
		}

		/** Bounds completely enclosing the mesh for this object */
		public Bounds GetBounds () {
			if (_dynamic) {
				RecalculateBounds();
			}
			return bounds;
		}

		public MeshFilter GetMeshFilter () {
			return GetComponent<MeshFilter>();
		}

		public Collider GetCollider () {
			return GetComponent<Collider>();
		}

		void OnDisable () {
			registered = false;

			if (_dynamic) {
				dynamicMeshObjs.Remove(this);
			} else {
				if (!tree.Remove(this)) {
					throw new System.Exception("Could not remove RecastMeshObj from tree even though it should exist in it. Has the object moved without being marked as dynamic?");
				}
			}
			_dynamic = dynamic;
		}
	}
}
