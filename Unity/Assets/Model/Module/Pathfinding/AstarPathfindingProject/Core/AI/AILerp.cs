using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PF;
using Mathf = UnityEngine.Mathf;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	using Pathfinding.Util;

	/** Linearly interpolating movement script.
	 * This movement script will follow the path exactly, it uses linear interpolation to move between the waypoints in the path.
	 * This is desirable for some types of games.
	 * It also works in 2D.
	 *
	 * \see You can see an example of this script in action in the example scene called \a Example15_2D.
	 *
	 * \section rec Configuration
	 * \subsection rec-snapped Recommended setup for movement along connections
	 *
	 * This depends on what type of movement you are aiming for.
	 * If you are aiming for movement where the unit follows the path exactly and move only along the graph connections on a grid/point graph.
	 * I recommend that you adjust the StartEndModifier on the Seeker component: set the 'Start Point Snapping' field to 'NodeConnection' and the 'End Point Snapping' field to 'SnapToNode'.
	 * \shadowimage{ailerp_recommended_snapped.png}
	 * \shadowimage{ailerp_snapped_movement.png}
	 *
	 * \subsection rec-smooth Recommended setup for smooth movement
	 * If you on the other hand want smoother movement I recommend setting 'Start Point Snapping' and 'End Point Snapping' to 'ClosestOnNode' and to add the Simple Smooth Modifier to the GameObject as well.
	 * Alternatively you can use the \link Pathfinding.FunnelModifier Funnel Modifier\endlink which works better on navmesh/recast graphs or the \link Pathfinding.RaycastModifier RaycastModifier\endlink.
	 *
	 * You should not combine the Simple Smooth Modifier or the Funnel Modifier with the NodeConnection snapping mode. This may lead to very odd behavior.
	 *
	 * \shadowimage{ailerp_recommended_smooth.png}
	 * \shadowimage{ailerp_smooth_movement.png}
	 * You may also want to tweak the #rotationSpeed.
	 *
	 * \ingroup movementscripts
	 */
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/AI/AILerp (2D,3D)")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_lerp.php")]
	public class AILerp : VersionedMonoBehaviour, IAstarAI {
		/** Determines how often it will search for new paths.
		 * If you have fast moving targets or AIs, you might want to set it to a lower value.
		 * The value is in seconds between path requests.
		 */
		public float repathRate = 0.5F;

		/** \copydoc Pathfinding::IAstarAI::canSearch */
		public bool canSearch = true;

		/** \copydoc Pathfinding::IAstarAI::canMove */
		public bool canMove = true;

		/** Speed in world units */
		public float speed = 3;

		/** If true, the AI will rotate to face the movement direction */
		public bool enableRotation = true;

		/** If true, rotation will only be done along the Z axis so that the Y axis is the forward direction of the character.
		 * This is useful for 2D games in which one often want to have the Y axis as the forward direction to get sprites and 2D colliders to work properly.
		 * \shadowimage{aibase_forward_axis.png}
		 */
		public bool rotationIn2D = false;

		/** How quickly to rotate */
		public float rotationSpeed = 10;

		/** If true, some interpolation will be done when a new path has been calculated.
		 * This is used to avoid short distance teleportation.
		 */
		public bool interpolatePathSwitches = true;

		/** How quickly to interpolate to the new path */
		public float switchPathInterpolationSpeed = 5;

		/** True if the end of the current path has been reached */
		public bool reachedEndOfPath { get; private set; }

		public Vector3 destination { get; set; }

		/** Determines if the character's position should be coupled to the Transform's position.
		 * If false then all movement calculations will happen as usual, but the object that this component is attached to will not move
		 * instead only the #position property will change.
		 *
		 * \see #canMove which in contrast to this field will disable all movement calculations.
		 * \see #updateRotation
		 */
		[System.NonSerialized]
		public bool updatePosition = true;

		/** Determines if the character's rotation should be coupled to the Transform's rotation.
		 * If false then all movement calculations will happen as usual, but the object that this component is attached to will not rotate
		 * instead only the #rotation property will change.
		 *
		 * \see #updatePosition
		 */
		[System.NonSerialized]
		public bool updateRotation = true;

		/** Target to move towards.
		 * The AI will try to follow/move towards this target.
		 * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
		 *
		 * \deprecated In 4.0 this will automatically add a \link Pathfinding.AIDestinationSetter AIDestinationSetter\endlink component and set the target on that component.
		 * Try instead to use the #destination property which does not require a transform to be created as the target or use
		 * the AIDestinationSetter component directly.
		 */
		[System.Obsolete("Use the destination property or the AIDestinationSetter component instead")]
		public Transform target {
			get {
				var setter = GetComponent<AIDestinationSetter>();
				return setter != null ? setter.target : null;
			}
			set {
				targetCompatibility = null;
				var setter = GetComponent<AIDestinationSetter>();
				if (setter == null) setter = gameObject.AddComponent<AIDestinationSetter>();
				setter.target = value;
				destination = value != null ? value.position : new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			}
		}

		/** \copydoc Pathfinding::IAstarAI::position */
		public Vector3 position { get { return updatePosition ? tr.position : simulatedPosition; } }

		/** \copydoc Pathfinding::IAstarAI::rotation */
		public Quaternion rotation { get { return updateRotation ? tr.rotation : simulatedRotation; } }

		#region IAstarAI implementation

		/** \copydoc Pathfinding::IAstarAI::Move */
		void IAstarAI.Move (Vector3 deltaPosition) {
			// This script does not know the concept of being away from the path that it is following
			// so this call will be ignored (as is also mentioned in the documentation).
		}

		/** \copydoc Pathfinding::IAstarAI::maxSpeed */
		float IAstarAI.maxSpeed { get { return speed; } set { speed = value; } }

		/** \copydoc Pathfinding::IAstarAI::canSearch */
		bool IAstarAI.canSearch { get { return canSearch; } set { canSearch = value; } }

		/** \copydoc Pathfinding::IAstarAI::canMove */
		bool IAstarAI.canMove { get { return canMove; } set { canMove = value; } }

		Vector3 IAstarAI.velocity {
			get {
				return Time.deltaTime > 0.00001f ? (previousPosition1 - previousPosition2) / Time.deltaTime : Vector3.zero;
			}
		}

		Vector3 IAstarAI.desiredVelocity {
			get {
				// The AILerp script sets the position every frame. It does not take into account physics
				// or other things. So the velocity should always be the same as the desired velocity.
				return (this as IAstarAI).velocity;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::steeringTarget */
		Vector3 IAstarAI.steeringTarget {
			get {
				// AILerp doesn't use steering at all, so we will just return a point ahead of the agent in the direction it is moving.
				return interpolator.valid ? interpolator.position + interpolator.tangent : simulatedPosition;
			}
		}
		#endregion

		public float remainingDistance {
			get {
				return Mathf.Max(interpolator.remainingDistance, 0);
			}
			set {
				interpolator.remainingDistance = Mathf.Max(value, 0);
			}
		}

		public bool hasPath {
			get {
				return interpolator.valid;
			}
		}

		public bool pathPending {
			get {
				return !canSearchAgain;
			}
		}

		/** \copydoc Pathfinding::IAstarAI::isStopped */
		public bool isStopped { get; set; }

		/** \copydoc Pathfinding::IAstarAI::onSearchPath */
		public System.Action onSearchPath { get; set; }

		/** Cached Seeker component */
		protected Seeker seeker;

		/** Cached Transform component */
		protected Transform tr;

		/** Time when the last path request was sent */
		protected float lastRepath = -9999;

		/** Current path which is followed */
		protected ABPath path;

		/** Only when the previous path has been returned should a search for a new path be done */
		protected bool canSearchAgain = true;

		/** When a new path was returned, the AI was moving along this ray.
		 * Used to smoothly interpolate between the previous movement and the movement along the new path.
		 * The speed is equal to movement direction.
		 */
		protected Vector3 previousMovementOrigin;
		protected Vector3 previousMovementDirection;

		/** Time since the path was replaced by a new path.
		 * \see #interpolatePathSwitches
		 */
		protected float pathSwitchInterpolationTime = 0;

		protected PathInterpolator interpolator = new PathInterpolator();


		/** Holds if the Start function has been run.
		 * Used to test if coroutines should be started in OnEnable to prevent calculating paths
		 * in the awake stage (or rather before start on frame 0).
		 */
		bool startHasRun = false;

		Vector3 previousPosition1, previousPosition2, simulatedPosition;
		Quaternion simulatedRotation;

		/** Required for serialization backward compatibility */
		[UnityEngine.Serialization.FormerlySerializedAs("target")][SerializeField][HideInInspector]
		Transform targetCompatibility;

		protected AILerp () {
			// Note that this needs to be set here in the constructor and not in e.g Awake
			// because it is possible that other code runs and sets the destination property
			// before the Awake method on this script runs.
			destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		/** Initializes reference variables.
		 * If you override this function you should in most cases call base.Awake () at the start of it.
		 * */
		protected override void Awake () {
			base.Awake();
			//This is a simple optimization, cache the transform component lookup
			tr = transform;

			seeker = GetComponent<Seeker>();

			// Tell the StartEndModifier to ask for our exact position when post processing the path This
			// is important if we are using prediction and requesting a path from some point slightly ahead
			// of us since then the start point in the path request may be far from our position when the
			// path has been calculated. This is also good because if a long path is requested, it may take
			// a few frames for it to be calculated so we could have moved some distance during that time
			seeker.startEndModifier.adjustStartPoint = () => simulatedPosition;
		}

		/** Starts searching for paths.
		 * If you override this function you should in most cases call base.Start () at the start of it.
		 * \see #Init
		 * \see #RepeatTrySearchPath
		 */
		protected virtual void Start () {
			startHasRun = true;
			Init();
		}

		/** Called when the component is enabled */
		protected virtual void OnEnable () {
			// Make sure we receive callbacks when paths complete
			seeker.pathCallback += OnPathComplete;
			Init();
		}

		void Init () {
			if (startHasRun) {
				// The Teleport call will make sure some variables are properly initialized (like #prevPosition1 and #prevPosition2)
				Teleport(position, false);
				lastRepath = float.NegativeInfinity;
				if (shouldRecalculatePath) SearchPath();
			}
		}

		public void OnDisable () {
			// Abort any calculations in progress
			if (seeker != null) seeker.CancelCurrentPathRequest();
			canSearchAgain = true;

			// Release current path so that it can be pooled
			if (path != null) path.Release(this);
			path = null;
			interpolator.SetPath(null);

			// Make sure we no longer receive callbacks when paths complete
			seeker.pathCallback -= OnPathComplete;
		}

		public void Teleport (Vector3 position, bool clearPath = true) {
			if (clearPath) interpolator.SetPath(null);
			simulatedPosition = previousPosition1 = previousPosition2 = position;
			if (updatePosition) tr.position = position;
			reachedEndOfPath = false;
			if (clearPath) SearchPath();
		}

		/** True if the path should be automatically recalculated as soon as possible */
		protected virtual bool shouldRecalculatePath {
			get {
				return Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && !float.IsPositiveInfinity(destination.x);
			}
		}

		/** Requests a path to the target.
		 * \deprecated Use #SearchPath instead.
		 */
		[System.Obsolete("Use SearchPath instead")]
		public virtual void ForceSearchPath () {
			SearchPath();
		}

		/** Requests a path to the target.
		 */
		public virtual void SearchPath () {
			if (float.IsPositiveInfinity(destination.x)) return;
			if (onSearchPath != null) onSearchPath();

			lastRepath = Time.time;

			// This is where the path should start to search from
			var currentPosition = GetFeetPosition();

			// If we are following a path, start searching from the node we will
			// reach next this can prevent odd turns right at the start of the path
			/*if (interpolator.valid) {
			    var prevDist = interpolator.distance;
			    // Move to the end of the current segment
			    interpolator.MoveToSegment(interpolator.segmentIndex, 1);
			    currentPosition = interpolator.position;
			    // Move back to the original position
			    interpolator.distance = prevDist;
			}*/

			canSearchAgain = false;

			// Alternative way of creating a path request
			//ABPath p = ABPath.Construct(currentPosition, targetPoint, null);
			//seeker.StartPath(p);

			// Create a new path request
			// The OnPathComplete method will later be called with the result
			seeker.StartPath(currentPosition, destination);
		}

		/** The end of the path has been reached.
		 * If you want custom logic for when the AI has reached it's destination
		 * add it here.
		 * You can also create a new script which inherits from this one
		 * and override the function in that script.
		 */
		public virtual void OnTargetReached () {
		}

		/** Called when a requested path has finished calculation.
		 * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
		 * Finally it is returned to the seeker which forwards it to this function.
		 */
		protected virtual void OnPathComplete (Path _p) {
			ABPath p = _p as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			canSearchAgain = true;

			// Increase the reference count on the path.
			// This is used for path pooling
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				return;
			}

			if (interpolatePathSwitches) {
				ConfigurePathSwitchInterpolation();
			}


			// Replace the old path
			var oldPath = path;
			path = p;
			reachedEndOfPath = false;

			// Just for the rest of the code to work, if there
			// is only one waypoint in the path add another one
			if (path.vectorPath != null && path.vectorPath.Count == 1) {
				path.vectorPath.Insert(0, GetFeetPosition());
			}

			// Reset some variables
			ConfigureNewPath();

			// Release the previous path
			// This is used for path pooling.
			// This is done after the interpolator has been configured in the ConfigureNewPath method
			// as this method would otherwise invalidate the interpolator
			// since the vectorPath list (which the interpolator uses) will be pooled.
			if (oldPath != null) oldPath.Release(this);

			if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath) {
				reachedEndOfPath = true;
				OnTargetReached();
			}
		}

		/** \copydoc Pathfinding::IAstarAI::SetPath */
		public void SetPath (Path path) {
			if (path.PipelineState == PathState.Created) {
				// Path has not started calculation yet
				lastRepath = Time.time;
				canSearchAgain = false;
				seeker.CancelCurrentPathRequest();
				seeker.StartPath(path);
			} else if (path.PipelineState == PathState.Returned) {
				// Path has already been calculated

				// We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
				if (seeker.GetCurrentPath() != path) seeker.CancelCurrentPathRequest();
				else throw new System.ArgumentException("If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

				OnPathComplete(path);
			} else {
				// Path calculation has been started, but it is not yet complete. Cannot really handle this.
				throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
			}
		}

		protected virtual void ConfigurePathSwitchInterpolation () {
			bool reachedEndOfPreviousPath = interpolator.valid && interpolator.remainingDistance < 0.0001f;

			if (interpolator.valid && !reachedEndOfPreviousPath) {
				previousMovementOrigin = interpolator.position;
				previousMovementDirection = interpolator.tangent.normalized * interpolator.remainingDistance;
				pathSwitchInterpolationTime = 0;
			} else {
				previousMovementOrigin = Vector3.zero;
				previousMovementDirection = Vector3.zero;
				pathSwitchInterpolationTime = float.PositiveInfinity;
			}
		}

		public virtual Vector3 GetFeetPosition () {
			return position;
		}

		/** Finds the closest point on the current path and configures the #interpolator */
		protected virtual void ConfigureNewPath () {
			var hadValidPath = interpolator.valid;
			var prevTangent = hadValidPath ? interpolator.tangent : Vector3.zero;

			interpolator.SetPath(path.vectorPath);
			interpolator.MoveToClosestPoint(GetFeetPosition());

			if (interpolatePathSwitches && switchPathInterpolationSpeed > 0.01f && hadValidPath) {
				var correctionFactor = Mathf.Max(-Vector3.Dot(prevTangent.normalized, interpolator.tangent.normalized), 0);
				interpolator.distance -= speed*correctionFactor*(1f/switchPathInterpolationSpeed);
			}
		}

		protected virtual void Update () {
			if (shouldRecalculatePath) SearchPath();
			if (canMove) {
				Vector3 nextPosition;
				Quaternion nextRotation;
				MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);
				FinalizeMovement(nextPosition, nextRotation);
			}
		}

		/** \copydoc Pathfinding::IAstarAI::MovementUpdate */
		public void MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			if (updatePosition) simulatedPosition = tr.position;
			if (updateRotation) simulatedRotation = tr.rotation;

			Vector3 direction;

			nextPosition = CalculateNextPosition(out direction, isStopped ? 0f : deltaTime);

			if (enableRotation) nextRotation = SimulateRotationTowards(direction, deltaTime);
			else nextRotation = simulatedRotation;
		}

		/** \copydoc Pathfinding::IAstarAI::FinalizeMovement */
		public void FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			previousPosition2 = previousPosition1;
			previousPosition1 = simulatedPosition = nextPosition;
			simulatedRotation = nextRotation;
			if (updatePosition) tr.position = nextPosition;
			if (updateRotation) tr.rotation = nextRotation;
		}

		Quaternion SimulateRotationTowards (Vector3 direction, float deltaTime) {
			// Rotate unless we are really close to the target
			if (direction != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(direction, rotationIn2D ? Vector3.back : Vector3.up);
				// This causes the character to only rotate around the Z axis
				if (rotationIn2D) targetRotation *= Quaternion.Euler(90, 0, 0);
				return Quaternion.Slerp(simulatedRotation, targetRotation, deltaTime * rotationSpeed);
			}
			return simulatedRotation;
		}

		/** Calculate the AI's next position (one frame in the future).
		 * \param direction The tangent of the segment the AI is currently traversing. Not normalized.
		 */
		protected virtual Vector3 CalculateNextPosition (out Vector3 direction, float deltaTime) {
			if (!interpolator.valid) {
				direction = Vector3.zero;
				return simulatedPosition;
			}

			interpolator.distance += deltaTime * speed;

			if (interpolator.remainingDistance < 0.0001f && !reachedEndOfPath) {
				reachedEndOfPath = true;
				OnTargetReached();
			}

			direction = interpolator.tangent;
			pathSwitchInterpolationTime += deltaTime;
			var alpha = switchPathInterpolationSpeed * pathSwitchInterpolationTime;
			if (interpolatePathSwitches && alpha < 1f) {
				// Find the approximate position we would be at if we
				// would have continued to follow the previous path
				Vector3 positionAlongPreviousPath = previousMovementOrigin + Vector3.ClampMagnitude(previousMovementDirection, speed * pathSwitchInterpolationTime);

				// Interpolate between the position on the current path and the position
				// we would have had if we would have continued along the previous path.
				return Vector3.Lerp(positionAlongPreviousPath, interpolator.position, alpha);
			} else {
				return interpolator.position;
			}
		}

		protected override int OnUpgradeSerializedData (int version, bool unityThread) {
			#pragma warning disable 618
			if (unityThread && targetCompatibility != null) target = targetCompatibility;
			#pragma warning restore 618
			return 2;
		}
	}
}
