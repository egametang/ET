using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Pathfinding.RVO.Sampled;

#if NETFX_CORE
using Thread = Pathfinding.WindowsStore.Thread;
using ThreadStart = Pathfinding.WindowsStore.ThreadStart;
#else
using Thread = System.Threading.Thread;
using ThreadStart = System.Threading.ThreadStart;
#endif

/** Local avoidance related classes */
namespace Pathfinding.RVO {
	/** Exposes properties of an Agent class.
	 *
	 * \see RVOController
	 * \see RVOSimulator
	 *
	 * \astarpro
	 */
	public interface IAgent {
		/** Position of the agent.
		 * The agent does not move by itself, a movement script has to be responsible for
		 * reading the CalculatedTargetPoint and CalculatedSpeed properties and move towards that point with that speed.
		 * This property should ideally be set every frame.
		 *
		 * Note that this is a Vector2, not a Vector3 since the RVO simulates everything internally in 2D. So if your agents move in the
		 * XZ plane you may have to convert it to a Vector3 like this.
		 *
		 * \code
		 * Vector3 position3D = new Vector3(agent.Position.x, agent.ElevationCoordinate, agent.Position.y);
		 * \endcode
		 */
		Vector2 Position { get; set; }

		/** Coordinate which separates characters in the height direction.
		 * Since RVO can be used either in 2D or 3D, it is not as simple as just using the y coordinate of the 3D position.
		 * In 3D this will most likely be set to the y coordinate, but in 2D (top down) it should in most cases be set to 0 since
		 * all characters are always in the same plane, however it may be set to some other value, for example if the game
		 * is 2D isometric.
		 *
		 * The position is assumed to be at the base of the character (near the feet).
		 */
		float ElevationCoordinate { get; set; }

		/** Optimal point to move towards to avoid collisions.
		 * The movement script should move towards this point with a speed of #CalculatedSpeed.
		 *
		 * \note This is a Vector2, not a Vector3 as that is what the #SetTarget method accepts.
		 *
		 * \see RVOController.CalculateMovementDelta.
		 */
		Vector2 CalculatedTargetPoint { get; }

		/** Optimal speed of the agent to avoid collisions.
		 * The movement script should move towards #CalculatedTargetPoint with this speed.
		 */
		float CalculatedSpeed { get; }

		/** Point towards which the agent should move.
		 * Usually you set this once per frame. The agent will try move as close to the target point as possible.
		 * Will take effect at the next simulation step.
		 *
		 * \note The system assumes that the agent will stop when it reaches the target point
		 * so if you just want to move the agent in a particular direction, make sure that you set the target point
		 * a good distance in front of the character as otherwise the system may not avoid colisions that well.
		 * What would happen is that the system (in simplified terms) would think that the agents would stop
		 * before the collision and thus it wouldn't slow down or change course. See the image below.
		 * In the image the desiredSpeed is the length of the blue arrow and the target point
		 * is the point where the black arrows point to.
		 * In the upper case the agent does not avoid the red agent (you can assume that the red
		 * agent has a very small velocity for simplicity) while in the lower case it does.\n
		 * If you are following a path a good way to pick the target point is to set it to
		 * \code
		 * targetPoint = directionToNextWaypoint.normalized * remainingPathDistance
		 * \endcode
		 * Where remainingPathDistance is the distance until the character would reach the end of the path.
		 * This works well because at the end of the path the direction to the next waypoint will just be the
		 * direction to the last point on the path and remainingPathDistance will be the distance to the last point
		 * in the path, so targetPoint will be set to simply the last point in the path. However when remainingPathDistance
		 * is large the target point will be so far away that the agent will essentially be told to move in a particular
		 * direction, which is precisely what we want.
		 * \shadowimage{rvo/rvo_target_point.png}
		 *
		 * \param targetPoint
		 *      Target point in world space (XZ plane or XY plane depending on if the simulation is configured for 2D or 3D).
		 *      Note that this is a Vector2, not a Vector3 since the system simulates everything internally in 2D. So if your agents move in the
		 *      XZ plane you will have to supply it as a Vector2 with (x,z) coordinates.
		 * \param desiredSpeed
		 *      Desired speed of the agent. In world units per second. The agent will try to move with this
		 *      speed if possible.
		 * \param maxSpeed
		 *      Max speed of the agent. In world units per second. If necessary (for example if another agent
		 *      is on a collision trajectory towards this agent) the agent can move at this speed.
		 *      Should be at least as high as desiredSpeed, but it is recommended to use a slightly
		 *      higher value than desiredSpeed (for example desiredSpeed*1.2).
		 */
		void SetTarget (Vector2 targetPoint, float desiredSpeed, float maxSpeed);

		/** Locked agents will be assumed not to move */
		bool Locked { get; set; }

		/** Radius of the agent in world units.
		 * Agents are modelled as circles/cylinders.
		 */
		float Radius { get; set; }

		/** Height of the agent in world units.
		 * Agents are modelled as circles/cylinders.
		 */
		float Height { get; set; }

		/** Max number of estimated seconds to look into the future for collisions with agents.
		 * As it turns out, this variable is also very good for controling agent avoidance priorities.
		 * Agents with lower values will avoid other agents less and thus you can make 'high priority agents' by
		 * giving them a lower value.
		 */
		float AgentTimeHorizon { get; set; }

		/** Max number of estimated seconds to look into the future for collisions with obstacles */
		float ObstacleTimeHorizon { get; set; }

		/** Max number of agents to take into account.
		 * Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation.
		 */
		int MaxNeighbours { get; set; }

		/** Number of neighbours that the agent took into account during the last simulation step */
		int NeighbourCount { get; }

		/** Specifies the avoidance layer for this agent.
		 * The #CollidesWith mask on other agents will determine if they will avoid this agent.
		 */
		RVOLayer Layer { get; set; }

		/** Layer mask specifying which layers this agent will avoid.
		 * You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		 *
		 * \see http://en.wikipedia.org/wiki/Mask_(computing)
		 */
		RVOLayer CollidesWith { get; set; }

		/** Draw debug information.
		 *
		 * \note Will always draw debug info in the XZ plane even if #Pathfinding.RVO.Simulator.movementPlane is set to XY.
		 * \note Ignored if multithreading on the simulator component has been enabled
		 * since Unity's Debug API can only be called from the main thread.
		 */
		bool DebugDraw { get; set; }

		/** List of obstacle segments which were close to the agent during the last simulation step.
		 * Can be used to apply additional wall avoidance forces for example.
		 * Segments are formed by the obstacle vertex and its .next property.
		 *
		 * \bug Always returns null
		 */
		[System.Obsolete()]
		List<ObstacleVertex> NeighbourObstacles { get; }

		/** How strongly other agents will avoid this agent.
		 * Usually a value between 0 and 1.
		 * Agents with similar priorities will avoid each other with an equal strength.
		 * If an agent sees another agent with a higher priority than itself it will avoid that agent more strongly.
		 * In the extreme case (e.g this agent has a priority of 0 and the other agent has a priority of 1) it will treat the other agent as being a moving obstacle.
		 * Similarly if an agent sees another agent with a lower priority than itself it will avoid that agent less.
		 *
		 * In general the avoidance strength for this agent is:
		 * \code
		 * if this.priority > 0 or other.priority > 0:
		 *     avoidanceStrength = other.priority / (this.priority + other.priority);
		 * else:
		 *     avoidanceStrength = 0.5
		 * \endcode
		 */
		float Priority { get; set; }

		/** Callback which will be called right before avoidance calculations are started.
		 * Used to update the other properties with the most up to date values
		 */
		System.Action PreCalculationCallback { set; }

		/** Set the normal of a wall (or something else) the agent is currently colliding with.
		 * This is used to make the RVO system aware of things like physics or an agent being clamped to the navmesh.
		 * The velocity of this agent that other agents observe will be modified so that there is no component
		 * into the wall. The agent will however not start to avoid the wall, for that you will need to add RVO obstacles.
		 *
		 * This value will be cleared after the next simulation step, normally it should be set every frame
		 * when the collision is still happening.
		 */
		void SetCollisionNormal (Vector2 normal);

		/** Set the current velocity of the agent.
		 * This will override the local avoidance input completely.
		 * It is useful if you have a player controlled character and want other agents to avoid it.
		 *
		 * Calling this method will mark the agent as being externally controlled for 1 simulation step.
		 * Local avoidance calculations will be skipped for the next simulation step but will be resumed
		 * after that unless this method is called again.
		 */
		void ForceSetVelocity (Vector2 velocity);
	}

	/** Plane which movement is primarily happening in */
	public enum MovementPlane {
		/** Movement happens primarily in the XZ plane (3D) */
		XZ,
		/** Movement happens primarily in the XY plane (2D) */
		XY
	}

	[System.Flags]
	public enum RVOLayer {
		DefaultAgent = 1 << 0,
		DefaultObstacle = 1 << 1,
		Layer2 = 1 << 2,
		Layer3 = 1 << 3,
		Layer4 = 1 << 4,
		Layer5 = 1 << 5,
		Layer6 = 1 << 6,
		Layer7 = 1 << 7,
		Layer8 = 1 << 8,
		Layer9 = 1 << 9,
		Layer10 = 1 << 10,
		Layer11 = 1 << 11,
		Layer12 = 1 << 12,
		Layer13 = 1 << 13,
		Layer14 = 1 << 14,
		Layer15 = 1 << 15,
		Layer16 = 1 << 16,
		Layer17 = 1 << 17,
		Layer18 = 1 << 18,
		Layer19 = 1 << 19,
		Layer20 = 1 << 20,
		Layer21 = 1 << 21,
		Layer22 = 1 << 22,
		Layer23 = 1 << 23,
		Layer24 = 1 << 24,
		Layer25 = 1 << 25,
		Layer26 = 1 << 26,
		Layer27 = 1 << 27,
		Layer28 = 1 << 28,
		Layer29 = 1 << 29,
		Layer30 = 1 << 30
	}

	/** Local Avoidance %Simulator.
	 * This class handles local avoidance simulation for a number of agents using
	 * Reciprocal Velocity Obstacles (RVO) and Optimal Reciprocal Collision Avoidance (ORCA).
	 *
	 * This class will handle calculation of velocities from desired velocities supplied by a script.
	 * It is, however, not responsible for moving any objects in a Unity Scene. For that there are other scripts (see below).
	 *
	 * Obstacles can be added and removed from the simulation, agents can also be added and removed at any time.
	 * \see RVOSimulator
	 * \see RVOAgent
	 * \see Pathfinding.RVO.IAgent
	 *
	 * The implementation uses a sampling based algorithm with gradient descent to find the avoidance velocities.
	 *
	 * You will most likely mostly use the wrapper class RVOSimulator.
	 *
	 * \astarpro
	 */
	public class Simulator {
		/** Use Double Buffering.
		* \see DoubleBuffering */
		private readonly bool doubleBuffering = true;

		/** Inverse desired simulation fps.
		 * \see DesiredDeltaTime
		 */
		private float desiredDeltaTime = 0.05f;

		/** Worker threads */
		readonly Worker[] workers;

		/** Agents in this simulation */
		List<Agent> agents;

		/** Obstacles in this simulation */
		public List<ObstacleVertex> obstacles;

		/** Quadtree for this simulation.
		 * Used internally by the simulation to perform fast neighbour lookups for each agent.
		 * Please only read from this tree, do not rebuild it since that can interfere with the simulation.
		 * It is rebuilt when necessary.
		 */
		public RVOQuadtree Quadtree { get; private set; }

		private float deltaTime;
		private float lastStep = -99999;

		private bool doUpdateObstacles = false;
		private bool doCleanObstacles = false;

		public float DeltaTime { get { return deltaTime; } }

		/** Is using multithreading */
		public bool Multithreading { get { return workers != null && workers.Length > 0; } }

		/** Time in seconds between each simulation step.
		 * This is the desired delta time, the simulation will never run at a higher fps than
		 * the rate at which the Update function is called.
		 */
		public float DesiredDeltaTime { get { return desiredDeltaTime; } set { desiredDeltaTime = System.Math.Max(value, 0.0f); } }

		/** Bias agents to pass each other on the right side.
		 * If the desired velocity of an agent puts it on a collision course with another agent or an obstacle
		 * its desired velocity will be rotated this number of radians (1 radian is approximately 57°) to the right.
		 * This helps to break up symmetries and makes it possible to resolve some situations much faster.
		 *
		 * When many agents have the same goal this can however have the side effect that the group
		 * clustered around the target point may as a whole start to spin around the target point.
		 *
		 * Recommended values are in the range of 0 to 0.2.
		 *
		 * If this value is negative, the agents will be biased towards passing each other on the left side instead.
		 */
		public float symmetryBreakingBias = 0.1f;

		/** Determines if the XY (2D) or XZ (3D) plane is used for movement */
		public readonly MovementPlane movementPlane = MovementPlane.XZ;

		/** Get a list of all agents.
		 *
		 * This is an internal list.
		 * I'm not going to be restrictive so you may access it since it is better for performance
		 * but please do not modify it since that can cause errors in the simulation.
		 *
		 * \warning Do not modify this list!
		 */
		public List<Agent> GetAgents () {
			return agents;
		}

		/** Get a list of all obstacles.
		 * This is a list of obstacle vertices.
		 * Each vertex is part of a doubly linked list loop
		 * forming an obstacle polygon.
		 *
		 * \warning Do not modify this list!
		 *
		 * \see AddObstacle
		 * \see RemoveObstacle
		 */
		public List<ObstacleVertex> GetObstacles () {
			return obstacles;
		}

		/** Create a new simulator.
		 *
		 * \param workers Use the specified number of worker threads.\n
		 * When the number zero is specified, no multithreading will be used.
		 * A good number is the number of cores on the machine.
		 * \param doubleBuffering Use Double Buffering for calculations.
		 * Testing done with 5000 agents and 0.1 desired delta time showed that with double buffering enabled
		 * the game ran at 50 fps for most frames, dropping to 10 fps during calculation frames. But without double buffering
		 * it ran at around 10 fps all the time.\n
		 * This will let threads calculate while the game progresses instead of waiting for the calculations
		 * to finish.
		 * \param movementPlane The plane that the movement happens in. XZ for 3D games, XY for 2D games.
		 *
		 * \note Will only have effect if using multithreading
		 *
		 * \see #Multithreading
		 */
		public Simulator (int workers, bool doubleBuffering, MovementPlane movementPlane) {
			this.workers = new Simulator.Worker[workers];
			this.doubleBuffering = doubleBuffering;
			this.DesiredDeltaTime = 1;
			this.movementPlane = movementPlane;
			Quadtree = new RVOQuadtree();

			for (int i = 0; i < workers; i++) this.workers[i] = new Simulator.Worker(this);

			agents = new List<Agent>();
			obstacles = new List<ObstacleVertex>();
		}

		/** Removes all agents from the simulation */
		public void ClearAgents () {
			//Bad to update agents while processing of current agents might be done
			//Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			for (int i = 0; i < agents.Count; i++) {
				agents[i].simulator = null;
			}
			agents.Clear();
		}

		public void OnDestroy () {
			if (workers != null) {
				for (int i = 0; i < workers.Length; i++) workers[i].Terminate();
			}
		}

		/** Terminates any worker threads */
		~Simulator () {
			OnDestroy();
		}

		/** Add a previously removed agent to the simulation.
		 * An agent can only be in one simulation at a time, any attempt to add an agent to two simulations
		 * or multiple times to the same simulation will result in an exception being thrown.
		 *
		 * \see RemoveAgent
		 */
		public IAgent AddAgent (IAgent agent) {
			if (agent == null) throw new System.ArgumentNullException("Agent must not be null");

			Agent agentReal = agent as Agent;
			if (agentReal == null) throw new System.ArgumentException("The agent must be of type Agent. Agent was of type "+agent.GetType());

			if (agentReal.simulator != null && agentReal.simulator == this) throw new System.ArgumentException("The agent is already in the simulation");
			else if (agentReal.simulator != null) throw new System.ArgumentException("The agent is already added to another simulation");
			agentReal.simulator = this;

			//Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			agents.Add(agentReal);
			return agent;
		}

		/** Add an agent at the specified position.
		 * You can use the returned interface to read several parameters such as position and velocity
		 * and set for example radius and desired velocity.
		 *
		 * \deprecated Use AddAgent(Vector2,float) instead
		 */
		[System.Obsolete("Use AddAgent(Vector2,float) instead")]
		public IAgent AddAgent (Vector3 position) {
			return AddAgent(new Vector2(position.x, position.z), position.y);
		}

		/** Add an agent at the specified position.
		 * You can use the returned interface to read and write parameters
		 * and set for example radius and desired point to move to.
		 *
		 * \see RemoveAgent
		 *
		 * \param position See IAgent.Position
		 * \param elevationCoordinate See IAgent.ElevationCoordinate
		 */
		public IAgent AddAgent (Vector2 position, float elevationCoordinate) {
			return AddAgent(new Agent(position, elevationCoordinate));
		}

		/** Removes a specified agent from this simulation.
		 * The agent can be added again later by using AddAgent.
		 *
		 * \see AddAgent(IAgent)
		 * \see ClearAgents
		 */
		public void RemoveAgent (IAgent agent) {
			if (agent == null) throw new System.ArgumentNullException("Agent must not be null");

			Agent agentReal = agent as Agent;
			if (agentReal == null) throw new System.ArgumentException("The agent must be of type Agent. Agent was of type "+agent.GetType());

			if (agentReal.simulator != this) throw new System.ArgumentException("The agent is not added to this simulation");

			//Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			agentReal.simulator = null;

			if (!agents.Remove(agentReal)) {
				throw new System.ArgumentException("Critical Bug! This should not happen. Please report this.");
			}
		}

		/** Adds a previously removed obstacle.
		 * This does not check if the obstacle is already added to the simulation, so please do not add an obstacle multiple times.
		 *
		 * It is assumed that this is a valid obstacle.
		 */
		public ObstacleVertex AddObstacle (ObstacleVertex v) {
			if (v == null) throw new System.ArgumentNullException("Obstacle must not be null");

			//Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			obstacles.Add(v);
			UpdateObstacles();
			return v;
		}

		/** Adds an obstacle described by the vertices.
		 *
		 * \see RemoveObstacle
		 */
		public ObstacleVertex AddObstacle (Vector3[] vertices, float height, bool cycle = true) {
			return AddObstacle(vertices, height, Matrix4x4.identity, RVOLayer.DefaultObstacle, cycle);
		}

		/** Adds an obstacle described by the vertices.
		 *
		 * \see RemoveObstacle
		 */
		public ObstacleVertex AddObstacle (Vector3[] vertices, float height, Matrix4x4 matrix, RVOLayer layer = RVOLayer.DefaultObstacle, bool cycle = true) {
			if (vertices == null) throw new System.ArgumentNullException("Vertices must not be null");
			if (vertices.Length < 2) throw new System.ArgumentException("Less than 2 vertices in an obstacle");

			ObstacleVertex first = null;
			ObstacleVertex prev = null;

			// Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			for (int i = 0; i < vertices.Length; i++) {
				var v = new ObstacleVertex {
					prev = prev,
					layer = layer,
					height = height
				};

				if (first == null) first = v;
				else prev.next = v;

				prev = v;
			}

			if (cycle) {
				prev.next = first;
				first.prev = prev;
			}

			UpdateObstacle(first, vertices, matrix);
			obstacles.Add(first);
			return first;
		}

		/**
		 * Adds a line obstacle with a specified height.
		 *
		 * \see RemoveObstacle
		 */
		public ObstacleVertex AddObstacle (Vector3 a, Vector3 b, float height) {
			ObstacleVertex first = new ObstacleVertex();
			ObstacleVertex second = new ObstacleVertex();

			first.layer = RVOLayer.DefaultObstacle;
			second.layer = RVOLayer.DefaultObstacle;

			first.prev = second;
			second.prev = first;
			first.next = second;
			second.next = first;

			first.position = a;
			second.position = b;
			first.height = height;
			second.height = height;
			second.ignore = true;

			first.dir = new Vector2(b.x-a.x, b.z-a.z).normalized;
			second.dir = -first.dir;

			//Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			obstacles.Add(first);

			UpdateObstacles();
			return first;
		}

		/** Updates the vertices of an obstacle.
		 * \param obstacle %Obstacle to update
		 * \param vertices New vertices for the obstacle, must have at least the number of vertices in the original obstacle
		 * \param matrix %Matrix to multiply vertices with before updating obstacle
		 *
		 * The number of vertices in an obstacle cannot be changed, existing vertices can only be moved.
		 */
		public void UpdateObstacle (ObstacleVertex obstacle, Vector3[] vertices, Matrix4x4 matrix) {
			if (vertices == null) throw new System.ArgumentNullException("Vertices must not be null");
			if (obstacle == null) throw new System.ArgumentNullException("Obstacle must not be null");

			if (vertices.Length < 2) throw new System.ArgumentException("Less than 2 vertices in an obstacle");

			bool identity = matrix == Matrix4x4.identity;

			// Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			int count = 0;

			// Obstacles are represented using linked lists
			var vertex = obstacle;
			do {
				if (count >= vertices.Length) {
					Debug.DrawLine(vertex.prev.position, vertex.position, Color.red);
					throw new System.ArgumentException("Obstacle has more vertices than supplied for updating (" + vertices.Length+ " supplied)");
				}

				// Premature optimization ftw!
				vertex.position = identity ? vertices[count] : matrix.MultiplyPoint3x4(vertices[count]);
				vertex = vertex.next;
				count++;
			} while (vertex != obstacle && vertex != null);

			vertex = obstacle;
			do {
				if (vertex.next == null) {
					vertex.dir = Vector2.zero;
				} else {
					Vector3 dir = vertex.next.position - vertex.position;
					vertex.dir = new Vector2(dir.x, dir.z).normalized;
				}

				vertex = vertex.next;
			} while (vertex != obstacle && vertex != null);

			ScheduleCleanObstacles();
			UpdateObstacles();
		}

		private void ScheduleCleanObstacles () {
			doCleanObstacles = true;
		}

		private void CleanObstacles () {
		}

		/** Removes the obstacle identified by the vertex.
		 * This must be the same vertex as the one returned by the AddObstacle call.
		 *
		 * \see AddObstacle
		 */
		public void RemoveObstacle (ObstacleVertex v) {
			if (v == null) throw new System.ArgumentNullException("Vertex must not be null");

			// Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			obstacles.Remove(v);
			UpdateObstacles();
		}

		/** Rebuilds the obstacle tree at next simulation frame.
		 * Add and remove obstacle functions call this automatically.
		 */
		public void UpdateObstacles () {
			// Update obstacles at next frame
			doUpdateObstacles = true;
		}

		void BuildQuadtree () {
			Quadtree.Clear();
			if (agents.Count > 0) {
				Rect bounds = Rect.MinMaxRect(agents[0].position.x, agents[0].position.y, agents[0].position.x, agents[0].position.y);
				for (int i = 1; i < agents.Count; i++) {
					Vector2 p = agents[i].position;
					bounds = Rect.MinMaxRect(Mathf.Min(bounds.xMin, p.x), Mathf.Min(bounds.yMin, p.y), Mathf.Max(bounds.xMax, p.x), Mathf.Max(bounds.yMax, p.y));
				}
				Quadtree.SetBounds(bounds);

				for (int i = 0; i < agents.Count; i++) {
					Quadtree.Insert(agents[i]);
				}

				//quadtree.DebugDraw ();
			}

			Quadtree.CalculateSpeeds();
		}

		/** Blocks until separate threads have finished with the current simulation step.
		 * When double buffering is done, the simulation is performed in between frames.
		 */
		void BlockUntilSimulationStepIsDone () {
			if (Multithreading && doubleBuffering) for (int j = 0; j < workers.Length; j++) workers[j].WaitOne();
		}

		private WorkerContext coroutineWorkerContext = new WorkerContext();

		void PreCalculation () {
			for (int i = 0; i < agents.Count; i++) agents[i].PreCalculation();
		}

		void CleanAndUpdateObstaclesIfNecessary () {
			if (doCleanObstacles) {
				CleanObstacles();
				doCleanObstacles = false;
				doUpdateObstacles = true;
			}

			if (doUpdateObstacles) {
				doUpdateObstacles = false;
			}
		}

		/** Should be called once per frame */
		public void Update () {
			// Initialize last step
			if (lastStep < 0) {
				lastStep = Time.time;
				deltaTime = DesiredDeltaTime;
			}

			if (Time.time - lastStep >= DesiredDeltaTime) {
				deltaTime = Time.time - lastStep;
				lastStep = Time.time;

				// Prevent a zero delta time
				deltaTime = System.Math.Max(deltaTime, 1.0f/2000f);

				if (Multithreading) {
					// Make sure the threads have completed their tasks
					// Otherwise block until they have
					if (doubleBuffering) {
						for (int i = 0; i < workers.Length; i++) workers[i].WaitOne();
						for (int i = 0; i < agents.Count; i++) agents[i].PostCalculation();
					}

					PreCalculation();
					CleanAndUpdateObstaclesIfNecessary();
					BuildQuadtree();

					for (int i = 0; i < workers.Length; i++) {
						workers[i].start = i*agents.Count / workers.Length;
						workers[i].end = (i+1)*agents.Count / workers.Length;
					}

					// BufferSwitch
					for (int i = 0; i < workers.Length; i++) workers[i].Execute(1);
					for (int i = 0; i < workers.Length; i++) workers[i].WaitOne();

					// Calculate New Velocity
					for (int i = 0; i < workers.Length; i++) workers[i].Execute(0);

					// Make sure the threads have completed their tasks
					// Otherwise block until they have
					if (!doubleBuffering) {
						for (int i = 0; i < workers.Length; i++) workers[i].WaitOne();
						for (int i = 0; i < agents.Count; i++) agents[i].PostCalculation();
					}
				} else {
					PreCalculation();
					CleanAndUpdateObstaclesIfNecessary();
					BuildQuadtree();

					for (int i = 0; i < agents.Count; i++) {
						agents[i].BufferSwitch();
					}

					for (int i = 0; i < agents.Count; i++) {
						agents[i].CalculateNeighbours();
						agents[i].CalculateVelocity(coroutineWorkerContext);
					}

					for (int i = 0; i < agents.Count; i++) agents[i].PostCalculation();
				}
			}
		}

		internal class WorkerContext {
			public Agent.VOBuffer vos = new Agent.VOBuffer(16);

			public const int KeepCount = 3;
			public Vector2[] bestPos = new Vector2[KeepCount];
			public float[] bestSizes = new float[KeepCount];
			public float[] bestScores = new float[KeepCount+1];

			public Vector2[] samplePos = new Vector2[50];
			public float[] sampleSize = new float[50];
		}

		/** Worker thread for RVO simulation */
		class Worker {
			public int start, end;
			readonly AutoResetEvent runFlag = new AutoResetEvent(false);
			readonly ManualResetEvent waitFlag = new ManualResetEvent(true);
			readonly Simulator simulator;
			int task = 0;
			bool terminate = false;
			WorkerContext context = new WorkerContext();

			public Worker (Simulator sim) {
				this.simulator = sim;
				var thread = new Thread(new ThreadStart(Run));
				thread.IsBackground = true;
				thread.Name = "RVO Simulator Thread";
				thread.Start();
			}

			public void Execute (int task) {
				this.task = task;
				waitFlag.Reset();
				runFlag.Set();
			}

			public void WaitOne () {
				if (!terminate) waitFlag.WaitOne();
			}

			public void Terminate () {
				WaitOne();
				terminate = true;
				Execute(-1);
			}

			public void Run () {
				runFlag.WaitOne();

				while (!terminate) {
					try {
						List<Agent> agents = simulator.GetAgents();
						if (task == 0) {
							for (int i = start; i < end; i++) {
								agents[i].CalculateNeighbours();
								agents[i].CalculateVelocity(context);
							}
						} else if (task == 1) {
							for (int i = start; i < end; i++) {
								agents[i].BufferSwitch();
							}
						} else if (task == 2) {
							simulator.BuildQuadtree();
						} else {
							Debug.LogError("Invalid Task Number: " + task);
							throw new System.Exception("Invalid Task Number: " + task);
						}
					} catch (System.Exception e) {
						Debug.LogError(e);
					}
					waitFlag.Set();
					runFlag.WaitOne();
				}
			}
		}
	}
}
