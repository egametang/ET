using UnityEngine;

namespace Pathfinding.RVO {
	/** One vertex in an obstacle.
	 * This is a linked list and one vertex can therefore be used to reference the whole obstacle
	 * \astarpro
	 */
	public class ObstacleVertex {
		public bool ignore;

		/** Position of the vertex */
		public Vector3 position;
		public Vector2 dir;

		/** Height of the obstacle in this vertex */
		public float height;

		/** Collision layer for this obstacle */
		public RVOLayer layer = RVOLayer.DefaultObstacle;


		/** Next vertex in the obstacle */
		public ObstacleVertex next;
		/** Previous vertex in the obstacle */
		public ObstacleVertex prev;
	}
}
