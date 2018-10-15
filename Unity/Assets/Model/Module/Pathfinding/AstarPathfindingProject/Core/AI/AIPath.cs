using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	using Pathfinding.RVO;
	using Pathfinding.Util;

	/** AI for following paths.
	 * This AI is the default movement script which comes with the A* Pathfinding Project.
	 * It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
	 * to set up movement for the characters in your game.
	 * This script works well for many types of units, but if you need the highest performance (for example if you are moving hundreds of characters) you
	 * may want to customize this script or write a custom movement script to be able to optimize it specifically for your game.
	 *
	 * This script will try to move to a given #destination. At \link repathRate regular intervals\endlink, the path to the destination will be recalculated.
	 * If you want to make the AI to follow a particular object you can attach the \link Pathfinding.AIDestinationSetter AIDestinationSetter\endlink component.
	 * Take a look at the \ref getstarted tutorial for more instructions on how to configure this script.
	 *
	 * Here is a video of this script being used move an agent around (technically it uses the #Pathfinding.Examples.MineBotAI script that inherits from this one but adds a bit of animation support for the example scenes):
	 * \video{wandering_ai.mp4}
	 *
	 * \section variables Quick overview of the variables
	 * In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.
	 *
	 * The #repathRate determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.
	 * The #destination field is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
	 * Or it can be the player object in a zombie game.
	 * The #maxSpeed is self-explanatory, as is #rotationSpeed. however #slowdownDistance might require some explanation:
	 * It is the approximate distance from the target where the AI will start to slow down. Setting it to a large value will make the AI slow down very gradually.
	 * #pickNextWaypointDist determines the distance to the point the AI will move to (see image below).
	 *
	 * Below is an image illustrating several variables that are exposed by this class (#pickNextWaypointDist, #steeringTarget, #desiredVelocity)
	 * \shadowimage{aipath_variables.png}
	 *
	 * This script has many movement fallbacks.
	 * If it finds an RVOController attached to the same GameObject as this component, it will use that. If it finds a character controller it will also use that.
	 * If it finds a rigidbody it will use that. Lastly it will fall back to simply modifying Transform.position which is guaranteed to always work and is also the most performant option.
	 *
	 * \section how-aipath-works How it works
	 * In this section I'm going to go over how this script is structured and how information flows.
	 * This is useful if you want to make changes to this script or if you just want to understand how it works a bit more deeply.
	 * However you do not need to read this section if you are just going to use the script as-is.
	 *
	 * This script inherits from the #AIBase class. The movement happens either in Unity's standard #Update or #FixedUpdate method.
	 * They are both defined in the AIBase class. Which one is actually used depends on if a rigidbody is used for movement or not.
	 * Rigidbody movement has to be done inside the FixedUpdate method while otherwise it is better to do it in Update.
	 *
	 * From there a call is made to the #MovementUpdate method (which in turn calls #MovementUpdateInternal).
	 * This method contains the main bulk of the code and calculates how the AI *wants* to move. However it doesn't do any movement itself.
	 * Instead it returns the position and rotation it wants the AI to move to have at the end of the frame.
	 * The #Update (or #FixedUpdate) method then passes these values to the #FinalizeMovement method which is responsible for actually moving the character.
	 * That method also handles things like making sure the AI doesn't fall through the ground using raycasting.
	 *
	 * The AI recalculates its path regularly. This happens in the Update method which checks #shouldRecalculatePath and if that returns true it will call #SearchPath.
	 * The #SearchPath method will prepare a path request and send it to the \link Pathfinding.Seeker Seeker\endlink component which should be attached to the same GameObject as this script.
	 * Since this script will when waking up register to the \link Pathfinding.Seeker.pathCallback Seeker.pathCallback\endlink delegate this script will be notified every time a new path is calculated by the #OnPathComplete method being called.
	 * It may take one or sometimes multiple frames for the path to be calculated, but finally the #OnPathComplete method will be called and the current path that the AI is following will be replaced.
	 */
	[AddComponentMenu("Pathfinding/AI/AIPath (2D,3D)")]
	public partial class AIPath : AIBase, IAstarAI {
		/** How quickly the agent accelerates.
		 * Positive values represent an acceleration in world units per second squared.
		 * Negative values are interpreted as an inverse time of how long it should take for the agent to reach its max speed.
		 * For example if it should take roughly 0.4 seconds for the agent to reach its max speed then this field should be set to -1/0.4 = -2.5.
		 * For a negative value the final acceleration will be: -acceleration*maxSpeed.
		 * This behaviour exists mostly for compatibility reasons.
		 *
		 * In the Unity inspector there are two modes: Default and Custom. In the Default mode this field is set to -2.5 which means that it takes about 0.4 seconds for the agent to reach its top speed.
		 * In the Custom mode you can set the acceleration to any positive value.
		 */
		public float maxAcceleration = -2.5f;

		/** Rotation speed in degrees per second.
		 * Rotation is calculated using Quaternion.RotateTowards. This variable represents the rotation speed in degrees per second.
		 * The higher it is, the faster the character will be able to rotate.
		 */
		[UnityEngine.Serialization.FormerlySerializedAs("turningSpeed")]
		public float rotationSpeed = 360;

		/** Distance from the end of the path where the AI will start to slow down */
		public float slowdownDistance = 0.6F;

		/** How far the AI looks ahead along the path to determine the point it moves to.
		 * In world units.
		 * If you enable the #alwaysDrawGizmos toggle this value will be visualized in the scene view as a blue circle around the agent.
		 * \shadowimage{aipath_variables.png}
		 *
		 * Here are a few example videos showing some typical outcomes with good values as well as how it looks when this value is too low and too high.
		 * <table>
		 * <tr><td>\video_wide{aipath/picknext/video_02_default.mp4}</td><td>\xmlonly <verbatim><span class="label label-danger">Too low</span><br/></verbatim>\endxmlonly A too low value and a too low acceleration will result in the agent overshooting a lot and not managing to follow the path well.</td></tr>
		 * <tr><td>\video_wide{aipath/picknext/video_02_100.mp4}</td><td>\xmlonly <verbatim><span class="label label-warning">Ok</span><br/></verbatim>\endxmlonly A low value but a high acceleration works decently to make the AI follow the path more closely. Note that the \link Pathfinding.AILerp AILerp\endlink component is better suited if you want the agent to follow the path without any deviations.</td></tr>
		 * <tr><td>\video_wide{aipath/picknext/video_07_default.mp4}</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example.</td></tr>
		 * <tr><td>\video_wide{aipath/picknext/video_10_default.mp4}</td><td>\xmlonly <verbatim><span class="label label-success">Ok</span><br/></verbatim>\endxmlonly A reasonable value in this example, but the path is followed slightly more loosely than in the previous video.</td></tr>
		 * <tr><td>\video_wide{aipath/picknext/video_20_default.mp4}</td><td>\xmlonly <verbatim><span class="label label-danger">Too high</span><br/></verbatim>\endxmlonly A too high value will make the agent follow the path too loosely and may cause it to try to move through obstacles.</td></tr>
		 * </table>
		 */
		public float pickNextWaypointDist = 2;

		/** Distance to the end point to consider the end of path to be reached.
		 * When the end is within this distance then #OnTargetReached will be called and #reachedEndOfPath will return true.
		 */
		public float endReachedDistance = 0.2F;

		/** Draws detailed gizmos constantly in the scene view instead of only when the agent is selected and settings are being modified */
		public bool alwaysDrawGizmos;

		/** Slow down when not facing the target direction.
		 * Incurs at a small performance overhead.
		 */
		public bool slowWhenNotFacingTarget = true;

		/** What to do when within #endReachedDistance units from the destination.
		 * The character can either stop immediately when it comes within that distance, which is useful for e.g archers
		 * or other ranged units that want to fire on a target. Or the character can continue to try to reach the exact
		 * destination point and come to a full stop there. This is useful if you want the character to reach the exact
		 * point that you specified.
		 *
		 * \note #reachedEndOfPath will become true when the character is within #endReachedDistance units from the destination
		 * regardless of what this field is set to.
		 */
		public CloseToDestinationMode whenCloseToDestination = CloseToDestinationMode.Stop;

		/** Ensure that the character is always on the traversable surface of the navmesh.
		 * When this option is enabled a \link AstarPath.GetNearest GetNearest\endlink query will be done every frame to find the closest node that the agent can walk on
		 * and if the agent is not inside that node, then the agent will be moved to it.
		 *
		 * This is especially useful together with local avoidance in order to avoid agents pushing each other into walls.
		 * \see \ref local-avoidance for more info about this.
		 *
		 * This option also integrates with local avoidance so that if the agent is say forced into a wall by other agents the local avoidance
		 * system will be informed about that wall and can take that into account.
		 *
		 * Enabling this has some performance impact depending on the graph type (pretty fast for grid graphs, slightly slower for navmesh/recast graphs).
		 * If you are using a navmesh/recast graph you may want to switch to the \link Pathfinding.RichAI RichAI\endlink movement script which is specifically written for navmesh/recast graphs and
		 * does this kind of clamping out of the box. In many cases it can also follow the path more smoothly around sharp bends in the path.
		 *
		 * It is not recommended that you use this option together with the funnel modifier on grid graphs because the funnel modifier will make the path
		 * go very close to the border of the graph and this script has a tendency to try to cut corners a bit. This may cause it to try to go slightly outside the
		 * traversable surface near corners and that will look bad if this option is enabled.
		 *
		 * \warning This option makes no sense to use on point graphs because point graphs do not have a surface.
		 * Enabling this option when using a point graph will lead to the agent being snapped to the closest node every frame which is likely not what you want.
		 *
		 * Below you can see an image where several agents using local avoidance were ordered to go to the same point in a corner.
		 * When not constraining the agents to the graph they are easily pushed inside obstacles.
		 * \shadowimage{constrained_inside.png}
		 */
		public bool constrainInsideGraph = false;

		/** Current path which is followed */
		protected Path path;

		/** Helper which calculates points along the current path */
		protected PathInterpolator interpolator = new PathInterpolator();

		#region IAstarAI implementation

		/** \copydoc Pathfinding::IAstarAI::Teleport */
		public override void Teleport (Vector3 newPosition, bool clearPath = true) {
			if (clearPath) interpolator.SetPath(null);
			reachedEndOfPath = false;
			base.Teleport(newPosition, clearPath);
		}

		/** \copydoc Pathfinding::IAstarAI::remainingDistance */
		public float remainingDistance {
			get {
				return interpolator.valid ? interpolator.remainingDistance + movementPlane.ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::reachedEndOfPath */
		public bool reachedEndOfPath { get; protected set; }

		/** \copydoc Pathfinding::IAstarAI::hasPath */
		public bool hasPath {
			get {
				return interpolator.valid;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::pathPending */
		public bool pathPending {
			get {
				return waitingForPathCalculation;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::steeringTarget */
		public Vector3 steeringTarget {
			get {
				return interpolator.valid ? interpolator.position : position;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::maxSpeed */
		float IAstarAI.maxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }

		/** \copydoc Pathfinding::IAstarAI::canSearch */
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/** \copydoc Pathfinding::IAstarAI::canMove */
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		#endregion

		protected override void OnDisable () {
			base.OnDisable();

			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);
		}

		/** The end of the path has been reached.
		 * If you want custom logic for when the AI has reached it's destination add it here. You can
		 * also create a new script which inherits from this one and override the function in that script.
		 *
		 * This method will be called again if a new path is calculated as the destination may have changed.
		 * So when the agent is close to the destination this method will typically be called every #repathRate seconds.
		 */
		public virtual void OnTargetReached () {
		}

		/** Called when a requested path has been calculated.
		 * A path is first requested by #UpdatePath, it is then calculated, probably in the same or the next frame.
		 * Finally it is returned to the seeker which forwards it to this function.
		 */
		protected override void OnPathComplete (Path newPath) {
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			waitingForPathCalculation = false;

			// Increase the reference count on the new path.
			// This is used for object pooling to reduce allocations.
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				return;
			}

			// Release the previous path.
			if (path != null) path.Release(this);

			// Replace the old path
			path = p;

			// Make sure the path contains at least 2 points
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolator.SetPath(path.vectorPath);

			var graph = UnityHelper.GetGraph(path.path[0]) as ITransformedGraph;
			movementPlane = graph != null ? graph.transform : (rotationIn2D ? new GraphTransform(Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 270, 90), Vector3.one)) : GraphTransform.identityTransform);

			// Reset some variables
			reachedEndOfPath = false;

			// Simulate movement from the point where the path was requested
			// to where we are right now. This reduces the risk that the agent
			// gets confused because the first point in the path is far away
			// from the current position (possibly behind it which could cause
			// the agent to turn around, and that looks pretty bad).
			interpolator.MoveToLocallyClosestPoint((GetFeetPosition() + p.originalStartPoint.ToUnityV3()) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(GetFeetPosition());

			// Update which point we are moving towards.
			// Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
			// (due to interpolator.remainingDistance being incorrect).
			interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);

			var distanceToEnd = remainingDistance;
			if (distanceToEnd <= endReachedDistance) {
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		/** Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not */
		protected override void MovementUpdateInternal (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			float currentAcceleration = maxAcceleration;

			// If negative, calculate the acceleration from the max speed
			if (currentAcceleration < 0) currentAcceleration *= -maxSpeed;

			if (updatePosition) {
				// Get our current position. We read from transform.position as few times as possible as it is relatively slow
				// (at least compared to a local variable)
				simulatedPosition = tr.position;
			}
			if (updateRotation) simulatedRotation = tr.rotation;

			var currentPosition = simulatedPosition;

			// Update which point we are moving towards
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			var dir = movementPlane.ToPlane(steeringTarget - currentPosition);

			// Calculate the distance to the end of the path
			float distanceToEnd = dir.magnitude + Mathf.Max(0, interpolator.remainingDistance);

			// Check if we have reached the target
			var prevTargetReached = reachedEndOfPath;
			reachedEndOfPath = distanceToEnd <= endReachedDistance && interpolator.valid;
			if (!prevTargetReached && reachedEndOfPath) OnTargetReached();
			float slowdown;

			// Normalized direction of where the agent is looking
			var forwards = movementPlane.ToPlane(simulatedRotation * (rotationIn2D ? Vector3.up : Vector3.forward));

			// Check if we have a valid path to follow and some other script has not stopped the character
			if (interpolator.valid && !isStopped) {
				// How fast to move depending on the distance to the destination.
				// Move slower as the character gets closer to the destination.
				// This is always a value between 0 and 1.
				slowdown = distanceToEnd < slowdownDistance ? Mathf.Sqrt(distanceToEnd / slowdownDistance) : 1;

				if (reachedEndOfPath && whenCloseToDestination == CloseToDestinationMode.Stop) {
					// Slow down as quickly as possible
					velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
				} else {
					velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir.ToUnityV2(), dir.normalized*maxSpeed, velocity2D, currentAcceleration, rotationSpeed, maxSpeed, forwards) * deltaTime;
				}
			} else {
				slowdown = 1;
				// Slow down as quickly as possible
				velocity2D -= Vector2.ClampMagnitude(velocity2D, currentAcceleration * deltaTime);
			}

			velocity2D = MovementUtilities.ClampVelocity(velocity2D, maxSpeed, slowdown, slowWhenNotFacingTarget, forwards.ToUnityV2());

			ApplyGravity(deltaTime);

			if (rvoController != null && rvoController.enabled) {
				// Send a message to the RVOController that we want to move
				// with this velocity. In the next simulation step, this
				// velocity will be processed and it will be fed back to the
				// rvo controller and finally it will be used by this script
				// when calling the CalculateMovementDelta method below

				// Make sure that we don't move further than to the end point
				// of the path. If the RVO simulation FPS is low and we did
				// not do this, the agent might overshoot the target a lot.
				var rvoTarget = currentPosition.ToPFV3() + movementPlane.ToWorld(Vector2.ClampMagnitude(velocity2D, distanceToEnd), 0f);
				rvoController.SetTarget(rvoTarget, velocity2D.magnitude, maxSpeed);
			}

			// Set how much the agent wants to move during this frame
			var delta2D = lastDeltaPosition = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			nextPosition = currentPosition + movementPlane.ToWorld(delta2D.ToPFV2(), verticalVelocity * lastDeltaTime).ToUnityV3();
			CalculateNextRotation(slowdown, out nextRotation);
		}

		protected virtual void CalculateNextRotation (float slowdown, out Quaternion nextRotation) {
			if (lastDeltaTime > 0.00001f) {
				Vector2 desiredRotationDirection;
				if (rvoController != null && rvoController.enabled) {
					// When using local avoidance, use the actual velocity we are moving with if that velocity
					// is high enough, otherwise fall back to the velocity that we want to move with (velocity2D).
					// The local avoidance velocity can be very jittery when the character is close to standing still
					// as it constantly makes small corrections. We do not want the rotation of the character to be jittery.
					var actualVelocity = lastDeltaPosition/lastDeltaTime;
					desiredRotationDirection = Vector2.Lerp(velocity2D, actualVelocity, 4 * actualVelocity.magnitude / (maxSpeed + 0.0001f));
				} else {
					desiredRotationDirection = velocity2D;
				}

				// Rotate towards the direction we are moving in.
				// Don't rotate when we are very close to the target.
				var currentRotationSpeed = rotationSpeed * Mathf.Max(0, (slowdown - 0.3f) / 0.7f);
				nextRotation = SimulateRotationTowards(desiredRotationDirection, currentRotationSpeed * lastDeltaTime);
			} else {
				// TODO: simulatedRotation
				nextRotation = rotation;
			}
		}

		static NNConstraint cachedNNConstraint = NNConstraint.Default;
		protected override Vector3 ClampToNavmesh (Vector3 position, out bool positionChanged) {
			if (constrainInsideGraph) {
				cachedNNConstraint.tags = seeker.traversableTags;
				cachedNNConstraint.graphMask = seeker.graphMask;
				cachedNNConstraint.distanceXZ = true;
				var clampedPosition = PathFindHelper.GetNearest(position, cachedNNConstraint).position;

				// We cannot simply check for equality because some precision may be lost
				// if any coordinate transformations are used.
				var difference = movementPlane.ToPlane(clampedPosition - position.ToPFV3());
				float sqrDifference = difference.sqrMagnitude;
				if (sqrDifference > 0.001f*0.001f) {
					// The agent was outside the navmesh. Remove that component of the velocity
					// so that the velocity only goes along the direction of the wall, not into it
					velocity2D -= difference.ToUnityV2() * Vector2.Dot(difference.ToUnityV2(), velocity2D) / sqrDifference;

					// Make sure the RVO system knows that there was a collision here
					// Otherwise other agents may think this agent continued
					// to move forwards and avoidance quality may suffer
					if (rvoController != null && rvoController.enabled) {
						rvoController.SetCollisionNormal(difference.ToUnityV2());
					}
					positionChanged = true;
					// Return the new position, but ignore any changes in the y coordinate from the ClampToNavmesh method as the y coordinates in the navmesh are rarely very accurate
					return position + movementPlane.ToWorld(difference).ToUnityV3();
				}
			}

			positionChanged = false;
			return position;
		}

	#if UNITY_EDITOR
		[System.NonSerialized]
		int gizmoHash = 0;

		[System.NonSerialized]
		float lastChangedTime = float.NegativeInfinity;

		protected static readonly Color GizmoColor = new Color(46.0f/255, 104.0f/255, 201.0f/255);

		protected override void OnDrawGizmos () {
			base.OnDrawGizmos();
			if (alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		protected override void OnDrawGizmosSelected () {
			base.OnDrawGizmosSelected();
			if (!alwaysDrawGizmos) OnDrawGizmosInternal();
		}

		void OnDrawGizmosInternal () {
			var newGizmoHash = pickNextWaypointDist.GetHashCode() ^ slowdownDistance.GetHashCode() ^ endReachedDistance.GetHashCode();

			if (newGizmoHash != gizmoHash && gizmoHash != 0) lastChangedTime = Time.realtimeSinceStartup;
			gizmoHash = newGizmoHash;
			float alpha = alwaysDrawGizmos ? 1 : Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - 5f)/0.5f) * (UnityEditor.Selection.gameObjects.Length == 1 ? 1 : 0);

			if (alpha > 0) {
				// Make sure the scene view is repainted while the gizmos are visible
				if (!alwaysDrawGizmos) UnityEditor.SceneView.RepaintAll();
				Draw.Gizmos.Line(position, steeringTarget, GizmoColor * new Color(1, 1, 1, alpha));
				Gizmos.matrix = Matrix4x4.TRS(position, transform.rotation * (rotationIn2D ? Quaternion.Euler(-90, 0, 0) : Quaternion.identity), Vector3.one);
				Draw.Gizmos.CircleXZ(Vector3.zero, pickNextWaypointDist, GizmoColor * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, slowdownDistance, Color.Lerp(GizmoColor, Color.red, 0.5f) * new Color(1, 1, 1, alpha));
				Draw.Gizmos.CircleXZ(Vector3.zero, endReachedDistance, Color.Lerp(GizmoColor, Color.red, 0.8f) * new Color(1, 1, 1, alpha));
			}
		}
	#endif

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			base.OnUpgradeSerializedData(version, unityThread);
			// Approximately convert from a damping value to a degrees per second value.
			if (version < 1) rotationSpeed *= 90;
			return 2;
		}
	}
}
