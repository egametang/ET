using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif
using UnityEditor;
using FairyGUI;

namespace FairyGUIEditor
{
	/// <summary>
	/// 
	/// </summary>
	[CustomEditor(typeof(UIPainter))]
	public class UIPainterEditor : Editor
	{
		SerializedProperty packageName;
		SerializedProperty componentName;
		SerializedProperty renderCamera;
		SerializedProperty fairyBatching;
		SerializedProperty touchDisabled;
		SerializedProperty sortingOrder;

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
		string[] propertyToExclude;
#endif
		void OnEnable()
		{
			packageName = serializedObject.FindProperty("packageName");
			componentName = serializedObject.FindProperty("componentName");
			renderCamera = serializedObject.FindProperty("renderCamera");
			fairyBatching = serializedObject.FindProperty("fairyBatching");
			touchDisabled = serializedObject.FindProperty("touchDisabled");
			sortingOrder = serializedObject.FindProperty("sortingOrder");

#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			propertyToExclude = new string[] { "m_Script", "packageName", "componentName", "packagePath",
				"renderCamera", "fairyBatching", "touchDisabled","sortingOrder" 
			};
#endif
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			UIPainter panel = target as UIPainter;
#if (UNITY_5 || UNITY_5_3_OR_NEWER)
			DrawPropertiesExcluding(serializedObject, propertyToExclude);
#endif
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Package Name");
			if (GUILayout.Button(packageName.stringValue, "ObjectField"))
				EditorWindow.GetWindow<PackagesWindow>(true, "Select a UI Component").SetSelection(packageName.stringValue, componentName.stringValue);

			if (GUILayout.Button("Clear", GUILayout.Width(50)))
			{
				bool isPrefab = PrefabUtility.GetPrefabType(panel) == PrefabType.Prefab;
				panel.SendMessage("OnUpdateSource", new object[] { null, null, null, !isPrefab });

#if UNITY_5_3_OR_NEWER
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#elif UNITY_5
				EditorApplication.MarkSceneDirty();
#else
				EditorUtility.SetDirty(panel);
#endif
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Component Name");
			if (GUILayout.Button(componentName.stringValue, "ObjectField"))
				EditorWindow.GetWindow<PackagesWindow>(true, "Select a UI Component").SetSelection(packageName.stringValue, componentName.stringValue);
			EditorGUILayout.EndHorizontal();
			int oldSortingOrder = panel.sortingOrder;
			EditorGUILayout.PropertyField(sortingOrder);
			EditorGUILayout.PropertyField(renderCamera);
			EditorGUILayout.PropertyField(fairyBatching);
			EditorGUILayout.PropertyField(touchDisabled);

			if (serializedObject.ApplyModifiedProperties())
			{
				if (PrefabUtility.GetPrefabType(panel) != PrefabType.Prefab)
				{
					panel.ApplyModifiedProperties(sortingOrder.intValue!=oldSortingOrder);
				}
			}
		}
	}
}
