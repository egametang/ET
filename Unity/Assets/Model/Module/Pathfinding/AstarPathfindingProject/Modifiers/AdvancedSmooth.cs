using System;
using System.Collections.Generic;
using PF;
using UnityEngine;
using Mathf = UnityEngine.Mathf;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Advanced Smooth")]
	[System.Serializable]
	/** \ingroup modifiers
	 * Smoothing by dividing path into turns and straight segments.
	 *
	 * \astarpro */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_advanced_smooth.php")]
	public class AdvancedSmooth : MonoModifier {
		public override int Order { get { return 40; } }

		public float turningRadius = 1.0F;

		public MaxTurn turnConstruct1 = new MaxTurn();
		public ConstantTurn turnConstruct2 = new ConstantTurn();

		public override void Apply (Path p) {
			PF.Vector3[] vectorPath = p.vectorPath.ToArray();

			if (vectorPath == null || vectorPath.Length <= 2) {
				return;
			}

			List<PF.Vector3> newPath = new List<PF.Vector3>();
			newPath.Add(vectorPath[0]);

			TurnConstructor.turningRadius = turningRadius;

			for (int i = 1; i < vectorPath.Length-1; i++) {
				List<Turn> turnList = new List<Turn>();

				TurnConstructor.Setup(i, vectorPath);
				turnConstruct1.Prepare(i, vectorPath);
				turnConstruct2.Prepare(i, vectorPath);

				TurnConstructor.PostPrepare();

				if (i == 1) {
					turnConstruct1.PointToTangent(turnList);
					turnConstruct2.PointToTangent(turnList);
				} else {
					turnConstruct1.TangentToTangent(turnList);
					turnConstruct2.TangentToTangent(turnList);
				}

				EvaluatePaths(turnList, newPath);

				//Last point
				if (i == vectorPath.Length-2) {
					turnConstruct1.TangentToPoint(turnList);
					turnConstruct2.TangentToPoint(turnList);
				}

				EvaluatePaths(turnList, newPath);
			}

			newPath.Add(vectorPath[vectorPath.Length-1]);
			p.vectorPath = newPath;
		}

		void EvaluatePaths (List<Turn> turnList, List<PF.Vector3> output) {
			turnList.Sort();

			for (int j = 0; j < turnList.Count; j++) {
				if (j == 0) {
					turnList[j].GetPath(output);
				}
			}

			turnList.Clear();

			if (TurnConstructor.changedPreviousTangent) {
				turnConstruct1.OnTangentUpdate();
				turnConstruct2.OnTangentUpdate();
			}
		}

		[System.Serializable]
		/** Type of turn.
		 *  \astarpro */
		public class MaxTurn : TurnConstructor {
			Vector3 preRightCircleCenter = Vector3.zero;
			Vector3 preLeftCircleCenter = Vector3.zero;

			Vector3 rightCircleCenter;
			Vector3 leftCircleCenter;

			double vaRight, vaLeft, preVaLeft, preVaRight;

			double gammaLeft, gammaRight;

			double betaRightRight, betaRightLeft, betaLeftRight, betaLeftLeft;

			double deltaRightLeft, deltaLeftRight;

			double alfaRightRight, alfaLeftLeft, alfaRightLeft, alfaLeftRight;

			public override void OnTangentUpdate () {
				rightCircleCenter = current + normal * turningRadius;
				leftCircleCenter  = current - normal * turningRadius;

				vaRight = Atan2(current-rightCircleCenter);
				vaLeft = vaRight + Math.PI;
			}

			public override void Prepare (int i, PF.Vector3[] vectorPath) {
				preRightCircleCenter = rightCircleCenter;
				preLeftCircleCenter = leftCircleCenter;

				rightCircleCenter = current + normal * turningRadius;
				leftCircleCenter = current - normal * turningRadius;

				preVaRight =  vaRight;
				preVaLeft = vaLeft;

				vaRight = Atan2(current-rightCircleCenter);
				vaLeft = vaRight + Math.PI;
			}

			public override void TangentToTangent (List<Turn> turnList) {
				alfaRightRight = Atan2(rightCircleCenter - preRightCircleCenter); // + Math.PI*0.5; //Angle tangent to the angle the previous circle (from the current circle)
				alfaLeftLeft = Atan2(leftCircleCenter - preLeftCircleCenter); // + Math.PI*0.5;
				alfaRightLeft = Atan2(leftCircleCenter - preRightCircleCenter); // + Math.PI*0.5; //RightLeft means: from the previous right circle to the current left circle
				alfaLeftRight = Atan2(rightCircleCenter - preLeftCircleCenter); // + Math.PI*0.5; //LeftRight means: from the previous left circle to the current right circle

				double magnRightLeft = (leftCircleCenter - preRightCircleCenter).magnitude;
				double magnLeftRight = (rightCircleCenter - preLeftCircleCenter).magnitude;

				bool noRightLeft = false;
				bool noLeftRight = false;

				//Discard RightLeft and LeftRight paths if the circles lie to close to each other
				if (magnRightLeft < turningRadius*2) {
					magnRightLeft = turningRadius*2;
					noRightLeft = true;
				}

				if (magnLeftRight < turningRadius*2) {
					magnLeftRight = turningRadius*2;
					noLeftRight = true;
				}

				deltaRightLeft = noRightLeft ? 0 : (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius*2 / magnRightLeft);  //turn*2 should be r1 + r2 for circles with different radiis
				deltaLeftRight = noLeftRight ? 0 : (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius*2 / magnLeftRight);  //turn*2 should be r1 + r2 for circles with different radiis


				//Length for the first turn
				betaRightRight = ClockwiseAngle(preVaRight, alfaRightRight - ThreeSixtyRadians*0.25);              // ThreeSixtyRadians * 0.25 = 90 degrees
				betaRightLeft = ClockwiseAngle(preVaRight, alfaRightLeft  - deltaRightLeft);
				betaLeftRight = CounterClockwiseAngle(preVaLeft, alfaLeftRight  + deltaLeftRight);
				betaLeftLeft = CounterClockwiseAngle(preVaLeft, alfaLeftLeft   + ThreeSixtyRadians*0.25);


				//Add length for the second turn
				betaRightRight += ClockwiseAngle(alfaRightRight - ThreeSixtyRadians*0.25, vaRight);
				betaRightLeft += CounterClockwiseAngle(alfaRightLeft  + deltaRightLeft, vaLeft);
				betaLeftRight += ClockwiseAngle(alfaLeftRight  - deltaLeftRight, vaRight);
				betaLeftLeft  += CounterClockwiseAngle(alfaLeftLeft   + ThreeSixtyRadians*0.25, vaLeft);

				betaRightRight = GetLengthFromAngle(betaRightRight, turningRadius);
				betaRightLeft = GetLengthFromAngle(betaRightLeft, turningRadius);
				betaLeftRight = GetLengthFromAngle(betaLeftRight, turningRadius);
				betaLeftLeft = GetLengthFromAngle(betaLeftLeft, turningRadius);

				Vector3
					pRightRight1, pRightRight2,
					pRightLeft1, pRightLeft2,
					pLeftRight1, pLeftRight2,
					pLeftLeft1, pLeftLeft2;

				//Debug.Log ("=== DELTA VALUES===\nRightLeft "+ToDegrees (deltaRightLeft)+" - LeftRight "+ToDegrees (deltaLeftRight));
				//Set up points where the straigh segments starts and ends (between the turns)
				pRightRight1 =  AngleToVector(alfaRightRight - ThreeSixtyRadians*0.25)*turningRadius + preRightCircleCenter;
				pRightLeft1  =  AngleToVector(alfaRightLeft  - deltaRightLeft)*turningRadius + preRightCircleCenter;
				pLeftRight1  =  AngleToVector(alfaLeftRight  + deltaLeftRight)*turningRadius + preLeftCircleCenter;
				pLeftLeft1   =  AngleToVector(alfaLeftLeft   + ThreeSixtyRadians*0.25)*turningRadius + preLeftCircleCenter;

				pRightRight2 =  AngleToVector(alfaRightRight - ThreeSixtyRadians*0.25)*turningRadius + rightCircleCenter;
				pRightLeft2  =  AngleToVector(alfaRightLeft  - deltaRightLeft + Math.PI)*turningRadius + leftCircleCenter;
				pLeftRight2  =  AngleToVector(alfaLeftRight  + deltaLeftRight + Math.PI)*turningRadius + rightCircleCenter;
				pLeftLeft2   =  AngleToVector(alfaLeftLeft   + ThreeSixtyRadians*0.25)*turningRadius + leftCircleCenter;
				betaRightRight += (pRightRight1 - pRightRight2).magnitude;
				betaRightLeft += (pRightLeft1  - pRightLeft2).magnitude;
				betaLeftRight += (pLeftRight1  - pLeftRight2).magnitude;
				betaLeftLeft  += (pLeftLeft1   - pLeftLeft2).magnitude;

				if (noRightLeft) {
					betaRightLeft += 10000000;
				}
				if (noLeftRight) {
					betaLeftRight += 10000000;
				}

				turnList.Add(new Turn((float)betaRightRight, this, 2));
				turnList.Add(new Turn((float)betaRightLeft, this, 3));
				turnList.Add(new Turn((float)betaLeftRight, this, 4));
				turnList.Add(new Turn((float)betaLeftLeft, this, 5));
			}

			public override void PointToTangent (List<Turn> turnList) {
				bool noRight = false, noLeft = false;

				float rightMagn = (prev-rightCircleCenter).magnitude;
				float leftMagn  = (prev-leftCircleCenter).magnitude;

				if (rightMagn < turningRadius)
					noRight = true;
				if (leftMagn < turningRadius)
					noLeft = true;

				double alfa = noRight  ? 0 : Atan2(prev-rightCircleCenter);
				double delta = noRight ? 0 : (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius / (prev-rightCircleCenter).magnitude);

				//Angle to the point where turning ends on the right circle
				gammaRight = alfa + delta;

				double betaRight = noRight ? 0 : ClockwiseAngle(gammaRight, vaRight);

				double alfaLeft =  noLeft ? 0 : Atan2(prev-leftCircleCenter);
				double deltaLeft = noLeft ? 0 : (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius / (prev-leftCircleCenter).magnitude);

				//Angle to the point where turning ends
				gammaLeft = alfaLeft - deltaLeft;

				double betaLeft = noLeft ? 0 : CounterClockwiseAngle(gammaLeft, vaLeft);

				if (!noRight)
					turnList.Add(new Turn((float)betaRight, this, 0));
				if (!noLeft)
					turnList.Add(new Turn((float)betaLeft, this, 1));
			}

			public override void TangentToPoint (List<Turn> turnList) {
				bool noRight = false, noLeft = false;

				float rightMagn = (next-rightCircleCenter).magnitude;
				float leftMagn  = (next-leftCircleCenter).magnitude;

				if (rightMagn < turningRadius)
					noRight = true;
				if (leftMagn < turningRadius)
					noLeft = true;

				if (!noRight) {
					double alfa = Atan2(next-rightCircleCenter);
					double delta = (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius / rightMagn);

					//Angle to the point where turning ends on the right circle
					gammaRight = alfa - delta;
					double betaRight = ClockwiseAngle(vaRight, gammaRight);

					turnList.Add(new Turn((float)betaRight, this, 6));
				}

				if (!noLeft) {
					double alfaLeft = Atan2(next-leftCircleCenter);
					double deltaLeft = (ThreeSixtyRadians * 0.25) - Math.Asin(turningRadius / leftMagn);

					//Angle to the point where turning ends
					gammaLeft = alfaLeft + deltaLeft;

					double betaLeft = CounterClockwiseAngle(vaLeft, gammaLeft);


					turnList.Add(new Turn((float)betaLeft, this, 7));
				}
			}

			public override void GetPath (Turn turn, List<PF.Vector3> output) {
				switch (turn.id) {
				case 0:
					//Right - Point to tangent
					AddCircleSegment(gammaRight, vaRight, true, rightCircleCenter, output, turningRadius);
					break;
				case 1:
					//Left - Point to tangent
					AddCircleSegment(gammaLeft, vaLeft, false, leftCircleCenter, output, turningRadius);
					break;
				case 2:
					//Right Right - Tangent to tangent
					AddCircleSegment(preVaRight, alfaRightRight - ThreeSixtyRadians*0.25, true, preRightCircleCenter, output, turningRadius);
					AddCircleSegment(alfaRightRight - ThreeSixtyRadians*0.25, vaRight, true, rightCircleCenter, output, turningRadius);
					break;
				case 3:
					//Right Left - Tangent to tangent
					AddCircleSegment(preVaRight, alfaRightLeft  - deltaRightLeft, true, preRightCircleCenter, output, turningRadius);
					AddCircleSegment(alfaRightLeft  - deltaRightLeft + Math.PI, vaLeft, false, leftCircleCenter, output, turningRadius);
					break;
				case 4:
					//Left Right - Tangent to tangent
					AddCircleSegment(preVaLeft, alfaLeftRight  + deltaLeftRight, false, preLeftCircleCenter, output, turningRadius);
					AddCircleSegment(alfaLeftRight  + deltaLeftRight + Math.PI, vaRight, true, rightCircleCenter, output, turningRadius);
					break;
				case 5:
					//Left Left - Tangent to tangent
					AddCircleSegment(preVaLeft, alfaLeftLeft   + ThreeSixtyRadians*0.25, false, preLeftCircleCenter, output, turningRadius);
					AddCircleSegment(alfaLeftLeft   + ThreeSixtyRadians*0.25, vaLeft, false, leftCircleCenter, output, turningRadius);
					break;
				case 6:
					//Right - Tangent to point
					AddCircleSegment(vaRight, gammaRight, true, rightCircleCenter, output, turningRadius);
					break;
				case 7:
					//Left - Tangent to point
					AddCircleSegment(vaLeft, gammaLeft, false, leftCircleCenter, output,  turningRadius);
					break;
				}
			}
		}

		[System.Serializable]
		/** Constant turning speed.
		 *  \astarpro */
		public class ConstantTurn : TurnConstructor {
			Vector3 circleCenter;
			double gamma1;
			double gamma2;

			bool clockwise;

			public override void Prepare (int i, PF.Vector3[] vectorPath) {}

			public override void TangentToTangent (List<Turn> turnList) {
				Vector3 preNormal = Vector3.Cross(t1, Vector3.up);

				Vector3 dir = (current-prev);
				Vector3 pos = dir*0.5F + prev;

				dir = Vector3.Cross(dir, Vector3.up);

				bool didIntersect;
				circleCenter = VectorMath.LineDirIntersectionPointXZ(prev, preNormal, pos, dir, out didIntersect);

				if (!didIntersect) {
					return;
				}

				gamma1 = Atan2(prev-circleCenter);
				gamma2 = Atan2(current-circleCenter);

				clockwise = !VectorMath.RightOrColinearXZ(circleCenter, prev, prev+t1);

				double beta = clockwise ? ClockwiseAngle(gamma1, gamma2) : CounterClockwiseAngle(gamma1, gamma2);

				beta = GetLengthFromAngle(beta, (circleCenter - current).magnitude);

				turnList.Add(new Turn((float)beta, this, 0));
			}

			public override void GetPath (Turn turn, List<PF.Vector3> output) {
				AddCircleSegment(gamma1, gamma2, clockwise, circleCenter, output, (circleCenter - current).magnitude);

				normal = (current - circleCenter).normalized;
				t2 = Vector3.Cross(normal, Vector3.up).normalized;
				normal = -normal;

				if (!clockwise) {
					t2 = -t2;
					normal = -normal;
				}

				changedPreviousTangent = true;
			}
		}

		/** Abstract turn constructor.
		 *  \astarpro */
		public abstract class TurnConstructor {
			/** Constant bias to add to the path lengths.
			 * This can be used to favor certain turn types before others.\n
			 * By for example setting this to -5, paths from this path constructor will be chosen
			 * if there are no other paths more than 5 world units shorter than this one (as opposed to just any shorter path) */
			public float constantBias = 0;

			/** Bias to multiply the path lengths with. This can be used to favor certain turn types before others.
			 * \see #constantBias */
			public float factorBias =   1;

			public static float turningRadius = 1.0F;

			public const double ThreeSixtyRadians = Math.PI * 2;

			public static Vector3 prev, current, next; //The current points
			public static Vector3 t1, t2; //The current tangents - t2 is at 'current', t1 is at 'prev'
			public static Vector3 normal, prevNormal; //Normal at 'current'

			public static bool changedPreviousTangent = false;

			public abstract void Prepare (int i, PF.Vector3[] vectorPath);
			public virtual void  OnTangentUpdate () {}
			public virtual void  PointToTangent (List<Turn> turnList) {}
			public virtual void  TangentToPoint (List<Turn> turnList) {}
			public virtual void TangentToTangent (List<Turn> turnList) {}
			public abstract void GetPath (Turn turn, List<PF.Vector3> output);
			//abstract void Evaluate (Turn turn);

			public static void Setup (int i, PF.Vector3[] vectorPath) {
				current = vectorPath[i];
				prev = vectorPath[i-1];
				next = vectorPath[i+1];

				prev.y = current.y;
				next.y = current.y;

				t1 = t2;

				t2 = (next-current).normalized - (prev-current).normalized;
				t2 = t2.normalized;

				prevNormal = normal;

				normal = Vector3.Cross(t2, Vector3.up);
				normal = normal.normalized;
			}

			public static void PostPrepare () {
				changedPreviousTangent = false;
			}

			//Utilities

			public void AddCircleSegment (double startAngle, double endAngle, bool clockwise, Vector3 center, List<PF.Vector3> output, float radius) {
				double step = ThreeSixtyRadians / 100;

				if (clockwise) {
					while (endAngle > startAngle+ThreeSixtyRadians) {
						endAngle -= ThreeSixtyRadians;
					}

					while (endAngle < startAngle) {
						endAngle += ThreeSixtyRadians;
					}
				} else {
					while (endAngle < startAngle-ThreeSixtyRadians) {
						endAngle += ThreeSixtyRadians;
					}

					while (endAngle > startAngle) {
						endAngle -= ThreeSixtyRadians;
					}
				}

				//Add curve
				if (clockwise) {
					for (double i = startAngle; i < endAngle; i += step) {
						output.Add(AngleToVector(i)*radius+center);
					}
				} else {
					for (double i = startAngle; i > endAngle; i -= step) {
						output.Add(AngleToVector(i)*radius+center);
					}
				}

				//Add last point
				output.Add(AngleToVector(endAngle)*radius+center);
			}

			public void DebugCircleSegment (Vector3 center, double startAngle, double endAngle, double radius, Color color) {
				double step = ThreeSixtyRadians / 100;

				while (endAngle < startAngle) {
					endAngle += ThreeSixtyRadians;
				}

				Vector3 prev = AngleToVector(startAngle)*(float)radius+center;
				for (double i = startAngle+step; i < endAngle; i += step) {
					Debug.DrawLine(prev, AngleToVector(i)*(float)radius+center);
				}

				Debug.DrawLine(prev, AngleToVector(endAngle)*(float)radius+center);
			}

			public void DebugCircle (Vector3 center, double radius, Color color) {
				double step = ThreeSixtyRadians / 100;
				Vector3 prePos = AngleToVector(-step)*(float)radius+center;

				for (double i = 0; i < ThreeSixtyRadians; i += step) {
					Vector3 pos = AngleToVector(i)*(float)radius+center;
					Debug.DrawLine(prePos, pos, color);
					prePos = pos;
				}
			}

			/** Returns the length of an circular arc with a radius and angle. Angle is specified in radians */
			public double GetLengthFromAngle (double angle, double radius) {
				return radius * angle;
			}

			/** Returns the angle between \a from and \a to in a clockwise direction */
			public double ClockwiseAngle (double from, double to) {
				return ClampAngle(to - from);
			}

			/** Returns the angle between \a from and \a to in a counter-clockwise direction */
			public double CounterClockwiseAngle (double from, double to) {
				return ClampAngle(from - to);
			}

			public Vector3 AngleToVector (double a) {
				return new Vector3((float)Math.Cos(a), 0, (float)Math.Sin(a));
			}

			public double ToDegrees (double rad) {
				return rad * Mathf.Rad2Deg;
			}

			public double ClampAngle (double a) {
				while (a < 0) { a += ThreeSixtyRadians; }
				while (a > ThreeSixtyRadians) { a -= ThreeSixtyRadians; }
				return a;
			}

			public double Atan2 (Vector3 v) {
				return Math.Atan2(v.z, v.x);
			}
		}

		//Turn class
		/** Represents a turn in a path.
		 *  \astarpro */
		public struct Turn : IComparable<Turn> {
			public float length;
			public int id;

			public TurnConstructor constructor;

			public float score {
				get {
					return length*constructor.factorBias+constructor.constantBias;
				}
			}

			public Turn (float length, TurnConstructor constructor, int id = 0) {
				this.length = length;
				this.id = id;
				this.constructor = constructor;
			}

			public void GetPath (List<PF.Vector3> output) {
				constructor.GetPath(this, output);
			}

			public int CompareTo (Turn t) {
				return t.score > score ? -1 : (t.score < score ? 1 : 0);
			}

			public static bool operator < (Turn lhs, Turn rhs) {
				return lhs.score < rhs.score;
			}

			public static bool operator > (Turn lhs, Turn rhs) {
				return lhs.score > rhs.score;
			}
		}
	}
}
