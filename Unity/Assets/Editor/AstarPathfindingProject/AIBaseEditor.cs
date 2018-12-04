using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	[CustomEditor(typeof(AIBase), true)]
	[CanEditMultipleObjects]
	public class BaseAIEditor : EditorBase {
		protected SerializedProperty gravity, groundMask, centerOffset, rotationIn2D, acceleration;
		float lastSeenCustomGravity = float.NegativeInfinity;

		protected override void OnEnable () {
			base.OnEnable();
			gravity = serializedObject.FindProperty("gravity");
			groundMask = serializedObject.FindProperty("groundMask");
			centerOffset = serializedObject.FindProperty("centerOffset");
			rotationIn2D = serializedObject.FindProperty("rotationIn2D");
			acceleration = serializedObject.FindProperty("maxAcceleration");
		}

		protected override void Inspector () {
			// Iterate over all properties of the script
			var p = serializedObject.GetIterator();

			p.Next(true);
			while (p.NextVisible(false)) {
				if (!SerializedProperty.EqualContents(p, groundMask) && !SerializedProperty.EqualContents(p, centerOffset) && !SerializedProperty.EqualContents(p, gravity) && !SerializedProperty.EqualContents(p, rotationIn2D)) {
					if (SerializedProperty.EqualContents(p, acceleration) && typeof(AIPath).IsAssignableFrom(target.GetType())) {
						EditorGUI.BeginChangeCheck();
						int grav = acceleration.hasMultipleDifferentValues ? -1 : (acceleration.floatValue >= 0 ? 1 : 0);
						var ngrav = EditorGUILayout.Popup("Max Acceleration", grav, new [] { "Default", "Custom" });
						if (EditorGUI.EndChangeCheck()) {
							if (ngrav == 0) acceleration.floatValue = -2.5f;
							else if (acceleration.floatValue < 0) acceleration.floatValue = 10;
						}

						if (!acceleration.hasMultipleDifferentValues && ngrav == 1) {
							EditorGUI.indentLevel++;
							PropertyField(acceleration.propertyPath);
							EditorGUI.indentLevel--;
							acceleration.floatValue = Mathf.Max(acceleration.floatValue, 0.01f);
						}
					} else {
						PropertyField(p);
					}
				}
			}

			PropertyField(rotationIn2D);

			var mono = target as MonoBehaviour;
			var rigid = mono.GetComponent<Rigidbody>();
			var rigid2D = mono.GetComponent<Rigidbody2D>();
			var controller = mono.GetComponent<CharacterController>();
			var canUseGravity = (controller != null && controller.enabled) || ((rigid == null || rigid.isKinematic) && (rigid2D == null || rigid2D.isKinematic));

			if (canUseGravity) {
				EditorGUI.BeginChangeCheck();
				int grav = gravity.hasMultipleDifferentValues ? -1 : (gravity.vector3Value == Vector3.zero ? 0 : (float.IsNaN(gravity.vector3Value.x) ? 1 : 2));
				var ngrav = EditorGUILayout.Popup("Gravity", grav, new [] { "None", "Use Project Settings", "Custom" });
				if (EditorGUI.EndChangeCheck()) {
					if (ngrav == 0) gravity.vector3Value = Vector3.zero;
					else if (ngrav == 1) gravity.vector3Value = new Vector3(float.NaN, float.NaN, float.NaN);
					else if (float.IsNaN(gravity.vector3Value.x) || gravity.vector3Value == Vector3.zero) gravity.vector3Value = Physics.gravity;
					lastSeenCustomGravity = float.NegativeInfinity;
				}

				if (!gravity.hasMultipleDifferentValues) {
					// A sort of delayed Vector3 field (to prevent the field from dissappearing if you happen to enter zeroes into x, y and z for a short time)
					// Note: cannot use != in this case because that will not give the correct result in case of NaNs
					if (!(gravity.vector3Value == Vector3.zero)) lastSeenCustomGravity = Time.realtimeSinceStartup;
					if (Time.realtimeSinceStartup - lastSeenCustomGravity < 2f) {
						EditorGUI.indentLevel++;
						if (!float.IsNaN(gravity.vector3Value.x)) {
							PropertyField(gravity.propertyPath);
						}

						if (controller == null || !controller.enabled) {
							PropertyField(groundMask.propertyPath, "Raycast Ground Mask");
							PropertyField(centerOffset.propertyPath, "Raycast Center Offset");
						}

						EditorGUI.indentLevel--;
					}
				}
			} else {
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Popup(new GUIContent(gravity.displayName, "Disabled because a non-kinematic rigidbody is attached"), 0, new [] { new GUIContent("Handled by Rigidbody") });
				EditorGUI.EndDisabledGroup();
			}

			if ((rigid != null || rigid2D != null) && (controller != null && controller.enabled)) {
				EditorGUILayout.HelpBox("You are using both a Rigidbody and a Character Controller. Those components are not really designed for that. Please use only one of them.", MessageType.Warning);
			}
		}
	}
}
