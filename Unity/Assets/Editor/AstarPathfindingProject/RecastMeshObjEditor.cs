using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(RecastMeshObj))]
	[CanEditMultipleObjects]
	public class RecastMeshObjEditor : EditorBase {
		SerializedProperty areaProp;
		SerializedProperty dynamicProp;

		protected override void OnEnable () {
			areaProp = serializedObject.FindProperty("area");
			dynamicProp = serializedObject.FindProperty("dynamic");
		}

		protected override void Inspector () {
			var customValue = 1;

			for (int i = 0; i < targets.Length; i++) {
				var script = targets[i] as RecastMeshObj;
				if (script.area > 0) {
					customValue = script.area;
				}
			}

			EditorGUILayout.IntPopup(areaProp, new GUIContent[] {
				new GUIContent("Unwalkable Surface"),
				new GUIContent("Walkable Surface"),
				new GUIContent("Custom Surface Area")
			}, new int[] {
				-1,
				0,
				customValue
			},
				new GUIContent("Area Type")
				);

			if (areaProp.intValue < -1) {
				areaProp.intValue = 0;
			}

			if (!areaProp.hasMultipleDifferentValues && areaProp.intValue >= 1) {
				PropertyField(areaProp);
			}

			if (!areaProp.hasMultipleDifferentValues) {
				if (areaProp.intValue == -1) {
					EditorGUILayout.HelpBox("All surfaces on this mesh will be made unwalkable", MessageType.None);
				} else if (areaProp.intValue == 0) {
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable", MessageType.None);
				} else if (areaProp.intValue > 0) {
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable and a " +
						"seam will be created between the surfaces on this mesh and the surfaces on other meshes", MessageType.None);
				}
			}

			PropertyField(dynamicProp, "Dynamic", "Setting this value to false will give better scanning performance, but you will not be able to move the object during runtime");

			if (!dynamicProp.hasMultipleDifferentValues && !dynamicProp.boolValue) {
				EditorGUILayout.HelpBox("This object must not be moved during runtime since 'dynamic' is set to false", MessageType.Info);
			}
		}
	}
}
