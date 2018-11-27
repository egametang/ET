using UnityEngine;
using System.Collections.Generic;

using Pathfinding.Util;
using PF;
using Mathf = UnityEngine.Mathf;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Simple Smooth")]
	[System.Serializable]
	[RequireComponent(typeof(Seeker))]
	/** Modifier which smooths the path. This modifier can smooth a path by either moving the points closer together (Simple) or using Bezier curves (Bezier).\n
	 * \ingroup modifiers
	 * Attach this component to the same GameObject as a Seeker component.
	 * \n
	 * This component will hook in to the Seeker's path post-processing system and will post process any paths it searches for.
	 * Take a look at the Modifier Priorities settings on the Seeker component to determine where in the process this modifier should process the path.
	 * \n
	 * \n
	 * Several smoothing types are available, here follows a list of them and a short description of what they do, and how they work.
	 * But the best way is really to experiment with them yourself.\n
	 *
	 * - <b>Simple</b> Smooths the path by drawing all points close to each other. This results in paths that might cut corners if you are not careful.
	 * It will also subdivide the path to create more more points to smooth as otherwise it would still be quite rough.
	 * \shadowimage{smooth_simple.png}
	 * - <b>Bezier</b> Smooths the path using Bezier curves. This results a smooth path which will always pass through all points in the path, but make sure it doesn't turn too quickly.
	 * \shadowimage{smooth_bezier.png}
	 * - <b>OffsetSimple</b> An alternative to Simple smooth which will offset the path outwards in each step to minimize the corner-cutting.
	 * But be careful, if too high values are used, it will turn into loops and look really ugly.
	 * - <b>Curved Non Uniform</b> \shadowimage{smooth_curved_nonuniform.png}
	 *
	 * \note Modifies vectorPath array
	 * \todo Make the smooth modifier take the world geometry into account when smoothing
	 * */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_simple_smooth_modifier.php")]
	public class SimpleSmoothModifier : MonoModifier {
	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Simple Smooth Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(SimpleSmoothModifier));
		}
	#endif

		public override int Order { get { return 50; } }

		/** Type of smoothing to use */
		public SmoothType smoothType = SmoothType.Simple;

		/** Number of times to subdivide when not using a uniform length */
		[Tooltip("The number of times to subdivide (divide in half) the path segments. [0...inf] (recommended [1...10])")]
		public int subdivisions = 2;

		/** Number of times to apply smoothing */
		[Tooltip("Number of times to apply smoothing")]
		public int iterations = 2;

		/** Determines how much smoothing to apply in each smooth iteration. 0.5 usually produces the nicest looking curves. */
		[Tooltip("Determines how much smoothing to apply in each smooth iteration. 0.5 usually produces the nicest looking curves")]
		[Range(0, 1)]
		public float strength = 0.5F;

		/** Toggle to divide all lines in equal length segments.
		 * \see #maxSegmentLength
		 */
		[Tooltip("Toggle to divide all lines in equal length segments")]
		public bool uniformLength = true;

		/** The length of the segments in the smoothed path when using #uniformLength.
		 * A high value yields rough paths and low value yields very smooth paths, but is slower */
		[Tooltip("The length of each segment in the smoothed path. A high value yields rough paths and low value yields very smooth paths, but is slower")]
		public float maxSegmentLength = 2F;

		/** Length factor of the bezier curves' tangents' */
		[Tooltip("Length factor of the bezier curves' tangents")]
		public float bezierTangentLength = 0.4F;

		/** Offset to apply in each smoothing iteration when using Offset Simple. \see #smoothType */
		[Tooltip("Offset to apply in each smoothing iteration when using Offset Simple")]
		public float offset = 0.2F;

		/** Roundness factor used for CurvedNonuniform */
		[Tooltip("How much to smooth the path. A higher value will give a smoother path, but might take the character far off the optimal path.")]
		public float factor = 0.1F;

		public enum SmoothType {
			Simple,
			Bezier,
			OffsetSimple,
			CurvedNonuniform
		}

		public override void Apply (Path p) {
			// This should never trigger unless some other modifier has messed stuff up
			if (p.vectorPath == null) {
				Debug.LogWarning("Can't process NULL path (has another modifier logged an error?)");
				return;
			}

			List<PF.Vector3> path = null;

			switch (smoothType) {
			case SmoothType.Simple:
				path = SmoothSimple(p.vectorPath); break;
			case SmoothType.Bezier:
				path = SmoothBezier(p.vectorPath); break;
			case SmoothType.OffsetSimple:
				path = SmoothOffsetSimple(p.vectorPath); break;
			case SmoothType.CurvedNonuniform:
				path = CurvedNonuniform(p.vectorPath); break;
			}

			if (path != p.vectorPath) {
				ListPool<PF.Vector3>.Release(ref p.vectorPath);
				p.vectorPath = path;
			}
		}

		public List<PF.Vector3> CurvedNonuniform (List<PF.Vector3> path) {
			if (maxSegmentLength <= 0) {
				Debug.LogWarning("Max Segment Length is <= 0 which would cause DivByZero-exception or other nasty errors (avoid this)");
				return path;
			}

			int pointCounter = 0;
			for (int i = 0; i < path.Count-1; i++) {
				//pointCounter += Mathf.FloorToInt ((path[i]-path[i+1]).magnitude / maxSegmentLength)+1;

				float dist = (path[i]-path[i+1]).magnitude;
				//In order to avoid floating point errors as much as possible, and in lack of a better solution
				//loop through it EXACTLY as the other code further down will
				for (float t = 0; t <= dist; t += maxSegmentLength) {
					pointCounter++;
				}
			}

			List<PF.Vector3> subdivided = ListPool<PF.Vector3>.Claim(pointCounter);

			// Set first velocity
			Vector3 preEndVel = (path[1]-path[0]).normalized;

			for (int i = 0; i < path.Count-1; i++) {
				float dist = (path[i]-path[i+1]).magnitude;

				Vector3 startVel1 = preEndVel;
				Vector3 endVel1 = i < path.Count-2 ? ((path[i+2]-path[i+1]).normalized - (path[i]-path[i+1]).normalized).normalized : (path[i+1]-path[i]).normalized;

				Vector3 startVel = startVel1 * dist * factor;
				Vector3 endVel = endVel1 * dist * factor;

				Vector3 start = path[i];
				Vector3 end = path[i+1];

				float onedivdist = 1F / dist;

				for (float t = 0; t <= dist; t += maxSegmentLength) {
					float t2 = t * onedivdist;

					subdivided.Add(GetPointOnCubic(start, end, startVel, endVel, t2));
				}

				preEndVel = endVel1;
			}

			subdivided[subdivided.Count-1] = path[path.Count-1];

			return subdivided;
		}

		public static Vector3 GetPointOnCubic (Vector3 a, Vector3 b, Vector3 tan1, Vector3 tan2, float t) {
			float t2 = t*t, t3 = t2*t;

			float h1 =  2*t3 - 3*t2 + 1;          // calculate basis function 1
			float h2 = -2*t3 + 3*t2;              // calculate basis function 2
			float h3 =   t3 -  2*t2 + t;          // calculate basis function 3
			float h4 =   t3 -  t2;                // calculate basis function 4

			return h1*a +                            // multiply and sum all funtions
				   h2*b +                            // together to build the interpolated
				   h3*tan1 +                         // point along the curve.
				   h4*tan2;
		}

		public List<PF.Vector3> SmoothOffsetSimple (List<PF.Vector3> path) {
			if (path.Count <= 2 || iterations <= 0) {
				return path;
			}

			if (iterations > 12) {
				Debug.LogWarning("A very high iteration count was passed, won't let this one through");
				return path;
			}

			int maxLength = (path.Count-2)*(int)Mathf.Pow(2, iterations)+2;

			List<PF.Vector3> subdivided = ListPool<PF.Vector3>.Claim(maxLength);
			List<PF.Vector3> subdivided2 = ListPool<PF.Vector3>.Claim(maxLength);

			for (int i = 0; i < maxLength; i++) { subdivided.Add(Vector3.zero); subdivided2.Add(Vector3.zero); }

			for (int i = 0; i < path.Count; i++) {
				subdivided[i] = path[i];
			}

			for (int iteration = 0; iteration < iterations; iteration++) {
				int currentPathLength = (path.Count-2)*(int)Mathf.Pow(2, iteration)+2;

				//Switch the arrays
				List<PF.Vector3> tmp = subdivided;
				subdivided = subdivided2;
				subdivided2 = tmp;

				const float nextMultiplier = 1F;

				for (int i = 0; i < currentPathLength-1; i++) {
					Vector3 current = subdivided2[i];
					Vector3 next = subdivided2[i+1];

					Vector3 normal = Vector3.Cross(next-current, Vector3.up);
					normal = normal.normalized;

					bool firstRight = false;
					bool secondRight = false;
					bool setFirst = false;
					bool setSecond = false;
					if (i != 0 && !VectorMath.IsColinearXZ(current, next, subdivided2[i-1])) {
						setFirst = true;
						firstRight = VectorMath.RightOrColinearXZ(current, next, subdivided2[i-1]);
					}
					if (i < currentPathLength-1 && !VectorMath.IsColinearXZ(current, next, subdivided2[i+2])) {
						setSecond = true;
						secondRight = VectorMath.RightOrColinearXZ(current, next, subdivided2[i+2]);
					}

					if (setFirst) {
						subdivided[i*2] = current + (firstRight ? normal*offset*nextMultiplier : -normal*offset*nextMultiplier);
					} else {
						subdivided[i*2] = current;
					}

					if (setSecond) {
						subdivided[i*2+1] = next  + (secondRight ? normal*offset*nextMultiplier : -normal*offset*nextMultiplier);
					} else {
						subdivided[i*2+1] = next;
					}
				}

				subdivided[(path.Count-2)*(int)Mathf.Pow(2, iteration+1)+2-1] = subdivided2[currentPathLength-1];
			}

			ListPool<PF.Vector3>.Release(ref subdivided2);

			return subdivided;
		}

		public List<PF.Vector3> SmoothSimple (List<PF.Vector3> path) {
			if (path.Count < 2) return path;

			List<PF.Vector3> subdivided;

			if (uniformLength) {
				// Clamp to a small value to avoid the path being divided into a huge number of segments
				maxSegmentLength = Mathf.Max(maxSegmentLength, 0.005f);

				float pathLength = 0;
				for (int i = 0; i < path.Count-1; i++) {
					pathLength += Vector3.Distance(path[i], path[i+1]);
				}

				int estimatedNumberOfSegments = Mathf.FloorToInt(pathLength / maxSegmentLength);
				// Get a list with an initial capacity high enough so that we can add all points
				subdivided = ListPool<PF.Vector3>.Claim(estimatedNumberOfSegments+2);

				float distanceAlong = 0;

				// Sample points every [maxSegmentLength] world units along the path
				for (int i = 0; i < path.Count-1; i++) {
					var start = path[i];
					var end = path[i+1];

					float length = Vector3.Distance(start, end);

					while (distanceAlong < length) {
						subdivided.Add(Vector3.Lerp(start, end, distanceAlong / length));
						distanceAlong += maxSegmentLength;
					}

					distanceAlong -= length;
				}

				// Make sure we get the exact position of the last point
				subdivided.Add(path[path.Count-1]);
			} else {
				subdivisions = Mathf.Max(subdivisions, 0);

				if (subdivisions > 10) {
					Debug.LogWarning("Very large number of subdivisions. Cowardly refusing to subdivide every segment into more than " + (1 << subdivisions) + " subsegments");
					subdivisions = 10;
				}

				int steps = 1 << subdivisions;
				subdivided = ListPool<PF.Vector3>.Claim((path.Count-1)*steps + 1);
				Polygon.Subdivide(path, subdivided, steps);
			}

			if (strength > 0) {
				for (int it = 0; it < iterations; it++) {
					Vector3 prev = subdivided[0];

					for (int i = 1; i < subdivided.Count-1; i++) {
						Vector3 tmp = subdivided[i];

						// prev is at this point set to the value that subdivided[i-1] had before this loop started
						// Move the point closer to the average of the adjacent points
						subdivided[i] = Vector3.Lerp(tmp, (prev+subdivided[i+1].ToUnityV3())/2F, strength);

						prev = tmp;
					}
				}
			}

			return subdivided;
		}

		public List<PF.Vector3> SmoothBezier (List<PF.Vector3> path) {
			if (subdivisions < 0) subdivisions = 0;

			int subMult = 1 << subdivisions;
			List<PF.Vector3> subdivided = ListPool<PF.Vector3>.Claim();

			for (int i = 0; i < path.Count-1; i++) {
				Vector3 tangent1;
				Vector3 tangent2;
				if (i == 0) {
					tangent1 = path[i+1]-path[i];
				} else {
					tangent1 = path[i+1]-path[i-1];
				}

				if (i == path.Count-2) {
					tangent2 = path[i]-path[i+1];
				} else {
					tangent2 = path[i]-path[i+2];
				}

				tangent1 *= bezierTangentLength;
				tangent2 *= bezierTangentLength;

				Vector3 v1 = path[i];
				Vector3 v2 = v1+tangent1;
				Vector3 v4 = path[i+1];
				Vector3 v3 = v4+tangent2;

				for (int j = 0; j < subMult; j++) {
					subdivided.Add(AstarSplines.CubicBezier(v1, v2, v3, v4, (float)j/subMult));
				}
			}

			// Assign the last point
			subdivided.Add(path[path.Count-1]);

			return subdivided;
		}
	}
}
