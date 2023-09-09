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

namespace DotRecast.Recast
{
    using static RcConstants;

    public static class RecastContour
    {
        private static int GetCornerHeight(int x, int y, int i, int dir, RcCompactHeightfield chf, out bool isBorderVertex)
        {
            isBorderVertex = false;

            RcCompactSpan s = chf.spans[i];
            int ch = s.y;
            int dirp = (dir + 1) & 0x3;

            int[] regs =
            {
                0, 0, 0, 0
            };

            // Combine region and area codes in order to prevent
            // border vertices which are in between two areas to be removed.
            regs[0] = chf.spans[i].reg | (chf.areas[i] << 16);

            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + RecastCommon.GetDirOffsetX(dir);
                int ay = y + RecastCommon.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(s, dir);
                RcCompactSpan @as = chf.spans[ai];
                ch = Math.Max(ch, @as.y);
                regs[1] = chf.spans[ai].reg | (chf.areas[ai] << 16);
                if (RecastCommon.GetCon(@as, dirp) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + RecastCommon.GetDirOffsetX(dirp);
                    int ay2 = ay + RecastCommon.GetDirOffsetY(dirp);
                    int ai2 = chf.cells[ax2 + ay2 * chf.width].index + RecastCommon.GetCon(@as, dirp);
                    RcCompactSpan as2 = chf.spans[ai2];
                    ch = Math.Max(ch, as2.y);
                    regs[2] = chf.spans[ai2].reg | (chf.areas[ai2] << 16);
                }
            }

            if (RecastCommon.GetCon(s, dirp) != RC_NOT_CONNECTED)
            {
                int ax = x + RecastCommon.GetDirOffsetX(dirp);
                int ay = y + RecastCommon.GetDirOffsetY(dirp);
                int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(s, dirp);
                RcCompactSpan @as = chf.spans[ai];
                ch = Math.Max(ch, @as.y);
                regs[3] = chf.spans[ai].reg | (chf.areas[ai] << 16);
                if (RecastCommon.GetCon(@as, dir) != RC_NOT_CONNECTED)
                {
                    int ax2 = ax + RecastCommon.GetDirOffsetX(dir);
                    int ay2 = ay + RecastCommon.GetDirOffsetY(dir);
                    int ai2 = chf.cells[ax2 + ay2 * chf.width].index + RecastCommon.GetCon(@as, dir);
                    RcCompactSpan as2 = chf.spans[ai2];
                    ch = Math.Max(ch, as2.y);
                    regs[2] = chf.spans[ai2].reg | (chf.areas[ai2] << 16);
                }
            }

            // Check if the vertex is special edge vertex, these vertices will be removed later.
            for (int j = 0; j < 4; ++j)
            {
                int a = j;
                int b = (j + 1) & 0x3;
                int c = (j + 2) & 0x3;
                int d = (j + 3) & 0x3;

                // The vertex is a border vertex there are two same exterior cells in a row,
                // followed by two interior cells and none of the regions are out of bounds.
                bool twoSameExts = (regs[a] & regs[b] & RC_BORDER_REG) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & RC_BORDER_REG) == 0;
                bool intsSameArea = (regs[c] >> 16) == (regs[d] >> 16);
                bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
                if (twoSameExts && twoInts && intsSameArea && noZeros)
                {
                    isBorderVertex = true;
                    break;
                }
            }

            return ch;
        }

        private static void WalkContour(int x, int y, int i, RcCompactHeightfield chf, int[] flags, List<int> points)
        {
            // Choose the first non-connected edge
            int dir = 0;
            while ((flags[i] & (1 << dir)) == 0)
                dir++;

            int startDir = dir;
            int starti = i;

            int area = chf.areas[i];

            int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) != 0)
                {
                    // Choose the edge corner
                    bool isBorderVertex = false;
                    bool isAreaBorder = false;
                    int px = x;
                    int py = GetCornerHeight(x, y, i, dir, chf, out isBorderVertex);
                    int pz = y;
                    switch (dir)
                    {
                        case 0:
                            pz++;
                            break;
                        case 1:
                            px++;
                            pz++;
                            break;
                        case 2:
                            px++;
                            break;
                    }

                    int r = 0;
                    RcCompactSpan s = chf.spans[i];
                    if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + RecastCommon.GetDirOffsetX(dir);
                        int ay = y + RecastCommon.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(s, dir);
                        r = chf.spans[ai].reg;
                        if (area != chf.areas[ai])
                            isAreaBorder = true;
                    }

                    if (isBorderVertex)
                        r |= RC_BORDER_VERTEX;
                    if (isAreaBorder)
                        r |= RC_AREA_BORDER;
                    points.Add(px);
                    points.Add(py);
                    points.Add(pz);
                    points.Add(r);

                    flags[i] &= ~(1 << dir); // Remove visited edges
                    dir = (dir + 1) & 0x3; // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + RecastCommon.GetDirOffsetX(dir);
                    int ny = y + RecastCommon.GetDirOffsetY(dir);
                    RcCompactSpan s = chf.spans[i];
                    if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        RcCompactCell nc = chf.cells[nx + ny * chf.width];
                        ni = nc.index + RecastCommon.GetCon(s, dir);
                    }

                    if (ni == -1)
                    {
                        // Should not happen.
                        return;
                    }

                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3; // Rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }
        }

        private static float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
        {
            float pqx = qx - px;
            float pqz = qz - pz;
            float dx = x - px;
            float dz = z - pz;
            float d = pqx * pqx + pqz * pqz;
            float t = pqx * dx + pqz * dz;
            if (d > 0)
                t /= d;
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

            dx = px + t * pqx - x;
            dz = pz + t * pqz - z;

            return dx * dx + dz * dz;
        }

        private static void SimplifyContour(List<int> points, List<int> simplified, float maxError, int maxEdgeLen, int buildFlags)
        {
            // Add initial points.
            bool hasConnections = false;
            for (int i = 0; i < points.Count; i += 4)
            {
                if ((points[i + 3] & RC_CONTOUR_REG_MASK) != 0)
                {
                    hasConnections = true;
                    break;
                }
            }

            if (hasConnections)
            {
                // The contour has some portals to other regions.
                // Add a new point to every location where the region changes.
                for (int i = 0, ni = points.Count / 4; i < ni; ++i)
                {
                    int ii = (i + 1) % ni;
                    bool differentRegs = (points[i * 4 + 3] & RC_CONTOUR_REG_MASK) != (points[ii * 4 + 3] & RC_CONTOUR_REG_MASK);
                    bool areaBorders = (points[i * 4 + 3] & RC_AREA_BORDER) != (points[ii * 4 + 3] & RC_AREA_BORDER);
                    if (differentRegs || areaBorders)
                    {
                        simplified.Add(points[i * 4 + 0]);
                        simplified.Add(points[i * 4 + 1]);
                        simplified.Add(points[i * 4 + 2]);
                        simplified.Add(i);
                    }
                }
            }

            if (simplified.Count == 0)
            {
                // If there is no connections at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                int llx = points[0];
                int lly = points[1];
                int llz = points[2];
                int lli = 0;
                int urx = points[0];
                int ury = points[1];
                int urz = points[2];
                int uri = 0;
                for (int i = 0; i < points.Count; i += 4)
                {
                    int x = points[i + 0];
                    int y = points[i + 1];
                    int z = points[i + 2];
                    if (x < llx || (x == llx && z < llz))
                    {
                        llx = x;
                        lly = y;
                        llz = z;
                        lli = i / 4;
                    }

                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        ury = y;
                        urz = z;
                        uri = i / 4;
                    }
                }

                simplified.Add(llx);
                simplified.Add(lly);
                simplified.Add(llz);
                simplified.Add(lli);

                simplified.Add(urx);
                simplified.Add(ury);
                simplified.Add(urz);
                simplified.Add(uri);
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            int pn = points.Count / 4;
            for (int i = 0; i < simplified.Count / 4;)
            {
                int ii = (i + 1) % (simplified.Count / 4);

                int ax = simplified[i * 4 + 0];
                int az = simplified[i * 4 + 2];
                int ai = simplified[i * 4 + 3];

                int bx = simplified[ii * 4 + 0];
                int bz = simplified[ii * 4 + 2];
                int bi = simplified[ii * 4 + 3];

                // Find maximum deviation from the segment.
                float maxd = 0;
                int maxi = -1;
                int ci, cinc, endi;

                // Traverse the segment in lexilogical order so that the
                // max deviation is calculated similarly when traversing
                // opposite segments.
                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc) % pn;
                    endi = bi;
                }
                else
                {
                    cinc = pn - 1;
                    ci = (bi + cinc) % pn;
                    endi = ai;
                    int temp = ax;
                    ax = bx;
                    bx = temp;
                    temp = az;
                    az = bz;
                    bz = temp;
                }

                // Tessellate only outer edges or edges between areas.
                if ((points[ci * 4 + 3] & RC_CONTOUR_REG_MASK) == 0 || (points[ci * 4 + 3] & RC_AREA_BORDER) != 0)
                {
                    while (ci != endi)
                    {
                        float d = DistancePtSeg(points[ci * 4 + 0], points[ci * 4 + 2], ax, az, bx, bz);
                        if (d > maxd)
                        {
                            maxd = d;
                            maxi = ci;
                        }

                        ci = (ci + cinc) % pn;
                    }
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    // Add the point.
                    simplified.Insert((i + 1) * 4 + 0, points[maxi * 4 + 0]);
                    simplified.Insert((i + 1) * 4 + 1, points[maxi * 4 + 1]);
                    simplified.Insert((i + 1) * 4 + 2, points[maxi * 4 + 2]);
                    simplified.Insert((i + 1) * 4 + 3, maxi);
                }
                else
                {
                    ++i;
                }
            }

            // Split too long edges.
            if (maxEdgeLen > 0 && (buildFlags & (RC_CONTOUR_TESS_WALL_EDGES | RC_CONTOUR_TESS_AREA_EDGES)) != 0)
            {
                for (int i = 0; i < simplified.Count / 4;)
                {
                    int ii = (i + 1) % (simplified.Count / 4);

                    int ax = simplified[i * 4 + 0];
                    int az = simplified[i * 4 + 2];
                    int ai = simplified[i * 4 + 3];

                    int bx = simplified[ii * 4 + 0];
                    int bz = simplified[ii * 4 + 2];
                    int bi = simplified[ii * 4 + 3];

                    // Find maximum deviation from the segment.
                    int maxi = -1;
                    int ci = (ai + 1) % pn;

                    // Tessellate only outer edges or edges between areas.
                    bool tess = false;
                    // Wall edges.
                    if ((buildFlags & RC_CONTOUR_TESS_WALL_EDGES) != 0 && (points[ci * 4 + 3] & RC_CONTOUR_REG_MASK) == 0)
                        tess = true;
                    // Edges between areas.
                    if ((buildFlags & RC_CONTOUR_TESS_AREA_EDGES) != 0 && (points[ci * 4 + 3] & RC_AREA_BORDER) != 0)
                        tess = true;

                    if (tess)
                    {
                        int dx = bx - ax;
                        int dz = bz - az;
                        if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                        {
                            // Round based on the segments in lexilogical order so that the
                            // max tesselation is consistent regardless in which direction
                            // segments are traversed.
                            int n = bi < ai ? (bi + pn - ai) : (bi - ai);
                            if (n > 1)
                            {
                                if (bx > ax || (bx == ax && bz > az))
                                    maxi = (ai + n / 2) % pn;
                                else
                                    maxi = (ai + (n + 1) / 2) % pn;
                            }
                        }
                    }

                    // If the max deviation is larger than accepted error,
                    // add new point, else continue to next segment.
                    if (maxi != -1)
                    {
                        // Add the point.
                        simplified.Insert((i + 1) * 4 + 0, points[maxi * 4 + 0]);
                        simplified.Insert((i + 1) * 4 + 1, points[maxi * 4 + 1]);
                        simplified.Insert((i + 1) * 4 + 2, points[maxi * 4 + 2]);
                        simplified.Insert((i + 1) * 4 + 3, maxi);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }

            for (int i = 0; i < simplified.Count / 4; ++i)
            {
                // The edge vertex flag is take from the current raw point,
                // and the neighbour region is take from the next raw point.
                int ai = (simplified[i * 4 + 3] + 1) % pn;
                int bi = simplified[i * 4 + 3];
                simplified[i * 4 + 3] = (points[ai * 4 + 3] & (RC_CONTOUR_REG_MASK | RC_AREA_BORDER))
                                        | points[bi * 4 + 3] & RC_BORDER_VERTEX;
            }
        }

        private static int CalcAreaOfPolygon2D(int[] verts, int nverts)
        {
            int area = 0;
            for (int i = 0, j = nverts - 1; i < nverts; j = i++)
            {
                int vi = i * 4;
                int vj = j * 4;
                area += verts[vi + 0] * verts[vj + 2] - verts[vj + 0] * verts[vi + 2];
            }

            return (area + 1) / 2;
        }

        private static bool IntersectSegContour(int d0, int d1, int i, int n, int[] verts, int[] d0verts, int[] d1verts)
        {
            // For each edge (k,k+1) of P
            int[] pverts = new int[4 * 4];
            for (int g = 0; g < 4; g++)
            {
                pverts[g] = d0verts[d0 + g];
                pverts[4 + g] = d1verts[d1 + g];
            }

            d0 = 0;
            d1 = 4;
            for (int k = 0; k < n; k++)
            {
                int k1 = RecastMesh.Next(k, n);
                // Skip edges incident to i.
                if (i == k || i == k1)
                    continue;
                int p0 = k * 4;
                int p1 = k1 * 4;
                for (int g = 0; g < 4; g++)
                {
                    pverts[8 + g] = verts[p0 + g];
                    pverts[12 + g] = verts[p1 + g];
                }

                p0 = 8;
                p1 = 12;
                if (RecastMesh.VEqual(pverts, d0, p0) || RecastMesh.VEqual(pverts, d1, p0) ||
                    RecastMesh.VEqual(pverts, d0, p1) || RecastMesh.VEqual(pverts, d1, p1))
                    continue;

                if (RecastMesh.Intersect(pverts, d0, d1, p0, p1))
                    return true;
            }

            return false;
        }

        private static bool InCone(int i, int n, int[] verts, int pj, int[] vertpj)
        {
            int pi = i * 4;
            int pi1 = RecastMesh.Next(i, n) * 4;
            int pin1 = RecastMesh.Prev(i, n) * 4;
            int[] pverts = new int[4 * 4];
            for (int g = 0; g < 4; g++)
            {
                pverts[g] = verts[pi + g];
                pverts[4 + g] = verts[pi1 + g];
                pverts[8 + g] = verts[pin1 + g];
                pverts[12 + g] = vertpj[pj + g];
            }

            pi = 0;
            pi1 = 4;
            pin1 = 8;
            pj = 12;
            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (RecastMesh.LeftOn(pverts, pin1, pi, pi1))
                return RecastMesh.Left(pverts, pi, pj, pin1) && RecastMesh.Left(pverts, pj, pi, pi1);
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(RecastMesh.LeftOn(pverts, pi, pj, pi1) && RecastMesh.LeftOn(pverts, pj, pi, pin1));
        }

        private static void RemoveDegenerateSegments(List<int> simplified)
        {
            // Remove adjacent vertices which are equal on xz-plane,
            // or else the triangulator will get confused.
            int npts = simplified.Count / 4;
            for (int i = 0; i < npts; ++i)
            {
                int ni = RecastMesh.Next(i, npts);

                // if (Vequal(&simplified[i*4], &simplified[ni*4]))
                if (simplified[i * 4] == simplified[ni * 4]
                    && simplified[i * 4 + 2] == simplified[ni * 4 + 2])
                {
                    // Degenerate segment, remove.
                    simplified.RemoveAt(i * 4);
                    simplified.RemoveAt(i * 4);
                    simplified.RemoveAt(i * 4);
                    simplified.RemoveAt(i * 4);
                    npts--;
                }
            }
        }

        private static void MergeContours(RcContour ca, RcContour cb, int ia, int ib)
        {
            int maxVerts = ca.nverts + cb.nverts + 2;
            int[] verts = new int[maxVerts * 4];

            int nv = 0;

            // Copy contour A.
            for (int i = 0; i <= ca.nverts; ++i)
            {
                int dst = nv * 4;
                int src = ((ia + i) % ca.nverts) * 4;
                verts[dst + 0] = ca.verts[src + 0];
                verts[dst + 1] = ca.verts[src + 1];
                verts[dst + 2] = ca.verts[src + 2];
                verts[dst + 3] = ca.verts[src + 3];
                nv++;
            }

            // Copy contour B
            for (int i = 0; i <= cb.nverts; ++i)
            {
                int dst = nv * 4;
                int src = ((ib + i) % cb.nverts) * 4;
                verts[dst + 0] = cb.verts[src + 0];
                verts[dst + 1] = cb.verts[src + 1];
                verts[dst + 2] = cb.verts[src + 2];
                verts[dst + 3] = cb.verts[src + 3];
                nv++;
            }

            ca.verts = verts;
            ca.nverts = nv;

            cb.verts = null;
            cb.nverts = 0;
        }

        // Finds the lowest leftmost vertex of a contour.
        private static int[] FindLeftMostVertex(RcContour contour)
        {
            int minx = contour.verts[0];
            int minz = contour.verts[2];
            int leftmost = 0;
            for (int i = 1; i < contour.nverts; i++)
            {
                int x = contour.verts[i * 4 + 0];
                int z = contour.verts[i * 4 + 2];
                if (x < minx || (x == minx && z < minz))
                {
                    minx = x;
                    minz = z;
                    leftmost = i;
                }
            }

            return new int[] { minx, minz, leftmost };
        }

        private static void MergeRegionHoles(RcTelemetry ctx, RcContourRegion region)
        {
            // Sort holes from left to right.
            for (int i = 0; i < region.nholes; i++)
            {
                int[] minleft = FindLeftMostVertex(region.holes[i].contour);
                region.holes[i].minx = minleft[0];
                region.holes[i].minz = minleft[1];
                region.holes[i].leftmost = minleft[2];
            }

            Array.Sort(region.holes, RcContourHoleComparer.Shared);

            int maxVerts = region.outline.nverts;
            for (int i = 0; i < region.nholes; i++)
                maxVerts += region.holes[i].contour.nverts;

            RcPotentialDiagonal[] diags = new RcPotentialDiagonal[maxVerts];
            for (int pd = 0; pd < maxVerts; pd++)
            {
                diags[pd] = new RcPotentialDiagonal();
            }

            RcContour outline = region.outline;

            // Merge holes into the outline one by one.
            for (int i = 0; i < region.nholes; i++)
            {
                RcContour hole = region.holes[i].contour;

                int index = -1;
                int bestVertex = region.holes[i].leftmost;
                for (int iter = 0; iter < hole.nverts; iter++)
                {
                    // Find potential diagonals.
                    // The 'best' vertex must be in the cone described by 3 consecutive vertices of the outline.
                    // ..o j-1
                    // |
                    // | * best
                    // |
                    // j o-----o j+1
                    // :
                    int ndiags = 0;
                    int corner = bestVertex * 4;
                    for (int j = 0; j < outline.nverts; j++)
                    {
                        if (InCone(j, outline.nverts, outline.verts, corner, hole.verts))
                        {
                            int dx = outline.verts[j * 4 + 0] - hole.verts[corner + 0];
                            int dz = outline.verts[j * 4 + 2] - hole.verts[corner + 2];
                            diags[ndiags].vert = j;
                            diags[ndiags].dist = dx * dx + dz * dz;
                            ndiags++;
                        }
                    }

                    // Sort potential diagonals by distance, we want to make the connection as short as possible.
                    Array.Sort(diags, 0, ndiags, RcPotentialDiagonalComparer.Shared);

                    // Find a diagonal that is not intersecting the outline not the remaining holes.
                    index = -1;
                    for (int j = 0; j < ndiags; j++)
                    {
                        int pt = diags[j].vert * 4;
                        bool intersect = IntersectSegContour(pt, corner, diags[j].vert, outline.nverts, outline.verts,
                            outline.verts, hole.verts);
                        for (int k = i; k < region.nholes && !intersect; k++)
                            intersect |= IntersectSegContour(pt, corner, -1, region.holes[k].contour.nverts,
                                region.holes[k].contour.verts, outline.verts, hole.verts);
                        if (!intersect)
                        {
                            index = diags[j].vert;
                            break;
                        }
                    }

                    // If found non-intersecting diagonal, stop looking.
                    if (index != -1)
                        break;
                    // All the potential diagonals for the current vertex were intersecting, try next vertex.
                    bestVertex = (bestVertex + 1) % hole.nverts;
                }

                if (index == -1)
                {
                    ctx.Warn("mergeHoles: Failed to find merge points for");
                    continue;
                }

                MergeContours(region.outline, hole, index, bestVertex);
            }
        }

        /// @par
        ///
        /// The raw contours will match the region outlines exactly. The @p maxError and @p maxEdgeLen
        /// parameters control how closely the simplified contours will match the raw contours.
        ///
        /// Simplified contours are generated such that the vertices for portals between areas match up.
        /// (They are considered mandatory vertices.)
        ///
        /// Setting @p maxEdgeLength to zero will disabled the edge length feature.
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// @see rcAllocContourSet, rcCompactHeightfield, rcContourSet, rcConfig
        public static RcContourSet BuildContours(RcTelemetry ctx, RcCompactHeightfield chf, float maxError, int maxEdgeLen,
            int buildFlags)
        {
            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;
            RcContourSet cset = new RcContourSet();

            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS);

            cset.bmin = chf.bmin;
            cset.bmax = chf.bmax;
            if (borderSize > 0)
            {
                // If the heightfield was build with bordersize, remove the offset.
                float pad = borderSize * chf.cs;
                cset.bmin.x += pad;
                cset.bmin.z += pad;
                cset.bmax.x -= pad;
                cset.bmax.z -= pad;
            }

            cset.cs = chf.cs;
            cset.ch = chf.ch;
            cset.width = chf.width - chf.borderSize * 2;
            cset.height = chf.height - chf.borderSize * 2;
            cset.borderSize = chf.borderSize;
            cset.maxError = maxError;

            int[] flags = new int[chf.spanCount];

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);

            // Mark boundaries.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        int res = 0;
                        RcCompactSpan s = chf.spans[i];
                        if (chf.spans[i].reg == 0 || (chf.spans[i].reg & RC_BORDER_REG) != 0)
                        {
                            flags[i] = 0;
                            continue;
                        }

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            int r = 0;
                            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastCommon.GetDirOffsetX(dir);
                                int ay = y + RecastCommon.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                                r = chf.spans[ai].reg;
                            }

                            if (r == chf.spans[i].reg)
                                res |= (1 << dir);
                        }

                        flags[i] = res ^ 0xf; // Inverse, mark non connected edges.
                    }
                }
            }

            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_TRACE);

            List<int> verts = new List<int>(256);
            List<int> simplified = new List<int>(64);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (flags[i] == 0 || flags[i] == 0xf)
                        {
                            flags[i] = 0;
                            continue;
                        }

                        int reg = chf.spans[i].reg;
                        if (reg == 0 || (reg & RC_BORDER_REG) != 0)
                            continue;
                        int area = chf.areas[i];

                        verts.Clear();
                        simplified.Clear();

                        ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_WALK);
                        WalkContour(x, y, i, chf, flags, verts);
                        ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_WALK);

                        ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_SIMPLIFY);
                        SimplifyContour(verts, simplified, maxError, maxEdgeLen, buildFlags);
                        RemoveDegenerateSegments(simplified);
                        ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_CONTOURS_SIMPLIFY);

                        // Store region->contour remap info.
                        // Create contour.
                        if (simplified.Count / 4 >= 3)
                        {
                            RcContour cont = new RcContour();
                            cset.conts.Add(cont);

                            cont.nverts = simplified.Count / 4;
                            cont.verts = new int[simplified.Count];
                            for (int l = 0; l < cont.verts.Length; l++)
                            {
                                cont.verts[l] = simplified[l];
                            }

                            if (borderSize > 0)
                            {
                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nverts; ++j)
                                {
                                    cont.verts[j * 4] -= borderSize;
                                    cont.verts[j * 4 + 2] -= borderSize;
                                }
                            }

                            cont.nrverts = verts.Count / 4;
                            cont.rverts = new int[verts.Count];
                            for (int l = 0; l < cont.rverts.Length; l++)
                            {
                                cont.rverts[l] = verts[l];
                            }

                            if (borderSize > 0)
                            {
                                // If the heightfield was build with bordersize, remove the offset.
                                for (int j = 0; j < cont.nrverts; ++j)
                                {
                                    cont.rverts[j * 4] -= borderSize;
                                    cont.rverts[j * 4 + 2] -= borderSize;
                                }
                            }

                            cont.reg = reg;
                            cont.area = area;
                        }
                    }
                }
            }

            // Merge holes if needed.
            if (cset.conts.Count > 0)
            {
                // Calculate winding of all polygons.
                int[] winding = new int[cset.conts.Count];
                int nholes = 0;
                for (int i = 0; i < cset.conts.Count; ++i)
                {
                    RcContour cont = cset.conts[i];
                    // If the contour is wound backwards, it is a hole.
                    winding[i] = CalcAreaOfPolygon2D(cont.verts, cont.nverts) < 0 ? -1 : 1;
                    if (winding[i] < 0)
                        nholes++;
                }

                if (nholes > 0)
                {
                    // Collect outline contour and holes contours per region.
                    // We assume that there is one outline and multiple holes.
                    int nregions = chf.maxRegions + 1;
                    RcContourRegion[] regions = new RcContourRegion[nregions];
                    for (int i = 0; i < nregions; i++)
                    {
                        regions[i] = new RcContourRegion();
                    }

                    for (int i = 0; i < cset.conts.Count; ++i)
                    {
                        RcContour cont = cset.conts[i];
                        // Positively would contours are outlines, negative holes.
                        if (winding[i] > 0)
                        {
                            if (regions[cont.reg].outline != null)
                            {
                                throw new Exception(
                                    "rcBuildContours: Multiple outlines for region " + cont.reg + ".");
                            }

                            regions[cont.reg].outline = cont;
                        }
                        else
                        {
                            regions[cont.reg].nholes++;
                        }
                    }

                    for (int i = 0; i < nregions; i++)
                    {
                        if (regions[i].nholes > 0)
                        {
                            regions[i].holes = new RcContourHole[regions[i].nholes];
                            for (int nh = 0; nh < regions[i].nholes; nh++)
                            {
                                regions[i].holes[nh] = new RcContourHole();
                            }

                            regions[i].nholes = 0;
                        }
                    }

                    for (int i = 0; i < cset.conts.Count; ++i)
                    {
                        RcContour cont = cset.conts[i];
                        RcContourRegion reg = regions[cont.reg];
                        if (winding[i] < 0)
                            reg.holes[reg.nholes++].contour = cont;
                    }

                    // Finally merge each regions holes into the outline.
                    for (int i = 0; i < nregions; i++)
                    {
                        RcContourRegion reg = regions[i];
                        if (reg.nholes == 0)
                            continue;

                        if (reg.outline != null)
                        {
                            MergeRegionHoles(ctx, reg);
                        }
                        else
                        {
                            // The region does not have an outline.
                            // This can happen if the contour becaomes selfoverlapping because of
                            // too aggressive simplification settings.
                            throw new Exception("rcBuildContours: Bad outline for region " + i
                                                                                           + ", contour simplification is likely too aggressive.");
                        }
                    }
                }
            }

            return cset;
        }
    }
}