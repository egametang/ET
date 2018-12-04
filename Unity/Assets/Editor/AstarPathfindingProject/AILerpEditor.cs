using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	[CustomEditor(typeof(AILerp), true)]
	[CanEditMultipleObjects]
	public class AILerpEditor : EditorBase {
		protected override void Inspector () {
			PropertyField("speed");
			PropertyField("repathRate");
			PropertyField("canSearch");
			PropertyField("canMove");
			if (PropertyField("enableRotation")) {
				EditorGUI.indentLevel++;
				PropertyField("rotationSpeed");
				PropertyField("rotationIn2D");
				EditorGUI.indentLevel--;
			}

			if (PropertyField("interpolatePathSwitches")) {
				EditorGUI.indentLevel++;
				PropertyField("switchPathInterpolationSpeed");
				EditorGUI.indentLevel--;
			}
		}
	}
}
