using PF;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	[CustomEditor(typeof(Pathfinding.RVO.RVOSimulator))]
	public class RVOSimulatorEditor : EditorBase {
		protected override void Inspector () {
			PropertyField("desiredSimulationFPS");
			ClampInt("desiredSimulationFPS", 1);

			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			PropertyField("movementPlane");
			PropertyField("workerThreads");
			if ((ThreadCount)FindProperty("workerThreads").intValue != ThreadCount.None) {
				EditorGUI.indentLevel++;
				PropertyField("doubleBuffering");
				EditorGUI.indentLevel--;
			}
			EditorGUI.EndDisabledGroup();
			PropertyField("symmetryBreakingBias");
			PropertyField("drawObstacles");
		}
	}
}
