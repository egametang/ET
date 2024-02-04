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
using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Detour.Crowd
{
    /// Represents an agent managed by a #dtCrowd object.
    /// @ingroup crowd
    public class DtCrowdAgent
    {
        public readonly long idx;

        /// The type of mesh polygon the agent is traversing. (See: #CrowdAgentState)
        public DtCrowdAgentState state;

        /// True if the agent has valid path (targetState == DT_CROWDAGENT_TARGET_VALID) and the path does not lead to the
        /// requested position, else false.
        public bool partial;

        /// The path corridor the agent is using.
        public DtPathCorridor corridor;

        /// The local boundary data for the agent.
        public DtLocalBoundary boundary;

        /// Time since the agent's path corridor was optimized.
        public float topologyOptTime;

        /// The known neighbors of the agent.
        public List<DtCrowdNeighbour> neis = new List<DtCrowdNeighbour>();

        /// The desired speed.
        public float desiredSpeed;

        public RcVec3f npos = new RcVec3f();

        /// < The current agent position. [(x, y, z)]
        public RcVec3f disp = new RcVec3f();

        /// < A temporary value used to accumulate agent displacement during iterative
        /// collision resolution. [(x, y, z)]
        public RcVec3f dvel = new RcVec3f();

        /// < The desired velocity of the agent. Based on the current path, calculated
        /// from
        /// scratch each frame. [(x, y, z)]
        public RcVec3f nvel = new RcVec3f();

        /// < The desired velocity adjusted by obstacle avoidance, calculated from scratch each
        /// frame. [(x, y, z)]
        public RcVec3f vel = new RcVec3f();

        /// < The actual velocity of the agent. The change from nvel -> vel is
        /// constrained by max acceleration. [(x, y, z)]
        /// The agent's configuration parameters.
        public DtCrowdAgentParams option;

        /// The local path corridor corners for the agent.
        public List<StraightPathItem> corners = new List<StraightPathItem>();

        public DtMoveRequestState targetState;

        /// < State of the movement request.
        public long targetRef;

        /// < Target polyref of the movement request.
        public RcVec3f targetPos = new RcVec3f();

        /// < Target position of the movement request (or velocity in case of
        /// DT_CROWDAGENT_TARGET_VELOCITY).
        public DtPathQueryResult targetPathQueryResult;

        /// < Path finder query
        public bool targetReplan;

        /// < Flag indicating that the current path is being replanned.
        public float targetReplanTime;

        /// <Time since the agent's target was replanned.
        public float targetReplanWaitTime;

        public DtCrowdAgentAnimation animation;

        public DtCrowdAgent(int idx)
        {
            this.idx = idx;
            corridor = new DtPathCorridor();
            boundary = new DtLocalBoundary();
            animation = new DtCrowdAgentAnimation();
        }

        public void Integrate(float dt)
        {
            // Fake dynamic constraint.
            float maxDelta = option.maxAcceleration * dt;
            RcVec3f dv = nvel.Subtract(vel);
            float ds = dv.Length();
            if (ds > maxDelta)
                dv = dv.Scale(maxDelta / ds);
            vel = vel.Add(dv);

            // Integrate
            if (vel.Length() > 0.0001f)
                npos = RcVec3f.Mad(npos, vel, dt);
            else
                vel = RcVec3f.Zero;
        }

        public bool OverOffmeshConnection(float radius)
        {
            if (0 == corners.Count)
                return false;

            bool offMeshConnection = ((corners[corners.Count - 1].flags
                                       & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                ? true
                : false;
            if (offMeshConnection)
            {
                float distSq = RcVec3f.Dist2DSqr(npos, corners[corners.Count - 1].pos);
                if (distSq < radius * radius)
                    return true;
            }

            return false;
        }

        public float GetDistanceToGoal(float range)
        {
            if (0 == corners.Count)
                return range;

            bool endOfPath = ((corners[corners.Count - 1].flags & DtNavMeshQuery.DT_STRAIGHTPATH_END) != 0) ? true : false;
            if (endOfPath)
                return Math.Min(RcVec3f.Dist2D(npos, corners[corners.Count - 1].pos), range);

            return range;
        }

        public RcVec3f CalcSmoothSteerDirection()
        {
            RcVec3f dir = new RcVec3f();
            if (0 < corners.Count)
            {
                int ip0 = 0;
                int ip1 = Math.Min(1, corners.Count - 1);
                var p0 = corners[ip0].pos;
                var p1 = corners[ip1].pos;

                var dir0 = p0.Subtract(npos);
                var dir1 = p1.Subtract(npos);
                dir0.y = 0;
                dir1.y = 0;

                float len0 = dir0.Length();
                float len1 = dir1.Length();
                if (len1 > 0.001f)
                    dir1 = dir1.Scale(1.0f / len1);

                dir.x = dir0.x - dir1.x * len0 * 0.5f;
                dir.y = 0;
                dir.z = dir0.z - dir1.z * len0 * 0.5f;
                dir.Normalize();
            }

            return dir;
        }

        public RcVec3f CalcStraightSteerDirection()
        {
            RcVec3f dir = new RcVec3f();
            if (0 < corners.Count)
            {
                dir = corners[0].pos.Subtract(npos);
                dir.y = 0;
                dir.Normalize();
            }

            return dir;
        }

        public void SetTarget(long refs, RcVec3f pos)
        {
            targetRef = refs;
            targetPos = pos;
            targetPathQueryResult = null;
            if (targetRef != 0)
            {
                targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_REQUESTING;
            }
            else
            {
                targetState = DtMoveRequestState.DT_CROWDAGENT_TARGET_FAILED;
            }
        }
    }
}