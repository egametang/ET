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
    using static DotRecast.Core.RcMath;

    /**
 * Represents a dynamic polygon corridor used to plan agent movement.
 *
 * The corridor is loaded with a path, usually obtained from a #NavMeshQuery::FindPath() query. The corridor is then
 * used to plan local movement, with the corridor automatically updating as needed to deal with inaccurate agent
 * locomotion.
 *
 * Example of a common use case:
 *
 * -# Construct the corridor object and call -# Obtain a path from a #dtNavMeshQuery object. -# Use #Reset() to set the
 * agent's current position. (At the beginning of the path.) -# Use #SetCorridor() to load the path and target. -# Use
 * #FindCorners() to plan movement. (This handles dynamic path straightening.) -# Use #MovePosition() to feed agent
 * movement back into the corridor. (The corridor will automatically adjust as needed.) -# If the target is moving, use
 * #MoveTargetPosition() to update the end of the corridor. (The corridor will automatically adjust as needed.) -#
 * Repeat the previous 3 steps to continue to move the agent.
 *
 * The corridor position and target are always constrained to the navigation mesh.
 *
 * One of the difficulties in maintaining a path is that floating point errors, locomotion inaccuracies, and/or local
 * steering can result in the agent crossing the boundary of the path corridor, temporarily invalidating the path. This
 * class uses local mesh queries to detect and update the corridor as needed to handle these types of issues.
 *
 * The fact that local mesh queries are used to move the position and target locations results in two beahviors that
 * need to be considered:
 *
 * Every time a move function is used there is a chance that the path will become non-optimial. Basically, the further
 * the target is moved from its original location, and the further the position is moved outside the original corridor,
 * the more likely the path will become non-optimal. This issue can be addressed by periodically running the
 * #OptimizePathTopology() and #OptimizePathVisibility() methods.
 *
 * All local mesh queries have distance limitations. (Review the #dtNavMeshQuery methods for details.) So the most
 * accurate use case is to move the position and target in small increments. If a large increment is used, then the
 * corridor may not be able to accurately find the new location. Because of this limiation, if a position is moved in a
 * large increment, then compare the desired and resulting polygon references. If the two do not match, then path
 * replanning may be needed. E.g. If you move the target, check #GetLastPoly() to see if it is the expected polygon.
 *
 */
    public class DtPathCorridor
    {
        private RcVec3f m_pos = new RcVec3f();
        private RcVec3f m_target = new RcVec3f();
        private List<long> m_path;


        /**
     * Allocates the corridor's path buffer.
     */
        public DtPathCorridor()
        {
            m_path = new List<long>();
        }

        /**
     * Resets the path corridor to the specified position.
     *
     * @param ref
     *            The polygon reference containing the position.
     * @param pos
     *            The new position in the corridor. [(x, y, z)]
     */
        public void Reset(long refs, RcVec3f pos)
        {
            m_path.Clear();
            m_path.Add(refs);
            m_pos = pos;
            m_target = pos;
        }

        private static readonly float MIN_TARGET_DIST = Sqr(0.01f);

        /**
     * Finds the corners in the corridor from the position toward the target. (The straightened path.)
     *
     * This is the function used to plan local movement within the corridor. One or more corners can be detected in
     * order to plan movement. It performs essentially the same function as #dtNavMeshQuery::findStraightPath.
     *
     * Due to internal optimizations, the maximum number of corners returned will be (@p maxCorners - 1) For example: If
     * the buffers are sized to hold 10 corners, the function will never return more than 9 corners. So if 10 corners
     * are needed, the buffers should be sized for 11 corners.
     *
     * If the target is within range, it will be the last corner and have a polygon reference id of zero.
     *
     * @param filter
     *
     * @param[in] navquery The query object used to build the corridor.
     * @return Corners
     */
        public int FindCorners(ref List<StraightPathItem> corners, int maxCorners, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            var result = navquery.FindStraightPath(m_pos, m_target, m_path, ref corners, maxCorners, 0);
            if (result.Succeeded())
            {
                // Prune points in the beginning of the path which are too close.
                int start = 0;
                foreach (StraightPathItem spi in corners)
                {
                    if ((spi.flags & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
                        || RcVec3f.Dist2DSqr(spi.pos, m_pos) > MIN_TARGET_DIST)
                    {
                        break;
                    }

                    start++;
                }

                int end = corners.Count;
                // Prune points after an off-mesh connection.
                for (int i = start; i < corners.Count; i++)
                {
                    StraightPathItem spi = corners[i];
                    if ((spi.flags & DtNavMeshQuery.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0)
                    {
                        end = i + 1;
                        break;
                    }
                }

                corners = corners.GetRange(start, end - start);
            }

            return corners.Count;
        }

        /**
     * Attempts to optimize the path if the specified point is visible from the current position.
     *
     * Inaccurate locomotion or dynamic obstacle avoidance can force the agent position significantly outside the
     * original corridor. Over time this can result in the formation of a non-optimal corridor. Non-optimal paths can
     * also form near the corners of tiles.
     *
     * This function uses an efficient local visibility search to try to optimize the corridor between the current
     * position and @p next.
     *
     * The corridor will change only if @p next is visible from the current position and moving directly toward the
     * point is better than following the existing path.
     *
     * The more inaccurate the agent movement, the more beneficial this function becomes. Simply adjust the frequency of
     * the call to match the needs to the agent.
     *
     * This function is not suitable for long distance searches.
     *
     * @param next
     *            The point to search toward. [(x, y, z])
     * @param pathOptimizationRange
     *            The maximum range to search. [Limit: > 0]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public void OptimizePathVisibility(RcVec3f next, float pathOptimizationRange, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            // Clamp the ray to max distance.
            float dist = RcVec3f.Dist2D(m_pos, next);

            // If too close to the goal, do not try to optimize.
            if (dist < 0.01f)
            {
                return;
            }

            // Overshoot a little. This helps to optimize open fields in tiled
            // meshes.
            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            // Adjust ray length.
            var delta = next.Subtract(m_pos);
            RcVec3f goal = RcVec3f.Mad(m_pos, delta, pathOptimizationRange / dist);

            var status = navquery.Raycast(m_path[0], m_pos, goal, filter, 0, 0, out var rayHit);
            if (status.Succeeded())
            {
                if (rayHit.path.Count > 1 && rayHit.t > 0.99f)
                {
                    m_path = PathUtils.MergeCorridorStartShortcut(m_path, rayHit.path);
                }
            }
        }

        /**
     * Attempts to optimize the path using a local area search. (Partial replanning.)
     *
     * Inaccurate locomotion or dynamic obstacle avoidance can force the agent position significantly outside the
     * original corridor. Over time this can result in the formation of a non-optimal corridor. This function will use a
     * local area path search to try to re-optimize the corridor.
     *
     * The more inaccurate the agent movement, the more beneficial this function becomes. Simply adjust the frequency of
     * the call to match the needs to the agent.
     *
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     *
     */
        public bool OptimizePathTopology(DtNavMeshQuery navquery, IDtQueryFilter filter, int maxIterations)
        {
            if (m_path.Count < 3)
            {
                return false;
            }

            var res = new List<long>();
            navquery.InitSlicedFindPath(m_path[0], m_path[m_path.Count - 1], m_pos, m_target, filter, 0);
            navquery.UpdateSlicedFindPath(maxIterations, out var _);
            var status = navquery.FinalizeSlicedFindPathPartial(m_path, ref res);

            if (status.Succeeded() && res.Count > 0)
            {
                m_path = PathUtils.MergeCorridorStartShortcut(m_path, res);
                return true;
            }

            return false;
        }

        public bool MoveOverOffmeshConnection(long offMeshConRef, long[] refs, ref RcVec3f startPos, ref RcVec3f endPos, DtNavMeshQuery navquery)
        {
            // Advance the path up to and over the off-mesh connection.
            long prevRef = 0, polyRef = m_path[0];
            int npos = 0;
            while (npos < m_path.Count && polyRef != offMeshConRef)
            {
                prevRef = polyRef;
                polyRef = m_path[npos];
                npos++;
            }

            if (npos == m_path.Count)
            {
                // Could not find offMeshConRef
                return false;
            }

            // Prune path
            m_path = m_path.GetRange(npos, m_path.Count - npos);
            refs[0] = prevRef;
            refs[1] = polyRef;

            DtNavMesh nav = navquery.GetAttachedNavMesh();
            var startEnd = nav.GetOffMeshConnectionPolyEndPoints(refs[0], refs[1], ref startPos, ref endPos);
            if (startEnd.Succeeded())
            {
                m_pos = endPos;
                return true;
            }

            return false;
        }

        /**
     * Moves the position from the current location to the desired location, adjusting the corridor as needed to reflect
     * the change.
     *
     * Behavior:
     *
     * - The movement is constrained to the surface of the navigation mesh. - The corridor is automatically adjusted
     * (shorted or lengthened) in order to remain valid. - The new position will be located in the adjusted corridor's
     * first polygon.
     *
     * The expected use case is that the desired position will be 'near' the current corridor. What is considered 'near'
     * depends on local polygon density, query search extents, etc.
     *
     * The resulting position will differ from the desired position if the desired position is not on the navigation
     * mesh, or it can't be reached using a local search.
     *
     * @param npos
     *            The desired new position. [(x, y, z)]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public bool MovePosition(RcVec3f npos, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            // Move along navmesh and update new position.
            var visited = new List<long>();
            var status = navquery.MoveAlongSurface(m_path[0], m_pos, npos, filter, out var result, ref visited);
            if (status.Succeeded())
            {
                m_path = PathUtils.MergeCorridorStartMoved(m_path, visited);

                // Adjust the position to stay on top of the navmesh.
                m_pos = result;
                status = navquery.GetPolyHeight(m_path[0], result, out var h);
                if (status.Succeeded())
                {
                    m_pos.y = h;
                }

                return true;
            }

            return false;
        }

        /**
     * Moves the target from the curent location to the desired location, adjusting the corridor as needed to reflect
     * the change. Behavior: - The movement is constrained to the surface of the navigation mesh. - The corridor is
     * automatically adjusted (shorted or lengthened) in order to remain valid. - The new target will be located in the
     * adjusted corridor's last polygon.
     *
     * The expected use case is that the desired target will be 'near' the current corridor. What is considered 'near'
     * depends on local polygon density, query search extents, etc. The resulting target will differ from the desired
     * target if the desired target is not on the navigation mesh, or it can't be reached using a local search.
     *
     * @param npos
     *            The desired new target position. [(x, y, z)]
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     */
        public bool MoveTargetPosition(RcVec3f npos, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            // Move along navmesh and update new position.
            var visited = new List<long>();
            var status = navquery.MoveAlongSurface(m_path[m_path.Count - 1], m_target, npos, filter, out var result, ref visited);
            if (status.Succeeded())
            {
                m_path = PathUtils.MergeCorridorEndMoved(m_path, visited);
                // TODO: should we do that?
                // Adjust the position to stay on top of the navmesh.
                /*
                 * float h = m_target.y; navquery->GetPolyHeight(m_path[m_npath-1],
                 * result, &h); result.y = h;
                 */
                m_target = result;
                return true;
            }

            return false;
        }

        /**
     * Loads a new path and target into the corridor. The current corridor position is expected to be within the first
     * polygon in the path. The target is expected to be in the last polygon.
     *
     * @warning The size of the path must not exceed the size of corridor's path buffer set during #Init().
     * @param target
     *            The target location within the last polygon of the path. [(x, y, z)]
     * @param path
     *            The path corridor.
     */
        public void SetCorridor(RcVec3f target, List<long> path)
        {
            m_target = target;
            m_path = new List<long>(path);
        }

        public void FixPathStart(long safeRef, RcVec3f safePos)
        {
            m_pos = safePos;
            if (m_path.Count < 3 && m_path.Count > 0)
            {
                long p = m_path[m_path.Count - 1];
                m_path.Clear();
                m_path.Add(safeRef);
                m_path.Add(0L);
                m_path.Add(p);
            }
            else
            {
                m_path.Clear();
                m_path.Add(safeRef);
                m_path.Add(0L);
            }
        }

        public bool TrimInvalidPath(long safeRef, float[] safePos, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            // Keep valid path as far as possible.
            int n = 0;
            while (n < m_path.Count && navquery.IsValidPolyRef(m_path[n], filter))
            {
                n++;
            }

            if (m_path.Count == n)
            {
                // All valid, no need to fix.
                return true;
            }
            else if (n == 0)
            {
                // The first polyref is bad, use current safe values.
                m_pos.Set(safePos);
                m_path.Clear();
                m_path.Add(safeRef);
            }
            else if (n < m_path.Count)
            {
                // The path is partially usable.
                m_path = m_path.GetRange(0, n);
            }

            // Clamp target pos to last poly
            navquery.ClosestPointOnPolyBoundary(m_path[m_path.Count - 1], m_target, out m_target);
            return true;
        }

        /**
     * Checks the current corridor path to see if its polygon references remain valid. The path can be invalidated if
     * there are structural changes to the underlying navigation mesh, or the state of a polygon within the path changes
     * resulting in it being filtered out. (E.g. An exclusion or inclusion flag changes.)
     *
     * @param maxLookAhead
     *            The number of polygons from the beginning of the corridor to search.
     * @param navquery
     *            The query object used to build the corridor.
     * @param filter
     *            The filter to apply to the operation.
     * @return
     */
        public bool IsValid(int maxLookAhead, DtNavMeshQuery navquery, IDtQueryFilter filter)
        {
            // Check that all polygons still pass query filter.
            int n = Math.Min(m_path.Count, maxLookAhead);
            for (int i = 0; i < n; ++i)
            {
                if (!navquery.IsValidPolyRef(m_path[i], filter))
                {
                    return false;
                }
            }

            return true;
        }

        /**
     * Gets the current position within the corridor. (In the first polygon.)
     *
     * @return The current position within the corridor.
     */
        public RcVec3f GetPos()
        {
            return m_pos;
        }

        /**
     * Gets the current target within the corridor. (In the last polygon.)
     *
     * @return The current target within the corridor.
     */
        public RcVec3f GetTarget()
        {
            return m_target;
        }

        /**
     * The polygon reference id of the first polygon in the corridor, the polygon containing the position.
     *
     * @return The polygon reference id of the first polygon in the corridor. (Or zero if there is no path.)
     */
        public long GetFirstPoly()
        {
            return 0 == m_path.Count ? 0 : m_path[0];
        }

        /**
     * The polygon reference id of the last polygon in the corridor, the polygon containing the target.
     *
     * @return The polygon reference id of the last polygon in the corridor. (Or zero if there is no path.)
     */
        public long GetLastPoly()
        {
            return 0 == m_path.Count ? 0 : m_path[m_path.Count - 1];
        }

        /**
     * The corridor's path.
     */
        public List<long> GetPath()
        {
            return m_path;
        }

        /**
     * The number of polygons in the current corridor path.
     *
     * @return The number of polygons in the current corridor path.
     */
        public int GetPathCount()
        {
            return m_path.Count;
        }
    }
}