using UnityEngine;
using System.Collections.Generic;
using PF;
using Vector3 = UnityEngine.Vector3;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace Pathfinding.RVO {
	using Pathfinding.Util;

	/** Adds a navmesh as RVO obstacles.
	 * Add this to a scene in which has a navmesh or grid based graph, when scanning (or loading from cache) the graph
	 * it will be added as RVO obstacles to the RVOSimulator (which must exist in the scene).
	 *
	 * \warning You should only have a single instance of this script in the scene, otherwise it will add duplicate
	 * obstacles and thereby increasing the CPU usage.
	 *
	 * If you update a graph during runtime the obstacles need to be recalculated which has a performance penalty.
	 * This can be quite significant for larger graphs.
	 *
	 * In the screenshot the generated obstacles are visible in red.
	 * \shadowimage{rvo/rvo_navmesh_obstacle.png}
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Navmesh")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_navmesh.php")]
	public class RVONavmesh : GraphModifier {
		/** Height of the walls added for each obstacle edge.
		 * If a graph contains overlapping regions (e.g multiple floor in a building)
		 * you should set this low enough so that edges on different levels do not interfere,
		 * but high enough so that agents cannot move over them by mistake.
		 */
		public float wallHeight = 5;

		/** Obstacles currently added to the simulator */
		readonly List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		/** Last simulator used */
		Simulator lastSim;

		public override void OnPostCacheLoad () {
			OnLatePostScan();
		}

		public override void OnGraphsPostUpdate () {
			OnLatePostScan();
		}

		public override void OnLatePostScan () {
			if (!Application.isPlaying) return;

			Profiler.BeginSample("Update RVO Obstacles From Graphs");
			RemoveObstacles();
			NavGraph[] graphs = PathFindHelper.graphs;
			RVOSimulator rvosim = RVOSimulator.active;
			if (rvosim == null) throw new System.NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");

			// Remember which simulator these obstacles were added to
			lastSim = rvosim.GetSimulator();

			for (int i = 0; i < graphs.Length; i++) {
				RecastGraph recast = graphs[i] as RecastGraph;
				INavmesh navmesh = graphs[i] as INavmesh;
				if (recast != null) {
					foreach (var tile in recast.GetTiles()) {
						AddGraphObstacles(lastSim, tile);
					}
				} else if (navmesh != null)
				{
					AddGraphObstacles(lastSim, navmesh);
				}
			}
			Profiler.EndSample();
		}

		protected override void OnDisable () {
			base.OnDisable();
			RemoveObstacles();
		}

		/** Removes all obstacles which have been added by this component */
		public void RemoveObstacles () {
			if (lastSim != null) {
				for (int i = 0; i < obstacles.Count; i++) lastSim.RemoveObstacle(obstacles[i]);
				lastSim = null;
			}

			obstacles.Clear();
		}

		/** Adds obstacles for a navmesh/recast graph */
		void AddGraphObstacles (Pathfinding.RVO.Simulator simulator, INavmesh navmesh) {
			GraphUtilities.GetContours(navmesh, (vertices, cycle) => {
				var verticesV3 = new Vector3[vertices.Count];
				for (int i = 0; i < verticesV3.Length; i++) verticesV3[i] = (Vector3)vertices[i];
				// Pool the 'vertices' list to reduce allocations
				ListPool<Int3>.Release(vertices);
				obstacles.Add(simulator.AddObstacle(verticesV3, wallHeight, cycle));
			});
		}
	}
}
