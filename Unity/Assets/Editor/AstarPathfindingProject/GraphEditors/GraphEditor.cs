using PF;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	public class GraphEditor : GraphEditorBase {
		public AstarPathEditor editor;

		/** Stores if the graph is visible or not in the inspector */
		public FadeArea fadeArea;

		/** Stores if the graph info box is visible or not in the inspector */
		public FadeArea infoFadeArea;

		/** Called by editor scripts to rescan the graphs e.g when the user moved a graph.
		 * Will only scan graphs if not playing and time to scan last graph was less than some constant (to avoid lag with large graphs) */
		public bool AutoScan () {
			if (!Application.isPlaying && AstarPath.active != null && AstarPath.active.lastScanTime < 0.11F) {
				AstarPath.active.Scan();
				return true;
			}
			return false;
		}

		public virtual void OnEnable () {
		}

		public static Object ObjectField (string label, Object obj, System.Type objType, bool allowSceneObjects) {
			return ObjectField(new GUIContent(label), obj, objType, allowSceneObjects);
		}

		public static Object ObjectField (GUIContent label, Object obj, System.Type objType, bool allowSceneObjects) {
			obj = EditorGUILayout.ObjectField(label, obj, objType, allowSceneObjects);

			if (obj != null) {
				if (allowSceneObjects && !EditorUtility.IsPersistent(obj)) {
					// Object is in the scene
					var com = obj as Component;
					var go = obj as GameObject;
					if (com != null) {
						go = com.gameObject;
					}
					if (go != null && go.GetComponent<UnityReferenceHelper>() == null) {
						if (FixLabel("Object's GameObject must have a UnityReferenceHelper component attached")) {
							go.AddComponent<UnityReferenceHelper>();
						}
					}
				} else if (EditorUtility.IsPersistent(obj)) {
					string path = AssetDatabase.GetAssetPath(obj).Replace("\\", "/");
					var rg = new System.Text.RegularExpressions.Regex(@"Resources/.*$");

					if (!rg.IsMatch(path)) {
						if (FixLabel("Object must be in the 'Resources' folder")) {
							if (!System.IO.Directory.Exists(Application.dataPath+"/Resources")) {
								System.IO.Directory.CreateDirectory(Application.dataPath+"/Resources");
								AssetDatabase.Refresh();
							}

							string ext = System.IO.Path.GetExtension(path);
							string error = AssetDatabase.MoveAsset(path, "Assets/Resources/"+obj.name+ext);

							if (error == "") {
								path = AssetDatabase.GetAssetPath(obj);
							} else {
								Debug.LogError("Couldn't move asset - "+error);
							}
						}
					}

					if (!AssetDatabase.IsMainAsset(obj) && obj.name != AssetDatabase.LoadMainAssetAtPath(path).name) {
						if (FixLabel("Due to technical reasons, the main asset must\nhave the same name as the referenced asset")) {
							string error = AssetDatabase.RenameAsset(path, obj.name);
							if (error != "") {
								Debug.LogError("Couldn't rename asset - "+error);
							}
						}
					}
				}
			}

			return obj;
		}

		/** Draws common graph settings */
		public void OnBaseInspectorGUI (NavGraph target) {
			int penalty = EditorGUILayout.IntField(new GUIContent("Initial Penalty", "Initial Penalty for nodes in this graph. Set during Scan."), (int)target.initialPenalty);

			if (penalty < 0) penalty = 0;
			target.initialPenalty = (uint)penalty;
		}

		/** Override to implement graph inspectors */
		public virtual void OnInspectorGUI (NavGraph target) {
		}

		/** Override to implement scene GUI drawing for the graph */
		public virtual void OnSceneGUI (NavGraph target) {
		}

		/** Draws a thin separator line */
		public static void Separator () {
			GUIStyle separator = AstarPathEditor.astarSkin.FindStyle("PixelBox3Separator") ?? new GUIStyle();

			Rect r = GUILayoutUtility.GetRect(new GUIContent(), separator);

			if (Event.current.type == EventType.Repaint) {
				separator.Draw(r, false, false, false, false);
			}
		}

		/** Draws a small help box with a 'Fix' button to the right. \returns Boolean - Returns true if the button was clicked */
		public static bool FixLabel (string label, string buttonLabel = "Fix", int buttonWidth = 40) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(14*EditorGUI.indentLevel);
			GUILayout.BeginHorizontal(AstarPathEditor.helpBox);
			GUILayout.Label(label, EditorGUIUtility.isProSkin ? EditorStyles.whiteMiniLabel : EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
			var returnValue = GUILayout.Button(buttonLabel, EditorStyles.miniButton, GUILayout.Width(buttonWidth));
			GUILayout.EndHorizontal();
			GUILayout.EndHorizontal();
			return returnValue;
		}

		/** Draws a toggle with a bold label to the right. Does not enable or disable GUI */
		public bool ToggleGroup (string label, bool value) {
			return ToggleGroup(new GUIContent(label), value);
		}

		/** Draws a toggle with a bold label to the right. Does not enable or disable GUI */
		public static bool ToggleGroup (GUIContent label, bool value) {
			GUILayout.BeginHorizontal();
			GUILayout.Space(13*EditorGUI.indentLevel);
			value = GUILayout.Toggle(value, "", GUILayout.Width(10));
			GUIStyle boxHeader = AstarPathEditor.astarSkin.FindStyle("CollisionHeader");
			if (GUILayout.Button(label, boxHeader, GUILayout.Width(100))) {
				value = !value;
			}

			GUILayout.EndHorizontal();
			return value;
		}
	}
}
