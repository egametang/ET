using PF;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinding {
	/** Common interface for all movement scripts in the A* Pathfinding Project.
	 * \see #Pathfinding.AIPath
	 * \see #Pathfinding.RichAI
	 * \see #Pathfinding.AILerp
	  */
	public interface IAstarAI {
		/** Position of the agent.
		 * In world space.
		 * \see #rotation
		 */
		Vector3 position { get; }

		/** Rotation of the agent.
		 * In world space.
		 * \see #position
		 */
		Quaternion rotation { get; }

		/** Max speed in world units per second */
		float maxSpeed { get; set; }

		/** Actual velocity that the agent is moving with.
		 * In world units per second.
		 *
		 * \see #desiredVelocity
		 */
		Vector3 velocity { get; }

		/** Velocity that this agent wants to move with.
		 * Includes gravity and local avoidance if applicable.
		 * In world units per second.
		 *
		 * \see #velocity
		 */
		Vector3 desiredVelocity { get; }

		/** Remaining distance along the current path to the end of the path.
		 * For the RichAI movement script this may not always be precisely known, especially when
		 * far away from the destination. In those cases an approximate distance will be returned.
		 *
		 * If the agent does not currently have a path, then positive infinity will be returned.
		 *
		 * \see #reachedEndOfPath
		 */
		float remainingDistance { get; }

		/** True if the agent has reached the end of the current path.
		 * This is the approximate distance the AI has to move to reach the end of the path it is currently traversing.
		 *
		 * Note that setting the #destination does not immediately update the path, nor is there any guarantee that the
		 * AI will actually be able to reach the destination that you set. The AI will try to get as close as possible.
		 *
		 * It is very hard to provide a method for detecting if the AI has reached the #destination that works across all different games
		 * because the destination may not even lie on the navmesh and how that is handled differs from game to game (see also the code snippet in the docs for #destination).
		 *
		 * \see #remainingDistance
		 */
		bool reachedEndOfPath { get; }

		/** Position in the world that this agent should move to.
		 *
		 * If no destination has been set yet, then (+infinity, +infinity, +infinity) will be returned.
		 *
		 * Note that setting this property does not immediately cause the agent to recalculate its path.
		 * So it may take some time before the agent starts to move towards this point.
		 * Most movement scripts have a \a repathRate field which indicates how often the agent looks
		 * for a new path. You can also call the #SearchPath method to immediately
		 * start to search for a new path. Paths are calculated asynchronously so when an agent starts to
		 * search for path it may take a few frames (usually 1 or 2) until the result is available.
		 * During this time the #pathPending property will return true.
		 *
		 * If you are setting a destination and then want to know when the agent has reached that destination
		 * then you should check both #pathPending and #reachedEndOfPath:
		 * \snippet MiscSnippets.cs IAstarAI.destination
		 */
		Vector3 destination { get; set; }

		/** Enables or disables recalculating the path at regular intervals.
		 * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
		 *
		 * Note that this only disables automatic path recalculations. If you call the #SearchPath() method a path will still be calculated.
		 *
		 * \see #canMove
		 * \see #isStopped
		 */
		bool canSearch { get; set; }

		/** Enables or disables movement completely.
		 * If you want the agent to stand still, but still react to local avoidance and use gravity: use #isStopped instead.
		 *
		 * This is also useful if you want to have full control over when the movement calculations run.
		 * Take a look at #MovementUpdate
		 *
		 * \see #canSearch
		 * \see #isStopped
		 */
		bool canMove { get; set; }

		/** True if this agent currently has a path that it follows */
		bool hasPath { get; }

		/** True if a path is currently being calculated */
		bool pathPending { get; }

		/** Gets or sets if the agent should stop moving.
		 * If this is set to true the agent will immediately start to slow down as quickly as it can to come to a full stop.
		 * The agent will still react to local avoidance and gravity (if applicable), but it will not try to move in any particular direction.
		 *
		 * The current path of the agent will not be cleared, so when this is set
		 * to false again the agent will continue moving along the previous path.
		 *
		 * This is a purely user-controlled parameter, so for example it is not set automatically when the agent stops
		 * moving because it has reached the target. Use #reachedEndOfPath for that.
		 *
		 * If this property is set to true while the agent is traversing an off-mesh link (RichAI script only), then the agent will
		 * continue traversing the link and stop once it has completed it.
		 *
		 * \note This is not the same as the #canMove setting which some movement scripts have. The #canMove setting
		 * disables movement calculations completely (which among other things makes it not be affected by local avoidance or gravity).
		 * For the AILerp movement script which doesn't use gravity or local avoidance anyway changing this property is very similar to
		 * changing #canMove.
		 *
		 * The #steeringTarget property will continue to indicate the point which the agent would move towards if it would not be stopped.
		 */
		bool isStopped { get; set; }

		/** Point on the path which the agent is currently moving towards.
		 * This is usually a point a small distance ahead of the agent
		 * or the end of the path.
		 *
		 * If the agent does not have a path at the moment, then the agent's current position will be returned.
		 */
		Vector3 steeringTarget { get; }

		/** Called when the agent recalculates its path.
		 * This is called both for automatic path recalculations (see #canSearch) and manual ones (see #SearchPath).
		 *
		 * \see Take a look at the #Pathfinding.AIDestinationSetter source code for an example of how it can be used.
		 */
		System.Action onSearchPath { get; set; }

		/** Recalculate the current path.
		 * You can for example use this if you want very quick reaction times when you have changed the #destination
		 * so that the agent does not have to wait until the next automatic path recalculation (see #canSearch).
		 *
		 * If there is an ongoing path calculation, it will be canceled, so make sure you leave time for the paths to get calculated before calling this function again.
		 * A canceled path will show up in the log with the message "Canceled by script" (see #Seeker.CancelCurrentPathRequest()).
		 *
		 * If no #destination has been set yet then nothing will be done.
		 *
		 * \note The path result may not become available until after a few frames.
		 * During the calculation time the #pathPending property will return true.
		 *
		 * \see #pathPending
		 */
		void SearchPath ();

		/** Make the AI follow the specified path.
		 * In case the path has not been calculated, the script will call seeker.StartPath to calculate it.
		 * This means the AI may not actually start to follow the path until in a few frames when the path has been calculated.
		 * The #pathPending field will as usual return true while the path is being calculated.
		 *
		 * In case the path has already been calculated it will immediately replace the current path the AI is following.
		 * This is useful if you want to replace how the AI calculates its paths.
		 * Note that if you calculate the path using seeker.StartPath then this script will already pick it up because it is listening for
		 * all paths that the Seeker finishes calculating. In that case you do not need to call this function.
		 *
		 * You can disable the automatic path recalculation by setting the #canSearch field to false.
		 *
		 * \snippet MiscSnippets.cs IAstarAI.SetPath
		 */
		void SetPath (Path path);

		/** Instantly move the agent to a new position.
		 * This will trigger a path recalculation (if \a clearPath is true, which is the default) so if you want to teleport the agent and change its #destination
		 * it is recommended that you set the #destination before calling this method.
		 *
		 * The current path will be cleared by default.
		 *
		 * \see Works similarly to Unity's NavmeshAgent.Warp.
		 * \see #SearchPath
		 */
		void Teleport (Vector3 newPosition, bool clearPath = true);

		/** Move the agent.
		 * \param deltaPosition Direction and distance to move the agent in world space.
		 *
		 * This is intended for external movement forces such as those applied by wind, conveyor belts, knockbacks etc.
		 *
		 * Some movement scripts may ignore this completely (notably the AILerp script) if it does not have
		 * any concept of being moved externally.
		 *
		 * The agent will not be moved immediately when calling this method. Instead this offset will be stored and then
		 * applied the next time the agent runs its movement calculations (which is usually later this frame or the next frame).
		 * If you want to move the agent immediately then call:
		 * \code
		 * ai.Move(someVector);
		 * ai.FinalizeMovement(ai.position, ai.rotation);
		 * \endcode
		 */
		void Move (Vector3 deltaPosition);

		/** Calculate how the character wants to move during this frame.
		 * \param deltaTime time to simulate movement for. Usually set to Time.deltaTime.
		 * \param nextPosition the position that the agent wants to move to during this frame.
		 * \param nextRotation the rotation that the agent wants to rotate to during this frame.
		 *
		 * Note that this does not actually move the character. You need to call #FinalizeMovement for that.
		 * This is called automatically unless #canMove is false.
		 *
		 * To handle movement yourself you can disable #canMove and call this method manually.
		 * This code will replicate the normal behavior of the component:
		 * \snippet MiscSnippets.cs IAstarAI.MovementUpdate
		 */
		void MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation);

		/** Move the agent.
		 * To be called as the last step when you are handling movement manually.
		 *
		 * The movement will be clamped to the navmesh if applicable (this is done for the RichAI movement script).
		 *
		 * \see #MovementUpdate for a code example.
		 */
		void FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation);
	}
}
