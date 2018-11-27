using PF;
using UnityEngine;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.Util {
	/** Helper methods for drawing gizmos and debug lines */
	public class Draw {
		public static readonly Draw Debug = new Draw { gizmos = false };
		public static readonly Draw Gizmos = new Draw { gizmos = true };

		bool gizmos;
		Matrix4x4 matrix = Matrix4x4.identity;

		void SetColor (Color color) {
			if (gizmos && UnityEngine.Gizmos.color != color) UnityEngine.Gizmos.color = color;
		}

		public void Line (Vector3 a, Vector3 b, Color color) {
			SetColor(color);
			if (gizmos) UnityEngine.Gizmos.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b));
			else UnityEngine.Debug.DrawLine(matrix.MultiplyPoint3x4(a), matrix.MultiplyPoint3x4(b), color);
		}

		public void CircleXZ (Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = 2*Mathf.PI) {
			int steps = 40;

#if UNITY_EDITOR
			if (gizmos) steps = (int)Mathf.Clamp(Mathf.Sqrt(radius / UnityEditor.HandleUtility.GetHandleSize((UnityEngine.Gizmos.matrix * matrix).MultiplyPoint3x4(center))) * 25, 4, 40);
#endif
			while (startAngle > endAngle) startAngle -= 2*Mathf.PI;

			Vector3 prev = new Vector3(Mathf.Cos(startAngle)*radius, 0, Mathf.Sin(startAngle)*radius);
			for (int i = 0; i <= steps; i++) {
				Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, i/(float)steps))*radius, 0, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, i/(float)steps))*radius);
				Line(center + prev, center + c, color);
				prev = c;
			}
		}

		public void Cylinder (Vector3 position, Vector3 up, float height, float radius, Color color) {
			var tangent = Vector3.Cross(up, Vector3.one).normalized;

			matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, up), new Vector3(radius, height, radius));
			CircleXZ(Vector3.zero, 1, color);

			if (height > 0) {
				CircleXZ(Vector3.up, 1, color);
				Line(new Vector3(1, 0, 0), new Vector3(1, 1, 0), color);
				Line(new Vector3(-1, 0, 0), new Vector3(-1, 1, 0), color);
				Line(new Vector3(0, 0, 1), new Vector3(0, 1, 1), color);
				Line(new Vector3(0, 0, -1), new Vector3(0, 1, -1), color);
			}

			matrix = Matrix4x4.identity;
		}

		public void CrossXZ (Vector3 position, Color color, float size = 1) {
			size *= 0.5f;
			Line(position - Vector3.right*size, position + Vector3.right*size, color);
			Line(position - Vector3.forward*size, position + Vector3.forward*size, color);
		}

		public void Bezier (Vector3 a, Vector3 b, Color color) {
			Vector3 dir = b - a;

			if (dir == Vector3.zero) return;

			Vector3 normal = Vector3.Cross(Vector3.up, dir);
			Vector3 normalUp = Vector3.Cross(dir, normal);

			normalUp = normalUp.normalized;
			normalUp *= dir.magnitude*0.1f;

			Vector3 p1c = a + normalUp;
			Vector3 p2c = b + normalUp;

			Vector3 prev = a;
			for (int i = 1; i <= 20; i++) {
				float t = i/20.0f;
				Vector3 p = AstarSplines.CubicBezier(a, p1c, p2c, b, t);
				Line(prev, p, color);
				prev = p;
			}
		}
	}
}
