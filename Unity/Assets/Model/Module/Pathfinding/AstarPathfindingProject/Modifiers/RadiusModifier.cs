using UnityEngine;
using System;
using System.Collections.Generic;
using PF;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	/** Radius path modifier for offsetting paths.
	 * \ingroup modifiers
	 *
	 * The radius modifier will offset the path to create the effect
	 * of adjusting it to the characters radius.
	 * It gives good results on navmeshes which have not been offset with the
	 * character radius during scan. Especially useful when characters with different
	 * radiuses are used on the same navmesh. It is also useful when using
	 * rvo local avoidance with the RVONavmesh since the RVONavmesh assumes the
	 * navmesh has not been offset with the character radius.
	 *
	 * This modifier assumes all paths are in the XZ plane (i.e Y axis is up).
	 *
	 * It is recommended to use the Funnel Modifier on the path as well.
	 *
	 * \shadowimage{radius_modifier.png}
	 *
	 * \see RVONavmesh
	 * \see modifiers
	 *
	 * Also check out the howto page "Using Modifiers".
	 *
	 * \astarpro
	 *
	 * \since Added in 3.2.6
	 */
	[AddComponentMenu("Pathfinding/Modifiers/Radius Offset")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_radius_modifier.php")]
	public class RadiusModifier : MonoModifier {
		#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Radius Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(RadiusModifier));
		}
		#endif

		public override int Order { get { return 41; } }

		/** Radius of the circle segments generated.
		 * Usually similar to the character radius.
		 */
		public float radius = 1f;

		/** Detail of generated circle segments.
		 * Measured as steps per full circle.
		 *
		 * It is more performant to use a low value.
		 * For movement, using a high value will barely improve path quality.
		 */
		public float detail = 10;

		/** Calculates inner tangents for a pair of circles.
		 * \param p1 Position of first circle
		 * \param p2 Position of the second circle
		 * \param r1 Radius of the first circle
		 * \param r2 Radius of the second circle
		 * \param a Angle from the line joining the centers of the circles to the inner tangents.
		 * \param sigma World angle from p1 to p2 (in XZ space)
		 *
		 * Add \a a to \a sigma to get the first tangent angle, subtract \a a from \a sigma to get the second tangent angle.
		 *
		 * \returns True on success. False when the circles are overlapping.
		 */
		bool CalculateCircleInner (Vector3 p1, Vector3 p2, float r1, float r2, out float a, out float sigma) {
			float dist = (p1-p2).magnitude;

			if (r1+r2 > dist) {
				a = 0;
				sigma = 0;
				return false;
			}

			a = (float)Math.Acos((r1+r2)/dist);

			sigma = (float)Math.Atan2(p2.z-p1.z, p2.x-p1.x);
			return true;
		}

		/** Calculates outer tangents for a pair of circles.
		 * \param p1 Position of first circle
		 * \param p2 Position of the second circle
		 * \param r1 Radius of the first circle
		 * \param r2 Radius of the second circle
		 * \param a Angle from the line joining the centers of the circles to the inner tangents.
		 * \param sigma World angle from p1 to p2 (in XZ space)
		 *
		 * Add \a a to \a sigma to get the first tangent angle, subtract \a a from \a sigma to get the second tangent angle.
		 *
		 * \returns True on success. False on failure (more specifically when |r1-r2| > |p1-p2| )
		 */
		bool CalculateCircleOuter (Vector3 p1, Vector3 p2, float r1, float r2, out float a, out float sigma) {
			float dist = (p1-p2).magnitude;

			if (Math.Abs(r1 - r2) > dist) {
				a = 0;
				sigma = 0;
				return false;
			}
			a = (float)Math.Acos((r1-r2)/dist);
			sigma = (float)Math.Atan2(p2.z-p1.z, p2.x-p1.x);
			return true;
		}

		[System.Flags]
		enum TangentType {
			OuterRight = 1 << 0,
			InnerRightLeft = 1 << 1,
			InnerLeftRight = 1 << 2,
			OuterLeft = 1 << 3,
			Outer = OuterRight | OuterLeft,
			Inner = InnerRightLeft | InnerLeftRight
		}

		TangentType CalculateTangentType (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
			bool l1 = VectorMath.RightOrColinearXZ(p1, p2, p3);
			bool l2 = VectorMath.RightOrColinearXZ(p2, p3, p4);

			return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
		}

		TangentType CalculateTangentTypeSimple (Vector3 p1, Vector3 p2, Vector3 p3) {
			bool l2 = VectorMath.RightOrColinearXZ(p1, p2, p3);
			bool l1 = l2;

			return (TangentType)(1 << ((l1 ? 2 : 0) + (l2 ? 1 : 0)));
		}

		public override void Apply (Path p) {
			List<PF.Vector3> vs = p.vectorPath;

			List<PF.Vector3> res = Apply(vs);

			if (res != vs) {
				ListPool<PF.Vector3>.Release(ref p.vectorPath);
				p.vectorPath = res;
			}
		}

		float[] radi = new float[10];
		float[] a1 = new float[10];
		float[] a2 = new float[10];
		bool[] dir = new bool[10];

		/** Apply this modifier on a raw Vector3 list */
		public List<PF.Vector3> Apply (List<PF.Vector3> vs) {
			if (vs == null || vs.Count < 3) return vs;

			/** \todo Do something about these allocations */
			if (radi.Length < vs.Count) {
				radi = new float[vs.Count];
				a1 = new float[vs.Count];
				a2 = new float[vs.Count];
				dir = new bool[vs.Count];
			}

			for (int i = 0; i < vs.Count; i++) {
				radi[i] = radius;
			}

			radi[0] = 0;
			radi[vs.Count-1] = 0;

			int count = 0;
			for (int i = 0; i < vs.Count-1; i++) {
				count++;
				if (count > 2*vs.Count) {
					Debug.LogWarning("Could not resolve radiuses, the path is too complex. Try reducing the base radius");
					break;
				}

				TangentType tt;

				if (i == 0) {
					tt = CalculateTangentTypeSimple(vs[i], vs[i+1], vs[i+2]);
				} else if (i == vs.Count-2) {
					tt = CalculateTangentTypeSimple(vs[i-1], vs[i], vs[i+1]);
				} else {
					tt = CalculateTangentType(vs[i-1], vs[i], vs[i+1], vs[i+2]);
				}

				//DrawCircle (vs[i], radi[i], Color.yellow);

				if ((tt & TangentType.Inner) != 0) {
					//Angle to tangent
					float a;
					//Angle to other circle
					float sigma;

					//Calculate angles to the next circle and angles for the inner tangents
					if (!CalculateCircleInner(vs[i], vs[i+1], radi[i], radi[i+1], out a, out sigma)) {
						//Failed, try modifying radiuses
						float magn = (vs[i+1]-vs[i]).magnitude;
						radi[i] = magn*(radi[i]/(radi[i]+radi[i+1]));
						radi[i+1] = magn - radi[i];
						radi[i] *= 0.99f;
						radi[i+1] *= 0.99f;
						i -= 2;
						continue;
					}

					if (tt == TangentType.InnerRightLeft) {
						a2[i] = sigma-a;
						a1[i+1] = sigma-a + (float)Math.PI;
						dir[i] = true;
					} else {
						a2[i] = sigma+a;
						a1[i+1] = sigma+a + (float)Math.PI;
						dir[i] = false;
					}
				} else {
					float sigma;
					float a;

					//Calculate angles to the next circle and angles for the outer tangents
					if (!CalculateCircleOuter(vs[i], vs[i+1], radi[i], radi[i+1], out a, out sigma)) {
						//Failed, try modifying radiuses
						if (i == vs.Count-2) {
							//The last circle has a fixed radius at 0, don't modify it
							radi[i] = (vs[i+1]-vs[i]).magnitude;
							radi[i] *= 0.99f;
							i -= 1;
						} else {
							if (radi[i] > radi[i+1]) {
								radi[i+1] = radi[i] - (vs[i+1]-vs[i]).magnitude;
							} else {
								radi[i+1] = radi[i] + (vs[i+1]-vs[i]).magnitude;
							}
							radi[i+1] *= 0.99f;
						}



						i -= 1;
						continue;
					}

					if (tt == TangentType.OuterRight) {
						a2[i] = sigma-a;
						a1[i+1] = sigma-a;
						dir[i] = true;
					} else {
						a2[i] = sigma+a;
						a1[i+1] = sigma+a;
						dir[i] = false;
					}
				}
			}

			List<PF.Vector3> res = ListPool<PF.Vector3>.Claim();
			res.Add(vs[0]);
			if (detail < 1) detail = 1;
			float step = (float)(2*Math.PI)/detail;
			for (int i = 1; i < vs.Count-1; i++) {
				float start = a1[i];
				float end = a2[i];
				float rad = radi[i];

				if (dir[i]) {
					if (end < start) end += (float)Math.PI*2;
					for (float t = start; t < end; t += step) {
						res.Add(new Vector3((float)Math.Cos(t), 0, (float)Math.Sin(t)).ToPFV3()*rad + vs[i]);
					}
				} else {
					if (start < end) start += (float)Math.PI*2;
					for (float t = start; t > end; t -= step) {
						res.Add(new Vector3((float)Math.Cos(t), 0, (float)Math.Sin(t)).ToPFV3()*rad + vs[i]);
					}
				}
			}

			res.Add(vs[vs.Count-1]);

			return res;
		}
	}
}
