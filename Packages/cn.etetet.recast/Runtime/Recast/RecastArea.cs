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
using DotRecast.Core;

namespace DotRecast.Recast
{
    using static RcConstants;

    public static class RecastArea
    {
        /// Erodes the walkable area within the heightfield by the specified radius.
        /// 
        /// Basically, any spans that are closer to a boundary or obstruction than the specified radius 
        /// are marked as un-walkable.
        ///
        /// This method is usually called immediately after the heightfield has been built.
        /// 
        /// @see rcCompactHeightfield, rcBuildCompactHeightfield, rcConfig::walkableRadius
        /// @ingroup recast
        ///
        /// @param[in,out]	context				The build context to use during the operation.
        /// @param[in]		erosionRadius		The radius of erosion. [Limits: 0 < value < 255] [Units: vx]
        /// @param[in,out]	compactHeightfield	The populated compact heightfield to erode.
        /// @returns True if the operation completed successfully.
        public static void ErodeWalkableArea(RcTelemetry context, int erosionRadius, RcCompactHeightfield compactHeightfield)
        {
            int xSize = compactHeightfield.width;
            int zSize = compactHeightfield.height;
            int zStride = xSize; // For readability

            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_ERODE_AREA);

            int[] distanceToBoundary = new int[compactHeightfield.spanCount];
            Array.Fill(distanceToBoundary, 255);

            // Mark boundary cells.
            for (int z = 0; z < zSize; ++z)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    for (int spanIndex = cell.index, maxSpanIndex = cell.index + cell.count; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        if (compactHeightfield.areas[spanIndex] == RC_NULL_AREA)
                        {
                            distanceToBoundary[spanIndex] = 0;
                        }
                        else
                        {
                            RcCompactSpan span = compactHeightfield.spans[spanIndex];

                            // Check that there is a non-null adjacent span in each of the 4 cardinal directions.
                            int neighborCount = 0;
                            for (int direction = 0; direction < 4; ++direction)
                            {
                                int neighborConnection = RecastCommon.GetCon(span, direction);
                                if (neighborConnection == RC_NOT_CONNECTED)
                                {
                                    break;
                                }

                                int neighborX = x + RecastCommon.GetDirOffsetX(direction);
                                int neighborZ = z + RecastCommon.GetDirOffsetY(direction);
                                int neighborSpanIndex = compactHeightfield.cells[neighborX + neighborZ * zStride].index + RecastCommon.GetCon(span, direction);
                                if (compactHeightfield.areas[neighborSpanIndex] == RC_NULL_AREA)
                                {
                                    break;
                                }

                                neighborCount++;
                            }

                            // At least one missing neighbour, so this is a boundary cell.
                            if (neighborCount != 4)
                            {
                                distanceToBoundary[spanIndex] = 0;
                            }
                        }
                    }
                }
            }

            int newDistance;

            // Pass 1
            for (int z = 0; z < zSize; ++z)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;
                    for (int spanIndex = cell.index; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        RcCompactSpan span = compactHeightfield.spans[spanIndex];

                        if (RecastCommon.GetCon(span, 0) != RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int aX = x + RecastCommon.GetDirOffsetX(0);
                            int aY = z + RecastCommon.GetDirOffsetY(0);
                            int aIndex = compactHeightfield.cells[aX + aY * xSize].index + RecastCommon.GetCon(span, 0);
                            RcCompactSpan aSpan = compactHeightfield.spans[aIndex];
                            newDistance = Math.Min(distanceToBoundary[aIndex] + 2, 255);
                            if (newDistance < distanceToBoundary[spanIndex])
                            {
                                distanceToBoundary[spanIndex] = newDistance;
                            }

                            // (-1,-1)
                            if (RecastCommon.GetCon(aSpan, 3) != RC_NOT_CONNECTED)
                            {
                                int bX = aX + RecastCommon.GetDirOffsetX(3);
                                int bY = aY + RecastCommon.GetDirOffsetY(3);
                                int bIndex = compactHeightfield.cells[bX + bY * xSize].index + RecastCommon.GetCon(aSpan, 3);
                                newDistance = Math.Min(distanceToBoundary[bIndex] + 3, 255);
                                if (newDistance < distanceToBoundary[spanIndex])
                                {
                                    distanceToBoundary[spanIndex] = newDistance;
                                }
                            }
                        }

                        if (RecastCommon.GetCon(span, 3) != RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int aX = x + RecastCommon.GetDirOffsetX(3);
                            int aY = z + RecastCommon.GetDirOffsetY(3);
                            int aIndex = compactHeightfield.cells[aX + aY * xSize].index + RecastCommon.GetCon(span, 3);
                            RcCompactSpan aSpan = compactHeightfield.spans[aIndex];
                            newDistance = Math.Min(distanceToBoundary[aIndex] + 2, 255);
                            if (newDistance < distanceToBoundary[spanIndex])
                            {
                                distanceToBoundary[spanIndex] = newDistance;
                            }

                            // (1,-1)
                            if (RecastCommon.GetCon(aSpan, 2) != RC_NOT_CONNECTED)
                            {
                                int bX = aX + RecastCommon.GetDirOffsetX(2);
                                int bY = aY + RecastCommon.GetDirOffsetY(2);
                                int bIndex = compactHeightfield.cells[bX + bY * xSize].index + RecastCommon.GetCon(aSpan, 2);
                                newDistance = Math.Min(distanceToBoundary[bIndex] + 3, 255);
                                if (newDistance < distanceToBoundary[spanIndex])
                                {
                                    distanceToBoundary[spanIndex] = newDistance;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int z = zSize - 1; z >= 0; --z)
            {
                for (int x = xSize - 1; x >= 0; --x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;
                    for (int i = cell.index; i < maxSpanIndex; ++i)
                    {
                        RcCompactSpan span = compactHeightfield.spans[i];

                        if (RecastCommon.GetCon(span, 2) != RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int aX = x + RecastCommon.GetDirOffsetX(2);
                            int aY = z + RecastCommon.GetDirOffsetY(2);
                            int aIndex = compactHeightfield.cells[aX + aY * xSize].index + RecastCommon.GetCon(span, 2);
                            RcCompactSpan aSpan = compactHeightfield.spans[aIndex];
                            newDistance = Math.Min(distanceToBoundary[aIndex] + 2, 255);
                            if (newDistance < distanceToBoundary[i])
                            {
                                distanceToBoundary[i] = newDistance;
                            }

                            // (1,1)
                            if (RecastCommon.GetCon(aSpan, 1) != RC_NOT_CONNECTED)
                            {
                                int bX = aX + RecastCommon.GetDirOffsetX(1);
                                int bY = aY + RecastCommon.GetDirOffsetY(1);
                                int bIndex = compactHeightfield.cells[bX + bY * xSize].index + RecastCommon.GetCon(aSpan, 1);
                                newDistance = Math.Min(distanceToBoundary[bIndex] + 3, 255);
                                if (newDistance < distanceToBoundary[i])
                                {
                                    distanceToBoundary[i] = newDistance;
                                }
                            }
                        }

                        if (RecastCommon.GetCon(span, 1) != RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int aX = x + RecastCommon.GetDirOffsetX(1);
                            int aY = z + RecastCommon.GetDirOffsetY(1);
                            int aIndex = compactHeightfield.cells[aX + aY * xSize].index + RecastCommon.GetCon(span, 1);
                            RcCompactSpan aSpan = compactHeightfield.spans[aIndex];
                            newDistance = Math.Min(distanceToBoundary[aIndex] + 2, 255);
                            if (newDistance < distanceToBoundary[i])
                            {
                                distanceToBoundary[i] = newDistance;
                            }

                            // (-1,1)
                            if (RecastCommon.GetCon(aSpan, 0) != RC_NOT_CONNECTED)
                            {
                                int bX = aX + RecastCommon.GetDirOffsetX(0);
                                int bY = aY + RecastCommon.GetDirOffsetY(0);
                                int bIndex = compactHeightfield.cells[bX + bY * xSize].index + RecastCommon.GetCon(aSpan, 0);
                                newDistance = Math.Min(distanceToBoundary[bIndex] + 3, 255);
                                if (newDistance < distanceToBoundary[i])
                                {
                                    distanceToBoundary[i] = newDistance;
                                }
                            }
                        }
                    }
                }
            }

            int minBoundaryDistance = erosionRadius * 2;
            for (int spanIndex = 0; spanIndex < compactHeightfield.spanCount; ++spanIndex)
            {
                if (distanceToBoundary[spanIndex] < minBoundaryDistance)
                {
                    compactHeightfield.areas[spanIndex] = RC_NULL_AREA;
                }
            }
        }

        /// Applies a median filter to walkable area types (based on area id), removing noise.
        /// 
        /// This filter is usually applied after applying area id's using functions
        /// such as #rcMarkBoxArea, #rcMarkConvexPolyArea, and #rcMarkCylinderArea.
        /// 
        /// @see rcCompactHeightfield
        /// @ingroup recast
        /// 
        /// @param[in,out]	context		The build context to use during the operation.
        /// @param[in,out]	compactHeightfield		A populated compact heightfield.
        /// @returns True if the operation completed successfully.
        public static bool MedianFilterWalkableArea(RcTelemetry context, RcCompactHeightfield compactHeightfield)
        {
            int xSize = compactHeightfield.width;
            int zSize = compactHeightfield.height;
            int zStride = xSize; // For readability

            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_MEDIAN_AREA);

            int[] areas = new int[compactHeightfield.spanCount];

            for (int z = 0; z < zSize; ++z)
            {
                for (int x = 0; x < xSize; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;
                    for (int spanIndex = cell.index; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        RcCompactSpan span = compactHeightfield.spans[spanIndex];
                        if (compactHeightfield.areas[spanIndex] == RC_NULL_AREA)
                        {
                            areas[spanIndex] = compactHeightfield.areas[spanIndex];
                            continue;
                        }

                        int[] neighborAreas = new int[9];
                        for (int neighborIndex = 0; neighborIndex < 9; ++neighborIndex)
                        {
                            neighborAreas[neighborIndex] = compactHeightfield.areas[spanIndex];
                        }

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (RecastCommon.GetCon(span, dir) == RC_NOT_CONNECTED)
                            {
                                continue;
                            }

                            int aX = x + RecastCommon.GetDirOffsetX(dir);
                            int aZ = z + RecastCommon.GetDirOffsetY(dir);
                            int aIndex = compactHeightfield.cells[aX + aZ * zStride].index + RecastCommon.GetCon(span, dir);
                            if (compactHeightfield.areas[aIndex] != RC_NULL_AREA)
                            {
                                neighborAreas[dir * 2 + 0] = compactHeightfield.areas[aIndex];
                            }

                            RcCompactSpan aSpan = compactHeightfield.spans[aIndex];
                            int dir2 = (dir + 1) & 0x3;
                            int neighborConnection2 = RecastCommon.GetCon(aSpan, dir2);
                            if (neighborConnection2 != RC_NOT_CONNECTED)
                            {
                                int bX = aX + RecastCommon.GetDirOffsetX(dir2);
                                int bZ = aZ + RecastCommon.GetDirOffsetY(dir2);
                                int bIndex = compactHeightfield.cells[bX + bZ * zStride].index + RecastCommon.GetCon(aSpan, dir2);
                                if (compactHeightfield.areas[bIndex] != RC_NULL_AREA)
                                {
                                    neighborAreas[dir * 2 + 1] = compactHeightfield.areas[bIndex];
                                }
                            }
                        }

                        //Array.Sort(neighborAreas);
                        neighborAreas.InsertSort();
                        areas[spanIndex] = neighborAreas[4];
                    }
                }
            }

            compactHeightfield.areas = areas;

            return true;
        }

        /// Applies an area id to all spans within the specified bounding box. (AABB) 
        /// 
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        /// @ingroup recast
        /// 
        /// @param[in,out]	context				The build context to use during the operation.
        /// @param[in]		boxMinBounds		The minimum extents of the bounding box. [(x, y, z)] [Units: wu]
        /// @param[in]		boxMaxBounds		The maximum extents of the bounding box. [(x, y, z)] [Units: wu]
        /// @param[in]		areaId				The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
        /// @param[in,out]	compactHeightfield	A populated compact heightfield.
        public static void MarkBoxArea(RcTelemetry context, float[] boxMinBounds, float[] boxMaxBounds, RcAreaModification areaId, RcCompactHeightfield compactHeightfield)
        {
            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_BOX_AREA);

            int xSize = compactHeightfield.width;
            int zSize = compactHeightfield.height;
            int zStride = xSize; // For readability

            // Find the footprint of the box area in grid cell coordinates. 
            int minX = (int)((boxMinBounds[0] - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int minY = (int)((boxMinBounds[1] - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int minZ = (int)((boxMinBounds[2] - compactHeightfield.bmin.z) / compactHeightfield.cs);
            int maxX = (int)((boxMaxBounds[0] - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int maxY = (int)((boxMaxBounds[1] - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int maxZ = (int)((boxMaxBounds[2] - compactHeightfield.bmin.z) / compactHeightfield.cs);

            if (maxX < 0)
            {
                return;
            }

            if (minX >= xSize)
            {
                return;
            }

            if (maxZ < 0)
            {
                return;
            }

            if (minZ >= zSize)
            {
                return;
            }

            if (minX < 0)
            {
                minX = 0;
            }

            if (maxX >= xSize)
            {
                maxX = xSize - 1;
            }

            if (minZ < 0)
            {
                minZ = 0;
            }

            if (maxZ >= zSize)
            {
                maxZ = zSize - 1;
            }

            for (int z = minZ; z <= maxZ; ++z)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;
                    for (int spanIndex = cell.index; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        RcCompactSpan span = compactHeightfield.spans[spanIndex];

                        // Skip if the span is outside the box extents.
                        if (span.y < minY || span.y > maxY)
                        {
                            continue;
                        }

                        // Skip if the span has been removed.
                        if (compactHeightfield.areas[spanIndex] == RC_NULL_AREA)
                        {
                            continue;
                        }

                        // Mark the span.
                        compactHeightfield.areas[spanIndex] = areaId.Apply(compactHeightfield.areas[spanIndex]);
                    }
                }
            }
        }

        /// Applies the area id to the all spans within the specified convex polygon. 
        ///
        /// The value of spacial parameters are in world units.
        /// 
        /// The y-values of the polygon vertices are ignored. So the polygon is effectively 
        /// projected onto the xz-plane, translated to @p minY, and extruded to @p maxY.
        /// 
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        /// @ingroup recast
        /// 
        /// @param[in,out]	context				The build context to use during the operation.
        /// @param[in]		verts				The vertices of the polygon [For: (x, y, z) * @p numVerts]
        /// @param[in]		numVerts			The number of vertices in the polygon.
        /// @param[in]		minY				The height of the base of the polygon. [Units: wu]
        /// @param[in]		maxY				The height of the top of the polygon. [Units: wu]
        /// @param[in]		areaId				The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
        /// @param[in,out]	compactHeightfield	A populated compact heightfield.
        public static void MarkConvexPolyArea(RcTelemetry context, float[] verts,
            float minY, float maxY, RcAreaModification areaId,
            RcCompactHeightfield compactHeightfield)
        {
            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_CONVEXPOLY_AREA);

            int xSize = compactHeightfield.width;
            int zSize = compactHeightfield.height;
            int zStride = xSize; // For readability

            // Compute the bounding box of the polygon
            RcVec3f bmin = new RcVec3f();
            RcVec3f bmax = new RcVec3f();
            RcVec3f.Copy(ref bmin, verts, 0);
            RcVec3f.Copy(ref bmax, verts, 0);
            for (int i = 3; i < verts.Length; i += 3)
            {
                bmin.Min(verts, i);
                bmax.Max(verts, i);
            }

            bmin.y = minY;
            bmax.y = maxY;

            // Compute the grid footprint of the polygon 
            int minx = (int)((bmin.x - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int miny = (int)((bmin.y - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int minz = (int)((bmin.z - compactHeightfield.bmin.z) / compactHeightfield.cs);
            int maxx = (int)((bmax.x - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int maxy = (int)((bmax.y - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int maxz = (int)((bmax.z - compactHeightfield.bmin.z) / compactHeightfield.cs);

            // Early-out if the polygon lies entirely outside the grid.
            if (maxx < 0)
            {
                return;
            }

            if (minx >= xSize)
            {
                return;
            }

            if (maxz < 0)
            {
                return;
            }

            if (minz >= zSize)
            {
                return;
            }

            // Clamp the polygon footprint to the grid
            if (minx < 0)
            {
                minx = 0;
            }

            if (maxx >= xSize)
            {
                maxx = xSize - 1;
            }

            if (minz < 0)
            {
                minz = 0;
            }

            if (maxz >= zSize)
            {
                maxz = zSize - 1;
            }

            // TODO: Optimize.
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;
                    for (int spanIndex = cell.index; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        RcCompactSpan span = compactHeightfield.spans[spanIndex];

                        // Skip if span is removed.
                        if (compactHeightfield.areas[spanIndex] == RC_NULL_AREA)
                            continue;

                        // Skip if y extents don't overlap.
                        if (span.y < miny || span.y > maxy)
                        {
                            continue;
                        }

                        RcVec3f point = new RcVec3f(
                            compactHeightfield.bmin.x + (x + 0.5f) * compactHeightfield.cs,
                            0,
                            compactHeightfield.bmin.z + (z + 0.5f) * compactHeightfield.cs
                        );

                        if (PolyUtils.PointInPoly(verts, point))
                        {
                            compactHeightfield.areas[spanIndex] = areaId.Apply(compactHeightfield.areas[spanIndex]);
                        }
                    }
                }
            }
        }


        /// Applies the area id to all spans within the specified y-axis-aligned cylinder.
        /// 
        /// @see rcCompactHeightfield, rcMedianFilterWalkableArea
        /// 
        /// @ingroup recast
        /// 
        /// @param[in,out]	context				The build context to use during the operation.
        /// @param[in]		position			The center of the base of the cylinder. [Form: (x, y, z)] [Units: wu] 
        /// @param[in]		radius				The radius of the cylinder. [Units: wu] [Limit: > 0]
        /// @param[in]		height				The height of the cylinder. [Units: wu] [Limit: > 0]
        /// @param[in]		areaId				The area id to apply. [Limit: <= #RC_WALKABLE_AREA]
        /// @param[in,out]	compactHeightfield	A populated compact heightfield.
        public static void MarkCylinderArea(RcTelemetry context, float[] position, float radius, float height,
            RcAreaModification areaId, RcCompactHeightfield compactHeightfield)
        {
            using var timer = context.ScopedTimer(RcTimerLabel.RC_TIMER_MARK_CYLINDER_AREA);

            int xSize = compactHeightfield.width;
            int zSize = compactHeightfield.height;
            int zStride = xSize; // For readability

            // Compute the bounding box of the cylinder
            RcVec3f cylinderBBMin = new RcVec3f(
                position[0] - radius,
                position[1],
                position[2] - radius
            );

            RcVec3f cylinderBBMax = new RcVec3f(
                position[0] + radius,
                position[1] + height,
                position[2] + radius
            );

            // Compute the grid footprint of the cylinder
            int minx = (int)((cylinderBBMin.x - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int miny = (int)((cylinderBBMin.y - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int minz = (int)((cylinderBBMin.z - compactHeightfield.bmin.z) / compactHeightfield.cs);
            int maxx = (int)((cylinderBBMax.x - compactHeightfield.bmin.x) / compactHeightfield.cs);
            int maxy = (int)((cylinderBBMax.y - compactHeightfield.bmin.y) / compactHeightfield.ch);
            int maxz = (int)((cylinderBBMax.z - compactHeightfield.bmin.z) / compactHeightfield.cs);

            // Early-out if the cylinder is completely outside the grid bounds.
            if (maxx < 0)
            {
                return;
            }

            if (minx >= xSize)
            {
                return;
            }

            if (maxz < 0)
            {
                return;
            }

            if (minz >= zSize)
            {
                return;
            }

            // Clamp the cylinder bounds to the grid.
            if (minx < 0)
            {
                minx = 0;
            }

            if (maxx >= xSize)
            {
                maxx = xSize - 1;
            }

            if (minz < 0)
            {
                minz = 0;
            }

            if (maxz >= zSize)
            {
                maxz = zSize - 1;
            }

            float radiusSq = radius * radius;
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    RcCompactCell cell = compactHeightfield.cells[x + z * zStride];
                    int maxSpanIndex = cell.index + cell.count;

                    float cellX = compactHeightfield.bmin[0] + ((float)x + 0.5f) * compactHeightfield.cs;
                    float cellZ = compactHeightfield.bmin[2] + ((float)z + 0.5f) * compactHeightfield.cs;
                    float deltaX = cellX - position[0];
                    float deltaZ = cellZ - position[2];

                    // Skip this column if it's too far from the center point of the cylinder.
                    if (RcMath.Sqr(deltaX) + RcMath.Sqr(deltaZ) >= radiusSq)
                    {
                        continue;
                    }

                    // Mark all overlapping spans
                    for (int spanIndex = cell.index; spanIndex < maxSpanIndex; ++spanIndex)
                    {
                        RcCompactSpan span = compactHeightfield.spans[spanIndex];

                        // Skip if span is removed.
                        if (compactHeightfield.areas[spanIndex] == RC_NULL_AREA)
                        {
                            continue;
                        }

                        // Mark if y extents overlap.
                        if (span.y >= miny && span.y <= maxy)
                        {
                            compactHeightfield.areas[spanIndex] = areaId.Apply(compactHeightfield.areas[spanIndex]);
                        }
                    }
                }
            }
        }
    }
}