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

namespace DotRecast.Detour
{
    using static RcMath;
    using static DtNode;

    public class DtNavMeshQuery
    {
        /**
     * Use raycasts during pathfind to "shortcut" (raycast still consider costs) Options for
     * NavMeshQuery::initSlicedFindPath and updateSlicedFindPath
     */
        public const int DT_FINDPATH_ANY_ANGLE = 0x02;

        /** Raycast should calculate movement cost along the ray and fill RaycastHit::cost */
        public const int DT_RAYCAST_USE_COSTS = 0x01;

        /// Vertex flags returned by findStraightPath.
        /** The vertex is the start position in the path. */
        public const int DT_STRAIGHTPATH_START = 0x01;

        /** The vertex is the end position in the path. */
        public const int DT_STRAIGHTPATH_END = 0x02;

        /** The vertex is the start of an off-mesh connection. */
        public const int DT_STRAIGHTPATH_OFFMESH_CONNECTION = 0x04;

        /// Options for findStraightPath.
        public const int DT_STRAIGHTPATH_AREA_CROSSINGS = 0x01;

        /// < Add a vertex at every polygon edge crossing
        /// where area changes.
        public const int DT_STRAIGHTPATH_ALL_CROSSINGS = 0x02;

        /// < Add a vertex at every polygon edge crossing.
        protected readonly DtNavMesh m_nav;

        protected readonly DtNodePool m_nodePool;
        protected readonly DtNodeQueue m_openList;
        protected DtQueryData m_query;

        /// < Sliced query state.
        public DtNavMeshQuery(DtNavMesh nav)
        {
            m_nav = nav;
            m_nodePool = new DtNodePool();
            m_openList = new DtNodeQueue();
        }

        /// Returns random location on navmesh.
        /// Polygons are chosen weighted by area. The search runs in linear related to number of polygon.
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[in]		frand			Function returning a random number [0..1).
        ///  @param[out]	randomRef		The reference id of the random location.
        ///  @param[out]	randomPt		The random location. 
        /// @returns The status flags for the query.
        public DtStatus FindRandomPoint(IDtQueryFilter filter, IRcRand frand, out long randomRef, out RcVec3f randomPt)
        {
            randomRef = 0;
            randomPt = RcVec3f.Zero;

            if (null == filter || null == frand)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // Randomly pick one tile. Assume that all tiles cover roughly the same area.
            DtMeshTile tile = null;
            float tsum = 0.0f;
            for (int i = 0; i < m_nav.GetMaxTiles(); i++)
            {
                DtMeshTile mt = m_nav.GetTile(i);
                if (mt == null || mt.data == null || mt.data.header == null)
                {
                    continue;
                }

                // Choose random tile using reservoir sampling.
                float area = 1.0f; // Could be tile area too.
                tsum += area;
                float u = frand.Next();
                if (u * tsum <= area)
                {
                    tile = mt;
                }
            }

            if (tile == null)
            {
                return DtStatus.DT_FAILURE;
            }

            // Randomly pick one polygon weighted by polygon area.
            DtPoly poly = null;
            long polyRef = 0;
            long @base = m_nav.GetPolyRefBase(tile);

            float areaSum = 0.0f;
            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly p = tile.data.polys[i];
                // Do not return off-mesh connection polygons.
                if (p.GetPolyType() != DtPoly.DT_POLYTYPE_GROUND)
                {
                    continue;
                }

                // Must pass filter
                long refs = @base | (long)i;
                if (!filter.PassFilter(refs, tile, p))
                {
                    continue;
                }

                // Calc area of the polygon.
                float polyArea = 0.0f;
                for (int j = 2; j < p.vertCount; ++j)
                {
                    int va = p.verts[0] * 3;
                    int vb = p.verts[j - 1] * 3;
                    int vc = p.verts[j] * 3;
                    polyArea += DtUtils.TriArea2D(tile.data.verts, va, vb, vc);
                }

                // Choose random polygon weighted by area, using reservoir sampling.
                areaSum += polyArea;
                float u = frand.Next();
                if (u * areaSum <= polyArea)
                {
                    poly = p;
                    polyRef = refs;
                }
            }

            if (poly == null)
            {
                return DtStatus.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            float[] verts = new float[3 * m_nav.GetMaxVertsPerPoly()];
            float[] areas = new float[m_nav.GetMaxVertsPerPoly()];
            Array.Copy(tile.data.verts, poly.verts[0] * 3, verts, 0, 3);
            for (int j = 1; j < poly.vertCount; ++j)
            {
                Array.Copy(tile.data.verts, poly.verts[j] * 3, verts, j * 3, 3);
            }

            float s = frand.Next();
            float t = frand.Next();

            var pt = DtUtils.RandomPointInConvexPoly(verts, poly.vertCount, areas, s, t);
            ClosestPointOnPoly(polyRef, pt, out var closest, out var _);

            randomRef = polyRef;
            randomPt = closest;

            return DtStatus.DT_SUCCSESS;
        }

        /**
     * Returns random location on navmesh within the reach of specified location. Polygons are chosen weighted by area.
     * The search runs in linear related to number of polygon. The location is not exactly constrained by the circle,
     * but it limits the visited polygons.
     *
     * @param startRef
     *            The reference id of the polygon where the search starts.
     * @param centerPos
     *            The center of the search circle. [(x, y, z)]
     * @param maxRadius
     * @param filter
     *            The polygon filter to apply to the query.
     * @param frand
     *            Function returning a random number [0..1).
     * @return Random location
     */
        public DtStatus FindRandomPointAroundCircle(long startRef, RcVec3f centerPos, float maxRadius,
            IDtQueryFilter filter, IRcRand frand, out long randomRef, out RcVec3f randomPt)
        {
            return FindRandomPointAroundCircle(startRef, centerPos, maxRadius, filter, frand, DtNoOpDtPolygonByCircleConstraint.Shared, out randomRef, out randomPt);
        }

        /**
     * Returns random location on navmesh within the reach of specified location. Polygons are chosen weighted by area.
     * The search runs in linear related to number of polygon. The location is strictly constrained by the circle.
     *
     * @param startRef
     *            The reference id of the polygon where the search starts.
     * @param centerPos
     *            The center of the search circle. [(x, y, z)]
     * @param maxRadius
     * @param filter
     *            The polygon filter to apply to the query.
     * @param frand
     *            Function returning a random number [0..1).
     * @return Random location
     */
        public DtStatus FindRandomPointWithinCircle(long startRef, RcVec3f centerPos, float maxRadius,
            IDtQueryFilter filter, IRcRand frand, out long randomRef, out RcVec3f randomPt)
        {
            return FindRandomPointAroundCircle(startRef, centerPos, maxRadius, filter, frand, DtStrictDtPolygonByCircleConstraint.Shared, out randomRef, out randomPt);
        }

        public DtStatus FindRandomPointAroundCircle(long startRef, RcVec3f centerPos, float maxRadius,
            IDtQueryFilter filter, IRcRand frand, IDtPolygonByCircleConstraint constraint,
            out long randomRef, out RcVec3f randomPt)
        {
            randomRef = startRef;
            randomPt = centerPos;

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(centerPos) || maxRadius < 0
                || !float.IsFinite(maxRadius) || null == filter || null == frand)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nav.GetTileAndPolyByRefUnsafe(startRef, out var startTile, out var startPoly);
            if (!filter.PassFilter(startRef, startTile, startPoly))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DT_NODE_OPEN;
            m_openList.Push(startNode);

            DtStatus status = DtStatus.DT_SUCCSESS;

            float radiusSqr = maxRadius * maxRadius;
            float areaSum = 0.0f;

            DtPoly randomPoly = null;
            long randomPolyRef = 0;
            float[] randomPolyVerts = null;

            while (!m_openList.IsEmpty())
            {
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DT_NODE_OPEN;
                bestNode.flags |= DT_NODE_CLOSED;
                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long bestRef = bestNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out var bestTile, out var bestPoly);

                // Place random locations on on ground.
                if (bestPoly.GetPolyType() == DtPoly.DT_POLYTYPE_GROUND)
                {
                    // Calc area of the polygon.
                    float polyArea = 0.0f;
                    float[] polyVerts = new float[bestPoly.vertCount * 3];
                    for (int j = 0; j < bestPoly.vertCount; ++j)
                    {
                        Array.Copy(bestTile.data.verts, bestPoly.verts[j] * 3, polyVerts, j * 3, 3);
                    }

                    float[] constrainedVerts = constraint.Apply(polyVerts, centerPos, maxRadius);
                    if (constrainedVerts != null)
                    {
                        int vertCount = constrainedVerts.Length / 3;
                        for (int j = 2; j < vertCount; ++j)
                        {
                            int va = 0;
                            int vb = (j - 1) * 3;
                            int vc = j * 3;
                            polyArea += DtUtils.TriArea2D(constrainedVerts, va, vb, vc);
                        }

                        // Choose random polygon weighted by area, using reservoir sampling.
                        areaSum += polyArea;
                        float u = frand.Next();
                        if (u * areaSum <= polyArea)
                        {
                            randomPoly = bestPoly;
                            randomPolyRef = bestRef;
                            randomPolyVerts = constrainedVerts;
                        }
                    }
                }

                // Get parent poly and tile.
                long parentRef = 0;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    DtLink link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    var ppStatus = GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef,
                        neighbourPoly, neighbourTile, out var va, out var vb);
                    if (ppStatus.Failed())
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    var distSqr = DtUtils.DistancePtSegSqr2D(centerPos, va, vb, out var tesg);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef);
                    if (null == neighbourNode)
                    {
                        status |= DtStatus.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = RcVec3f.Lerp(va, vb, 0.5f);
                    }

                    float total = bestNode.total + RcVec3f.Distance(bestNode.pos, neighbourNode.pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~DtNode.DT_NODE_CLOSED);
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            if (randomPoly == null)
            {
                return DtStatus.DT_FAILURE;
            }

            // Randomly pick point on polygon.
            float s = frand.Next();
            float t = frand.Next();

            float[] areas = new float[randomPolyVerts.Length / 3];
            RcVec3f pt = DtUtils.RandomPointInConvexPoly(randomPolyVerts, randomPolyVerts.Length / 3, areas, s, t);
            ClosestPointOnPoly(randomPolyRef, pt, out var closest, out var _);

            randomRef = randomPolyRef;
            randomPt = closest;

            return status;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        /// @par
        ///
        /// Uses the detail polygons to find the surface height. (Most accurate.)
        ///
        /// @p pos does not have to be within the bounds of the polygon or navigation mesh.
        ///
        /// See ClosestPointOnPolyBoundary() for a limited but faster option.
        ///
        /// Finds the closest point on the specified polygon.
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] pos The position to check. [(x, y, z)]
        /// @param[out] closest
        /// @param[out] posOverPoly
        /// @returns The status flags for the query.
        public DtStatus ClosestPointOnPoly(long refs, RcVec3f pos, out RcVec3f closest, out bool posOverPoly)
        {
            closest = pos;
            posOverPoly = false;

            if (!m_nav.IsValidPolyRef(refs) || !RcVec3f.IsFinite(pos))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nav.ClosestPointOnPoly(refs, pos, out closest, out posOverPoly);
            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// Much faster than ClosestPointOnPoly().
        ///
        /// If the provided position lies within the polygon's xz-bounds (above or below), 
        /// then @p pos and @p closest will be equal.
        ///
        /// The height of @p closest will be the polygon boundary.  The height detail is not used.
        /// 
        /// @p pos does not have to be within the bounds of the polybon or the navigation mesh.
        /// 
        /// Returns a point on the boundary closest to the source point if the source point is outside the 
        /// polygon's xz-bounds.
        ///  @param[in]		ref			The reference id to the polygon.
        ///  @param[in]		pos			The position to check. [(x, y, z)]
        ///  @param[out]	closest		The closest point. [(x, y, z)]
        /// @returns The status flags for the query.
        public DtStatus ClosestPointOnPolyBoundary(long refs, RcVec3f pos, out RcVec3f closest)
        {
            closest = pos;
            var status = m_nav.GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (tile == null || !RcVec3f.IsFinite(pos))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // Collect vertices.
            float[] verts = new float[m_nav.GetMaxVertsPerPoly() * 3];
            float[] edged = new float[m_nav.GetMaxVertsPerPoly()];
            float[] edget = new float[m_nav.GetMaxVertsPerPoly()];
            int nv = poly.vertCount;
            for (int i = 0; i < nv; ++i)
            {
                Array.Copy(tile.data.verts, poly.verts[i] * 3, verts, i * 3, 3);
            }

            if (DtUtils.DistancePtPolyEdgesSqr(pos, verts, nv, edged, edget))
            {
                closest = pos;
            }
            else
            {
                // Point is outside the polygon, dtClamp to nearest edge.
                float dmin = edged[0];
                int imin = 0;
                for (int i = 1; i < nv; ++i)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }

                int va = imin * 3;
                int vb = ((imin + 1) % nv) * 3;
                closest = RcVec3f.Lerp(verts, va, vb, edget[imin]);
            }

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// Will return #DT_FAILURE if the provided position is outside the xz-bounds
        /// of the polygon.
        ///
        /// Gets the height of the polygon at the provided position using the height detail. (Most accurate.)
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] pos A position within the xz-bounds of the polygon. [(x, y, z)]
        /// @param[out] height The height at the surface of the polygon.
        /// @returns The status flags for the query.
        public DtStatus GetPolyHeight(long refs, RcVec3f pos, out float height)
        {
            height = default;

            var status = m_nav.GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (!RcVec3f.IsFinite2D(pos))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // We used to return success for offmesh connections, but the
            // getPolyHeight in DetourNavMesh does not do this, so special
            // case it here.
            if (poly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                int i = poly.verts[0] * 3;
                var v0 = new RcVec3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                i = poly.verts[1] * 3;
                var v1 = new RcVec3f { x = tile.data.verts[i], y = tile.data.verts[i + 1], z = tile.data.verts[i + 2] };
                DtUtils.DistancePtSegSqr2D(pos, v0, v1, out var t);
                height = v0.y + (v1.y - v0.y) * t;

                return DtStatus.DT_SUCCSESS;
            }

            if (!m_nav.GetPolyHeight(tile, poly, pos, out var h))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            height = h;
            return DtStatus.DT_SUCCSESS;
        }

        /// Finds the polygon nearest to the specified center point.
        /// [opt] means the specified parameter can be a null pointer, in that case the output parameter will not be set.
        /// 
        ///  @param[in]		center		The center of the search box. [(x, y, z)]
        ///  @param[in]		halfExtents	The search distance along each axis. [(x, y, z)]
        ///  @param[in]		filter		The polygon filter to apply to the query.
        ///  @param[out]	nearestRef	The reference id of the nearest polygon. Will be set to 0 if no polygon is found.
        ///  @param[out]	nearestPt	The nearest point on the polygon. Unchanged if no polygon is found. [opt] [(x, y, z)]
        ///  @param[out]	isOverPoly 	Set to true if the point's X/Z coordinate lies inside the polygon, false otherwise. Unchanged if no polygon is found. [opt]
        /// @returns The status flags for the query.
        public DtStatus FindNearestPoly(RcVec3f center, RcVec3f halfExtents, IDtQueryFilter filter,
            out long nearestRef, out RcVec3f nearestPt, out bool isOverPoly)
        {
            nearestRef = 0;
            nearestPt = center;
            isOverPoly = false;

            // Get nearby polygons from proximity grid.
            DtFindNearestPolyQuery query = new DtFindNearestPolyQuery(this, center);
            DtStatus status = QueryPolygons(center, halfExtents, filter, query);
            if (status.Failed())
            {
                return status;
            }

            nearestRef = query.NearestRef();
            nearestPt = query.NearestPt();
            isOverPoly = query.OverPoly();

            return DtStatus.DT_SUCCSESS;
        }

        // FIXME: (PP) duplicate?
        protected void QueryPolygonsInTile(DtMeshTile tile, RcVec3f qmin, RcVec3f qmax, IDtQueryFilter filter, IDtPolyQuery query)
        {
            if (tile.data.bvTree != null)
            {
                int nodeIndex = 0;
                var tbmin = tile.data.header.bmin;
                var tbmax = tile.data.header.bmax;
                float qfac = tile.data.header.bvQuantFactor;
                // Calculate quantized box
                int[] bmin = new int[3];
                int[] bmax = new int[3];
                // dtClamp query box to world box.
                float minx = Clamp(qmin.x, tbmin.x, tbmax.x) - tbmin.x;
                float miny = Clamp(qmin.y, tbmin.y, tbmax.y) - tbmin.y;
                float minz = Clamp(qmin.z, tbmin.z, tbmax.z) - tbmin.z;
                float maxx = Clamp(qmax.x, tbmin.x, tbmax.x) - tbmin.x;
                float maxy = Clamp(qmax.y, tbmin.y, tbmax.y) - tbmin.y;
                float maxz = Clamp(qmax.z, tbmin.z, tbmax.z) - tbmin.z;
                // Quantize
                bmin[0] = (int)(qfac * minx) & 0x7ffffffe;
                bmin[1] = (int)(qfac * miny) & 0x7ffffffe;
                bmin[2] = (int)(qfac * minz) & 0x7ffffffe;
                bmax[0] = (int)(qfac * maxx + 1) | 1;
                bmax[1] = (int)(qfac * maxy + 1) | 1;
                bmax[2] = (int)(qfac * maxz + 1) | 1;

                // Traverse tree
                long @base = m_nav.GetPolyRefBase(tile);
                int end = tile.data.header.bvNodeCount;
                while (nodeIndex < end)
                {
                    DtBVNode node = tile.data.bvTree[nodeIndex];
                    bool overlap = DtUtils.OverlapQuantBounds(bmin, bmax, node.bmin, node.bmax);
                    bool isLeafNode = node.i >= 0;

                    if (isLeafNode && overlap)
                    {
                        long refs = @base | (long)node.i;
                        if (filter.PassFilter(refs, tile, tile.data.polys[node.i]))
                        {
                            query.Process(tile, tile.data.polys[node.i], refs);
                        }
                    }

                    if (overlap || isLeafNode)
                    {
                        nodeIndex++;
                    }
                    else
                    {
                        int escapeIndex = -node.i;
                        nodeIndex += escapeIndex;
                    }
                }
            }
            else
            {
                RcVec3f bmin = new RcVec3f();
                RcVec3f bmax = new RcVec3f();
                long @base = m_nav.GetPolyRefBase(tile);
                for (int i = 0; i < tile.data.header.polyCount; ++i)
                {
                    DtPoly p = tile.data.polys[i];
                    // Do not return off-mesh connection polygons.
                    if (p.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    long refs = @base | (long)i;
                    if (!filter.PassFilter(refs, tile, p))
                    {
                        continue;
                    }

                    // Calc polygon bounds.
                    int v = p.verts[0] * 3;
                    bmin.Set(tile.data.verts, v);
                    bmax.Set(tile.data.verts, v);
                    for (int j = 1; j < p.vertCount; ++j)
                    {
                        v = p.verts[j] * 3;
                        bmin.Min(tile.data.verts, v);
                        bmax.Max(tile.data.verts, v);
                    }

                    if (DtUtils.OverlapBounds(qmin, qmax, bmin, bmax))
                    {
                        query.Process(tile, p, refs);
                    }
                }
            }
        }

        /**
     * Finds polygons that overlap the search box.
     *
     * If no polygons are found, the function will return with a polyCount of zero.
     *
     * @param center
     *            The center of the search box. [(x, y, z)]
     * @param halfExtents
     *            The search distance along each axis. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @return The reference ids of the polygons that overlap the query box.
     */
        public DtStatus QueryPolygons(RcVec3f center, RcVec3f halfExtents, IDtQueryFilter filter, IDtPolyQuery query)
        {
            if (!RcVec3f.IsFinite(center) || !RcVec3f.IsFinite(halfExtents) || null == filter)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            // Find tiles the query touches.
            RcVec3f bmin = center.Subtract(halfExtents);
            RcVec3f bmax = center.Add(halfExtents);
            foreach (var t in QueryTiles(center, halfExtents))
            {
                QueryPolygonsInTile(t, bmin, bmax, filter, query);
            }

            return DtStatus.DT_SUCCSESS;
        }

        /**
     * Finds tiles that overlap the search box.
     */
        public IList<DtMeshTile> QueryTiles(RcVec3f center, RcVec3f halfExtents)
        {
            if (!RcVec3f.IsFinite(center) || !RcVec3f.IsFinite(halfExtents))
            {
                return RcImmutableArray<DtMeshTile>.Empty;
            }

            RcVec3f bmin = center.Subtract(halfExtents);
            RcVec3f bmax = center.Add(halfExtents);
            m_nav.CalcTileLoc(bmin, out var minx, out var miny);
            m_nav.CalcTileLoc(bmax, out var maxx, out var maxy);

            List<DtMeshTile> tiles = new List<DtMeshTile>();
            for (int y = miny; y <= maxy; ++y)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    tiles.AddRange(m_nav.GetTilesAt(x, y));
                }
            }

            return tiles;
        }

        /**
     * Finds a path from the start polygon to the end polygon.
     *
     * If the end polygon cannot be reached through the navigation graph, the last polygon in the path will be the
     * nearest the end polygon.
     *
     * The start and end positions are used to calculate traversal costs. (The y-values impact the result.)
     *
     * @param startRef
     *            The reference id of the start polygon.
     * @param endRef
     *            The reference id of the end polygon.
     * @param startPos
     *            A position within the start polygon. [(x, y, z)]
     * @param endPos
     *            A position within the end polygon. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @return Found path
     */
        public DtStatus FindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, ref List<long> path, DtFindPathOption fpo)
        {
            if (null == path)
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;

            path.Clear();

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !m_nav.IsValidPolyRef(endRef) || !RcVec3f.IsFinite(startPos) || !RcVec3f.IsFinite(endPos) || null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            var heuristic = fpo.heuristic;
            var raycastLimit = fpo.raycastLimit;
            var options = fpo.options;

            float raycastLimitSqr = Sqr(raycastLimit);

            // trade quality with performance?
            if ((options & DT_FINDPATH_ANY_ANGLE) != 0 && raycastLimit < 0f)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive
                // so it is enough to compute it from the first tile.
                DtMeshTile tile = m_nav.GetTileByRef(startRef);
                float agentRadius = tile.data.header.walkableRadius;
                raycastLimitSqr = Sqr(agentRadius * DtNavMesh.DT_RAY_CAST_LIMIT_PROPORTIONS);
            }

            if (startRef == endRef)
            {
                path.Add(startRef);
                return DtStatus.DT_SUCCSESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = startPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = heuristic.GetCost(startPos, endPos);
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_OPEN;
            m_openList.Push(startNode);

            DtNode lastBestNode = startNode;
            float lastBestNodeCost = startNode.total;


            while (!m_openList.IsEmpty())
            {
                // Remove node from open list and put it in closed list.
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DtNode.DT_NODE_OPEN;
                bestNode.flags |= DtNode.DT_NODE_CLOSED;

                // Reached the goal, stop searching.
                if (bestNode.id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                // Get current poly and tile.
                // The API input has been checked already, skip checking internal data.
                long bestRef = bestNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out var bestTile, out var bestPoly);

                // Get parent poly and tile.
                long parentRef = 0, grandpaRef = 0;
                DtMeshTile parentTile = null;
                DtPoly parentPoly = null;
                DtNode parentNode = null;
                if (bestNode.pidx != 0)
                {
                    parentNode = m_nodePool.GetNodeAtIdx(bestNode.pidx);
                    parentRef = parentNode.id;
                    if (parentNode.pidx != 0)
                    {
                        grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.pidx).id;
                    }
                }

                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((options & DT_FINDPATH_ANY_ANGLE) != 0)
                {
                    if ((parentRef != 0) && (raycastLimitSqr >= float.MaxValue
                                             || RcVec3f.DistSqr(parentNode.pos, bestNode.pos) < raycastLimitSqr))
                    {
                        tryLOS = true;
                    }
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    long neighbourRef = bestTile.links[i].refs;

                    // Skip invalid ids and do not expand back to where we came from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been checked already, skip checking internal data.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // get the node
                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef, 0);

                    // do not expand to nodes that were already visited from the
                    // same parent
                    if (neighbourNode.pidx != 0 && neighbourNode.pidx == bestNode.pidx)
                    {
                        continue;
                    }

                    // If the node is visited the first time, calculate node position.
                    var neighbourPos = neighbourNode.pos;
                    var empStatus = neighbourRef == endRef
                        ? GetEdgeIntersectionPoint(bestNode.pos, bestRef, bestPoly, bestTile,
                            endPos, neighbourRef, neighbourPoly, neighbourTile,
                            ref neighbourPos)
                        : GetEdgeMidPoint(bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            ref neighbourPos);

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristicCost = 0;

                    // raycast parent
                    bool foundShortCut = false;
                    List<long> shortcut = null;
                    if (tryLOS)
                    {
                        var rayStatus = Raycast(parentRef, parentNode.pos, neighbourPos, filter,
                            DT_RAYCAST_USE_COSTS, grandpaRef, out var rayHit);
                        if (rayStatus.Succeeded())
                        {
                            foundShortCut = rayHit.t >= 1.0f;
                            if (foundShortCut)
                            {
                                shortcut = rayHit.path;
                                // shortcut found using raycast. Using shorter cost
                                // instead
                                cost = parentNode.cost + rayHit.pathCost;
                            }
                        }
                    }

                    // update move cost
                    if (!foundShortCut)
                    {
                        float curCost = filter.GetCost(bestNode.pos, neighbourPos, parentRef, parentTile,
                            parentPoly, bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == endRef)
                    {
                        // Cost
                        float endCost = filter.GetCost(neighbourPos, endPos, bestRef, bestTile, bestPoly, neighbourRef,
                            neighbourTile, neighbourPoly, 0L, null, null);
                        cost = cost + endCost;
                    }
                    else
                    {
                        // Cost
                        heuristicCost = heuristic.GetCost(neighbourPos, endPos);
                    }

                    float total = cost + heuristicCost;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // The node is already visited and process, and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.pidx = foundShortCut ? bestNode.pidx : m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~DtNode.DT_NODE_CLOSED);
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;
                    neighbourNode.pos = neighbourPos;
                    neighbourNode.shortcut = shortcut;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristicCost < lastBestNodeCost)
                    {
                        lastBestNodeCost = heuristicCost;
                        lastBestNode = neighbourNode;
                    }
                }
            }

            var status = GetPathToNode(lastBestNode, ref path);
            if (lastBestNode.id != endRef)
            {
                status |= DtStatus.DT_PARTIAL_RESULT;
            }

            return status;
        }

        /**
     * Intializes a sliced path query.
     *
     * Common use case: -# Call InitSlicedFindPath() to initialize the sliced path query. -# Call UpdateSlicedFindPath()
     * until it returns complete. -# Call FinalizeSlicedFindPath() to get the path.
     *
     * @param startRef
     *            The reference id of the start polygon.
     * @param endRef
     *            The reference id of the end polygon.
     * @param startPos
     *            A position within the start polygon. [(x, y, z)]
     * @param endPos
     *            A position within the end polygon. [(x, y, z)]
     * @param filter
     *            The polygon filter to apply to the query.
     * @param options
     *            query options (see: #FindPathOptions)
     * @return
     */
        public DtStatus InitSlicedFindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, int options)
        {
            return InitSlicedFindPath(startRef, endRef, startPos, endPos, filter, options, DefaultQueryHeuristic.Default, -1.0f);
        }

        public DtStatus InitSlicedFindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, int options, float raycastLimit)
        {
            return InitSlicedFindPath(startRef, endRef, startPos, endPos, filter, options, DefaultQueryHeuristic.Default, raycastLimit);
        }

        public DtStatus InitSlicedFindPath(long startRef, long endRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, int options, IQueryHeuristic heuristic, float raycastLimit)
        {
            // Init path state.
            m_query = new DtQueryData();
            m_query.status = DtStatus.DT_FAILURE;
            m_query.startRef = startRef;
            m_query.endRef = endRef;
            m_query.startPos = startPos;
            m_query.endPos = endPos;
            m_query.filter = filter;
            m_query.options = options;
            m_query.heuristic = heuristic;
            m_query.raycastLimitSqr = Sqr(raycastLimit);

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !m_nav.IsValidPolyRef(endRef) || !RcVec3f.IsFinite(startPos) || !RcVec3f.IsFinite(endPos) || null == filter)
            {
                return DtStatus.DT_INVALID_PARAM;
            }

            // trade quality with performance?
            if ((options & DT_FINDPATH_ANY_ANGLE) != 0 && raycastLimit < 0f)
            {
                // limiting to several times the character radius yields nice results. It is not sensitive
                // so it is enough to compute it from the first tile.
                DtMeshTile tile = m_nav.GetTileByRef(startRef);
                float agentRadius = tile.data.header.walkableRadius;
                m_query.raycastLimitSqr = Sqr(agentRadius * DtNavMesh.DT_RAY_CAST_LIMIT_PROPORTIONS);
            }

            if (startRef == endRef)
            {
                m_query.status = DtStatus.DT_SUCCSESS;
                return DtStatus.DT_SUCCSESS;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = startPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = heuristic.GetCost(startPos, endPos);
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_OPEN;
            m_openList.Push(startNode);

            m_query.status = DtStatus.DT_IN_PROGRESS;
            m_query.lastBestNode = startNode;
            m_query.lastBestNodeCost = startNode.total;

            return m_query.status;
        }

        /**
     * Updates an in-progress sliced path query.
     *
     * @param maxIter
     *            The maximum number of iterations to perform.
     * @return The status flags for the query.
     */
        public virtual DtStatus UpdateSlicedFindPath(int maxIter, out int doneIters)
        {
            doneIters = 0;
            if (!m_query.status.InProgress())
            {
                return m_query.status;
            }

            // Make sure the request is still valid.
            if (!m_nav.IsValidPolyRef(m_query.startRef) || !m_nav.IsValidPolyRef(m_query.endRef))
            {
                m_query.status = DtStatus.DT_FAILURE;
                return DtStatus.DT_FAILURE;
            }

            int iter = 0;
            while (iter < maxIter && !m_openList.IsEmpty())
            {
                iter++;

                // Remove node from open list and put it in closed list.
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DtNode.DT_NODE_OPEN;
                bestNode.flags |= DtNode.DT_NODE_CLOSED;

                // Reached the goal, stop searching.
                if (bestNode.id == m_query.endRef)
                {
                    m_query.lastBestNode = bestNode;
                    var details = m_query.status & DtStatus.DT_STATUS_DETAIL_MASK;
                    m_query.status = DtStatus.DT_SUCCSESS | details;
                    doneIters = iter;
                    return m_query.status;
                }

                // Get current poly and tile.
                // The API input has been checked already, skip checking internal
                // data.
                long bestRef = bestNode.id;
                var status = m_nav.GetTileAndPolyByRef(bestRef, out var bestTile, out var bestPoly);
                if (status.Failed())
                {
                    // The polygon has disappeared during the sliced query, fail.
                    m_query.status = DtStatus.DT_FAILURE;
                    doneIters = iter;
                    return m_query.status;
                }

                // Get parent and grand parent poly and tile.
                long parentRef = 0, grandpaRef = 0;
                DtMeshTile parentTile = null;
                DtPoly parentPoly = null;
                DtNode parentNode = null;
                if (bestNode.pidx != 0)
                {
                    parentNode = m_nodePool.GetNodeAtIdx(bestNode.pidx);
                    parentRef = parentNode.id;
                    if (parentNode.pidx != 0)
                    {
                        grandpaRef = m_nodePool.GetNodeAtIdx(parentNode.pidx).id;
                    }
                }

                if (parentRef != 0)
                {
                    bool invalidParent = false;
                    status = m_nav.GetTileAndPolyByRef(parentRef, out parentTile, out parentPoly);
                    invalidParent = status.Failed();
                    if (invalidParent || (grandpaRef != 0 && !m_nav.IsValidPolyRef(grandpaRef)))
                    {
                        // The polygon has disappeared during the sliced query fail.
                        m_query.status = DtStatus.DT_FAILURE;
                        doneIters = iter;
                        return m_query.status;
                    }
                }

                // decide whether to test raycast to previous nodes
                bool tryLOS = false;
                if ((m_query.options & DT_FINDPATH_ANY_ANGLE) != 0)
                {
                    if ((parentRef != 0) && (m_query.raycastLimitSqr >= float.MaxValue
                                             || RcVec3f.DistSqr(parentNode.pos, bestNode.pos) < m_query.raycastLimitSqr))
                    {
                        tryLOS = true;
                    }
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    long neighbourRef = bestTile.links[i].refs;

                    // Skip invalid ids and do not expand back to where we came
                    // from.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Get neighbour poly and tile.
                    // The API input has been checked already, skip checking internal
                    // data.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    if (!m_query.filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // get the neighbor node
                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef, 0);

                    // do not expand to nodes that were already visited from the
                    // same parent
                    if (neighbourNode.pidx != 0 && neighbourNode.pidx == bestNode.pidx)
                    {
                        continue;
                    }

                    // If the node is visited the first time, calculate node
                    // position.
                    var neighbourPos = neighbourNode.pos;
                    var empStatus = neighbourRef == m_query.endRef
                        ? GetEdgeIntersectionPoint(bestNode.pos, bestRef, bestPoly, bestTile,
                            m_query.endPos, neighbourRef, neighbourPoly, neighbourTile,
                            ref neighbourPos)
                        : GetEdgeMidPoint(bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            ref neighbourPos);

                    // Calculate cost and heuristic.
                    float cost = 0;
                    float heuristic = 0;

                    // raycast parent
                    bool foundShortCut = false;
                    List<long> shortcut = null;
                    if (tryLOS)
                    {
                        status = Raycast(parentRef, parentNode.pos, neighbourPos, m_query.filter, DT_RAYCAST_USE_COSTS, grandpaRef, out var rayHit);
                        if (status.Succeeded())
                        {
                            foundShortCut = rayHit.t >= 1.0f;
                            if (foundShortCut)
                            {
                                shortcut = rayHit.path;
                                // shortcut found using raycast. Using shorter cost
                                // instead
                                cost = parentNode.cost + rayHit.pathCost;
                            }
                        }
                    }


                    // update move cost
                    if (!foundShortCut)
                    {
                        // No shortcut found.
                        float curCost = m_query.filter.GetCost(bestNode.pos, neighbourPos, parentRef, parentTile,
                            parentPoly, bestRef, bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);
                        cost = bestNode.cost + curCost;
                    }

                    // Special case for last node.
                    if (neighbourRef == m_query.endRef)
                    {
                        float endCost = m_query.filter.GetCost(neighbourPos, m_query.endPos, bestRef, bestTile,
                            bestPoly, neighbourRef, neighbourTile, neighbourPoly, 0, null, null);

                        cost = cost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        heuristic = m_query.heuristic.GetCost(neighbourPos, m_query.endPos);
                    }

                    float total = cost + heuristic;

                    // The node is already in open list and the new result is worse,
                    // skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // The node is already visited and process, and the new result
                    // is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    // Add or update the node.
                    neighbourNode.pidx = foundShortCut ? bestNode.pidx : m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~DT_NODE_CLOSED);
                    neighbourNode.cost = cost;
                    neighbourNode.total = total;
                    neighbourNode.pos = neighbourPos;
                    neighbourNode.shortcut = shortcut;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        // Already in open, update node location.
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        // Put the node in open list.
                        neighbourNode.flags |= DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }

                    // Update nearest node to target so far.
                    if (heuristic < m_query.lastBestNodeCost)
                    {
                        m_query.lastBestNodeCost = heuristic;
                        m_query.lastBestNode = neighbourNode;
                    }
                }
            }

            // Exhausted all nodes, but could not find path.
            if (m_openList.IsEmpty())
            {
                var details = m_query.status & DtStatus.DT_STATUS_DETAIL_MASK;
                m_query.status = DtStatus.DT_SUCCSESS | details;
            }

            doneIters = iter;
            return m_query.status;
        }

        /// Finalizes and returns the results of a sliced path query.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public virtual DtStatus FinalizeSlicedFindPath(ref List<long> path)
        {
            if (null == path)
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;

            path.Clear();

            if (m_query.status.Failed())
            {
                // Reset query.
                m_query = new DtQueryData();
                return DtStatus.DT_FAILURE;
            }

            if (m_query.startRef == m_query.endRef)
            {
                // Special case: the search starts and ends at same poly.
                path.Add(m_query.startRef);
            }
            else
            {
                // Reverse the path.
                if (m_query.lastBestNode.id != m_query.endRef)
                {
                    m_query.status |= DtStatus.DT_PARTIAL_RESULT;
                }

                GetPathToNode(m_query.lastBestNode, ref path);
            }

            var details = m_query.status & DtStatus.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new DtQueryData();

            return DtStatus.DT_SUCCSESS | details;
        }

        /// Finalizes and returns the results of an incomplete sliced path query, returning the path to the furthest
        /// polygon on the existing path that was visited during the search.
        /// @param[in] existing An array of polygon references for the existing path.
        /// @param[in] existingSize The number of polygon in the @p existing array.
        /// @param[out] path An ordered list of polygon references representing the path. (Start to end.)
        /// [(polyRef) * @p pathCount]
        /// @returns The status flags for the query.
        public virtual DtStatus FinalizeSlicedFindPathPartial(List<long> existing, ref List<long> path)
        {
            if (null == path)
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;

            path.Clear();

            if (null == existing || existing.Count <= 0)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (m_query.status.Failed())
            {
                // Reset query.
                m_query = new DtQueryData();
                return DtStatus.DT_FAILURE;
            }

            if (m_query.startRef == m_query.endRef)
            {
                // Special case: the search starts and ends at same poly.
                path.Add(m_query.startRef);
            }
            else
            {
                // Find furthest existing node that was visited.
                DtNode node = null;
                for (int i = existing.Count - 1; i >= 0; --i)
                {
                    node = m_nodePool.FindNode(existing[i]);
                    if (node != null)
                    {
                        break;
                    }
                }

                if (node == null)
                {
                    m_query.status |= DtStatus.DT_PARTIAL_RESULT;
                    node = m_query.lastBestNode;
                }

                GetPathToNode(node, ref path);
            }

            var details = m_query.status & DtStatus.DT_STATUS_DETAIL_MASK;

            // Reset query.
            m_query = new DtQueryData();

            return DtStatus.DT_SUCCSESS | details;
        }

        protected DtStatus AppendVertex(RcVec3f pos, int flags, long refs, ref List<StraightPathItem> straightPath,
            int maxStraightPath)
        {
            if (straightPath.Count > 0 && DtUtils.VEqual(straightPath[straightPath.Count - 1].pos, pos))
            {
                // The vertices are equal, update flags and poly.
                straightPath[straightPath.Count - 1] = new StraightPathItem(straightPath[straightPath.Count - 1].pos, flags, refs);
            }
            else
            {
                if (straightPath.Count < maxStraightPath)
                {
                    // Append new vertex.
                    straightPath.Add(new StraightPathItem(pos, flags, refs));
                }

                // If reached end of path or there is no space to append more vertices, return.
                if (flags == DT_STRAIGHTPATH_END || straightPath.Count >= maxStraightPath)
                {
                    return DtStatus.DT_SUCCSESS;
                }
            }

            return DtStatus.DT_IN_PROGRESS;
        }

        protected DtStatus AppendPortals(int startIdx, int endIdx, RcVec3f endPos, List<long> path,
            ref List<StraightPathItem> straightPath, int maxStraightPath, int options)
        {
            var startPos = straightPath[straightPath.Count - 1].pos;
            // Append or update last vertex
            DtStatus stat;
            for (int i = startIdx; i < endIdx; i++)
            {
                // Calculate portal
                long from = path[i];
                var status = m_nav.GetTileAndPolyByRef(from, out var fromTile, out var fromPoly);
                if (status.Failed())
                {
                    return DtStatus.DT_FAILURE;
                }

                long to = path[i + 1];
                status = m_nav.GetTileAndPolyByRef(to, out var toTile, out var toPoly);
                if (status.Failed())
                {
                    return DtStatus.DT_FAILURE;
                }

                var ppStatus = GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out var left, out var right);
                if (ppStatus.Failed())
                {
                    break;
                }

                if ((options & DT_STRAIGHTPATH_AREA_CROSSINGS) != 0)
                {
                    // Skip intersection if only area crossings are requested.
                    if (fromPoly.GetArea() == toPoly.GetArea())
                    {
                        continue;
                    }
                }

                // Append intersection
                if (DtUtils.IntersectSegSeg2D(startPos, endPos, left, right, out var _, out var t))
                {
                    var pt = RcVec3f.Lerp(left, right, t);
                    stat = AppendVertex(pt, 0, path[i + 1], ref straightPath, maxStraightPath);
                    if (!stat.InProgress())
                    {
                        return stat;
                    }
                }
            }

            return DtStatus.DT_IN_PROGRESS;
        }

        /// @par
        /// 
        /// This method peforms what is often called 'string pulling'.
        ///
        /// The start position is clamped to the first polygon in the path, and the 
        /// end position is clamped to the last. So the start and end positions should 
        /// normally be within or very near the first and last polygons respectively.
        ///
        /// The returned polygon references represent the reference id of the polygon 
        /// that is entered at the associated path position. The reference id associated 
        /// with the end point will always be zero.  This allows, for example, matching 
        /// off-mesh link points to their representative polygons.
        ///
        /// If the provided result buffers are too small for the entire result set, 
        /// they will be filled as far as possible from the start toward the end 
        /// position.
        ///
        /// Finds the straight path from the start to the end position within the polygon corridor.
        ///  @param[in]		startPos			Path start position. [(x, y, z)]
        ///  @param[in]		endPos				Path end position. [(x, y, z)]
        ///  @param[in]		path				An array of polygon references that represent the path corridor.
        ///  @param[in]		pathSize			The number of polygons in the @p path array.
        ///  @param[out]	straightPath		Points describing the straight path. [(x, y, z) * @p straightPathCount].
        ///  @param[in]		maxStraightPath		The maximum number of points the straight path arrays can hold.  [Limit: > 0]
        ///  @param[in]		options				Query options. (see: #dtStraightPathOptions)
        /// @returns The status flags for the query.
        public virtual DtStatus FindStraightPath(RcVec3f startPos, RcVec3f endPos, List<long> path,
            ref List<StraightPathItem> straightPath,
            int maxStraightPath, int options)
        {
            if (!RcVec3f.IsFinite(startPos) || !RcVec3f.IsFinite(endPos) || null == straightPath
                || null == path || 0 == path.Count || path[0] == 0 || maxStraightPath <= 0)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            straightPath.Clear();

            // TODO: Should this be callers responsibility?
            var closestStartPosRes = ClosestPointOnPolyBoundary(path[0], startPos, out var closestStartPos);
            if (closestStartPosRes.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            var closestEndPosRes = ClosestPointOnPolyBoundary(path[path.Count - 1], endPos, out var closestEndPos);
            if (closestEndPosRes.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // Add start point.
            DtStatus stat = AppendVertex(closestStartPos, DT_STRAIGHTPATH_START, path[0], ref straightPath, maxStraightPath);
            if (!stat.InProgress())
            {
                return stat;
            }

            if (path.Count > 1)
            {
                RcVec3f portalApex = closestStartPos;
                RcVec3f portalLeft = portalApex;
                RcVec3f portalRight = portalApex;
                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                int leftPolyType = 0;
                int rightPolyType = 0;

                long leftPolyRef = path[0];
                long rightPolyRef = path[0];

                for (int i = 0; i < path.Count; ++i)
                {
                    RcVec3f left;
                    RcVec3f right;
                    int toType;

                    if (i + 1 < path.Count)
                    {
                        int fromType; // // fromType is ignored.

                        // Next portal.
                        var ppStatus = GetPortalPoints(path[i], path[i + 1], out left, out right, out fromType, out toType);
                        if (ppStatus.Failed())
                        {
                            // Failed to get portal points, in practice this means that path[i+1] is invalid polygon.
                            // Clamp the end point to path[i], and return the path so far.
                            var cpStatus = ClosestPointOnPolyBoundary(path[i], endPos, out closestEndPos);
                            if (cpStatus.Failed())
                            {
                                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
                            }

                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                // Ignore status return value as we're just about to return anyway.
                                AppendPortals(apexIndex, i, closestEndPos, path, ref straightPath, maxStraightPath, options);
                            }

                            // Ignore status return value as we're just about to return anyway.
                            AppendVertex(closestEndPos, 0, path[i], ref straightPath, maxStraightPath);

                            return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM | (straightPath.Count >= maxStraightPath ? DtStatus.DT_BUFFER_TOO_SMALL : DtStatus.DT_STATUS_NOTHING);
                        }

                        // If starting really close the portal, advance.
                        if (i == 0)
                        {
                            var distSqr = DtUtils.DistancePtSegSqr2D(portalApex, left, right, out var t);
                            if (distSqr < Sqr(0.001f))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        // End of the path.
                        left = closestEndPos;
                        right = closestEndPos;
                        toType = DtPoly.DT_POLYTYPE_GROUND;
                    }

                    // Right vertex.
                    if (DtUtils.TriArea2D(portalApex, portalRight, right) <= 0.0f)
                    {
                        if (DtUtils.VEqual(portalApex, portalRight) || DtUtils.TriArea2D(portalApex, portalLeft, right) > 0.0f)
                        {
                            portalRight = right;
                            rightPolyRef = (i + 1 < path.Count) ? path[i + 1] : 0;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                stat = AppendPortals(apexIndex, leftIndex, portalLeft, path, ref straightPath, maxStraightPath, options);
                                if (!stat.InProgress())
                                {
                                    return stat;
                                }
                            }

                            portalApex = portalLeft;
                            apexIndex = leftIndex;

                            int flags = 0;
                            if (leftPolyRef == 0)
                            {
                                flags = DT_STRAIGHTPATH_END;
                            }
                            else if (leftPolyType == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }

                            long refs = leftPolyRef;

                            // Append or update vertex
                            stat = AppendVertex(portalApex, flags, refs, ref straightPath, maxStraightPath);
                            if (!stat.InProgress())
                            {
                                return stat;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;

                            continue;
                        }
                    }

                    // Left vertex.
                    if (DtUtils.TriArea2D(portalApex, portalLeft, left) >= 0.0f)
                    {
                        if (DtUtils.VEqual(portalApex, portalLeft) || DtUtils.TriArea2D(portalApex, portalRight, left) < 0.0f)
                        {
                            portalLeft = left;
                            leftPolyRef = (i + 1 < path.Count) ? path[i + 1] : 0;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            // Append portals along the current straight path segment.
                            if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                            {
                                stat = AppendPortals(apexIndex, rightIndex, portalRight, path, ref straightPath, maxStraightPath, options);
                                if (!stat.InProgress())
                                {
                                    return stat;
                                }
                            }

                            portalApex = portalRight;
                            apexIndex = rightIndex;

                            int flags = 0;
                            if (rightPolyRef == 0)
                            {
                                flags = DT_STRAIGHTPATH_END;
                            }
                            else if (rightPolyType == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                            {
                                flags = DT_STRAIGHTPATH_OFFMESH_CONNECTION;
                            }

                            long refs = rightPolyRef;

                            // Append or update vertex
                            stat = AppendVertex(portalApex, flags, refs, ref straightPath, maxStraightPath);
                            if (!stat.InProgress())
                            {
                                return stat;
                            }

                            portalLeft = portalApex;
                            portalRight = portalApex;
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            // Restart
                            i = apexIndex;

                            continue;
                        }
                    }
                }

                // Append portals along the current straight path segment.
                if ((options & (DT_STRAIGHTPATH_AREA_CROSSINGS | DT_STRAIGHTPATH_ALL_CROSSINGS)) != 0)
                {
                    stat = AppendPortals(apexIndex, path.Count - 1, closestEndPos, path, ref straightPath, maxStraightPath, options);
                    if (!stat.InProgress())
                    {
                        return stat;
                    }
                }
            }

            // Ignore status return value as we're just about to return anyway.
            AppendVertex(closestEndPos, DT_STRAIGHTPATH_END, 0, ref straightPath, maxStraightPath);
            return DtStatus.DT_SUCCSESS | (straightPath.Count >= maxStraightPath ? DtStatus.DT_BUFFER_TOO_SMALL : DtStatus.DT_STATUS_NOTHING);
        }

        /// @par
        ///
        /// This method is optimized for small delta movement and a small number of 
        /// polygons. If used for too great a distance, the result set will form an 
        /// incomplete path.
        ///
        /// @p resultPos will equal the @p endPos if the end is reached. 
        /// Otherwise the closest reachable position will be returned.
        /// 
        /// @p resultPos is not projected onto the surface of the navigation 
        /// mesh. Use #getPolyHeight if this is needed.
        ///
        /// This method treats the end position in the same manner as 
        /// the #raycast method. (As a 2D point.) See that method's documentation 
        /// for details.
        /// 
        /// If the @p visited array is too small to hold the entire result set, it will 
        /// be filled as far as possible from the start position toward the end 
        /// position.
        ///
        /// Moves from the start to the end position constrained to the navigation mesh.
        ///  @param[in]		startRef		The reference id of the start polygon.
        ///  @param[in]		startPos		A position of the mover within the start polygon. [(x, y, x)]
        ///  @param[in]		endPos			The desired end position of the mover. [(x, y, z)]
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[out]	resultPos		The result position of the mover. [(x, y, z)]
        ///  @param[out]	visited			The reference ids of the polygons visited during the move.
        ///  @param[out]	visitedCount	The number of polygons visited during the move.
        ///  @param[in]		maxVisitedSize	The maximum number of polygons the @p visited array can hold.
        /// @returns The status flags for the query.
        public DtStatus MoveAlongSurface(long startRef, RcVec3f startPos, RcVec3f endPos,
            IDtQueryFilter filter,
            out RcVec3f resultPos, ref List<long> visited)
        {
            resultPos = RcVec3f.Zero;

            if (null != visited)
                visited.Clear();

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(startPos)
                                                || !RcVec3f.IsFinite(endPos) || null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtNodePool tinyNodePool = new DtNodePool();

            DtNode startNode = tinyNodePool.GetNode(startRef);
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_CLOSED;
            LinkedList<DtNode> stack = new LinkedList<DtNode>();
            stack.AddLast(startNode);

            RcVec3f bestPos = new RcVec3f();
            float bestDist = float.MaxValue;
            DtNode bestNode = null;
            bestPos = startPos;

            // Search constraints
            var searchPos = RcVec3f.Lerp(startPos, endPos, 0.5f);
            float searchRadSqr = Sqr(RcVec3f.Distance(startPos, endPos) / 2.0f + 0.001f);

            float[] verts = new float[m_nav.GetMaxVertsPerPoly() * 3];

            while (0 < stack.Count)
            {
                // Pop front.
                DtNode curNode = stack.First?.Value;
                stack.RemoveFirst();

                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long curRef = curNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(curRef, out var curTile, out var curPoly);

                // Collect vertices.
                int nverts = curPoly.vertCount;
                for (int i = 0; i < nverts; ++i)
                {
                    Array.Copy(curTile.data.verts, curPoly.verts[i] * 3, verts, i * 3, 3);
                }

                // If target is inside the poly, stop search.
                if (DtUtils.PointInPolygon(endPos, verts, nverts))
                {
                    bestNode = curNode;
                    bestPos = endPos;
                    break;
                }

                // Find wall edges and find nearest point inside the walls.
                for (int i = 0, j = curPoly.vertCount - 1; i < curPoly.vertCount; j = i++)
                {
                    // Find links to neighbours.
                    int MAX_NEIS = 8;
                    int nneis = 0;
                    long[] neis = new long[MAX_NEIS];

                    if ((curPoly.neis[j] & DtNavMesh.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        for (int k = curTile.polyLinks[curPoly.index]; k != DtNavMesh.DT_NULL_LINK; k = curTile.links[k].next)
                        {
                            DtLink link = curTile.links[k];
                            if (link.edge == j)
                            {
                                if (link.refs != 0)
                                {
                                    m_nav.GetTileAndPolyByRefUnsafe(link.refs, out var neiTile, out var neiPoly);
                                    if (filter.PassFilter(link.refs, neiTile, neiPoly))
                                    {
                                        if (nneis < MAX_NEIS)
                                        {
                                            neis[nneis++] = link.refs;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (curPoly.neis[j] != 0)
                    {
                        int idx = curPoly.neis[j] - 1;
                        long refs = m_nav.GetPolyRefBase(curTile) | (long)idx;
                        if (filter.PassFilter(refs, curTile, curTile.data.polys[idx]))
                        {
                            // Internal edge, encode id.
                            neis[nneis++] = refs;
                        }
                    }

                    if (nneis == 0)
                    {
                        // Wall edge, calc distance.
                        int vj = j * 3;
                        int vi = i * 3;
                        var distSqr = DtUtils.DistancePtSegSqr2D(endPos, verts, vj, vi, out var tseg);
                        if (distSqr < bestDist)
                        {
                            // Update nearest distance.
                            bestPos = RcVec3f.Lerp(verts, vj, vi, tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nneis; ++k)
                        {
                            DtNode neighbourNode = tinyNodePool.GetNode(neis[k]);
                            // Skip if already visited.
                            if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                            {
                                continue;
                            }

                            // Skip the link if it is too far from search constraint.
                            // TODO: Maybe should use GetPortalPoints(), but this one is way faster.
                            int vj = j * 3;
                            int vi = i * 3;
                            var distSqr = DtUtils.DistancePtSegSqr2D(searchPos, verts, vj, vi, out var _);
                            if (distSqr > searchRadSqr)
                            {
                                continue;
                            }

                            // Mark as the node as visited and push to queue.
                            neighbourNode.pidx = tinyNodePool.GetNodeIdx(curNode);
                            neighbourNode.flags |= DtNode.DT_NODE_CLOSED;
                            stack.AddLast(neighbourNode);
                        }
                    }
                }
            }

            if (bestNode != null)
            {
                // Reverse the path.
                DtNode prev = null;
                DtNode node = bestNode;
                do
                {
                    DtNode next = tinyNodePool.GetNodeAtIdx(node.pidx);
                    node.pidx = tinyNodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                } while (node != null);

                // Store result
                node = prev;
                do
                {
                    visited.Add(node.id);
                    node = tinyNodePool.GetNodeAtIdx(node.pidx);
                } while (node != null);
            }

            resultPos = bestPos;

            return DtStatus.DT_SUCCSESS;
        }

        protected DtStatus GetPortalPoints(long from, long to, out RcVec3f left, out RcVec3f right, out int fromType, out int toType)
        {
            left = RcVec3f.Zero;
            right = RcVec3f.Zero;
            fromType = 0;
            toType = 0;

            var status = m_nav.GetTileAndPolyByRef(from, out var fromTile, out var fromPoly);
            if (status.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            fromType = fromPoly.GetPolyType();

            status = m_nav.GetTileAndPolyByRef(to, out var toTile, out var toPoly);
            if (status.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            toType = toPoly.GetPolyType();

            return GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out left, out right);
        }

        // Returns portal points between two polygons.
        protected DtStatus GetPortalPoints(long from, DtPoly fromPoly, DtMeshTile fromTile,
            long to, DtPoly toPoly, DtMeshTile toTile,
            out RcVec3f left, out RcVec3f right)
        {
            left = RcVec3f.Zero;
            right = RcVec3f.Zero;

            // Find the link that points to the 'to' polygon.
            DtLink link = null;
            for (int i = fromTile.polyLinks[fromPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = fromTile.links[i].next)
            {
                if (fromTile.links[i].refs == to)
                {
                    link = fromTile.links[i];
                    break;
                }
            }

            if (link == null)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // Handle off-mesh connections.
            if (fromPoly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                // Find link that points to first vertex.
                for (int i = fromTile.polyLinks[fromPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = fromTile.links[i].next)
                {
                    if (fromTile.links[i].refs == to)
                    {
                        int v = fromTile.links[i].edge;
                        left.x = fromTile.data.verts[fromPoly.verts[v] * 3];
                        left.y = fromTile.data.verts[fromPoly.verts[v] * 3 + 1];
                        left.z = fromTile.data.verts[fromPoly.verts[v] * 3 + 2];

                        right.x = fromTile.data.verts[fromPoly.verts[v] * 3];
                        right.y = fromTile.data.verts[fromPoly.verts[v] * 3 + 1];
                        right.z = fromTile.data.verts[fromPoly.verts[v] * 3 + 2];

                        return DtStatus.DT_SUCCSESS;
                    }
                }

                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (toPoly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
            {
                for (int i = toTile.polyLinks[toPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = toTile.links[i].next)
                {
                    if (toTile.links[i].refs == from)
                    {
                        int v = toTile.links[i].edge;
                        left.x = toTile.data.verts[toPoly.verts[v] * 3];
                        left.y = toTile.data.verts[toPoly.verts[v] * 3 + 1];
                        left.z = toTile.data.verts[toPoly.verts[v] * 3 + 2];

                        right.x = toTile.data.verts[toPoly.verts[v] * 3];
                        right.y = toTile.data.verts[toPoly.verts[v] * 3 + 1];
                        right.z = toTile.data.verts[toPoly.verts[v] * 3 + 2];

                        return DtStatus.DT_SUCCSESS;
                    }
                }

                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            // Find portal vertices.
            int v0 = fromPoly.verts[link.edge];
            int v1 = fromPoly.verts[(link.edge + 1) % fromPoly.vertCount];
            left.x = fromTile.data.verts[v0 * 3];
            left.y = fromTile.data.verts[v0 * 3 + 1];
            left.z = fromTile.data.verts[v0 * 3 + 2];

            right.x = fromTile.data.verts[v1 * 3];
            right.y = fromTile.data.verts[v1 * 3 + 1];
            right.z = fromTile.data.verts[v1 * 3 + 2];

            // If the link is at tile boundary, dtClamp the vertices to
            // the link width.
            if (link.side != 0xff)
            {
                // Unpack portal limits.
                if (link.bmin != 0 || link.bmax != 255)
                {
                    float s = 1.0f / 255.0f;
                    float tmin = link.bmin * s;
                    float tmax = link.bmax * s;
                    left = RcVec3f.Lerp(fromTile.data.verts, v0 * 3, v1 * 3, tmin);
                    right = RcVec3f.Lerp(fromTile.data.verts, v0 * 3, v1 * 3, tmax);
                }
            }

            return DtStatus.DT_SUCCSESS;
        }

        protected DtStatus GetEdgeMidPoint(long from, DtPoly fromPoly, DtMeshTile fromTile, long to,
            DtPoly toPoly, DtMeshTile toTile, ref RcVec3f mid)
        {
            var ppStatus = GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out var left, out var right);
            if (ppStatus.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            mid.x = (left.x + right.x) * 0.5f;
            mid.y = (left.y + right.y) * 0.5f;
            mid.z = (left.z + right.z) * 0.5f;

            return DtStatus.DT_SUCCSESS;
        }

        protected DtStatus GetEdgeIntersectionPoint(RcVec3f fromPos, long from, DtPoly fromPoly, DtMeshTile fromTile,
            RcVec3f toPos, long to, DtPoly toPoly, DtMeshTile toTile,
            ref RcVec3f pt)
        {
            var ppStatus = GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, out var left, out var right);
            if (ppStatus.Failed())
            {
                return DtStatus.DT_FAILURE;
            }

            float t = 0.5f;
            if (DtUtils.IntersectSegSeg2D(fromPos, toPos, left, right, out var _, out var t2))
            {
                t = Clamp(t2, 0.1f, 0.9f);
            }

            pt = RcVec3f.Lerp(left, right, t);
            return DtStatus.DT_SUCCSESS;
        }


        /// @par
        ///
        /// This method is meant to be used for quick, short distance checks.
        ///
        /// If the path array is too small to hold the result, it will be filled as
        /// far as possible from the start postion toward the end position.
        ///
        /// <b>Using the Hit Parameter t of RaycastHit</b>
        ///
        /// If the hit parameter is a very high value (FLT_MAX), then the ray has hit
        /// the end position. In this case the path represents a valid corridor to the
        /// end position and the value of @p hitNormal is undefined.
        ///
        /// If the hit parameter is zero, then the start position is on the wall that
        /// was hit and the value of @p hitNormal is undefined.
        ///
        /// If 0 < t < 1.0 then the following applies:
        ///
        /// @code
        /// distanceToHitBorder = distanceToEndPosition * t
        /// hitPoint = startPos + (endPos - startPos) * t
        /// @endcode
        ///
        /// <b>Use Case Restriction</b>
        ///
        /// The raycast ignores the y-value of the end position. (2D check.) This
        /// places significant limits on how it can be used. For example:
        ///
        /// Consider a scene where there is a main floor with a second floor balcony
        /// that hangs over the main floor. So the first floor mesh extends below the
        /// balcony mesh. The start position is somewhere on the first floor. The end
        /// position is on the balcony.
        ///
        /// The raycast will search toward the end position along the first floor mesh.
        /// If it reaches the end position's xz-coordinates it will indicate FLT_MAX
        /// (no wall hit), meaning it reached the end position. This is one example of why
        /// this method is meant for short distance checks.
        ///
        /// Casts a 'walkability' ray along the surface of the navigation mesh from
        /// the start position toward the end position.
        /// @note A wrapper around Raycast(..., RaycastHit*). Retained for backward compatibility.
        /// @param[in] startRef The reference id of the start polygon.
        /// @param[in] startPos A position within the start polygon representing
        /// the start of the ray. [(x, y, z)]
        /// @param[in] endPos The position to cast the ray toward. [(x, y, z)]
        /// @param[out] t The hit parameter. (FLT_MAX if no wall hit.)
        /// @param[out] hitNormal The normal of the nearest wall hit. [(x, y, z)]
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] path The reference ids of the visited polygons. [opt]
        /// @param[out] pathCount The number of visited polygons. [opt]
        /// @param[in] maxPath The maximum number of polygons the @p path array can hold.
        /// @returns The status flags for the query.
        public DtStatus Raycast(long startRef, RcVec3f startPos, RcVec3f endPos, IDtQueryFilter filter, int options,
            long prevRef, out DtRaycastHit hit)
        {
            hit = null;

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(startPos) || !RcVec3f.IsFinite(endPos)
                || null == filter || (prevRef != 0 && !m_nav.IsValidPolyRef(prevRef)))
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            hit = new DtRaycastHit();

            RcVec3f[] verts = new RcVec3f[m_nav.GetMaxVertsPerPoly() + 1];

            RcVec3f curPos = RcVec3f.Zero;
            RcVec3f lastPos = RcVec3f.Zero;

            curPos = startPos;
            var dir = endPos.Subtract(startPos);

            DtMeshTile prevTile, tile, nextTile;
            DtPoly prevPoly, poly, nextPoly;

            // The API input has been checked already, skip checking internal data.
            long curRef = startRef;
            m_nav.GetTileAndPolyByRefUnsafe(curRef, out tile, out poly);
            nextTile = prevTile = tile;
            nextPoly = prevPoly = poly;
            if (prevRef != 0)
            {
                m_nav.GetTileAndPolyByRefUnsafe(prevRef, out prevTile, out prevPoly);
            }

            while (curRef != 0)
            {
                // Cast ray against current polygon.

                // Collect vertices.
                int nv = 0;
                for (int i = 0; i < poly.vertCount; ++i)
                {
                    verts[nv] = RcVec3f.Of(tile.data.verts, poly.verts[i] * 3);
                    nv++;
                }

                bool intersects = DtUtils.IntersectSegmentPoly2D(startPos, endPos, verts, nv, out var tmin, out var tmax, out var segMin, out var segMax);
                if (!intersects)
                {
                    // Could not hit the polygon, keep the old t and report hit.
                    return DtStatus.DT_SUCCSESS;
                }

                hit.hitEdgeIndex = segMax;

                // Keep track of furthest t so far.
                if (tmax > hit.t)
                {
                    hit.t = tmax;
                }

                // Store visited polygons.
                hit.path.Add(curRef);

                // Ray end is completely inside the polygon.
                if (segMax == -1)
                {
                    hit.t = float.MaxValue;

                    // add the cost
                    if ((options & DT_RAYCAST_USE_COSTS) != 0)
                    {
                        hit.pathCost += filter.GetCost(curPos, endPos, prevRef, prevTile, prevPoly, curRef, tile, poly,
                            curRef, tile, poly);
                    }

                    return DtStatus.DT_SUCCSESS;
                }

                // Follow neighbours.
                long nextRef = 0;

                for (int i = tile.polyLinks[poly.index]; i != DtNavMesh.DT_NULL_LINK; i = tile.links[i].next)
                {
                    DtLink link = tile.links[i];

                    // Find link which contains this edge.
                    if (link.edge != segMax)
                    {
                        continue;
                    }

                    // Get pointer to the next polygon.
                    m_nav.GetTileAndPolyByRefUnsafe(link.refs, out nextTile, out nextPoly);

                    // Skip off-mesh connections.
                    if (nextPoly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Skip links based on filter.
                    if (!filter.PassFilter(link.refs, nextTile, nextPoly))
                    {
                        continue;
                    }

                    // If the link is internal, just return the ref.
                    if (link.side == 0xff)
                    {
                        nextRef = link.refs;
                        break;
                    }

                    // If the link is at tile boundary,

                    // Check if the link spans the whole edge, and accept.
                    if (link.bmin == 0 && link.bmax == 255)
                    {
                        nextRef = link.refs;
                        break;
                    }

                    // Check for partial edge links.
                    int v0 = poly.verts[link.edge];
                    int v1 = poly.verts[(link.edge + 1) % poly.vertCount];
                    int left = v0 * 3;
                    int right = v1 * 3;

                    // Check that the intersection lies inside the link portal.
                    if (link.side == 0 || link.side == 4)
                    {
                        // Calculate link size.
                        const float s = 1.0f / 255.0f;
                        float lmin = tile.data.verts[left + 2]
                                     + (tile.data.verts[right + 2] - tile.data.verts[left + 2]) * (link.bmin * s);
                        float lmax = tile.data.verts[left + 2]
                                     + (tile.data.verts[right + 2] - tile.data.verts[left + 2]) * (link.bmax * s);
                        if (lmin > lmax)
                        {
                            (lmin, lmax) = (lmax, lmin);
                        }

                        // Find Z intersection.
                        float z = startPos.z + (endPos.z - startPos.z) * tmax;
                        if (z >= lmin && z <= lmax)
                        {
                            nextRef = link.refs;
                            break;
                        }
                    }
                    else if (link.side == 2 || link.side == 6)
                    {
                        // Calculate link size.
                        const float s = 1.0f / 255.0f;
                        float lmin = tile.data.verts[left]
                                     + (tile.data.verts[right] - tile.data.verts[left]) * (link.bmin * s);
                        float lmax = tile.data.verts[left]
                                     + (tile.data.verts[right] - tile.data.verts[left]) * (link.bmax * s);
                        if (lmin > lmax)
                        {
                            (lmin, lmax) = (lmax, lmin);
                        }

                        // Find X intersection.
                        float x = startPos.x + (endPos.x - startPos.x) * tmax;
                        if (x >= lmin && x <= lmax)
                        {
                            nextRef = link.refs;
                            break;
                        }
                    }
                }

                // add the cost
                if ((options & DT_RAYCAST_USE_COSTS) != 0)
                {
                    // compute the intersection point at the furthest end of the polygon
                    // and correct the height (since the raycast moves in 2d)
                    lastPos = curPos;
                    curPos = RcVec3f.Mad(startPos, dir, hit.t);
                    var e1 = verts[segMax];
                    var e2 = verts[(segMax + 1) % nv];
                    var eDir = e2.Subtract(e1);
                    var diff = curPos.Subtract(e1);
                    float s = Sqr(eDir.x) > Sqr(eDir.z) ? diff.x / eDir.x : diff.z / eDir.z;
                    curPos.y = e1.y + eDir.y * s;

                    hit.pathCost += filter.GetCost(lastPos, curPos, prevRef, prevTile, prevPoly, curRef, tile, poly,
                        nextRef, nextTile, nextPoly);
                }

                if (nextRef == 0)
                {
                    // No neighbour, we hit a wall.

                    // Calculate hit normal.
                    int a = segMax;
                    int b = segMax + 1 < nv ? segMax + 1 : 0;
                    // int va = a * 3;
                    // int vb = b * 3;
                    float dx = verts[b].x - verts[a].x;
                    float dz = verts[b].z - verts[a].x;
                    hit.hitNormal.x = dz;
                    hit.hitNormal.y = 0;
                    hit.hitNormal.z = -dx;
                    hit.hitNormal.Normalize();
                    return DtStatus.DT_SUCCSESS;
                }

                // No hit, advance to neighbour polygon.
                prevRef = curRef;
                curRef = nextRef;
                prevTile = tile;
                tile = nextTile;
                prevPoly = poly;
                poly = nextPoly;
            }

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// At least one result array must be provided.
        ///
        /// The order of the result set is from least to highest cost to reach the polygon.
        ///
        /// A common use case for this method is to perform Dijkstra searches.
        /// Candidate polygons are found by searching the graph beginning at the start polygon.
        ///
        /// If a polygon is not found via the graph search, even if it intersects the
        /// search circle, it will not be included in the result set. For example:
        ///
        /// polyA is the start polygon.
        /// polyB shares an edge with polyA. (Is adjacent.)
        /// polyC shares an edge with polyB, but not with polyA
        /// Even if the search circle overlaps polyC, it will not be included in the
        /// result set unless polyB is also in the set.
        ///
        /// The value of the center point is used as the start position for cost
        /// calculations. It is not projected onto the surface of the mesh, so its
        /// y-value will effect the costs.
        ///
        /// Intersection tests occur in 2D. All polygons and the search circle are
        /// projected onto the xz-plane. So the y-value of the center point does not
        /// effect intersection tests.
        ///
        /// If the result arrays are to small to hold the entire result set, they will be
        /// filled to capacity.
        ///
        ///@}
        /// @name Dijkstra Search Functions
        /// @{ 
        /// Finds the polygons along the navigation graph that touch the specified circle.
        ///  @param[in]		startRef		The reference id of the polygon where the search starts.
        ///  @param[in]		centerPos		The center of the search circle. [(x, y, z)]
        ///  @param[in]		radius			The radius of the search circle.
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[out]	resultRef		The reference ids of the polygons touched by the circle. [opt]
        ///  @param[out]	resultParent	The reference ids of the parent polygons for each result. 
        ///  								Zero if a result polygon has no parent. [opt]
        ///  @param[out]	resultCost		The search cost from @p centerPos to the polygon. [opt]
        ///  @param[out]	resultCount		The number of polygons found. [opt]
        ///  @param[in]		maxResult		The maximum number of polygons the result arrays can hold.
        /// @returns The status flags for the query.
        public DtStatus FindPolysAroundCircle(long startRef, RcVec3f centerPos, float radius, IDtQueryFilter filter,
            ref List<long> resultRef, ref List<long> resultParent, ref List<float> resultCost)
        {
            if (null != resultRef)
            {
                resultRef.Clear();
                resultParent.Clear();
                resultCost.Clear();
            }

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(centerPos) || radius < 0
                || !float.IsFinite(radius) || null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_OPEN;
            m_openList.Push(startNode);

            float radiusSqr = Sqr(radius);

            while (!m_openList.IsEmpty())
            {
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DtNode.DT_NODE_OPEN;
                bestNode.flags |= DtNode.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long bestRef = bestNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out var bestTile, out var bestPoly);

                // Get parent poly and tile.
                long parentRef = 0;
                DtMeshTile parentTile = null;
                DtPoly parentPoly = null;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                resultRef.Add(bestRef);
                resultParent.Add(parentRef);
                resultCost.Add(bestNode.total);

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    DtLink link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    var ppStatus = GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                        neighbourTile, out var va, out var vb);
                    if (ppStatus.Failed())
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    var distSqr = DtUtils.DistancePtSegSqr2D(centerPos, va, vb, out var _);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef);

                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = RcVec3f.Lerp(va, vb, 0.5f);
                    }

                    float cost = filter.GetCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly, bestRef,
                        bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.total + cost;
                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// The order of the result set is from least to highest cost.
        /// 
        /// At least one result array must be provided.
        ///
        /// A common use case for this method is to perform Dijkstra searches. 
        /// Candidate polygons are found by searching the graph beginning at the start 
        /// polygon.
        /// 
        /// The same intersection test restrictions that apply to findPolysAroundCircle()
        /// method apply to this method.
        /// 
        /// The 3D centroid of the search polygon is used as the start position for cost 
        /// calculations.
        /// 
        /// Intersection tests occur in 2D. All polygons are projected onto the 
        /// xz-plane. So the y-values of the vertices do not effect intersection tests.
        /// 
        /// If the result arrays are is too small to hold the entire result set, they will 
        /// be filled to capacity.
        ///
        /// Finds the polygons along the naviation graph that touch the specified convex polygon.
        ///  @param[in]		startRef		The reference id of the polygon where the search starts.
        ///  @param[in]		verts			The vertices describing the convex polygon. (CCW) 
        ///  								[(x, y, z) * @p nverts]
        ///  @param[in]		nverts			The number of vertices in the polygon.
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[out]	resultRef		The reference ids of the polygons touched by the search polygon. [opt]
        ///  @param[out]	resultParent	The reference ids of the parent polygons for each result. Zero if a 
        ///  								result polygon has no parent. [opt]
        ///  @param[out]	resultCost		The search cost from the centroid point to the polygon. [opt]
        ///  @param[out]	resultCount		The number of polygons found.
        ///  @param[in]		maxResult		The maximum number of polygons the result arrays can hold.
        /// @returns The status flags for the query.
        public DtStatus FindPolysAroundShape(long startRef, RcVec3f[] verts, IDtQueryFilter filter,
            ref List<long> resultRef, ref List<long> resultParent, ref List<float> resultCost)
        {
            resultRef.Clear();
            resultParent.Clear();
            resultCost.Clear();

            // Validate input
            int nverts = verts.Length;
            if (!m_nav.IsValidPolyRef(startRef) || null == verts || nverts < 3 || null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            RcVec3f centerPos = RcVec3f.Zero;
            for (int i = 0; i < nverts; ++i)
            {
                centerPos += verts[i];
            }

            float scale = 1.0f / nverts;
            centerPos.x *= scale;
            centerPos.y *= scale;
            centerPos.z *= scale;

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_OPEN;
            m_openList.Push(startNode);

            while (!m_openList.IsEmpty())
            {
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DtNode.DT_NODE_OPEN;
                bestNode.flags |= DtNode.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long bestRef = bestNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out var bestTile, out var bestPoly);

                // Get parent poly and tile.
                long parentRef = 0;
                DtMeshTile parentTile = null;
                DtPoly parentPoly = null;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                if (parentRef != 0)
                {
                    m_nav.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                resultRef.Add(bestRef);
                resultParent.Add(parentRef);
                resultCost.Add(bestNode.total);

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    DtLink link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    var ppStatus = GetPortalPoints(bestRef, bestPoly, bestTile, neighbourRef, neighbourPoly,
                        neighbourTile, out var va, out var vb);
                    if (ppStatus.Failed())
                    {
                        continue;
                    }

                    // If the poly is not touching the edge to the next polygon, skip the connection it.
                    bool intersects = DtUtils.IntersectSegmentPoly2D(va, vb, verts, nverts, out var tmin, out var tmax, out var segMin, out var segMax);
                    if (!intersects)
                    {
                        continue;
                    }

                    if (tmin > 1.0f || tmax < 0.0f)
                    {
                        continue;
                    }

                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef);

                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        neighbourNode.pos = RcVec3f.Lerp(va, vb, 0.5f);
                    }

                    float cost = filter.GetCost(bestNode.pos, neighbourNode.pos, parentRef, parentTile, parentPoly, bestRef,
                        bestTile, bestPoly, neighbourRef, neighbourTile, neighbourPoly);

                    float total = bestNode.total + cost;

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags = DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// This method is optimized for a small search radius and small number of result 
        /// polygons.
        ///
        /// Candidate polygons are found by searching the navigation graph beginning at 
        /// the start polygon.
        ///
        /// The same intersection test restrictions that apply to the findPolysAroundCircle 
        /// mehtod applies to this method.
        ///
        /// The value of the center point is used as the start point for cost calculations. 
        /// It is not projected onto the surface of the mesh, so its y-value will effect 
        /// the costs.
        /// 
        /// Intersection tests occur in 2D. All polygons and the search circle are 
        /// projected onto the xz-plane. So the y-value of the center point does not 
        /// effect intersection tests.
        /// 
        /// If the result arrays are is too small to hold the entire result set, they will 
        /// be filled to capacity.
        /// 
        /// Finds the non-overlapping navigation polygons in the local neighbourhood around the center position.
        ///  @param[in]		startRef		The reference id of the polygon where the search starts.
        ///  @param[in]		centerPos		The center of the query circle. [(x, y, z)]
        ///  @param[in]		radius			The radius of the query circle.
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[out]	resultRef		The reference ids of the polygons touched by the circle.
        ///  @param[out]	resultParent	The reference ids of the parent polygons for each result. 
        /// @returns The status flags for the query.
        public DtStatus FindLocalNeighbourhood(long startRef, RcVec3f centerPos, float radius,
            IDtQueryFilter filter,
            ref List<long> resultRef, ref List<long> resultParent)
        {
            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(centerPos) || radius < 0
                || !float.IsFinite(radius) || null == filter
                || null == resultRef || null == resultParent)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            resultRef.Clear();
            resultParent.Clear();

            DtNodePool tinyNodePool = new DtNodePool();

            DtNode startNode = tinyNodePool.GetNode(startRef);
            startNode.pidx = 0;
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_CLOSED;
            LinkedList<DtNode> stack = new LinkedList<DtNode>();
            stack.AddLast(startNode);

            resultRef.Add(startNode.id);
            resultParent.Add(0L);

            float radiusSqr = Sqr(radius);

            float[] pa = new float[m_nav.GetMaxVertsPerPoly() * 3];
            float[] pb = new float[m_nav.GetMaxVertsPerPoly() * 3];

            while (0 < stack.Count)
            {
                // Pop front.
                DtNode curNode = stack.First?.Value;
                stack.RemoveFirst();

                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long curRef = curNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(curRef, out var curTile, out var curPoly);

                for (int i = curTile.polyLinks[curPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = curTile.links[i].next)
                {
                    DtLink link = curTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours.
                    if (neighbourRef == 0)
                    {
                        continue;
                    }

                    DtNode neighbourNode = tinyNodePool.GetNode(neighbourRef);
                    // Skip visited.
                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Expand to neighbour
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    // Skip off-mesh connections.
                    if (neighbourPoly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Do not advance if the polygon is excluded by the filter.
                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    // Find edge and calc distance to the edge.
                    var ppStatus = GetPortalPoints(curRef, curPoly, curTile, neighbourRef, neighbourPoly,
                        neighbourTile, out var va, out var vb);
                    if (ppStatus.Failed())
                    {
                        continue;
                    }

                    // If the circle is not touching the next polygon, skip it.
                    var distSqr = DtUtils.DistancePtSegSqr2D(centerPos, va, vb, out var _);
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Mark node visited, this is done before the overlap test so that
                    // we will not visit the poly again if the test fails.
                    neighbourNode.flags |= DtNode.DT_NODE_CLOSED;
                    neighbourNode.pidx = tinyNodePool.GetNodeIdx(curNode);

                    // Check that the polygon does not collide with existing polygons.

                    // Collect vertices of the neighbour poly.
                    int npa = neighbourPoly.vertCount;
                    for (int k = 0; k < npa; ++k)
                    {
                        Array.Copy(neighbourTile.data.verts, neighbourPoly.verts[k] * 3, pa, k * 3, 3);
                    }

                    bool overlap = false;
                    for (int j = 0; j < resultRef.Count; ++j)
                    {
                        long pastRef = resultRef[j];

                        // Connected polys do not overlap.
                        bool connected = false;
                        for (int k = curTile.polyLinks[curPoly.index]; k != DtNavMesh.DT_NULL_LINK; k = curTile.links[k].next)
                        {
                            if (curTile.links[k].refs == pastRef)
                            {
                                connected = true;
                                break;
                            }
                        }

                        if (connected)
                        {
                            continue;
                        }

                        // Potentially overlapping.
                        m_nav.GetTileAndPolyByRefUnsafe(pastRef, out var pastTile, out var pastPoly);

                        // Get vertices and test overlap
                        int npb = pastPoly.vertCount;
                        for (int k = 0; k < npb; ++k)
                        {
                            Array.Copy(pastTile.data.verts, pastPoly.verts[k] * 3, pb, k * 3, 3);
                        }

                        if (DtUtils.OverlapPolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (overlap)
                    {
                        continue;
                    }

                    resultRef.Add(neighbourRef);
                    resultParent.Add(curRef);
                    stack.AddLast(neighbourNode);
                }
            }

            return DtStatus.DT_SUCCSESS;
        }


        protected void InsertInterval(List<DtSegInterval> ints, int tmin, int tmax, long refs)
        {
            // Find insertion point.
            int idx = 0;
            while (idx < ints.Count)
            {
                if (tmax <= ints[idx].tmin)
                {
                    break;
                }

                idx++;
            }

            // Store
            ints.Insert(idx, new DtSegInterval(refs, tmin, tmax));
        }

        /// @par
        ///
        /// If the @p segmentRefs parameter is provided, then all polygon segments will be returned.
        /// Otherwise only the wall segments are returned.
        ///
        /// A segment that is normally a portal will be included in the result set as a
        /// wall if the @p filter results in the neighbor polygon becoomming impassable.
        ///
        /// The @p segmentVerts and @p segmentRefs buffers should normally be sized for the
        /// maximum segments per polygon of the source navigation mesh.
        ///
        /// Returns the segments for the specified polygon, optionally including portals.
        /// @param[in] ref The reference id of the polygon.
        /// @param[in] filter The polygon filter to apply to the query.
        /// @param[out] segmentVerts The segments. [(ax, ay, az, bx, by, bz) * segmentCount]
        /// @param[out] segmentRefs The reference ids of each segment's neighbor polygon.
        /// Or zero if the segment is a wall. [opt] [(parentRef) * @p segmentCount]
        /// @param[out] segmentCount The number of segments returned.
        /// @param[in] maxSegments The maximum number of segments the result arrays can hold.
        /// @returns The status flags for the query.
        public DtStatus GetPolyWallSegments(long refs, bool storePortals, IDtQueryFilter filter,
            ref List<RcSegmentVert> segmentVerts, ref List<long> segmentRefs)
        {
            segmentVerts.Clear();
            segmentRefs.Clear();

            var status = m_nav.GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Failed())
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            if (null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            List<DtSegInterval> ints = new List<DtSegInterval>(16);
            for (int i = 0, j = poly.vertCount - 1; i < poly.vertCount; j = i++)
            {
                // Skip non-solid edges.
                ints.Clear();
                if ((poly.neis[j] & DtNavMesh.DT_EXT_LINK) != 0)
                {
                    // Tile border.
                    for (int k = tile.polyLinks[poly.index]; k != DtNavMesh.DT_NULL_LINK; k = tile.links[k].next)
                    {
                        DtLink link = tile.links[k];
                        if (link.edge == j)
                        {
                            if (link.refs != 0)
                            {
                                m_nav.GetTileAndPolyByRefUnsafe(link.refs, out var neiTile, out var neiPoly);
                                if (filter.PassFilter(link.refs, neiTile, neiPoly))
                                {
                                    InsertInterval(ints, link.bmin, link.bmax, link.refs);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Internal edge
                    long neiRef = 0;
                    if (poly.neis[j] != 0)
                    {
                        int idx = (poly.neis[j] - 1);
                        neiRef = m_nav.GetPolyRefBase(tile) | (long)idx;
                        if (!filter.PassFilter(neiRef, tile, tile.data.polys[idx]))
                        {
                            neiRef = 0;
                        }
                    }

                    // If the edge leads to another polygon and portals are not stored, skip.
                    if (neiRef != 0 && !storePortals)
                    {
                        continue;
                    }

                    int ivj = poly.verts[j] * 3;
                    int ivi = poly.verts[i] * 3;
                    var seg = new RcSegmentVert();
                    seg.vmin.Set(tile.data.verts, ivj);
                    seg.vmax.Set(tile.data.verts, ivi);
                    // Array.Copy(tile.data.verts, ivj, seg, 0, 3);
                    // Array.Copy(tile.data.verts, ivi, seg, 3, 3);
                    segmentVerts.Add(seg);
                    segmentRefs.Add(neiRef);
                    continue;
                }

                // Add sentinels
                InsertInterval(ints, -1, 0, 0);
                InsertInterval(ints, 255, 256, 0);

                // Store segments.
                int vj = poly.verts[j] * 3;
                int vi = poly.verts[i] * 3;
                for (int k = 1; k < ints.Count; ++k)
                {
                    // Portal segment.
                    if (storePortals && ints[k].refs != 0)
                    {
                        float tmin = ints[k].tmin / 255.0f;
                        float tmax = ints[k].tmax / 255.0f;
                        var seg = new RcSegmentVert();
                        seg.vmin = RcVec3f.Lerp(tile.data.verts, vj, vi, tmin);
                        seg.vmax = RcVec3f.Lerp(tile.data.verts, vj, vi, tmax);
                        segmentVerts.Add(seg);
                        segmentRefs.Add(ints[k].refs);
                    }

                    // Wall segment.
                    int imin = ints[k - 1].tmax;
                    int imax = ints[k].tmin;
                    if (imin != imax)
                    {
                        float tmin = imin / 255.0f;
                        float tmax = imax / 255.0f;
                        var seg = new RcSegmentVert();
                        seg.vmin = RcVec3f.Lerp(tile.data.verts, vj, vi, tmin);
                        seg.vmax = RcVec3f.Lerp(tile.data.verts, vj, vi, tmax);
                        segmentVerts.Add(seg);
                        segmentRefs.Add(0L);
                    }
                }
            }

            return DtStatus.DT_SUCCSESS;
        }

        /// @par
        ///
        /// @p hitPos is not adjusted using the height detail data.
        ///
        /// @p hitDist will equal the search radius if there is no wall within the
        /// radius. In this case the values of @p hitPos and @p hitNormal are
        /// undefined.
        ///
        /// The normal will become unpredicable if @p hitDist is a very small number.
        ///
        /// Finds the distance from the specified position to the nearest polygon wall.
        ///  @param[in]		startRef		The reference id of the polygon containing @p centerPos.
        ///  @param[in]		centerPos		The center of the search circle. [(x, y, z)]
        ///  @param[in]		maxRadius		The radius of the search circle.
        ///  @param[in]		filter			The polygon filter to apply to the query.
        ///  @param[out]	hitDist			The distance to the nearest wall from @p centerPos.
        ///  @param[out]	hitPos			The nearest position on the wall that was hit. [(x, y, z)]
        ///  @param[out]	hitNormal		The normalized ray formed from the wall point to the 
        ///  								source point. [(x, y, z)]
        /// @returns The status flags for the query.
        public virtual DtStatus FindDistanceToWall(long startRef, RcVec3f centerPos, float maxRadius,
            IDtQueryFilter filter,
            out float hitDist, out RcVec3f hitPos, out RcVec3f hitNormal)
        {
            hitDist = 0;
            hitPos = RcVec3f.Zero;
            hitNormal = RcVec3f.Zero;

            // Validate input
            if (!m_nav.IsValidPolyRef(startRef) || !RcVec3f.IsFinite(centerPos) || maxRadius < 0
                || !float.IsFinite(maxRadius) || null == filter)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            m_nodePool.Clear();
            m_openList.Clear();

            DtNode startNode = m_nodePool.GetNode(startRef);
            startNode.pos = centerPos;
            startNode.pidx = 0;
            startNode.cost = 0;
            startNode.total = 0;
            startNode.id = startRef;
            startNode.flags = DtNode.DT_NODE_OPEN;
            m_openList.Push(startNode);

            float radiusSqr = Sqr(maxRadius);

            var hasBestV = false;
            var bestvj = RcVec3f.Zero;
            var bestvi = RcVec3f.Zero;

            var status = DtStatus.DT_SUCCSESS;
            while (!m_openList.IsEmpty())
            {
                DtNode bestNode = m_openList.Pop();
                bestNode.flags &= ~DtNode.DT_NODE_OPEN;
                bestNode.flags |= DtNode.DT_NODE_CLOSED;

                // Get poly and tile.
                // The API input has been checked already, skip checking internal data.
                long bestRef = bestNode.id;
                m_nav.GetTileAndPolyByRefUnsafe(bestRef, out var bestTile, out var bestPoly);

                // Get parent poly and tile.
                long parentRef = 0;
                if (bestNode.pidx != 0)
                {
                    parentRef = m_nodePool.GetNodeAtIdx(bestNode.pidx).id;
                }

                // Hit test walls.
                for (int i = 0, j = bestPoly.vertCount - 1; i < bestPoly.vertCount; j = i++)
                {
                    // Skip non-solid edges.
                    if ((bestPoly.neis[j] & DtNavMesh.DT_EXT_LINK) != 0)
                    {
                        // Tile border.
                        bool solid = true;
                        for (int k = bestTile.polyLinks[bestPoly.index]; k != DtNavMesh.DT_NULL_LINK; k = bestTile.links[k].next)
                        {
                            DtLink link = bestTile.links[k];
                            if (link.edge == j)
                            {
                                if (link.refs != 0)
                                {
                                    m_nav.GetTileAndPolyByRefUnsafe(link.refs, out var neiTile, out var neiPoly);
                                    if (filter.PassFilter(link.refs, neiTile, neiPoly))
                                    {
                                        solid = false;
                                    }
                                }

                                break;
                            }
                        }

                        if (!solid)
                        {
                            continue;
                        }
                    }
                    else if (bestPoly.neis[j] != 0)
                    {
                        // Internal edge
                        int idx = (bestPoly.neis[j] - 1);
                        long refs = m_nav.GetPolyRefBase(bestTile) | (long)idx;
                        if (filter.PassFilter(refs, bestTile, bestTile.data.polys[idx]))
                        {
                            continue;
                        }
                    }

                    // Calc distance to the edge.
                    int vj = bestPoly.verts[j] * 3;
                    int vi = bestPoly.verts[i] * 3;
                    var distSqr = DtUtils.DistancePtSegSqr2D(centerPos, bestTile.data.verts, vj, vi, out var tseg);

                    // Edge is too far, skip.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    // Hit wall, update radius.
                    radiusSqr = distSqr;
                    // Calculate hit pos.
                    hitPos.x = bestTile.data.verts[vj + 0] + (bestTile.data.verts[vi + 0] - bestTile.data.verts[vj + 0]) * tseg;
                    hitPos.y = bestTile.data.verts[vj + 1] + (bestTile.data.verts[vi + 1] - bestTile.data.verts[vj + 1]) * tseg;
                    hitPos.z = bestTile.data.verts[vj + 2] + (bestTile.data.verts[vi + 2] - bestTile.data.verts[vj + 2]) * tseg;
                    hasBestV = true;
                    bestvj = RcVec3f.Of(bestTile.data.verts, vj);
                    bestvi = RcVec3f.Of(bestTile.data.verts, vi);
                }

                for (int i = bestTile.polyLinks[bestPoly.index]; i != DtNavMesh.DT_NULL_LINK; i = bestTile.links[i].next)
                {
                    DtLink link = bestTile.links[i];
                    long neighbourRef = link.refs;
                    // Skip invalid neighbours and do not follow back to parent.
                    if (neighbourRef == 0 || neighbourRef == parentRef)
                    {
                        continue;
                    }

                    // Expand to neighbour.
                    m_nav.GetTileAndPolyByRefUnsafe(neighbourRef, out var neighbourTile, out var neighbourPoly);

                    // Skip off-mesh connections.
                    if (neighbourPoly.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                    {
                        continue;
                    }

                    // Calc distance to the edge.
                    int va = bestPoly.verts[link.edge] * 3;
                    int vb = bestPoly.verts[(link.edge + 1) % bestPoly.vertCount] * 3;
                    var distSqr = DtUtils.DistancePtSegSqr2D(centerPos, bestTile.data.verts, va, vb, out var tseg);
                    // If the circle is not touching the next polygon, skip it.
                    if (distSqr > radiusSqr)
                    {
                        continue;
                    }

                    if (!filter.PassFilter(neighbourRef, neighbourTile, neighbourPoly))
                    {
                        continue;
                    }

                    DtNode neighbourNode = m_nodePool.GetNode(neighbourRef);
                    if (null == neighbourNode)
                    {
                        status |= DtStatus.DT_OUT_OF_NODES;
                        continue;
                    }

                    if ((neighbourNode.flags & DtNode.DT_NODE_CLOSED) != 0)
                    {
                        continue;
                    }

                    // Cost
                    if (neighbourNode.flags == 0)
                    {
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile,
                            neighbourRef, neighbourPoly, neighbourTile,
                            ref neighbourNode.pos);
                    }

                    float total = bestNode.total + RcVec3f.Distance(bestNode.pos, neighbourNode.pos);

                    // The node is already in open list and the new result is worse, skip.
                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0 && total >= neighbourNode.total)
                    {
                        continue;
                    }

                    neighbourNode.id = neighbourRef;
                    neighbourNode.flags = (neighbourNode.flags & ~DtNode.DT_NODE_CLOSED);
                    neighbourNode.pidx = m_nodePool.GetNodeIdx(bestNode);
                    neighbourNode.total = total;

                    if ((neighbourNode.flags & DtNode.DT_NODE_OPEN) != 0)
                    {
                        m_openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.flags |= DtNode.DT_NODE_OPEN;
                        m_openList.Push(neighbourNode);
                    }
                }
            }

            // Calc hit normal.
            if (hasBestV)
            {
                var tangent = bestvi.Subtract(bestvj);
                hitNormal.x = tangent.z;
                hitNormal.y = 0;
                hitNormal.z = -tangent.x;
                hitNormal.Normalize();
            }

            hitDist = (float)Math.Sqrt(radiusSqr);

            return status;
        }

        /// Returns true if the polygon reference is valid and passes the filter restrictions.
        /// @param[in] ref The polygon reference to check.
        /// @param[in] filter The filter to apply.
        public bool IsValidPolyRef(long refs, IDtQueryFilter filter)
        {
            var status = m_nav.GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Failed())
            {
                return false;
            }

            // If cannot pass filter, assume flags has changed and boundary is invalid.
            if (!filter.PassFilter(refs, tile, poly))
            {
                return false;
            }

            return true;
        }

        /// Gets the navigation mesh the query object is using.
        /// @return The navigation mesh the query object is using.
        public DtNavMesh GetAttachedNavMesh()
        {
            return m_nav;
        }

        /**
     * Gets a path from the explored nodes in the previous search.
     *
     * @param endRef
     *            The reference id of the end polygon.
     * @returns An ordered list of polygon references representing the path. (Start to end.)
     * @remarks The result of this function depends on the state of the query object. For that reason it should only be
     *          used immediately after one of the two Dijkstra searches, findPolysAroundCircle or findPolysAroundShape.
     */
        public DtStatus GetPathFromDijkstraSearch(long endRef, ref List<long> path)
        {
            if (!m_nav.IsValidPolyRef(endRef) || null == path)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            path.Clear();

            List<DtNode> nodes = m_nodePool.FindNodes(endRef);
            if (nodes.Count != 1)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            DtNode endNode = nodes[0];
            if ((endNode.flags & DT_NODE_CLOSED) == 0)
            {
                return DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM;
            }

            return GetPathToNode(endNode, ref path);
        }

        // Gets the path leading to the specified end node.
        protected DtStatus GetPathToNode(DtNode endNode, ref List<long> path)
        {
            // Reverse the path.
            DtNode curNode = endNode;
            do
            {
                path.Add(curNode.id);
                DtNode nextNode = m_nodePool.GetNodeAtIdx(curNode.pidx);
                if (curNode.shortcut != null)
                {
                    // remove potential duplicates from shortcut path
                    for (int i = curNode.shortcut.Count - 1; i >= 0; i--)
                    {
                        long id = curNode.shortcut[i];
                        if (id != curNode.id && id != nextNode.id)
                        {
                            path.Add(id);
                        }
                    }
                }

                curNode = nextNode;
            } while (curNode != null);

            path.Reverse();
            return DtStatus.DT_SUCCSESS;
        }

        /**
     * The closed list is the list of polygons that were fully evaluated during the last navigation graph search. (A* or
     * Dijkstra)
     */
        public bool IsInClosedList(long refs)
        {
            if (m_nodePool == null)
            {
                return false;
            }

            foreach (DtNode n in m_nodePool.FindNodes(refs))
            {
                if ((n.flags & DT_NODE_CLOSED) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public DtNodePool GetNodePool()
        {
            return m_nodePool;
        }
    }
}