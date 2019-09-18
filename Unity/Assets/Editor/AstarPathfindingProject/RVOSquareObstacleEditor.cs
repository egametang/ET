using UnityEditor;
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVOSquareObstacle))]
	[CanEditMultipleObjects]
	public class RVOSquareObstacleEditor : Editor {
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
		}
	}
}
