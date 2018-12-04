using UnityEngine;

namespace Pathfinding.RVO {
	/**
	 * Square Obstacle for RVO Simulation.
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/Square Obstacle")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_square_obstacle.php")]
	public class RVOSquareObstacle : RVOObstacle {
		/** Height of the obstacle */
		public float height = 1;

		/** Size of the square */
		public Vector2 size = Vector3.one;

		/** Center of the square */
		public Vector2 center = Vector3.zero;

		protected override bool StaticObstacle { get { return false; } }
		protected override bool ExecuteInEditor { get { return true; } }
		protected override bool LocalCoordinates { get { return true; } }
		protected override float Height { get { return height; } }

		//If UNITY_EDITOR to save a few bytes, these are only needed in the editor
	#if UNITY_EDITOR
		private Vector2 _size;
		private Vector2 _center;
		private float _height;
	#endif

		protected override bool AreGizmosDirty () {
	#if UNITY_EDITOR
			bool ret = _size != size || _height != height || _center != center;
			_size = size;
			_center = center;
			_height = height;
			return ret;
	#else
			return false;
	#endif
		}

		protected override void CreateObstacles () {
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			height = Mathf.Abs(height);

			var verts = new [] { new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, -1) };
			for (int i = 0; i < verts.Length; i++) {
				verts[i].Scale(new Vector3(size.x * 0.5f, 0, size.y * 0.5f));
				verts[i] += new Vector3(center.x, 0, center.y);
			}

			AddObstacle(verts, height);
		}
	}
}
