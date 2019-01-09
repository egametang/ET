using PF;

namespace PF {
	/** Transforms to and from world space to a 2D movement plane.
	 * The transformation is guaranteed to be purely a rotation
	 * so no scale or offset is used. This interface is primarily
	 * used to make it easier to write movement scripts which can
	 * handle movement both in the XZ plane and in the XY plane.
	 *
	 * \see #Pathfinding.Util.GraphTransform
	 */
	public interface IMovementPlane {
		Vector2 ToPlane (Vector3 p);
		Vector2 ToPlane (Vector3 p, out float elevation);
		Vector3 ToWorld (Vector2 p, float elevation = 0);
	}

	/** Generic 3D coordinate transformation */
	public interface ITransform {
		Vector3 Transform (Vector3 position);
		Vector3 InverseTransform (Vector3 position);
	}

	/** Defines a transformation from graph space to world space.
	 * This is essentially just a simple wrapper around a matrix, but it has several utilities that are useful.
	 */
	public class GraphTransform : IMovementPlane, ITransform {
		/** True if this transform is the identity transform (i.e it does not do anything) */
		public readonly bool identity;

		/** True if this transform is a pure translation without any scaling or rotation */
		public readonly bool onlyTranslational;

		readonly bool isXY;
		readonly bool isXZ;

		readonly Matrix4x4 matrix;
		readonly Matrix4x4 inverseMatrix;
		readonly Vector3 up;
		public readonly Vector3 translation;
		readonly Int3 i3translation;
		readonly Quaternion rotation;
		readonly Quaternion inverseRotation;

		public static readonly GraphTransform identityTransform = new GraphTransform(Matrix4x4.identity);

		public GraphTransform (Matrix4x4 matrix)
		{
			this.matrix = matrix;
			inverseMatrix = matrix.inverse;
			identity = matrix.isIdentity;
			onlyTranslational = MatrixIsTranslational(matrix);
			up = matrix.MultiplyVector(Vector3.up).normalized;
			translation = matrix.MultiplyPoint3x4(Vector3.zero);
			i3translation = (Int3)translation;

			// Extract the rotation from the matrix. This is only correct if the matrix has no skew, but we only
			// want to use it for the movement plane so as long as the Up axis is parpendicular to the Forward
			// axis everything should be ok. In fact the only case in the project when all three axes are not
			// perpendicular is when hexagon or isometric grid graphs are used, but in those cases only the
			// X and Z axes are not perpendicular.
			rotation = Quaternion.LookRotation(TransformVector(Vector3.forward), TransformVector(Vector3.up));
			inverseRotation = Quaternion.Inverse(rotation);
			// Some short circuiting code for the movement plane calculations
			isXY = rotation == Quaternion.Euler(-90, 0, 0);
			isXZ = rotation == Quaternion.Euler(0, 0, 0);
		}

		public Vector3 WorldUpAtGraphPosition (Vector3 point) {
			return up;
		}

		static bool MatrixIsTranslational (Matrix4x4 matrix) {
			return matrix.GetColumn(0) == new Vector4(1, 0, 0, 0) && matrix.GetColumn(1) == new Vector4(0, 1, 0, 0) && matrix.GetColumn(2) == new Vector4(0, 0, 1, 0) && matrix.m33 == 1;
		}

		public Vector3 Transform (Vector3 point) {
			if (onlyTranslational) return point + translation;
			return matrix.MultiplyPoint3x4(point);
		}

		public Vector3 TransformVector (Vector3 point) {
			if (onlyTranslational) return point;
			return matrix.MultiplyVector(point);
		}

		public void Transform (Int3[] arr) {
			if (onlyTranslational) {
				for (int i = arr.Length - 1; i >= 0; i--) arr[i] += i3translation;
			} else {
				for (int i = arr.Length - 1; i >= 0; i--) arr[i] = (Int3)matrix.MultiplyPoint3x4((Vector3)arr[i]);
			}
		}

		public void Transform (Vector3[] arr) {
			if (onlyTranslational) {
				for (int i = arr.Length - 1; i >= 0; i--) arr[i] += translation;
			} else {
				for (int i = arr.Length - 1; i >= 0; i--) arr[i] = matrix.MultiplyPoint3x4(arr[i]);
			}
		}

		public Vector3 InverseTransform (Vector3 point) {
			if (onlyTranslational) return point - translation;
			return inverseMatrix.MultiplyPoint3x4(point);
		}

		public Int3 InverseTransform (Int3 point) {
			if (onlyTranslational) return point - i3translation;
			return (Int3)inverseMatrix.MultiplyPoint3x4((Vector3)point);
		}

		public void InverseTransform (Int3[] arr) {
			for (int i = arr.Length - 1; i >= 0; i--) arr[i] = (Int3)inverseMatrix.MultiplyPoint3x4((Vector3)arr[i]);
		}

		public static GraphTransform operator * (GraphTransform lhs, Matrix4x4 rhs) {
			return new GraphTransform(lhs.matrix * rhs);
		}

		public static GraphTransform operator * (Matrix4x4 lhs, GraphTransform rhs) {
			return new GraphTransform(lhs * rhs.matrix);
		}

		#region IMovementPlane implementation

		/** Transforms from world space to the 'ground' plane of the graph.
		 * The transformation is purely a rotation so no scale or offset is used.
		 *
		 * For a graph rotated with the rotation (-90, 0, 0) this will transform
		 * a coordinate (x,y,z) to (x,y). For a graph with the rotation (0,0,0)
		 * this will tranform a coordinate (x,y,z) to (x,z). More generally for
		 * a graph with a quaternion rotation R this will transform a vector V
		 * to R * V (i.e rotate the vector V using the rotation R).
		 */
		Vector2 IMovementPlane.ToPlane (Vector3 point) {
			// These special cases cover most graph orientations used in practice.
			// Having them here improves performance in those cases by a factor of
			// 2.5 without impacting the generic case in any significant way.
			if (isXY) return new Vector2(point.x, point.y);
			if (!isXZ) point = inverseRotation * point;
			return new Vector2(point.x, point.z);
		}

		/** Transforms from world space to the 'ground' plane of the graph.
		 * The transformation is purely a rotation so no scale or offset is used.
		 */
		Vector2 IMovementPlane.ToPlane (Vector3 point, out float elevation) {
			if (!isXZ) point = inverseRotation * point;
			elevation = point.y;
			return new Vector2(point.x, point.z);
		}

		/** Transforms from the 'ground' plane of the graph to world space.
		 * The transformation is purely a rotation so no scale or offset is used.
		 */
		Vector3 IMovementPlane.ToWorld (Vector2 point, float elevation) {
			return rotation * new Vector3(point.x, elevation, point.y);
		}

		#endregion
	}
}
