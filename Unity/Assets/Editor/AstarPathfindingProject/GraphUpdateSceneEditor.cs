using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	/** Editor for GraphUpdateScene */
	[CustomEditor(typeof(GraphUpdateScene))]
	[CanEditMultipleObjects]
	public class GraphUpdateSceneEditor : EditorBase {
		int selectedPoint = -1;

		const float pointGizmosRadius = 0.09F;
		static Color PointColor = new Color(1, 0.36F, 0, 0.6F);
		static Color PointSelectedColor = new Color(1, 0.24F, 0, 1.0F);

		GraphUpdateScene[] scripts;

		protected override void Inspector () {
			// Find all properties
			var points = FindProperty("points");
			var legacyMode = FindProperty("legacyMode");

			// Get a list of inspected components
			scripts = new GraphUpdateScene[targets.Length];
			targets.CopyTo(scripts, 0);

			EditorGUI.BeginChangeCheck();

			// Make sure no point arrays are null
			for (int i = 0; i < scripts.Length; i++) {
				scripts[i].points = scripts[i].points ?? new PF.Vector3[0];
			}

			if (!points.hasMultipleDifferentValues && points.arraySize == 0) {
				if (scripts[0].GetComponent<PolygonCollider2D>() != null) {
					EditorGUILayout.HelpBox("Using polygon collider shape", MessageType.Info);
				} else if (scripts[0].GetComponent<Collider>() != null || scripts[0].GetComponent<Collider2D>() != null) {
					EditorGUILayout.HelpBox("No points, using collider.bounds", MessageType.Info);
				} else if (scripts[0].GetComponent<Renderer>() != null) {
					EditorGUILayout.HelpBox("No points, using renderer.bounds", MessageType.Info);
				} else {
					EditorGUILayout.HelpBox("No points and no collider or renderer attached, will not affect anything\nPoints can be added using the transform tool and holding shift", MessageType.Warning);
				}
			}

			DrawPointsField();

			EditorGUI.indentLevel = 0;

			DrawPhysicsField();

			PropertyField("updateErosion", null, "Recalculate erosion for grid graphs.\nSee online documentation for more info");

			DrawConvexField();

			// Minimum bounds height is not applied when using the bounds from a collider or renderer
			if (points.hasMultipleDifferentValues || points.arraySize > 0) {
				PropertyField("minBoundsHeight");
				Clamp("minBoundsHeight", 0.1f);
			}
			PropertyField("applyOnStart");
			PropertyField("applyOnScan");

			DrawWalkableField();
			DrawPenaltyField();
			DrawTagField();

			EditorGUILayout.Separator();

			if (legacyMode.hasMultipleDifferentValues || legacyMode.boolValue) {
				EditorGUILayout.HelpBox("Legacy mode is enabled because you have upgraded from an earlier version of the A* Pathfinding Project. " +
					"Disabling legacy mode is recommended but you may have to tweak the point locations or object rotation in some cases", MessageType.Warning);
				if (GUILayout.Button("Disable Legacy Mode")) {
					for (int i = 0; i < scripts.Length; i++) {
						Undo.RecordObject(scripts[i], "Disable Legacy Mode");
						scripts[i].DisableLegacyMode();
					}
				}
			}

			if (scripts.Length == 1 && scripts[0].points.Length >= 3) {
				var size = scripts[0].GetBounds().size;
				if (Mathf.Min(Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y)), Mathf.Abs(size.z)) < 0.05f) {
					EditorGUILayout.HelpBox("The bounding box is very thin. Your shape might be oriented incorrectly. The shape will be projected down on the XZ plane in local space. Rotate this object " +
						"so that the local XZ plane corresponds to the plane in which you want to create your shape. For example if you want to create your shape in the XY plane then " +
						"this object should have the rotation (-90,0,0). You will need to recreate your shape after rotating this object.", MessageType.Warning);
				}
			}

			if (GUILayout.Button("Clear all points")) {
				for (int i = 0; i < scripts.Length; i++) {
					Undo.RecordObject(scripts[i], "Clear points");
					scripts[i].points = new PF.Vector3[0];
					scripts[i].RecalcConvex();
				}
			}

			if (EditorGUI.EndChangeCheck()) {
				for (int i = 0; i < scripts.Length; i++) {
					EditorUtility.SetDirty(scripts[i]);
				}

				// Repaint the scene view if necessary
				if (!Application.isPlaying || EditorApplication.isPaused) SceneView.RepaintAll();
			}
		}

		void DrawPointsField () {
			EditorGUI.BeginChangeCheck();
			PropertyField("points");
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				for (int i = 0; i < scripts.Length; i++) {
					scripts[i].RecalcConvex();
				}
				HandleUtility.Repaint();
			}
		}

		void DrawPhysicsField () {
			if (PropertyField("updatePhysics", "Update Physics", "Perform similar calculations on the nodes as during scan.\n" +
					"Grid Graphs will update the position of the nodes and also check walkability using collision.\nSee online documentation for more info.")) {
				EditorGUI.indentLevel++;
				PropertyField("resetPenaltyOnPhysics");
				EditorGUI.indentLevel--;
			}
		}

		void DrawConvexField () {
			EditorGUI.BeginChangeCheck();
			PropertyField("convex");
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
				for (int i = 0; i < scripts.Length; i++) {
					scripts[i].RecalcConvex();
				}
				HandleUtility.Repaint();
			}
		}

		void DrawWalkableField () {
			if (PropertyField("modifyWalkability")) {
				EditorGUI.indentLevel++;
				PropertyField("setWalkability", "Walkability Value");
				EditorGUI.indentLevel--;
			}
		}

		void DrawPenaltyField () {
			PropertyField("penaltyDelta", "Penalty Delta");

			if (!FindProperty("penaltyDelta").hasMultipleDifferentValues && FindProperty("penaltyDelta").intValue < 0) {
				EditorGUILayout.HelpBox("Be careful when lowering the penalty. Negative penalties are not supported and will instead underflow and get really high.\n" +
					"You can set an initial penalty on graphs (see their settings) and then lower them like this to get regions which are easier to traverse.", MessageType.Warning);
			}
		}

		void DrawTagField () {
			if (PropertyField("modifyTag")) {
				var tagValue = FindProperty("setTag");
				EditorGUI.indentLevel++;
				EditorGUI.showMixedValue = tagValue.hasMultipleDifferentValues;
				EditorGUI.BeginChangeCheck();
				var newTag = EditorGUILayoutx.TagField("Tag Value", tagValue.intValue);
				if (EditorGUI.EndChangeCheck()) {
					tagValue.intValue = newTag;
				}

				if (GUILayout.Button("Tags can be used to restrict which units can walk on what ground. Click here for more info", "HelpBox")) {
					Application.OpenURL(AstarUpdateChecker.GetURL("tags"));
				}
				EditorGUI.indentLevel--;
			}
		}

		static void SphereCap (int controlID, Vector3 position, Quaternion rotation, float size) {
#if UNITY_5_5_OR_NEWER
			Handles.SphereHandleCap(controlID, position, rotation, size, Event.current.type);
#else
			Handles.SphereCap(controlID, position, rotation, size);
#endif
		}

		public void OnSceneGUI () {
			var script = target as GraphUpdateScene;

			// Don't allow editing unless it is the active object
			if (Selection.activeGameObject != script.gameObject || script.legacyMode) return;

			// Make sure the points array is not null
			if (script.points == null) {
				script.points = new PF.Vector3[0];
				EditorUtility.SetDirty(script);
			}

			List<PF.Vector3> points = ListPool<PF.Vector3>.Claim();
			points.AddRange(script.points);

			Matrix4x4 invMatrix = script.transform.worldToLocalMatrix;

			Matrix4x4 matrix = script.transform.localToWorldMatrix;
			for (int i = 0; i < points.Count; i++) points[i] = matrix.MultiplyPoint3x4(points[i]);


			if (Tools.current != Tool.View && Event.current.type == EventType.Layout) {
				for (int i = 0; i < script.points.Length; i++) {
					HandleUtility.AddControl(-i - 1, HandleUtility.DistanceToLine(points[i], points[i]));
				}
			}

			if (Tools.current != Tool.View)
				HandleUtility.AddDefaultControl(0);

			for (int i = 0; i < points.Count; i++) {
				if (i == selectedPoint && Tools.current == Tool.Move) {
					Handles.color = PointSelectedColor;
					SphereCap(-i-1, points[i], Quaternion.identity, HandleUtility.GetHandleSize(points[i])*pointGizmosRadius*2);

					Vector3 pre = points[i];
					Vector3 post = Handles.PositionHandle(points[i], Quaternion.identity);
					if (pre != post) {
						Undo.RecordObject(script, "Moved Point");
						script.points[i] = invMatrix.MultiplyPoint3x4(post);
					}
				} else {
					Handles.color = PointColor;
					SphereCap(-i-1, points[i], Quaternion.identity, HandleUtility.GetHandleSize(points[i])*pointGizmosRadius);
				}
			}

			if (Event.current.type == EventType.MouseDown) {
				int pre = selectedPoint;
				selectedPoint = -(HandleUtility.nearestControl+1);
				if (pre != selectedPoint) GUI.changed = true;
			}

			if (Event.current.shift && Tools.current == Tool.Move) {
				HandleUtility.Repaint();

				if (((int)Event.current.modifiers & (int)EventModifiers.Alt) != 0) {
					if (Event.current.type == EventType.MouseDown && selectedPoint >= 0 && selectedPoint < points.Count) {
						Undo.RecordObject(script, "Removed Point");
						var arr = new List<PF.Vector3>(script.points);
						arr.RemoveAt(selectedPoint);
						points.RemoveAt(selectedPoint);
						script.points = arr.ToArray();
						GUI.changed = true;
					} else if (points.Count > 0) {
						var index = -(HandleUtility.nearestControl+1);
						if (index >= 0 && index < points.Count) {
							Handles.color = Color.red;
							SphereCap(0, points[index], Quaternion.identity, HandleUtility.GetHandleSize(points[index])*2f*pointGizmosRadius);
						}
					}
				} else {
					// Find the closest segment
					int insertionIndex = points.Count;
					float minDist = float.PositiveInfinity;
					for (int i = 0; i < points.Count; i++) {
						float dist = HandleUtility.DistanceToLine(points[i], points[(i+1)%points.Count]);
						if (dist < minDist) {
							insertionIndex = i + 1;
							minDist = dist;
						}
					}

					var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					System.Object hit = HandleUtility.RaySnap(ray);
					Vector3 rayhit = Vector3.zero;
					bool didHit = false;
					if (hit != null) {
						rayhit = ((RaycastHit)hit).point;
						didHit = true;
					} else {
						var plane = new Plane(script.transform.up, script.transform.position);
						float distance;
						plane.Raycast(ray, out distance);
						if (distance > 0) {
							rayhit = ray.GetPoint(distance);
							didHit = true;
						}
					}

					if (didHit) {
						if (Event.current.type == EventType.MouseDown) {
							points.Insert(insertionIndex, rayhit);

							Undo.RecordObject(script, "Added Point");
							var arr = new List<PF.Vector3>(script.points);
							arr.Insert(insertionIndex, invMatrix.MultiplyPoint3x4(rayhit));
							script.points = arr.ToArray();
							GUI.changed = true;
						} else if (points.Count > 0) {
							Handles.color = Color.green;
							Handles.DrawDottedLine(points[(insertionIndex-1 + points.Count) % points.Count], rayhit, 8);
							Handles.DrawDottedLine(points[insertionIndex % points.Count], rayhit, 8);
							SphereCap(0, rayhit, Quaternion.identity, HandleUtility.GetHandleSize(rayhit)*pointGizmosRadius);
							// Project point down onto a plane
							var zeroed = invMatrix.MultiplyPoint3x4(rayhit);
							zeroed.y = 0;
							Handles.color = new Color(1, 1, 1, 0.5f);
							Handles.DrawDottedLine(matrix.MultiplyPoint3x4(zeroed), rayhit, 4);
						}
					}
				}

				if (Event.current.type == EventType.MouseDown) {
					Event.current.Use();
				}
			}

			// Make sure the convex hull stays up to date
			script.RecalcConvex();
			ListPool<PF.Vector3>.Release(ref points);

			if (GUI.changed) HandleUtility.Repaint();
		}
	}
}
