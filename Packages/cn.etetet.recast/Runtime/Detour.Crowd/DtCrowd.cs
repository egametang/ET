/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotRecast.Core;
using DotRecast.Detour.Crowd.Tracking;


namespace DotRecast.Detour.Crowd
{
    using static DotRecast.Core.RcMath;

    /**
 * Members in this module implement local steering and dynamic avoidance features.
 *
 * The crowd is the big beast of the navigation features. It not only handles a lot of the path management for you, but
 * also local steering and dynamic avoidance between members of the crowd. I.e. It can keep your agents from running
 * into each other.
 *
 * Main class: Crowd
 *
 * The #dtNavMeshQuery and #dtPathCorridor classes provide perfectly good, easy to use path planning features. But in
 * the end they only give you points that your navigation client should be moving toward. When it comes to deciding
 * things like agent velocity and steering to avoid other agents, that is up to you to implement. Unless, of course, you
 * decide to use Crowd.
 *
 * Basically, you add an agent to the crowd, providing various configuration settings such as maximum speed and
 * acceleration. You also provide a local target to move toward. The crowd manager then provides, with every update, the
 * new agent position and velocity for the frame. The movement will be constrained to the navigation mesh, and steering
 * will be applied to ensure agents managed by the crowd do not collide with each other.
 *
 * This is very powerful feature set. But it comes with limitations.
 *
 * The biggest limitation is that you must give control of the agent's position completely over to the crowd manager.
 * You can update things like maximum speed and acceleration. But in order for the crowd manager to do its thing, it
 * can't allow you to constantly be giving it overrides to position and velocity. So you give up direct control of the
 * agent's movement. It belongs to the crowd.
 *
 * The second biggest limitation revolves around the fact that the crowd manager deals with local planning. So the
 * agent's target should never be more than 256 polygons away from its current position. If it is, you risk your agent
 * failing to reach its target. So you may still need to do long distance planning and provide the crowd manager with
 * intermediate targets.
 *
 * Other significant limitations:
 *
 * - All agents using the crowd manager will use the same #dtQueryFilter. - Crowd management is relatively expensive.
 * The maximum agents under crowd management at any one time is between 20 and 30. A good place to start is a maximum of
 * 25 agents for 0.5ms per frame.
 *
 * @note This is a summary list of members. Use the index or search feature to find minor members.
 *
 * @struct dtCrowdAgentParams
 * @see CrowdAgent, Crowd::AddAgent(), Crowd::UpdateAgentParameters()
 *
 * @var dtCrowdAgentParams::obstacleAvoidanceType
 * @par
 *
 * 		#dtCrowd permits agents to use different avoidance configurations. This value is the index of the
 *      #dtObstacleAvoidanceParams within the crowd.
 *
 * @see dtObstacleAvoidanceParams, dtCrowd::SetObstacleAvoidanceParams(), dtCrowd::GetObstacleAvoidanceParams()
 *
 * @var dtCrowdAgentParams::collisionQueryRange
 * @par
 *
 * 		Collision elements include other agents and navigation mesh boundaries.
 *
 *      This value is often based on the agent radius and/or maximum speed. E.g. radius * 8
 *
 * @var dtCrowdAgentParams::pathOptimizationRange
 * @par
 *
 * 		Only applicable if #updateFlags includes the #DT_CROWD_OPTIMIZE_VIS flag.
 *
 *      This value is often based on the agent radius. E.g. radius * 30
 *
 * @see dtPathCorridor::OptimizePathVisibility()
 *
 * @var dtCrowdAgentParams::separationWeight
 * @par
 *
 * 		A higher value will result in agents trying to stay farther away from each other at the cost of more difficult
 *      steering in tight spaces.
 *
 */
    /**
     * This is the core class of the refs crowd module. See the refs crowd documentation for a summary of the crowd
     * features. A common method for setting up the crowd is as follows: -# Allocate the crowd -# Set the avoidance
     * configurations using #SetObstacleAvoidanceParams(). -# Add agents using #AddAgent() and make an initial movement
     * request using #RequestMoveTarget(). A common process for managing the crowd is as follows: -# Call #Update() to allow
     * the crowd to manage its agents. -# Retrieve agent information using #GetActiveAgents(). -# Make movement requests
     * using #RequestMoveTarget() when movement goal changes. -# Repeat every frame. Some agent configuration settings can
     * be updated using #UpdateAgentParameters(). But the crowd owns the agent position. So it is not possible to update an
     * active agent's position. If agent position must be fed back into the crowd, the agent must be removed and re-added.
     * Notes: - Path related information is available for newly added agents only after an #Update() has been performed. -
     * Agent objects are kept in a pool and re-used. So it is important when using agent objects to check the value of
     * #dtCrowdAgent::active to determine if the agent is actually in use or not. - This class is meant to provide 'local'
     * movement. There is a limit of 256 polygons in the path corridor. So it is not meant to provide automatic pathfinding
     * services over long distances.
     *
     * @see DtAllocCrowd(), DtFreeCrowd(), Init(), dtCrowdAgent
     */
    public class DtCrowd
    {
        /// The maximum number of corners a crowd agent will look ahead in the path.
        /// This value is used for sizing the crowd agent corner buffers.
        /// Due to the behavior of the crowd manager, the actual number of useful
        /// corners will be one less than this number.
        /// @ingroup crowd
        public const int DT_CROWDAGENT_MAX_CORNERS = 4;

        /// The maximum number of crowd avoidance configurations supported by the
        /// crowd manager.
        /// @ingroup crowd
        /// @see dtObstacleAvoidanceParams, dtCrowd::SetObstacleAvoidanceParams(), dtCrowd::GetObstacleAvoidanceParams(),
        /// dtCrowdAgentParams::obstacleAvoidanceType
        public const int DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS = 8;

        /// The maximum number of query filter types supported by the crowd manager.
        /// @ingroup crowd
        /// @see dtQueryFilter, dtCrowd::GetFilter() dtCrowd::GetEditableFilter(),
        /// dtCrowdAgentParams::queryFilterType
        public const int DT_CROWD_MAX_QUERY_FILTER_TYPE = 16;

        private readonly RcAtomicInteger _agentId = new RcAtomicInteger();
        private readonly List<DtCrowdAgent> _agents;
        private readonly DtPathQueue _pathQ;
        private readonly DtObstacleAvoidanceParams[] _obstacleQueryParams = new DtObstacleAvoidanceParams[DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS];
        private readonly DtObstacleAvoidanceQuery _obstacleQuery;
        private DtProximityGrid _grid;
        private readonly RcVec3f _ext = new RcVec3f();
        private readonly IDtQueryFilter[] _filters = new IDtQueryFilter[DT_CROWD_MAX_QUERY_FILTER_TYPE];
        private DtNavMeshQuery _navQuery;
        private DtNavMesh _navMesh;
        private readonly DtCrowdConfig _config;
        private readonly DtCrowdTelemetry _telemetry = new DtCrowdTelemetry();
        private int _velocitySampleCount;

        public DtCrowd(DtCrowdConfig config, DtNavMesh nav) :
            this(config, nav, i => new DtQueryDefaultFilter())
        {
        }

        public DtCrowd(DtCrowdConfig config, DtNavMesh nav, Func<int, IDtQueryFilter> queryFilterFactory)
        {
            _config = config;
            _ext.Set(config.maxAgentRadius * 2.0f, config.maxAgentRadius * 1.5f, config.maxAgentRadius * 2.0f);

            _obstacleQuery = new DtObstacleAvoidanceQuery(config.maxObstacleAvoidanceCircles, config.maxObstacleAvoidanceSegments);

            for (int i = 0; i < DT_CROWD_MAX_QUERY_FILTER_TYPE; i++)
            {
                _filters[i] = queryFilterFactory.Invoke(i);
            }

            // Init obstacle query option.
            for (int i = 0; i < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS; ++i)
            {
                _obstacleQueryParams[i] = new DtObstacleAvoidanceParams();
            }

            // Allocate temp buffer for merging paths.
            _pathQ = new DtPathQueue(config);
            _agents = new List<DtCrowdAgent>();

            // The navQuery is mostly used for local searches, no need for large node pool.
            _navMesh = nav;
            _navQuery = new DtNavMeshQuery(nav);
        }

        public void SetNavMesh(DtNavMesh nav)
        {
            _navMesh = nav;
            _navQuery = new DtNavMeshQuery(nav);
        }

        /// Sets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index. [Limits: 0 <= value <
        /// #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @param[in] option The new configuration.
        public void SetObstacleAvoidanceParams(int idx, DtObstacleAvoidanceParams option)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                _obstacleQueryParams[idx] = new DtObstacleAvoidanceParams(option);
            }
        }

        /// Gets the shared avoidance configuration for the specified index.
        /// @param[in] idx The index of the configuration to retreive.
        /// [Limits: 0 <= value < #DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS]
        /// @return The requested configuration.
        public DtObstacleAvoidanceParams GetObstacleAvoidanceParams(int idx)
        {
            if (idx >= 0 && idx < DT_CROWD_MAX_OBSTAVOIDANCE_PARAMS)
            {
                return _obstacleQueryParams[idx];
            }

            return null;
        }

        /// Updates the specified agent's configuration.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] params The new agent configuration.
        public void UpdateAgentParameters(DtCrowdAgent agent, DtCrowdAgentParams option)
        {
            agent.option = option;
        }

        /**
     * Adds a new agent to the crowd.
     *
     * @param pos
     *            The requested position of the agent. [(x, y, z)]
     * @param params
     *            The configuration of the agent.
     * @return The newly created agent object
     */
        public DtCrowdAgent AddAgent(RcVec3f pos, DtCrowdAgentParams option)
        {
            DtCrowdAgent ag = new DtCrowdAgent(_agentId.GetAndIncrement());
            _agents.Add(ag);
            UpdateAgentParameters(ag, option);

            // Find nearest position on navmesh and place the agent there.
            var status = _navQuery.FindNearestPoly(pos, _ext, _filters[ag.option.queryFilterType], out var refs, out var nearestPt, out var _);
            if (status.Failed())
            {
                nearestPt = pos;
                refs = 0;
            }

            ag.corridor.Reset(refs, nearestPt);
            ag.boundary.Reset();
            ag.partial = false;

            ag.topologyOptTime = 0;
            ag.targetReplanTime = 0;

            ag.dvel = RcVec3f.Zero;
            ag.nvel = RcVec3f.Zero;
            ag.vel = RcVec3f.Zero;
            ag.npos = nearestPt;

            ag.desiredSpeed = 0;

            if (refs != 0)
            {
                ag.state = DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
            }
            else
            {
                ag.state = DtCrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
            }

            ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE;

            return ag;
        }

        /**
     * Removes the agent from the crowd.
     *
     * @param agent
     *            Agent to be removed
     */
        public void RemoveAgent(DtCrowdAgent agent)
        {
            _agents.Remove(agent);
        }

        private bool RequestMoveTargetReplan(DtCrowdAgent ag, long refs, RcVec3f pos)
        {
            ag.SetTarget(refs, pos);
            ag.targetReplan = true;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] ref The position's polygon reference.
        /// @param[in] pos The position within the polygon. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        ///
        /// This method is used when a new target is set.
        ///
        /// The position will be constrained to the surface of the navigation mesh.
        ///
        /// The request will be processed during the next #Update().
        public bool RequestMoveTarget(DtCrowdAgent agent, long refs, RcVec3f pos)
        {
            if (refs == 0)
            {
                return false;
            }

            // Initialize request.
            agent.SetTarget(refs, pos);
            agent.targetReplan = false;
            return true;
        }

        /// Submits a new move request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @param[in] vel The movement velocity. [(x, y, z)]
        /// @return True if the request was successfully submitted.
        public bool RequestMoveVelocity(DtCrowdAgent agent, RcVec3f vel)
        {
            // Initialize request.
            agent.targetRef = 0;
            agent.targetPos = vel;
            agent.targetPathQueryResult = null;
            agent.targetReplan = false;
            agent.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY;

            return true;
        }

        /// Resets any request for the specified agent.
        /// @param[in] idx The agent index. [Limits: 0 <= value < #GetAgentCount()]
        /// @return True if the request was successfully reseted.
        public bool ResetMoveTarget(DtCrowdAgent agent)
        {
            // Initialize request.
            agent.targetRef = 0;
            agent.targetPos = RcVec3f.Zero;
            agent.dvel = RcVec3f.Zero;
            agent.targetPathQueryResult = null;
            agent.targetReplan = false;
            agent.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE;
            return true;
        }

        /**
     * Gets the active agents int the agent pool.
     *
     * @return List of active agents
     */
        public IList<DtCrowdAgent> GetActiveAgents()
        {
            return _agents;
        }

        public RcVec3f GetQueryExtents()
        {
            return _ext;
        }

        public IDtQueryFilter GetFilter(int i)
        {
            return i >= 0 && i < DT_CROWD_MAX_QUERY_FILTER_TYPE ? _filters[i] : null;
        }

        public DtProximityGrid GetGrid()
        {
            return _grid;
        }

        public DtPathQueue GetPathQueue()
        {
            return _pathQ;
        }

        public DtCrowdTelemetry Telemetry()
        {
            return _telemetry;
        }

        public DtCrowdConfig Config()
        {
            return _config;
        }

        public DtCrowdTelemetry Update(float dt, DtCrowdAgentDebugInfo debug)
        {
            _velocitySampleCount = 0;

            _telemetry.Start();

            IList<DtCrowdAgent> agents = GetActiveAgents();

            // Check that all agents still have valid paths.
            CheckPathValidity(agents, dt);

            // Update async move request and path finder.
            UpdateMoveRequest(agents, dt);

            // Optimize path topology.
            UpdateTopologyOptimization(agents, dt);

            // Register agents to proximity grid.
            BuildProximityGrid(agents);

            // Get nearby navmesh segments and agents to collide with.
            BuildNeighbours(agents);

            // Find next corner to steer to.
            FindCorners(agents, debug);

            // Trigger off-mesh connections (depends on corners).
            TriggerOffMeshConnections(agents);

            // Calculate steering.
            CalculateSteering(agents);

            // Velocity planning.
            PlanVelocity(debug, agents);

            // Integrate.
            Integrate(dt, agents);

            // Handle collisions.
            HandleCollisions(agents);

            MoveAgents(agents);

            // Update agents using off-mesh connection.
            UpdateOffMeshConnections(agents, dt);
            return _telemetry;
        }


        private void CheckPathValidity(IList<DtCrowdAgent> agents, float dt)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.CheckPathValidity);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.targetReplanTime += dt;

                bool replan = false;

                // First check that the current location is valid.
                RcVec3f agentPos = new RcVec3f();
                long agentRef = ag.corridor.GetFirstPoly();
                agentPos = ag.npos;
                if (!_navQuery.IsValidPolyRef(agentRef, _filters[ag.option.queryFilterType]))
                {
                    // Current location is not valid, try to reposition.
                    // TODO: this can snap agents, how to handle that?
                    _navQuery.FindNearestPoly(ag.npos, _ext, _filters[ag.option.queryFilterType], out agentRef, out var nearestPt, out var _);
                    agentPos = nearestPt;

                    if (agentRef == 0)
                    {
                        // Could not find location in navmesh, set state to invalid.
                        ag.corridor.Reset(0, agentPos);
                        ag.partial = false;
                        ag.boundary.Reset();
                        ag.state = DtCrowdAgentState.DT_CROWDAGENT_STATE_INVALID;
                        continue;
                    }

                    // Make sure the first polygon is valid, but leave other valid
                    // polygons in the path so that replanner can adjust the path
                    // better.
                    ag.corridor.FixPathStart(agentRef, agentPos);
                    // ag.corridor.TrimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    ag.boundary.Reset();
                    ag.npos = agentPos;

                    replan = true;
                }

                // If the agent does not have move target or is controlled by
                // velocity, no need to recover the target nor replan.
                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Try to recover move request position.
                if (ag.targetState != DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    && ag.targetState != DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED)
                {
                    if (!_navQuery.IsValidPolyRef(ag.targetRef, _filters[ag.option.queryFilterType]))
                    {
                        // Current target is not valid, try to reposition.
                        _navQuery.FindNearestPoly(ag.targetPos, _ext, _filters[ag.option.queryFilterType], out ag.targetRef, out var nearestPt, out var _);
                        ag.targetPos = nearestPt;
                        replan = true;
                    }

                    if (ag.targetRef == 0)
                    {
                        // Failed to reposition target, fail moverequest.
                        ag.corridor.Reset(agentRef, agentPos);
                        ag.partial = false;
                        ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE;
                    }
                }

                // If nearby corridor is not valid, replan.
                if (!ag.corridor.IsValid(_config.checkLookAhead, _navQuery, _filters[ag.option.queryFilterType]))
                {
                    // Fix current path.
                    // ag.corridor.TrimInvalidPath(agentRef, agentPos, m_navquery,
                    // &m_filter);
                    // ag.boundary.Reset();
                    replan = true;
                }

                // If the end of the path is near and it is not the requested
                // location, replan.
                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VALID)
                {
                    if (ag.targetReplanTime > _config.targetReplanDelay && ag.corridor.GetPathCount() < _config.checkLookAhead
                                                                        && ag.corridor.GetLastPoly() != ag.targetRef)
                    {
                        replan = true;
                    }
                }

                // Try to replan path to goal.
                if (replan)
                {
                    if (ag.targetState != DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                    {
                        RequestMoveTargetReplan(ag, ag.targetRef, ag.targetPos);
                    }
                }
            }
        }

        private void UpdateMoveRequest(IList<DtCrowdAgent> agents, float dt)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.UpdateMoveRequest);

            RcSortedQueue<DtCrowdAgent> queue = new RcSortedQueue<DtCrowdAgent>((a1, a2) => a2.targetReplanTime.CompareTo(a1.targetReplanTime));

            // Fire off new requests.
            List<long> reqPath = new List<long>();
            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state == DtCrowdAgentState.DT_CROWDAGENT_STATE_INVALID)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING)
                {
                    List<long> path = ag.corridor.GetPath();
                    if (0 == path.Count)
                    {
                        throw new ArgumentException("Empty path");
                    }


                    // Quick search towards the goal.
                    _navQuery.InitSlicedFindPath(path[0], ag.targetRef, ag.npos, ag.targetPos,
                        _filters[ag.option.queryFilterType], 0);
                    _navQuery.UpdateSlicedFindPath(_config.maxTargetFindPathIterations, out var _);

                    DtStatus status;
                    if (ag.targetReplan) // && npath > 10)
                    {
                        // Try to use existing steady path during replan if possible.
                        status = _navQuery.FinalizeSlicedFindPathPartial(path, ref reqPath);
                    }
                    else
                    {
                        // Try to move towards target when goal changes.
                        status = _navQuery.FinalizeSlicedFindPath(ref reqPath);
                    }

                    RcVec3f reqPos = new RcVec3f();
                    if (status.Succeeded() && reqPath.Count > 0)
                    {
                        // In progress or succeed.
                        if (reqPath[reqPath.Count - 1] != ag.targetRef)
                        {
                            // Partial path, constrain target position inside the
                            // last polygon.
                            var cr = _navQuery.ClosestPointOnPoly(reqPath[reqPath.Count - 1], ag.targetPos, out reqPos, out var _);
                            if (cr.Failed())
                            {
                                reqPath = new List<long>();
                            }
                        }
                        else
                        {
                            reqPos = ag.targetPos;
                        }
                    }
                    else
                    {
                        // Could not find path, start the request from current
                        // location.
                        reqPos = ag.npos;
                        reqPath = new List<long>();
                        reqPath.Add(path[0]);
                    }

                    ag.corridor.SetCorridor(reqPos, reqPath);
                    ag.boundary.Reset();
                    ag.partial = false;

                    if (reqPath[reqPath.Count - 1] == ag.targetRef)
                    {
                        ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        ag.targetReplanTime = 0;
                    }
                    else
                    {
                        // The path is longer or potentially unreachable, full plan.
                        ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE;
                    }

                    ag.targetReplanWaitTime = 0;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE)
                {
                    queue.Enqueue(ag);
                }
            }

            while (!queue.IsEmpty())
            {
                DtCrowdAgent ag = queue.Dequeue();
                ag.targetPathQueryResult = _pathQ.Request(ag.corridor.GetLastPoly(), ag.targetRef, ag.corridor.GetTarget(), ag.targetPos, _filters[ag.option.queryFilterType]);
                if (ag.targetPathQueryResult != null)
                {
                    ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH;
                }
                else
                {
                    _telemetry.RecordMaxTimeToEnqueueRequest(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }

            // Update requests.
            using (var timer2 = _telemetry.ScopedTimer(DtCrowdTimerLabel.PathQueueUpdate))
            {
                _pathQ.Update(_navMesh);
            }

            // Process path results.
            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_WAITING_FOR_PATH)
                {
                    // _telemetry.RecordPathWaitTime(ag.targetReplanTime);
                    // Poll path queue.
                    DtStatus status = ag.targetPathQueryResult.status;
                    if (status.Failed())
                    {
                        // Path find failed, retry if the target location is still
                        // valid.
                        ag.targetPathQueryResult = null;
                        if (ag.targetRef != 0)
                        {
                            ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
                        }
                        else
                        {
                            ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.targetReplanTime = 0;
                    }
                    else if (status.Succeeded())
                    {
                        List<long> path = ag.corridor.GetPath();
                        if (0 == path.Count)
                        {
                            throw new ArgumentException("Empty path");
                        }

                        // Apply results.
                        var targetPos = ag.targetPos;

                        bool valid = true;
                        List<long> res = ag.targetPathQueryResult.path;
                        if (status.Failed() || 0 == res.Count)
                        {
                            valid = false;
                        }

                        if (status.IsPartial())
                        {
                            ag.partial = true;
                        }
                        else
                        {
                            ag.partial = false;
                        }

                        // Merge result and existing path.
                        // The agent might have moved whilst the request is
                        // being processed, so the path may have changed.
                        // We assume that the end of the path is at the same
                        // location
                        // where the request was issued.

                        // The last ref in the old path should be the same as
                        // the location where the request was issued..
                        if (valid && path[path.Count - 1] != res[0])
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            // Put the old path infront of the old path.
                            if (path.Count > 1)
                            {
                                path.RemoveAt(path.Count - 1);
                                path.AddRange(res);
                                res = path;
                                // Remove trackbacks
                                for (int j = 1; j < res.Count - 1; ++j)
                                {
                                    if (j - 1 >= 0 && j + 1 < res.Count)
                                    {
                                        if (res[j - 1] == res[j + 1])
                                        {
                                            res.RemoveAt(j + 1);
                                            res.RemoveAt(j);
                                            j -= 2;
                                        }
                                    }
                                }
                            }

                            // Check for partial path.
                            if (res[res.Count - 1] != ag.targetRef)
                            {
                                // Partial path, constrain target position inside
                                // the last polygon.
                                var cr = _navQuery.ClosestPointOnPoly(res[res.Count - 1], targetPos, out var nearest, out var _);
                                if (cr.Succeeded())
                                {
                                    targetPos = nearest;
                                }
                                else
                                {
                                    valid = false;
                                }
                            }
                        }

                        if (valid)
                        {
                            // Set current corridor.
                            ag.corridor.SetCorridor(targetPos, res);
                            // Force to update boundary.
                            ag.boundary.Reset();
                            ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_VALID;
                        }
                        else
                        {
                            // Something went wrong.
                            ag.targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
                        }

                        ag.targetReplanTime = 0;
                    }

                    _telemetry.RecordMaxTimeToFindPath(ag.targetReplanWaitTime);
                    ag.targetReplanWaitTime += dt;
                }
            }
        }

        private void UpdateTopologyOptimization(IList<DtCrowdAgent> agents, float dt)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.UpdateTopologyOptimization);

            RcSortedQueue<DtCrowdAgent> queue = new RcSortedQueue<DtCrowdAgent>((a1, a2) => a2.topologyOptTime.CompareTo(a1.topologyOptTime));

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                if ((ag.option.updateFlags & DtCrowdAgentParams.DT_CROWD_OPTIMIZE_TOPO) == 0)
                {
                    continue;
                }

                ag.topologyOptTime += dt;
                if (ag.topologyOptTime >= _config.topologyOptimizationTimeThreshold)
                {
                    queue.Enqueue(ag);
                }
            }

            while (!queue.IsEmpty())
            {
                DtCrowdAgent ag = queue.Dequeue();
                ag.corridor.OptimizePathTopology(_navQuery, _filters[ag.option.queryFilterType], _config.maxTopologyOptimizationIterations);
                ag.topologyOptTime = 0;
            }
        }

        private void BuildProximityGrid(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.BuildProximityGrid);

            _grid = new DtProximityGrid(_config.maxAgentRadius * 3);

            foreach (DtCrowdAgent ag in agents)
            {
                RcVec3f p = ag.npos;
                float r = ag.option.radius;
                _grid.AddItem(ag, p.x - r, p.z - r, p.x + r, p.z + r);
            }
        }

        private void BuildNeighbours(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.BuildNeighbours);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Update the collision boundary after certain distance has been passed or
                // if it has become invalid.
                float updateThr = ag.option.collisionQueryRange * 0.25f;
                if (RcVec3f.Dist2DSqr(ag.npos, ag.boundary.GetCenter()) > Sqr(updateThr)
                    || !ag.boundary.IsValid(_navQuery, _filters[ag.option.queryFilterType]))
                {
                    ag.boundary.Update(ag.corridor.GetFirstPoly(), ag.npos, ag.option.collisionQueryRange, _navQuery,
                        _filters[ag.option.queryFilterType]);
                }

                // Query neighbour agents
                GetNeighbours(ag.npos, ag.option.height, ag.option.collisionQueryRange, ag, ref ag.neis, _grid);
            }
        }


        private int GetNeighbours(RcVec3f pos, float height, float range, DtCrowdAgent skip, ref List<DtCrowdNeighbour> result, DtProximityGrid grid)
        {
            result.Clear();

            var proxAgents = new HashSet<DtCrowdAgent>();
            int nids = grid.QueryItems(pos.x - range, pos.z - range, pos.x + range, pos.z + range, ref proxAgents);
            foreach (DtCrowdAgent ag in proxAgents)
            {
                if (ag == skip)
                {
                    continue;
                }

                // Check for overlap.
                RcVec3f diff = pos.Subtract(ag.npos);
                if (Math.Abs(diff.y) >= (height + ag.option.height) / 2.0f)
                {
                    continue;
                }

                diff.y = 0;
                float distSqr = RcVec3f.LenSqr(diff);
                if (distSqr > Sqr(range))
                {
                    continue;
                }

                result.Add(new DtCrowdNeighbour(ag, distSqr));
            }

            result.Sort((o1, o2) => o1.dist.CompareTo(o2.dist));
            return result.Count;
        }

        private void FindCorners(IList<DtCrowdAgent> agents, DtCrowdAgentDebugInfo debug)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.FindCorners);

            DtCrowdAgent debugAgent = debug != null ? debug.agent : null;
            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Find corners for steering
                ag.corridor.FindCorners(ref ag.corners, DT_CROWDAGENT_MAX_CORNERS, _navQuery, _filters[ag.option.queryFilterType]);

                // Check to see if the corner after the next corner is directly visible,
                // and short cut to there.
                if ((ag.option.updateFlags & DtCrowdAgentParams.DT_CROWD_OPTIMIZE_VIS) != 0 && ag.corners.Count > 0)
                {
                    RcVec3f target = ag.corners[Math.Min(1, ag.corners.Count - 1)].pos;
                    ag.corridor.OptimizePathVisibility(target, ag.option.pathOptimizationRange, _navQuery,
                        _filters[ag.option.queryFilterType]);

                    // Copy data for debug purposes.
                    if (debugAgent == ag)
                    {
                        debug.optStart = ag.corridor.GetPos();
                        debug.optEnd = target;
                    }
                }
                else
                {
                    // Copy data for debug purposes.
                    if (debugAgent == ag)
                    {
                        debug.optStart = RcVec3f.Zero;
                        debug.optEnd = RcVec3f.Zero;
                    }
                }
            }
        }

        private void TriggerOffMeshConnections(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.TriggerOffMeshConnections);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    continue;
                }

                // Check
                float triggerRadius = ag.option.radius * 2.25f;
                if (ag.OverOffmeshConnection(triggerRadius))
                {
                    // Prepare to off-mesh connection.
                    DtCrowdAgentAnimation anim = ag.animation;

                    // Adjust the path over the off-mesh connection.
                    long[] refs = new long[2];
                    if (ag.corridor.MoveOverOffmeshConnection(ag.corners[ag.corners.Count - 1].refs, refs, ref anim.startPos,
                            ref anim.endPos, _navQuery))
                    {
                        anim.initPos = ag.npos;
                        anim.polyRef = refs[1];
                        anim.active = true;
                        anim.t = 0.0f;
                        anim.tmax = (RcVec3f.Dist2D(anim.startPos, anim.endPos) / ag.option.maxSpeed) * 0.5f;

                        ag.state = DtCrowdAgentState.DT_CROWDAGENT_STATE_OFFMESH;
                        ag.corners.Clear();
                        ag.neis.Clear();
                        continue;
                    }
                    else
                    {
                        // Path validity check will ensure that bad/blocked connections will be replanned.
                    }
                }
            }
        }

        private void CalculateSteering(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.CalculateSteering);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE)
                {
                    continue;
                }

                RcVec3f dvel = new RcVec3f();

                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    dvel = ag.targetPos;
                    ag.desiredSpeed = ag.targetPos.Length();
                }
                else
                {
                    // Calculate steering direction.
                    if ((ag.option.updateFlags & DtCrowdAgentParams.DT_CROWD_ANTICIPATE_TURNS) != 0)
                    {
                        dvel = ag.CalcSmoothSteerDirection();
                    }
                    else
                    {
                        dvel = ag.CalcStraightSteerDirection();
                    }

                    // Calculate speed scale, which tells the agent to slowdown at the end of the path.
                    float slowDownRadius = ag.option.radius * 2; // TODO: make less hacky.
                    float speedScale = ag.GetDistanceToGoal(slowDownRadius) / slowDownRadius;

                    ag.desiredSpeed = ag.option.maxSpeed;
                    dvel = dvel.Scale(ag.desiredSpeed * speedScale);
                }

                // Separation
                if ((ag.option.updateFlags & DtCrowdAgentParams.DT_CROWD_SEPARATION) != 0)
                {
                    float separationDist = ag.option.collisionQueryRange;
                    float invSeparationDist = 1.0f / separationDist;
                    float separationWeight = ag.option.separationWeight;

                    float w = 0;
                    RcVec3f disp = new RcVec3f();

                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        DtCrowdAgent nei = ag.neis[j].agent;

                        RcVec3f diff = ag.npos.Subtract(nei.npos);
                        diff.y = 0;

                        float distSqr = RcVec3f.LenSqr(diff);
                        if (distSqr < 0.00001f)
                        {
                            continue;
                        }

                        if (distSqr > Sqr(separationDist))
                        {
                            continue;
                        }

                        float dist = (float)Math.Sqrt(distSqr);
                        float weight = separationWeight * (1.0f - Sqr(dist * invSeparationDist));

                        disp = RcVec3f.Mad(disp, diff, weight / dist);
                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        // Adjust desired velocity.
                        dvel = RcVec3f.Mad(dvel, disp, 1.0f / w);
                        // Clamp desired velocity to desired speed.
                        float speedSqr = RcVec3f.LenSqr(dvel);
                        float desiredSqr = Sqr(ag.desiredSpeed);
                        if (speedSqr > desiredSqr)
                        {
                            dvel = dvel.Scale(desiredSqr / speedSqr);
                        }
                    }
                }

                // Set the desired velocity.
                ag.dvel = dvel;
            }
        }

        private void PlanVelocity(DtCrowdAgentDebugInfo debug, IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.PlanVelocity);

            DtCrowdAgent debugAgent = debug != null ? debug.agent : null;
            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                if ((ag.option.updateFlags & DtCrowdAgentParams.DT_CROWD_OBSTACLE_AVOIDANCE) != 0)
                {
                    _obstacleQuery.Reset();

                    // Add neighbours as obstacles.
                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        DtCrowdAgent nei = ag.neis[j].agent;
                        _obstacleQuery.AddCircle(nei.npos, nei.option.radius, nei.vel, nei.dvel);
                    }

                    // Append neighbour segments as obstacles.
                    for (int j = 0; j < ag.boundary.GetSegmentCount(); ++j)
                    {
                        RcVec3f[] s = ag.boundary.GetSegment(j);
                        RcVec3f s3 = s[1];
                        //Array.Copy(s, 3, s3, 0, 3);
                        if (DtUtils.TriArea2D(ag.npos, s[0], s3) < 0.0f)
                        {
                            continue;
                        }

                        _obstacleQuery.AddSegment(s[0], s3);
                    }

                    DtObstacleAvoidanceDebugData vod = null;
                    if (debugAgent == ag)
                    {
                        vod = debug.vod;
                    }

                    // Sample new safe velocity.
                    bool adaptive = true;
                    int ns = 0;

                    DtObstacleAvoidanceParams option = _obstacleQueryParams[ag.option.obstacleAvoidanceType];

                    if (adaptive)
                    {
                        ns = _obstacleQuery.SampleVelocityAdaptive(ag.npos, ag.option.radius, ag.desiredSpeed,
                            ag.vel, ag.dvel, out ag.nvel, option, vod);
                    }
                    else
                    {
                        ns = _obstacleQuery.SampleVelocityGrid(ag.npos, ag.option.radius,
                            ag.desiredSpeed, ag.vel, ag.dvel, out ag.nvel, option, vod);
                    }

                    _velocitySampleCount += ns;
                }
                else
                {
                    // If not using velocity planning, new velocity is directly the desired velocity.
                    ag.nvel = ag.dvel;
                }
            }
        }

        private void Integrate(float dt, IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.Integrate);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                ag.Integrate(dt);
            }
        }

        private void HandleCollisions(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.HandleCollisions);

            for (int iter = 0; iter < 4; ++iter)
            {
                foreach (DtCrowdAgent ag in agents)
                {
                    long idx0 = ag.idx;
                    if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.disp = RcVec3f.Zero;

                    float w = 0;

                    for (int j = 0; j < ag.neis.Count; ++j)
                    {
                        DtCrowdAgent nei = ag.neis[j].agent;
                        long idx1 = nei.idx;
                        RcVec3f diff = ag.npos.Subtract(nei.npos);
                        diff.y = 0;

                        float dist = RcVec3f.LenSqr(diff);
                        if (dist > Sqr(ag.option.radius + nei.option.radius))
                        {
                            continue;
                        }

                        dist = (float)Math.Sqrt(dist);
                        float pen = (ag.option.radius + nei.option.radius) - dist;
                        if (dist < 0.0001f)
                        {
                            // Agents on top of each other, try to choose diverging separation directions.
                            if (idx0 > idx1)
                            {
                                diff.Set(-ag.dvel.z, 0, ag.dvel.x);
                            }
                            else
                            {
                                diff.Set(ag.dvel.z, 0, -ag.dvel.x);
                            }

                            pen = 0.01f;
                        }
                        else
                        {
                            pen = (1.0f / dist) * (pen * 0.5f) * _config.collisionResolveFactor;
                        }

                        ag.disp = RcVec3f.Mad(ag.disp, diff, pen);

                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        float iw = 1.0f / w;
                        ag.disp = ag.disp.Scale(iw);
                    }
                }

                foreach (DtCrowdAgent ag in agents)
                {
                    if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                    {
                        continue;
                    }

                    ag.npos = ag.npos.Add(ag.disp);
                }
            }
        }

        private void MoveAgents(IList<DtCrowdAgent> agents)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.MoveAgents);

            foreach (DtCrowdAgent ag in agents)
            {
                if (ag.state != DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING)
                {
                    continue;
                }

                // Move along navmesh.
                ag.corridor.MovePosition(ag.npos, _navQuery, _filters[ag.option.queryFilterType]);
                // Get valid constrained position back.
                ag.npos = ag.corridor.GetPos();

                // If not using path, truncate the corridor to just one poly.
                if (ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_NONE
                    || ag.targetState == DtMoveRequestState.DT_CROWDAGENT_TARGET_VELOCITY)
                {
                    ag.corridor.Reset(ag.corridor.GetFirstPoly(), ag.npos);
                    ag.partial = false;
                }
            }
        }

        private void UpdateOffMeshConnections(IList<DtCrowdAgent> agents, float dt)
        {
            using var timer = _telemetry.ScopedTimer(DtCrowdTimerLabel.UpdateOffMeshConnections);

            foreach (DtCrowdAgent ag in agents)
            {
                DtCrowdAgentAnimation anim = ag.animation;
                if (!anim.active)
                {
                    continue;
                }

                anim.t += dt;
                if (anim.t > anim.tmax)
                {
                    // Reset animation
                    anim.active = false;
                    // Prepare agent for walking.
                    ag.state = DtCrowdAgentState.DT_CROWDAGENT_STATE_WALKING;
                    continue;
                }

                // Update position
                float ta = anim.tmax * 0.15f;
                float tb = anim.tmax;
                if (anim.t < ta)
                {
                    float u = Tween(anim.t, 0.0f, ta);
                    ag.npos = RcVec3f.Lerp(anim.initPos, anim.startPos, u);
                }
                else
                {
                    float u = Tween(anim.t, ta, tb);
                    ag.npos = RcVec3f.Lerp(anim.startPos, anim.endPos, u);
                }

                // Update velocity.
                ag.vel = RcVec3f.Zero;
                ag.dvel = RcVec3f.Zero;
            }
        }

        private float Tween(float t, float t0, float t1)
        {
            return Clamp((t - t0) / (t1 - t0), 0.0f, 1.0f);
        }
    }
}