using PF;

namespace PF {
	/** Holds a coordinate in integers */
	public struct Int3 : System.IEquatable<Int3> {
		public int x;
		public int y;
		public int z;

		//These should be set to the same value (only PrecisionFactor should be 1 divided by Precision)

		/** Precision for the integer coordinates.
		 * One world unit is divided into [value] pieces. A value of 1000 would mean millimeter precision, a value of 1 would mean meter precision (assuming 1 world unit = 1 meter).
		 * This value affects the maximum coordinates for nodes as well as how large the cost values are for moving between two nodes.
		 * A higher value means that you also have to set all penalty values to a higher value to compensate since the normal cost of moving will be higher.
		 */
		public const int Precision = 1000;

		/** #Precision as a float */
		public const float FloatPrecision = 1000F;

		/** 1 divided by #Precision */
		public const float PrecisionFactor = 0.001F;

		public static Int3 zero { get { return new Int3(); } }

		public Int3 (Vector3 position) {
			x = (int)System.Math.Round(position.x*FloatPrecision);
			y = (int)System.Math.Round(position.y*FloatPrecision);
			z = (int)System.Math.Round(position.z*FloatPrecision);
		}

		public Int3 (int _x, int _y, int _z) {
			x = _x;
			y = _y;
			z = _z;
		}

		public static bool operator == (Int3 lhs, Int3 rhs) {
			return lhs.x == rhs.x &&
				   lhs.y == rhs.y &&
				   lhs.z == rhs.z;
		}

		public static bool operator != (Int3 lhs, Int3 rhs) {
			return lhs.x != rhs.x ||
				   lhs.y != rhs.y ||
				   lhs.z != rhs.z;
		}

		public static explicit operator Int3 (Vector3 ob) {
			return new Int3(
				(int)System.Math.Round(ob.x*FloatPrecision),
				(int)System.Math.Round(ob.y*FloatPrecision),
				(int)System.Math.Round(ob.z*FloatPrecision)
				);
		}
		
		public static explicit operator Vector3 (Int3 ob) {
			return new Vector3(ob.x*PrecisionFactor, ob.y*PrecisionFactor, ob.z*PrecisionFactor);
		}

#if !SERVER	
		public static explicit operator Int3 (UnityEngine.Vector3 ob) {
			return new Int3(
			                (int)System.Math.Round(ob.x*FloatPrecision),
			                (int)System.Math.Round(ob.y*FloatPrecision),
			                (int)System.Math.Round(ob.z*FloatPrecision)
			               );
		}
	
		public static explicit operator UnityEngine.Vector3 (Int3 ob) {
			return new UnityEngine.Vector3(ob.x*PrecisionFactor, ob.y*PrecisionFactor, ob.z*PrecisionFactor);
		}
#endif

		public static Int3 operator - (Int3 lhs, Int3 rhs) {
			lhs.x -= rhs.x;
			lhs.y -= rhs.y;
			lhs.z -= rhs.z;
			return lhs;
		}

		public static Int3 operator - (Int3 lhs) {
			lhs.x = -lhs.x;
			lhs.y = -lhs.y;
			lhs.z = -lhs.z;
			return lhs;
		}

		public static Int3 operator + (Int3 lhs, Int3 rhs) {
			lhs.x += rhs.x;
			lhs.y += rhs.y;
			lhs.z += rhs.z;
			return lhs;
		}

		public static Int3 operator * (Int3 lhs, int rhs) {
			lhs.x *= rhs;
			lhs.y *= rhs;
			lhs.z *= rhs;

			return lhs;
		}

		public static Int3 operator * (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round(lhs.x * rhs);
			lhs.y = (int)System.Math.Round(lhs.y * rhs);
			lhs.z = (int)System.Math.Round(lhs.z * rhs);

			return lhs;
		}

		public static Int3 operator * (Int3 lhs, double rhs) {
			lhs.x = (int)System.Math.Round(lhs.x * rhs);
			lhs.y = (int)System.Math.Round(lhs.y * rhs);
			lhs.z = (int)System.Math.Round(lhs.z * rhs);

			return lhs;
		}

		public static Int3 operator / (Int3 lhs, float rhs) {
			lhs.x = (int)System.Math.Round(lhs.x / rhs);
			lhs.y = (int)System.Math.Round(lhs.y / rhs);
			lhs.z = (int)System.Math.Round(lhs.z / rhs);
			return lhs;
		}

		public int this[int i] {
			get {
				return i == 0 ? x : (i == 1 ? y : z);
			}
			set {
				if (i == 0) x = value;
				else if (i == 1) y = value;
				else z = value;
			}
		}

		/** Angle between the vectors in radians */
		public static float Angle (Int3 lhs, Int3 rhs) {
			double cos = Dot(lhs, rhs)/ ((double)lhs.magnitude*(double)rhs.magnitude);

			cos = cos < -1 ? -1 : (cos > 1 ? 1 : cos);
			return (float)System.Math.Acos(cos);
		}

		public static int Dot (Int3 lhs, Int3 rhs) {
			return
				lhs.x * rhs.x +
				lhs.y * rhs.y +
				lhs.z * rhs.z;
		}

		public static long DotLong (Int3 lhs, Int3 rhs) {
			return
				(long)lhs.x * (long)rhs.x +
				(long)lhs.y * (long)rhs.y +
				(long)lhs.z * (long)rhs.z;
		}

		/** Normal in 2D space (XZ).
		 * Equivalent to Cross(this, Int3(0,1,0) )
		 * except that the Y coordinate is left unchanged with this operation.
		 */
		public Int3 Normal2D () {
			return new Int3(z, y, -x);
		}

		/** Returns the magnitude of the vector. The magnitude is the 'length' of the vector from 0,0,0 to this point. Can be used for distance calculations:
		 * \code Debug.Log ("Distance between 3,4,5 and 6,7,8 is: "+(new Int3(3,4,5) - new Int3(6,7,8)).magnitude); \endcode
		 */
		public float magnitude {
			get {
				//It turns out that using doubles is just as fast as using ints with Mathf.Sqrt. And this can also handle larger numbers (possibly with small errors when using huge numbers)!

				double _x = x;
				double _y = y;
				double _z = z;

				return (float)System.Math.Sqrt(_x*_x+_y*_y+_z*_z);
			}
		}

		/** Magnitude used for the cost between two nodes. The default cost between two nodes can be calculated like this:
		 * \code int cost = (node1.position-node2.position).costMagnitude; \endcode
		 *
		 * This is simply the magnitude, rounded to the nearest integer
		 */
		public int costMagnitude {
			get {
				return (int)System.Math.Round(magnitude);
			}
		}

		/** The magnitude in world units.
		 * \deprecated This property is deprecated. Use magnitude or cast to a Vector3
		 */
		[System.Obsolete("This property is deprecated. Use magnitude or cast to a Vector3")]
		public float worldMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;

				return (float)System.Math.Sqrt(_x*_x+_y*_y+_z*_z)*PrecisionFactor;
			}
		}

		/** The squared magnitude of the vector */
		public float sqrMagnitude {
			get {
				double _x = x;
				double _y = y;
				double _z = z;
				return (float)(_x*_x+_y*_y+_z*_z);
			}
		}

		/** The squared magnitude of the vector */
		public long sqrMagnitudeLong {
			get {
				long _x = x;
				long _y = y;
				long _z = z;
				return (_x*_x+_y*_y+_z*_z);
			}
		}

		public static implicit operator string(Int3 obj) {
			return obj.ToString();
		}

		/** Returns a nicely formatted string representing the vector */
		public override string ToString () {
			return "( "+x+", "+y+", "+z+")";
		}

		public override bool Equals (System.Object obj) {
			if (obj == null) return false;

			var rhs = (Int3)obj;

			return x == rhs.x &&
				   y == rhs.y &&
				   z == rhs.z;
		}

		#region IEquatable implementation

		public bool Equals (Int3 other) {
			return x == other.x && y == other.y && z == other.z;
		}

		#endregion

		public override int GetHashCode () {
			return x*73856093 ^ y*19349663 ^ z*83492791;
		}
	}

	/** Two Dimensional Integer Coordinate Pair */
	public struct Int2 : System.IEquatable<Int2> {
		public int x;
		public int y;

		public Int2 (int x, int y) {
			this.x = x;
			this.y = y;
		}

		public long sqrMagnitudeLong {
			get {
				return (long)x*(long)x+(long)y*(long)y;
			}
		}

		public static Int2 operator + (Int2 a, Int2 b) {
			return new Int2(a.x+b.x, a.y+b.y);
		}

		public static Int2 operator - (Int2 a, Int2 b) {
			return new Int2(a.x-b.x, a.y-b.y);
		}

		public static bool operator == (Int2 a, Int2 b) {
			return a.x == b.x && a.y == b.y;
		}

		public static bool operator != (Int2 a, Int2 b) {
			return a.x != b.x || a.y != b.y;
		}

		/** Dot product of the two coordinates */
		public static long DotLong (Int2 a, Int2 b) {
			return (long)a.x*(long)b.x + (long)a.y*(long)b.y;
		}

		public override bool Equals (System.Object o) {
			if (o == null) return false;
			var rhs = (Int2)o;

			return x == rhs.x && y == rhs.y;
		}

		#region IEquatable implementation

		public bool Equals (Int2 other) {
			return x == other.x && y == other.y;
		}

		#endregion

		public override int GetHashCode () {
			return x*49157+y*98317;
		}

		/** Matrices for rotation.
		 * Each group of 4 elements is a 2x2 matrix.
		 * The XZ position is multiplied by this.
		 * So
		 * \code
		 * //A rotation by 90 degrees clockwise, second matrix in the array
		 * (5,2) * ((0, 1), (-1, 0)) = (2,-5)
		 * \endcode
		 */
		private static readonly int[] Rotations = {
			1, 0,  //Identity matrix
			0, 1,

			0, 1,
			-1, 0,

			-1, 0,
			0, -1,

			0, -1,
			1, 0
		};

		/** Returns a new Int2 rotated 90*r degrees around the origin.
		 * \deprecated Deprecated becuase it is not used by any part of the A* Pathfinding Project
		 */
		[System.Obsolete("Deprecated becuase it is not used by any part of the A* Pathfinding Project")]
		public static Int2 Rotate (Int2 v, int r) {
			r = r % 4;
			return new Int2(v.x*Rotations[r*4+0] + v.y*Rotations[r*4+1], v.x*Rotations[r*4+2] + v.y*Rotations[r*4+3]);
		}

		public static Int2 Min (Int2 a, Int2 b) {
			return new Int2(System.Math.Min(a.x, b.x), System.Math.Min(a.y, b.y));
		}

		public static Int2 Max (Int2 a, Int2 b) {
			return new Int2(System.Math.Max(a.x, b.x), System.Math.Max(a.y, b.y));
		}

		public static Int2 FromInt3XZ (Int3 o) {
			return new Int2(o.x, o.z);
		}

		public static Int3 ToInt3XZ (Int2 o) {
			return new Int3(o.x, 0, o.y);
		}

		public override string ToString () {
			return "("+x+", " +y+")";
		}
	}
}
