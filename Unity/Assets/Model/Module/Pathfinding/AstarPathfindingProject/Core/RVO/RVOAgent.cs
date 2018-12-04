using UnityEngine;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding.RVO.Sampled {
	using Pathfinding;
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/** Internal agent for the RVO system.
	 * Usually you will interface with the IAgent interface instead.
	 *
	 * \see IAgent
	 */
	public class Agent : IAgent {
		//Current values for double buffer calculation

		internal float radius, height, desiredSpeed, maxSpeed, agentTimeHorizon, obstacleTimeHorizon;
		internal bool locked = false;

		RVOLayer layer, collidesWith;

		int maxNeighbours;
		internal Vector2 position;
		float elevationCoordinate;
		Vector2 currentVelocity;

		/** Desired target point - position */
		Vector2 desiredTargetPointInVelocitySpace;
		Vector2 desiredVelocity;

		Vector2 nextTargetPoint;
		float nextDesiredSpeed;
		float nextMaxSpeed;
		Vector2 collisionNormal;
		bool manuallyControlled;
		bool debugDraw;

		#region IAgent Properties

		/** \copydoc Pathfinding::RVO::IAgent::Position */
		public Vector2 Position { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::ElevationCoordinate */
		public float ElevationCoordinate { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::CalculatedTargetPoint */
		public Vector2 CalculatedTargetPoint { get; private set; }

		/** \copydoc Pathfinding::RVO::IAgent::CalculatedSpeed */
		public float CalculatedSpeed { get; private set; }

		/** \copydoc Pathfinding::RVO::IAgent::Locked */
		public bool Locked { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::Radius */
		public float Radius { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::Height */
		public float Height { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::AgentTimeHorizon */
		public float AgentTimeHorizon { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::ObstacleTimeHorizon */
		public float ObstacleTimeHorizon { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::MaxNeighbours */
		public int MaxNeighbours { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::NeighbourCount */
		public int NeighbourCount { get; private set; }

		/** \copydoc Pathfinding::RVO::IAgent::Layer */
		public RVOLayer Layer { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::CollidesWith */
		public RVOLayer CollidesWith { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::DebugDraw */
		public bool DebugDraw {
			get {
				return debugDraw;
			}
			set {
				debugDraw = value && simulator != null && !simulator.Multithreading;
			}
		}

		/** \copydoc Pathfinding::RVO::IAgent::Priority */
		public float Priority { get; set; }

		/** \copydoc Pathfinding::RVO::IAgent::PreCalculationCallback */
		public System.Action PreCalculationCallback { private get; set; }

		#endregion

		#region IAgent Methods

		/** \copydoc Pathfinding::RVO::IAgent::SetTarget */
		public void SetTarget (Vector2 targetPoint, float desiredSpeed, float maxSpeed) {
			maxSpeed = System.Math.Max(maxSpeed, 0);
			desiredSpeed = System.Math.Min(System.Math.Max(desiredSpeed, 0), maxSpeed);

			nextTargetPoint = targetPoint;
			nextDesiredSpeed = desiredSpeed;
			nextMaxSpeed = maxSpeed;
		}

		/** \copydoc Pathfinding::RVO::IAgent::SetCollisionNormal */
		public void SetCollisionNormal (Vector2 normal) {
			collisionNormal = normal;
		}

		/** \copydoc Pathfinding::RVO::IAgent::ForceSetVelocity */
		public void ForceSetVelocity (Vector2 velocity) {
			// A bit hacky, but it is approximately correct
			// assuming the agent does not move significantly
			nextTargetPoint = CalculatedTargetPoint = position + velocity * 1000;
			nextDesiredSpeed = CalculatedSpeed = velocity.magnitude;
			manuallyControlled = true;
		}

		#endregion

		/** Used internally for a linked list */
		internal Agent next;

		float calculatedSpeed;
		Vector2 calculatedTargetPoint;

		/** Simulator which handles this agent.
		 * Used by this script as a reference and to prevent
		 * adding this agent to multiple simulations.
		 */
		internal Simulator simulator;

		List<Agent> neighbours = new List<Agent>();
		List<float> neighbourDists = new List<float>();
		List<ObstacleVertex> obstaclesBuffered = new List<ObstacleVertex>();
		List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		const float DesiredVelocityWeight = 0.1f;

		/** Extra weight that walls will have */
		const float WallWeight = 5;

		public List<ObstacleVertex> NeighbourObstacles {
			get {
				return null;
			}
		}

		public Agent (Vector2 pos, float elevationCoordinate) {
			AgentTimeHorizon = 2;
			ObstacleTimeHorizon = 2;
			Height = 5;
			Radius = 5;
			MaxNeighbours = 10;
			Locked = false;
			Position = pos;
			ElevationCoordinate = elevationCoordinate;
			Layer = RVOLayer.DefaultAgent;
			CollidesWith = (RVOLayer)(-1);
			Priority = 0.5f;
			CalculatedTargetPoint = pos;
			CalculatedSpeed = 0;
			SetTarget(pos, 0, 0);
		}

		/** Reads public properties and stores them in internal fields.
		 * This is required because multithreading is used and if another script
		 * updated the fields at the same time as this class used them in another thread
		 * weird things could happen.
		 *
		 * Will also set CalculatedTargetPoint and CalculatedSpeed to the result
		 * which was last calculated.
		 */
		public void BufferSwitch () {
			// <== Read public properties
			radius = Radius;
			height = Height;
			maxSpeed = nextMaxSpeed;
			desiredSpeed = nextDesiredSpeed;
			agentTimeHorizon = AgentTimeHorizon;
			obstacleTimeHorizon = ObstacleTimeHorizon;
			maxNeighbours = MaxNeighbours;
			// Manually controlled overrides the agent being locked
			// (if one for some reason uses them at the same time)
			locked = Locked && !manuallyControlled;
			position = Position;
			elevationCoordinate = ElevationCoordinate;
			collidesWith = CollidesWith;
			layer = Layer;

			if (locked) {
				// Locked agents do not move at all
				desiredTargetPointInVelocitySpace = position;
				desiredVelocity = currentVelocity = Vector2.zero;
			} else {
				desiredTargetPointInVelocitySpace = nextTargetPoint - position;

				// Estimate our current velocity
				// This is necessary because other agents need to know
				// how this agent is moving to be able to avoid it
				currentVelocity = (CalculatedTargetPoint - position).normalized * CalculatedSpeed;

				// Calculate the desired velocity from the point we want to reach
				desiredVelocity = desiredTargetPointInVelocitySpace.normalized*desiredSpeed;

				if (collisionNormal != Vector2.zero) {
					collisionNormal.Normalize();
					var dot = Vector2.Dot(currentVelocity, collisionNormal);

					// Check if the velocity is going into the wall
					if (dot < 0) {
						// If so: remove that component from the velocity
						currentVelocity -= collisionNormal * dot;
					}

					// Clear the normal
					collisionNormal = Vector2.zero;
				}
			}
		}

		public void PreCalculation () {
			if (PreCalculationCallback != null) {
				PreCalculationCallback();
			}
		}

		public void PostCalculation () {
			// ==> Set public properties
			if (!manuallyControlled) {
				CalculatedTargetPoint = calculatedTargetPoint;
				CalculatedSpeed = calculatedSpeed;
			}

			List<ObstacleVertex> tmp = obstaclesBuffered;
			obstaclesBuffered = obstacles;
			obstacles = tmp;

			manuallyControlled = false;
		}

		/** Populate the neighbours and neighbourDists lists with the closest agents to this agent */
		public void CalculateNeighbours () {
			neighbours.Clear();
			neighbourDists.Clear();

			if (MaxNeighbours > 0 && !locked) simulator.Quadtree.Query(position, maxSpeed, agentTimeHorizon, radius, this);

			NeighbourCount = neighbours.Count;
		}

		/** Square a number */
		static float Sqr (float x) {
			return x*x;
		}

		/** Used by the Quadtree.
		 * \see CalculateNeighbours
		 */
		internal float InsertAgentNeighbour (Agent agent, float rangeSq) {
			// Check if this agent collides with the other agent
			if (this == agent || (agent.layer & collidesWith) == 0) return rangeSq;

			// 2D distance
			float dist = (agent.position - position).sqrMagnitude;

			if (dist < rangeSq) {
				if (neighbours.Count < maxNeighbours) {
					neighbours.Add(null);
					neighbourDists.Add(float.PositiveInfinity);
				}

				// Insert the agent into the ordered list of neighbours
				int i = neighbours.Count-1;
				if (dist < neighbourDists[i]) {
					while (i != 0 && dist < neighbourDists[i-1]) {
						neighbours[i] = neighbours[i-1];
						neighbourDists[i] = neighbourDists[i-1];
						i--;
					}
					neighbours[i] = agent;
					neighbourDists[i] = dist;
				}

				if (neighbours.Count == maxNeighbours) {
					rangeSq = neighbourDists[neighbourDists.Count-1];
				}
			}
			return rangeSq;
		}


		/** (x, 0, y) */
		static Vector3 FromXZ (Vector2 p) {
			return new Vector3(p.x, 0, p.y);
		}

		/** (x, z) */
		static Vector2 ToXZ (Vector3 p) {
			return new Vector2(p.x, p.z);
		}

		/** Converts a 3D vector to a 2D vector in the movement plane.
		 * If movementPlane is XZ it will be projected onto the XZ plane
		 * and the elevation coordinate will be the Y coordinate
		 * otherwise it will be projected onto the XY plane and elevation
		 * will be the Z coordinate.
		 */
		Vector2 To2D (Vector3 p, out float elevation) {
			if (simulator.movementPlane == MovementPlane.XY) {
				elevation = -p.z;
				return new Vector2(p.x, p.y);
			} else {
				elevation = p.y;
				return new Vector2(p.x, p.z);
			}
		}

		static void DrawVO (Vector2 circleCenter, float radius, Vector2 origin) {
			float alpha = Mathf.Atan2((origin - circleCenter).y, (origin - circleCenter).x);
			float gamma = radius/(origin-circleCenter).magnitude;
			float delta = gamma <= 1.0f ? Mathf.Abs(Mathf.Acos(gamma)) : 0;

			Draw.Debug.CircleXZ(FromXZ(circleCenter), radius, Color.black, alpha-delta, alpha+delta);
			Vector2 p1 = new Vector2(Mathf.Cos(alpha-delta), Mathf.Sin(alpha-delta)) * radius;
			Vector2 p2 = new Vector2(Mathf.Cos(alpha+delta), Mathf.Sin(alpha+delta)) * radius;

			Vector2 p1t = -new Vector2(-p1.y, p1.x);
			Vector2 p2t = new Vector2(-p2.y, p2.x);
			p1 += circleCenter;
			p2 += circleCenter;

			Debug.DrawRay(FromXZ(p1), FromXZ(p1t).normalized*100, Color.black);
			Debug.DrawRay(FromXZ(p2), FromXZ(p2t).normalized*100, Color.black);
		}

		/** Velocity Obstacle.
		 * This is a struct to avoid too many allocations.
		 *
		 * \see https://en.wikipedia.org/wiki/Velocity_obstacle
		 */
		internal struct VO {
			Vector2 line1, line2, dir1, dir2;

			Vector2 cutoffLine, cutoffDir;
			Vector2 circleCenter;

			bool colliding;
			float radius;
			float weightFactor;
			float weightBonus;

			Vector2 segmentStart, segmentEnd;
			bool segment;

			/** Creates a VO for avoiding another agent.
			 * \param center The position of the other agent relative to this agent.
			 * \param offset Offset of the velocity obstacle. For example to account for the agents' relative velocities.
			 * \param radius Combined radius of the two agents (radius1 + radius2).
			 * \param inverseDt 1 divided by the local avoidance time horizon (e.g avoid agents that we will hit within the next 2 seconds).
			 * \param inverseDeltaTime 1 divided by the time step length.
			 */
			public VO (Vector2 center, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime) {
				// Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
				this.weightFactor = 1;
				weightBonus = 0;

				//this.radius = radius;
				Vector2 globalCenter;

				circleCenter = center*inverseDt + offset;

				this.weightFactor = 4*Mathf.Exp(-Sqr(center.sqrMagnitude/(radius*radius))) + 1;
				// Collision?
				if (center.magnitude < radius) {
					colliding = true;

					// 0.001 is there to make sure lin1.magnitude is not so small that the normalization
					// below will return Vector2.zero as that will make the VO invalid and it will be ignored.
					line1 = center.normalized * (center.magnitude - radius - 0.001f) * 0.3f * inverseDeltaTime;
					dir1 = new Vector2(line1.y, -line1.x).normalized;
					line1 += offset;

					cutoffDir = Vector2.zero;
					cutoffLine = Vector2.zero;
					dir2 = Vector2.zero;
					line2 = Vector2.zero;
					this.radius = 0;
				} else {
					colliding = false;

					center *= inverseDt;
					radius *= inverseDt;
					globalCenter = center+offset;

					// 0.001 is there to make sure cutoffDistance is not so small that the normalization
					// below will return Vector2.zero as that will make the VO invalid and it will be ignored.
					var cutoffDistance = center.magnitude - radius + 0.001f;

					cutoffLine = center.normalized * cutoffDistance;
					cutoffDir = new Vector2(-cutoffLine.y, cutoffLine.x).normalized;
					cutoffLine += offset;

					float alpha = Mathf.Atan2(-center.y, -center.x);

					float delta = Mathf.Abs(Mathf.Acos(radius/center.magnitude));

					this.radius = radius;

					// Bounding Lines

					// Point on circle
					line1 = new Vector2(Mathf.Cos(alpha+delta), Mathf.Sin(alpha+delta));
					// Vector tangent to circle which is the correct line tangent
					// Note that this vector is normalized
					dir1 = new Vector2(line1.y, -line1.x);

					// Point on circle
					line2 = new Vector2(Mathf.Cos(alpha-delta), Mathf.Sin(alpha-delta));
					// Vector tangent to circle which is the correct line tangent
					// Note that this vector is normalized
					dir2 = new Vector2(line2.y, -line2.x);

					line1 = line1 * radius + globalCenter;
					line2 = line2 * radius + globalCenter;
				}

				segmentStart = Vector2.zero;
				segmentEnd = Vector2.zero;
				segment = false;
			}

			/** Creates a VO for avoiding another agent.
			 * Note that the segment is directed, the agent will want to be on the left side of the segment.
			 */
			public static VO SegmentObstacle (Vector2 segmentStart, Vector2 segmentEnd, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime) {
				var vo = new VO();

				// Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
				vo.weightFactor = 1;
				// Just higher than anything else
				vo.weightBonus = Mathf.Max(radius, 1)*40;

				var closestOnSegment = VectorMath.ClosestPointOnSegment(segmentStart.ToPFV2(), segmentEnd.ToPFV2(), Vector2.zero.ToPFV2());

				// Collision?
				if (closestOnSegment.magnitude <= radius) {
					vo.colliding = true;

					vo.line1 = closestOnSegment.normalized.ToUnityV3() * (closestOnSegment.magnitude - radius) * 0.3f * inverseDeltaTime;
					vo.dir1 = new Vector2(vo.line1.y, -vo.line1.x).normalized;
					vo.line1 += offset;

					vo.cutoffDir = Vector2.zero;
					vo.cutoffLine = Vector2.zero;
					vo.dir2 = Vector2.zero;
					vo.line2 = Vector2.zero;
					vo.radius = 0;

					vo.segmentStart = Vector2.zero;
					vo.segmentEnd = Vector2.zero;
					vo.segment = false;
				} else {
					vo.colliding = false;

					segmentStart *= inverseDt;
					segmentEnd *= inverseDt;
					radius *= inverseDt;

					var cutoffTangent = (segmentEnd - segmentStart).normalized;
					vo.cutoffDir = cutoffTangent;
					vo.cutoffLine = segmentStart + new Vector2(-cutoffTangent.y, cutoffTangent.x) * radius;
					vo.cutoffLine += offset;

					// See documentation for details
					// The call to Max is just to prevent floating point errors causing NaNs to appear
					var startSqrMagnitude = segmentStart.sqrMagnitude;
					var normal1 = -VectorMath.ComplexMultiply(segmentStart, new Vector2(radius, Mathf.Sqrt(Mathf.Max(0, startSqrMagnitude - radius*radius)))) / startSqrMagnitude;
					var endSqrMagnitude = segmentEnd.sqrMagnitude;
					var normal2 = -VectorMath.ComplexMultiply(segmentEnd, new Vector2(radius, -Mathf.Sqrt(Mathf.Max(0, endSqrMagnitude - radius*radius)))) / endSqrMagnitude;

					vo.line1 = segmentStart + normal1.ToUnityV2() * radius + offset;
					vo.line2 = segmentEnd + normal2.ToUnityV2() * radius + offset;

					// Note that the normals are already normalized
					vo.dir1 = new Vector2(normal1.y, -normal1.x);
					vo.dir2 = new Vector2(normal2.y, -normal2.x);

					vo.segmentStart = segmentStart;
					vo.segmentEnd = segmentEnd;
					vo.radius = radius;
					vo.segment = true;
				}

				return vo;
			}

			/** Returns a negative number of if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
			 * The number can be seen as the double signed area of the triangle {a, a+dir, p} multiplied by the length of \a dir.
			 * If dir.magnitude=1 this is also the distance from p to the line {a, a+dir}.
			 */
			public static float SignedDistanceFromLine (Vector2 a, Vector2 dir, Vector2 p) {
				return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
			}

			/** Gradient and value of the cost function of this VO.
			 * Very similar to the #Gradient method however the gradient
			 * and value have been scaled and tweaked slightly.
			 */
			public Vector2 ScaledGradient (Vector2 p, out float weight) {
				var grad = Gradient(p, out weight);

				if (weight > 0) {
					const float Scale = 2;
					grad *= Scale * weightFactor;
					weight *= Scale * weightFactor;
					weight += 1 + weightBonus;
				}

				return grad;
			}

			/** Gradient and value of the cost function of this VO.
			 * The VO has a cost function which is 0 outside the VO
			 * and increases inside it as the point moves further into
			 * the VO.
			 *
			 * This is the negative gradient of that function as well as its
			 * value (the weight). The negative gradient points in the direction
			 * where the function decreases the fastest.
			 *
			 * The value of the function is the distance to the closest edge
			 * of the VO and the gradient is normalized.
			 */
			public Vector2 Gradient (Vector2 p, out float weight) {
				if (colliding) {
					// Calculate double signed area of the triangle consisting of the points
					// {line1, line1+dir1, p}
					float l1 = SignedDistanceFromLine(line1, dir1, p);

					// Serves as a check for which side of the line the point p is
					if (l1 >= 0) {
						weight = l1;
						return new Vector2(-dir1.y, dir1.x);
					} else {
						weight = 0;
						return new Vector2(0, 0);
					}
				}

				float det3 = SignedDistanceFromLine(cutoffLine, cutoffDir, p);
				if (det3 <= 0) {
					weight = 0;
					return Vector2.zero;
				} else {
					// Signed distances to the two edges along the sides of the VO
					float det1 = SignedDistanceFromLine(line1, dir1, p);
					float det2 = SignedDistanceFromLine(line2, dir2, p);
					if (det1 >= 0 && det2 >= 0) {
						// We are inside both of the half planes
						// (all three if we count the cutoff line)
						// and thus inside the forbidden region in velocity space

						// Actually the negative gradient because we want the
						// direction where it slopes the most downwards, not upwards
						Vector2 gradient;

						// Check if we are in the semicircle region near the cap of the VO
						if (Vector2.Dot(p - line1, dir1) > 0 && Vector2.Dot(p - line2, dir2) < 0) {
							if (segment) {
								// This part will only be reached for line obstacles (i.e not other agents)
								if (det3 < radius) {
									PF.Vector3 closestPointOnLine = VectorMath.ClosestPointOnSegment(segmentStart.ToPFV2(), segmentEnd.ToPFV2(), p.ToPFV2());
									var dirFromCenter = p.ToPFV2() - closestPointOnLine.ToV2();
									float distToCenter;
									gradient = VectorMath.Normalize(dirFromCenter, out distToCenter);
									// The weight is the distance to the edge
									weight = radius - distToCenter;
									return gradient;
								}
							} else {
								var dirFromCenter = p - circleCenter;
								float distToCenter;
								gradient = VectorMath.Normalize(dirFromCenter, out distToCenter);
								// The weight is the distance to the edge
								weight = radius - distToCenter;
								return gradient;
							}
						}

						if (segment && det3 < det1 && det3 < det2) {
							weight = det3;
							gradient = new Vector2(-cutoffDir.y, cutoffDir.x);
							return gradient;
						}

						// Just move towards the closest edge
						// The weight is the distance to the edge
						if (det1 < det2) {
							weight = det1;
							gradient = new Vector2(-dir1.y, dir1.x);
						} else {
							weight = det2;
							gradient = new Vector2(-dir2.y, dir2.x);
						}

						return gradient;
					}

					weight = 0;
					return Vector2.zero;
				}
			}
		}

		/** Very simple list.
		 * Cannot use a List<T> because when indexing into a List<T> and T is
		 * a struct (which VO is) then the whole struct will be copied.
		 * When indexing into an array, that copy can be skipped.
		 */
		internal class VOBuffer {
			public VO[] buffer;
			public int length;

			public void Clear () {
				length = 0;
			}

			public VOBuffer (int n) {
				buffer = new VO[n];
				length = 0;
			}

			public void Add (VO vo) {
				if (length >= buffer.Length) {
					var nbuffer = new VO[buffer.Length * 2];
					buffer.CopyTo(nbuffer, 0);
					buffer = nbuffer;
				}
				buffer[length++] = vo;
			}
		}

		internal void CalculateVelocity (Pathfinding.RVO.Simulator.WorkerContext context) {
			if (manuallyControlled) {
				return;
			}

			if (locked) {
				calculatedSpeed = 0;
				calculatedTargetPoint = position;
				return;
			}

			// Buffer which will be filled up with velocity obstacles (VOs)
			var vos = context.vos;
			vos.Clear();

			GenerateObstacleVOs(vos);
			GenerateNeighbourAgentVOs(vos);

			bool insideAnyVO = BiasDesiredVelocity(vos, ref desiredVelocity, ref desiredTargetPointInVelocitySpace, simulator.symmetryBreakingBias);

			if (!insideAnyVO) {
				// Desired velocity can be used directly since it was not inside any velocity obstacle.
				// No need to run optimizer because this will be the global minima.
				// This is also a special case in which we can set the
				// calculated target point to the desired target point
				// instead of calculating a point based on a calculated velocity
				// which is an important difference when the agent is very close
				// to the target point
				// TODO: Not actually guaranteed to be global minima if desiredTargetPointInVelocitySpace.magnitude < desiredSpeed
				// maybe do something different here?
				calculatedTargetPoint = desiredTargetPointInVelocitySpace + position;
				calculatedSpeed = desiredSpeed;
				if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(calculatedTargetPoint), Color.white);
				return;
			}

			Vector2 result = Vector2.zero;

			result = GradientDescent(vos, currentVelocity, desiredVelocity);

			if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(result+position), Color.white);
			//Debug.DrawRay (To3D (position), To3D (result));

			calculatedTargetPoint = position + result;
			calculatedSpeed = Mathf.Min(result.magnitude, maxSpeed);
		}

		static Color Rainbow (float v) {
			Color c = new Color(v, 0, 0);

			if (c.r > 1) { c.g = c.r - 1; c.r = 1; }
			if (c.g > 1) { c.b = c.g - 1; c.g = 1; }
			return c;
		}

		void GenerateObstacleVOs (VOBuffer vos) {
			var range = maxSpeed * obstacleTimeHorizon;

			// Iterate through all obstacles that we might need to avoid
			for (int i = 0; i < simulator.obstacles.Count; i++) {
				var obstacle = simulator.obstacles[i];
				var vertex = obstacle;
				// Iterate through all edges (defined by vertex and vertex.dir) in the obstacle
				do {
					// Ignore the edge if the agent should not collide with it
					if (vertex.ignore || (vertex.layer & collidesWith) == 0) {
						vertex = vertex.next;
						continue;
					}

					// Start and end points of the current segment
					float elevation1, elevation2;
					var p1 = To2D(vertex.position, out elevation1);
					var p2 = To2D(vertex.next.position, out elevation2);

					Vector2 dir = (p2 - p1).normalized;

					// Signed distance from the line (not segment, lines are infinite)
					// TODO: Can be optimized
					float dist = VO.SignedDistanceFromLine(p1, dir, position);

					if (dist >= -0.01f && dist < range) {
						float factorAlongSegment = Vector2.Dot(position - p1, p2 - p1) / (p2 - p1).sqrMagnitude;

						// Calculate the elevation (y) coordinate of the point on the segment closest to the agent
						var segmentY = Mathf.Lerp(elevation1, elevation2, factorAlongSegment);

						// Calculate distance from the segment (not line)
						var sqrDistToSegment = (Vector2.Lerp(p1, p2, factorAlongSegment) - position).sqrMagnitude;

						// Ignore the segment if it is too far away
						// or the agent is too high up (or too far down) on the elevation axis (usually y axis) to avoid it.
						// If the XY plane is used then all elevation checks are disabled
						if (sqrDistToSegment < range*range && (simulator.movementPlane == MovementPlane.XY || (elevationCoordinate <= segmentY + vertex.height && elevationCoordinate+height >= segmentY))) {
							vos.Add(VO.SegmentObstacle(p2 - position, p1 - position, Vector2.zero, radius * 0.01f, 1f / ObstacleTimeHorizon, 1f / simulator.DeltaTime));
						}
					}

					vertex = vertex.next;
				} while (vertex != obstacle && vertex != null && vertex.next != null);
			}
		}

		void GenerateNeighbourAgentVOs (VOBuffer vos) {
			float inverseAgentTimeHorizon = 1.0f/agentTimeHorizon;

			// The RVO algorithm assumes we will continue to
			// move in roughly the same direction
			Vector2 optimalVelocity = currentVelocity;

			for (int o = 0; o < neighbours.Count; o++) {
				Agent other = neighbours[o];

				// Don't avoid ourselves
				if (other == this)
					continue;

				// Interval along the y axis in which the agents overlap
				float maxY = System.Math.Min(elevationCoordinate + height, other.elevationCoordinate + other.height);
				float minY = System.Math.Max(elevationCoordinate, other.elevationCoordinate);

				// The agents cannot collide since they are on different y-levels
				if (maxY - minY < 0) {
					continue;
				}

				float totalRadius = radius + other.radius;

				// Describes a circle on the border of the VO
				Vector2 voBoundingOrigin = other.position - position;

				float avoidanceStrength;
				if (other.locked || other.manuallyControlled) {
					avoidanceStrength = 1;
				} else if (other.Priority > 0.00001f || Priority > 0.00001f) {
					avoidanceStrength = other.Priority / (Priority + other.Priority);
				} else {
					// Both this agent's priority and the other agent's priority is zero or negative
					// Assume they have the same priority
					avoidanceStrength = 0.5f;
				}

				// We assume that the other agent will continue to move with roughly the same velocity if the priorities for the agents are similar.
				// If the other agent has a higher priority than this agent (avoidanceStrength > 0.5) then we will assume it will move more along its
				// desired velocity. This will have the effect of other agents trying to clear a path for where a high priority agent wants to go.
				// If this is not done then even high priority agents can get stuck when it is really crowded and they have had to slow down.
				Vector2 otherOptimalVelocity = Vector2.Lerp(other.currentVelocity, other.desiredVelocity, 2*avoidanceStrength - 1);

				var voCenter = Vector2.Lerp(optimalVelocity, otherOptimalVelocity, avoidanceStrength);

				vos.Add(new VO(voBoundingOrigin, voCenter, totalRadius, inverseAgentTimeHorizon, 1 / simulator.DeltaTime));
				if (DebugDraw)
					DrawVO(position + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, position + voCenter);
			}
		}

		Vector2 GradientDescent (VOBuffer vos, Vector2 sampleAround1, Vector2 sampleAround2) {
			float score1;
			var minima1 = Trace(vos, sampleAround1, out score1);

			if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(minima1 + position), Color.yellow, 0.5f);

			// Can be uncommented for higher quality local avoidance
			// for ( int i = 0; i < 3; i++ ) {
			//	Vector2 p = desiredVelocity + new Vector2(Mathf.Cos(Mathf.PI*2*(i/3.0f)), Mathf.Sin(Mathf.PI*2*(i/3.0f)));
			//	float score;Vector2 res = Trace ( vos, p, velocity.magnitude*simulator.qualityCutoff, out score );
			//
			//	if ( score < best ) {
			//		result = res;
			//		best = score;
			//	}
			// }

			float score2;
			Vector2 minima2 = Trace(vos, sampleAround2, out score2);
			if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(minima2 + position), Color.magenta, 0.5f);

			return score1 < score2 ? minima1 : minima2;
		}


		/** Bias towards the right side of agents.
		 * Rotate desiredVelocity at most [value] number of radians. 1 radian ≈ 57°
		 * This breaks up symmetries.
		 *
		 * The desired velocity will only be rotated if it is inside a velocity obstacle (VO).
		 * If it is inside one, it will not be rotated further than to the edge of it
		 *
		 * The targetPointInVelocitySpace will be rotated by the same amount as the desired velocity
		 *
		 * \returns True if the desired velocity was inside any VO
		 */
		static bool BiasDesiredVelocity (VOBuffer vos, ref Vector2 desiredVelocity, ref Vector2 targetPointInVelocitySpace, float maxBiasRadians) {
			var desiredVelocityMagn = desiredVelocity.magnitude;
			var maxValue = 0f;

			for (int i = 0; i < vos.length; i++) {
				float value;
				// The value is approximately the distance to the edge of the VO
				// so taking the maximum will give us the distance to the edge of the VO
				// which the desired velocity is furthest inside
				vos.buffer[i].Gradient(desiredVelocity, out value);
				maxValue = Mathf.Max(maxValue, value);
			}

			// Check if the agent was inside any VO
			var inside = maxValue > 0;

			// Avoid division by zero below
			if (desiredVelocityMagn < 0.001f) {
				return inside;
			}

			// Rotate the desired velocity clockwise (to the right) at most maxBiasRadians number of radians
			// Assuming maxBiasRadians is small, we can just move it instead and it will give approximately the same effect
			// See https://en.wikipedia.org/wiki/Small-angle_approximation
			var angle = Mathf.Min(maxBiasRadians, maxValue / desiredVelocityMagn);
			desiredVelocity += new Vector2(desiredVelocity.y, -desiredVelocity.x) * angle;
			targetPointInVelocitySpace += new Vector2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;
			return inside;
		}

		/** Evaluate gradient and value of the cost function at velocity p */
		Vector2 EvaluateGradient (VOBuffer vos, Vector2 p, out float value) {
			Vector2 gradient = Vector2.zero;

			value = 0;

			// Avoid other agents
			for (int i = 0; i < vos.length; i++) {
				float w;
				var grad = vos.buffer[i].ScaledGradient(p, out w);
				if (w > value) {
					value = w;
					gradient = grad;
				}
			}

			// Move closer to the desired velocity
			var dirToDesiredVelocity = desiredVelocity - p;
			var distToDesiredVelocity = dirToDesiredVelocity.magnitude;
			if (distToDesiredVelocity > 0.0001f) {
				gradient += dirToDesiredVelocity * (DesiredVelocityWeight/distToDesiredVelocity);
				value += distToDesiredVelocity * DesiredVelocityWeight;
			}

			// Prefer speeds lower or equal to the desired speed
			// and avoid speeds greater than the max speed
			var sqrSpeed = p.sqrMagnitude;
			if (sqrSpeed > desiredSpeed*desiredSpeed) {
				var speed = Mathf.Sqrt(sqrSpeed);

				if (speed > maxSpeed) {
					const float MaxSpeedWeight = 3;
					value += MaxSpeedWeight * (speed - maxSpeed);
					gradient -= MaxSpeedWeight * (p/speed);
				}

				// Scale needs to be strictly greater than DesiredVelocityWeight
				// otherwise the agent will not prefer the desired speed over
				// the maximum speed
				float scale = 2*DesiredVelocityWeight;
				value += scale * (speed - desiredSpeed);
				gradient -= scale * (p/speed);
			}

			return gradient;
		}

		/** Traces the vector field constructed out of the velocity obstacles.
		 * Returns the position which gives the minimum score (approximately).
		 *
		 * \see https://en.wikipedia.org/wiki/Gradient_descent
		 */
		Vector2 Trace (VOBuffer vos, Vector2 p, out float score) {
			// Pick a reasonable initial step size
			float stepSize = Mathf.Max(radius, 0.2f * desiredSpeed);

			float bestScore = float.PositiveInfinity;
			Vector2 bestP = p;

			// TODO: Add momentum to speed up convergence?

			const int MaxIterations = 50;

			for (int s = 0; s < MaxIterations; s++) {
				float step = 1.0f - (s/(float)MaxIterations);
				step = Sqr(step) * stepSize;

				float value;
				var gradient = EvaluateGradient(vos, p, out value);

				if (value < bestScore) {
					bestScore = value;
					bestP = p;
				}

				// TODO: Add cutoff for performance

				gradient.Normalize();

				gradient *= step;
				Vector2 prev = p;
				p += gradient;

				if (DebugDraw) Debug.DrawLine(FromXZ(prev + position), FromXZ(p + position), Rainbow(s*0.1f) * new Color(1, 1, 1, 1f));
			}

			score = bestScore;
			return bestP;
		}
	}
}
