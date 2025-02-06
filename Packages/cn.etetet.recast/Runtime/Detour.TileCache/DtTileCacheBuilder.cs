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
using System.IO;
using DotRecast.Core;
using DotRecast.Detour.TileCache.Io;
using DotRecast.Detour.TileCache.Io.Compress;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.TileCache
{
    public class DtTileCacheBuilder
    {
        public const int DT_TILECACHE_NULL_AREA = 0;
        public const int DT_TILECACHE_WALKABLE_AREA = 63;
        public const int DT_TILECACHE_NULL_IDX = 0xffff;
        private static readonly int[] DirOffsetX = { -1, 0, 1, 0, };
        private static readonly int[] DirOffsetY = { 0, 1, 0, -1 };

        private readonly DtTileCacheLayerHeaderReader reader = new DtTileCacheLayerHeaderReader();

        public void BuildTileCacheRegions(DtTileCacheLayer layer, int walkableClimb)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            Array.Fill(layer.regs, (short)0x00FF);
            int nsweeps = w;
            DtLayerSweepSpan[] sweeps = new DtLayerSweepSpan[nsweeps];
            for (int i = 0; i < sweeps.Length; i++)
            {
                sweeps[i] = new DtLayerSweepSpan();
            }

            // Partition walkable area into monotone regions.
            int[] prevCount = new int[256];
            int regId = 0;

            for (int y = 0; y < h; ++y)
            {
                if (regId > 0)
                {
                    Array.Fill(prevCount, 0, 0, regId);
                }

                // Memset(prevCount,0,Sizeof(char)*regId);
                int sweepId = 0;

                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layer.areas[idx] == DT_TILECACHE_NULL_AREA)
                        continue;

                    int sid = 0xff;

                    // -x
                    int xidx = (x - 1) + y * w;
                    if (x > 0 && IsConnected(layer, idx, xidx, walkableClimb))
                    {
                        if (layer.regs[xidx] != 0xff)
                            sid = layer.regs[xidx];
                    }

                    if (sid == 0xff)
                    {
                        sid = sweepId++;
                        sweeps[sid].nei = 0xff;
                        sweeps[sid].ns = 0;
                    }

                    // -y
                    int yidx = x + (y - 1) * w;
                    if (y > 0 && IsConnected(layer, idx, yidx, walkableClimb))
                    {
                        int nr = layer.regs[yidx];
                        if (nr != 0xff)
                        {
                            // Set neighbour when first valid neighbour is
                            // encoutered.
                            if (sweeps[sid].ns == 0)
                                sweeps[sid].nei = nr;

                            if (sweeps[sid].nei == nr)
                            {
                                // Update existing neighbour
                                sweeps[sid].ns++;
                                prevCount[nr]++;
                            }
                            else
                            {
                                // This is hit if there is nore than one neighbour.
                                // Invalidate the neighbour.
                                sweeps[sid].nei = 0xff;
                            }
                        }
                    }

                    layer.regs[idx] = (byte)sid;
                }

                // Create unique ID.
                for (int i = 0; i < sweepId; ++i)
                {
                    // If the neighbour is set and there is only one continuous
                    // connection to it,
                    // the sweep will be merged with the previous one, else new
                    // region is created.
                    if (sweeps[i].nei != 0xff && prevCount[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        if (regId == 255)
                        {
                            // Region ID's overflow.
                            throw new Exception("Buffer too small");
                        }

                        sweeps[i].id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    if (layer.regs[idx] != 0xff)
                        layer.regs[idx] = (short)sweeps[layer.regs[idx]].id;
                }
            }

            // Allocate and init layer regions.
            int nregs = regId;
            DtLayerMonotoneRegion[] regs = new DtLayerMonotoneRegion[nregs];

            for (int i = 0; i < nregs; ++i)
            {
                regs[i] = new DtLayerMonotoneRegion();
                regs[i].regId = 0xff;
            }

            // Find region neighbours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = layer.regs[idx];
                    if (ri == 0xff)
                        continue;

                    // Update area.
                    regs[ri].area++;
                    regs[ri].areaId = layer.areas[idx];

                    // Update neighbours
                    int ymi = x + (y - 1) * w;
                    if (y > 0 && IsConnected(layer, idx, ymi, walkableClimb))
                    {
                        int rai = layer.regs[ymi];
                        if (rai != 0xff && rai != ri)
                        {
                            AddUniqueLast(regs[ri].neis, rai);
                            AddUniqueLast(regs[rai].neis, ri);
                        }
                    }
                }
            }

            for (int i = 0; i < nregs; ++i)
                regs[i].regId = i;

            for (int i = 0; i < nregs; ++i)
            {
                DtLayerMonotoneRegion reg = regs[i];

                int merge = -1;
                int mergea = 0;
                foreach (int nei in reg.neis)
                {
                    DtLayerMonotoneRegion regn = regs[nei];
                    if (reg.regId == regn.regId)
                        continue;
                    if (reg.areaId != regn.areaId)
                        continue;
                    if (regn.area > mergea)
                    {
                        if (CanMerge(reg.regId, regn.regId, regs, nregs))
                        {
                            mergea = regn.area;
                            merge = nei;
                        }
                    }
                }

                if (merge != -1)
                {
                    int oldId = reg.regId;
                    int newId = regs[merge].regId;
                    for (int j = 0; j < nregs; ++j)
                        if (regs[j].regId == oldId)
                            regs[j].regId = newId;
                }
            }

            // Compact ids.
            int[] remap = new int[256];
            // Find number of unique regions.
            regId = 0;
            for (int i = 0; i < nregs; ++i)
                remap[regs[i].regId] = 1;
            for (int i = 0; i < 256; ++i)
                if (remap[i] != 0)
                    remap[i] = regId++;
            // Remap ids.
            for (int i = 0; i < nregs; ++i)
                regs[i].regId = remap[regs[i].regId];

            layer.regCount = regId;

            for (int i = 0; i < w * h; ++i)
            {
                if (layer.regs[i] != 0xff)
                    layer.regs[i] = (short)regs[layer.regs[i]].regId;
            }
        }

        void AddUniqueLast(List<int> a, int v)
        {
            int n = a.Count;
            if (n > 0 && a[n - 1] == v)
                return;
            a.Add(v);
        }

        bool IsConnected(DtTileCacheLayer layer, int ia, int ib, int walkableClimb)
        {
            if (layer.areas[ia] != layer.areas[ib])
                return false;
            if (Math.Abs(layer.heights[ia] - layer.heights[ib]) > walkableClimb)
                return false;
            return true;
        }

        bool CanMerge(int oldRegId, int newRegId, DtLayerMonotoneRegion[] regs, int nregs)
        {
            int count = 0;
            for (int i = 0; i < nregs; ++i)
            {
                DtLayerMonotoneRegion reg = regs[i];
                if (reg.regId != oldRegId)
                    continue;
                foreach (int nei in reg.neis)
                {
                    if (regs[nei].regId == newRegId)
                        count++;
                }
            }

            return count == 1;
        }

        private void AppendVertex(DtTempContour cont, int x, int y, int z, int r)
        {
            // Try to merge with existing segments.
            if (cont.nverts > 1)
            {
                int pa = (cont.nverts - 2) * 4;
                int pb = (cont.nverts - 1) * 4;
                if (cont.verts[pb + 3] == r)
                {
                    if (cont.verts[pa] == cont.verts[pb] && cont.verts[pb] == x)
                    {
                        // The verts are aligned aling x-axis, update z.
                        cont.verts[pb + 1] = y;
                        cont.verts[pb + 2] = z;
                        return;
                    }
                    else if (cont.verts[pa + 2] == cont.verts[pb + 2]
                             && cont.verts[pb + 2] == z)
                    {
                        // The verts are aligned aling z-axis, update x.
                        cont.verts[pb] = x;
                        cont.verts[pb + 1] = y;
                        return;
                    }
                }
            }

            cont.verts.Add(x);
            cont.verts.Add(y);
            cont.verts.Add(z);
            cont.verts.Add(r);
            cont.nverts++;
        }

        private int GetNeighbourReg(DtTileCacheLayer layer, int ax, int ay, int dir)
        {
            int w = layer.header.width;
            int ia = ax + ay * w;

            int con = layer.cons[ia] & 0xf;
            int portal = layer.cons[ia] >> 4;
            int mask = 1 << dir;

            if ((con & mask) == 0)
            {
                // No connection, return portal or hard edge.
                if ((portal & mask) != 0)
                    return 0xf8 + dir;
                return 0xff;
            }

            int bx = ax + GetDirOffsetX(dir);
            int by = ay + GetDirOffsetY(dir);
            int ib = bx + by * w;
            return layer.regs[ib];
        }

        private int GetDirOffsetX(int dir)
        {
            return DirOffsetX[dir & 0x03];
        }

        private int GetDirOffsetY(int dir)
        {
            return DirOffsetY[dir & 0x03];
        }

        private void WalkContour(DtTileCacheLayer layer, int x, int y, DtTempContour cont)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            cont.Clear();

            int startX = x;
            int startY = y;
            int startDir = -1;

            for (int i = 0; i < 4; ++i)
            {
                int ndir = (i + 3) & 3;
                int rn = GetNeighbourReg(layer, x, y, ndir);
                if (rn != layer.regs[x + y * w])
                {
                    startDir = ndir;
                    break;
                }
            }

            if (startDir == -1)
                return;

            int dir = startDir;
            int maxIter = w * h;
            int iter = 0;
            while (iter < maxIter)
            {
                int rn = GetNeighbourReg(layer, x, y, dir);

                int nx = x;
                int ny = y;
                int ndir = dir;

                if (rn != layer.regs[x + y * w])
                {
                    // Solid edge.
                    int px = x;
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

                    // Try to merge with previous vertex.
                    AppendVertex(cont, px, layer.heights[x + y * w], pz, rn);
                    ndir = (dir + 1) & 0x3; // Rotate CW
                }
                else
                {
                    // Move to next.
                    nx = x + GetDirOffsetX(dir);
                    ny = y + GetDirOffsetY(dir);
                    ndir = (dir + 3) & 0x3; // Rotate CCW
                }

                if (iter > 0 && x == startX && y == startY && dir == startDir)
                    break;

                x = nx;
                y = ny;
                dir = ndir;

                iter++;
            }

            // Remove last vertex if it is duplicate of the first one.
            int pa = (cont.nverts - 1) * 4;
            int pb = 0;
            if (cont.verts[pa] == cont.verts[pb]
                && cont.verts[pa + 2] == cont.verts[pb + 2])
                cont.nverts--;
        }

        private float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
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

        private void SimplifyContour(DtTempContour cont, float maxError)
        {
            cont.poly.Clear();

            for (int i = 0; i < cont.nverts; ++i)
            {
                int j = (i + 1) % cont.nverts;
                // Check for start of a wall segment.
                int ra = j * 4 + 3;
                int rb = i * 4 + 3;
                if (cont.verts[ra] != cont.verts[rb])
                    cont.poly.Add(i);
            }

            if (cont.Npoly() < 2)
            {
                // If there is no transitions at all,
                // create some initial points for the simplification process.
                // Find lower-left and upper-right vertices of the contour.
                int llx = cont.verts[0];
                int llz = cont.verts[2];
                int lli = 0;
                int urx = cont.verts[0];
                int urz = cont.verts[2];
                int uri = 0;
                for (int i = 1; i < cont.nverts; ++i)
                {
                    int x = cont.verts[i * 4 + 0];
                    int z = cont.verts[i * 4 + 2];
                    if (x < llx || (x == llx && z < llz))
                    {
                        llx = x;
                        llz = z;
                        lli = i;
                    }

                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        urz = z;
                        uri = i;
                    }
                }

                cont.poly.Clear();
                cont.poly.Add(lli);
                cont.poly.Add(uri);
            }

            // Add points until all raw points are within
            // error tolerance to the simplified shape.
            for (int i = 0; i < cont.Npoly();)
            {
                int ii = (i + 1) % cont.Npoly();

                int ai = cont.poly[i];
                int ax = cont.verts[ai * 4];
                int az = cont.verts[ai * 4 + 2];

                int bi = cont.poly[ii];
                int bx = cont.verts[bi * 4];
                int bz = cont.verts[bi * 4 + 2];

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
                    ci = (ai + cinc) % cont.nverts;
                    endi = bi;
                }
                else
                {
                    cinc = cont.nverts - 1;
                    ci = (bi + cinc) % cont.nverts;
                    endi = ai;
                }

                // Tessellate only outer edges or edges between areas.
                while (ci != endi)
                {
                    float d = DistancePtSeg(cont.verts[ci * 4], cont.verts[ci * 4 + 2], ax, az, bx, bz);
                    if (d > maxd)
                    {
                        maxd = d;
                        maxi = ci;
                    }

                    ci = (ci + cinc) % cont.nverts;
                }

                // If the max deviation is larger than accepted error,
                // add new point, else continue to next segment.
                if (maxi != -1 && maxd > (maxError * maxError))
                {
                    cont.poly.Insert(i + 1, maxi);
                }
                else
                {
                    ++i;
                }
            }

            // Remap vertices
            int start = 0;
            for (int i = 1; i < cont.Npoly(); ++i)
                if (cont.poly[i] < cont.poly[start])
                    start = i;

            cont.nverts = 0;
            for (int i = 0; i < cont.Npoly(); ++i)
            {
                int j = (start + i) % cont.Npoly();
                int src = cont.poly[j] * 4;
                int dst = cont.nverts * 4;
                cont.verts[dst] = cont.verts[src];
                cont.verts[dst + 1] = cont.verts[src + 1];
                cont.verts[dst + 2] = cont.verts[src + 2];
                cont.verts[dst + 3] = cont.verts[src + 3];
                cont.nverts++;
            }
        }

        static int GetCornerHeight(DtTileCacheLayer layer, int x, int y, int z, int walkableClimb, out bool shouldRemove)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            int n = 0;

            int portal = 0xf;
            int height = 0;
            int preg = 0xff;
            bool allSameReg = true;

            for (int dz = -1; dz <= 0; ++dz)
            {
                for (int dx = -1; dx <= 0; ++dx)
                {
                    int px = x + dx;
                    int pz = z + dz;
                    if (px >= 0 && pz >= 0 && px < w && pz < h)
                    {
                        int idx = px + pz * w;
                        int lh = layer.heights[idx];
                        if (Math.Abs(lh - y) <= walkableClimb && layer.areas[idx] != DT_TILECACHE_NULL_AREA)
                        {
                            height = Math.Max(height, (char)lh);
                            portal &= (layer.cons[idx] >> 4);
                            if (preg != 0xff && preg != layer.regs[idx])
                                allSameReg = false;
                            preg = layer.regs[idx];
                            n++;
                        }
                    }
                }
            }

            int portalCount = 0;
            for (int dir = 0; dir < 4; ++dir)
                if ((portal & (1 << dir)) != 0)
                    portalCount++;

            shouldRemove = false;
            if (n > 1 && portalCount == 1 && allSameReg)
            {
                shouldRemove = true;
            }

            return height;
        }

        // TODO: move this somewhere else, once the layer meshing is done.
        public DtTileCacheContourSet BuildTileCacheContours(DtTileCacheLayer layer, int walkableClimb, float maxError)
        {
            int w = layer.header.width;
            int h = layer.header.height;

            DtTileCacheContourSet lcset = new DtTileCacheContourSet();
            lcset.nconts = layer.regCount;
            lcset.conts = new DtTileCacheContour[lcset.nconts];
            for (int i = 0; i < lcset.nconts; i++)
            {
                lcset.conts[i] = new DtTileCacheContour();
            }

            // Allocate temp buffer for contour tracing.
            DtTempContour temp = new DtTempContour();

            // Find contours.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int idx = x + y * w;
                    int ri = layer.regs[idx];
                    if (ri == 0xff)
                        continue;

                    DtTileCacheContour cont = lcset.conts[ri];

                    if (cont.nverts > 0)
                        continue;

                    cont.reg = ri;
                    cont.area = layer.areas[idx];

                    WalkContour(layer, x, y, temp);

                    SimplifyContour(temp, maxError);

                    // Store contour.
                    cont.nverts = temp.nverts;
                    if (cont.nverts > 0)
                    {
                        cont.verts = new int[4 * temp.nverts];

                        for (int i = 0, j = temp.nverts - 1; i < temp.nverts; j = i++)
                        {
                            int dst = j * 4;
                            int v = j * 4;
                            int vn = i * 4;
                            int nei = temp.verts[vn + 3]; // The neighbour reg
                            // is
                            // stored at segment
                            // vertex of a
                            // segment.
                            int lh = GetCornerHeight(layer, temp.verts[v], temp.verts[v + 1], temp.verts[v + 2],
                                walkableClimb, out var shouldRemove);
                            cont.verts[dst + 0] = temp.verts[v];
                            cont.verts[dst + 1] = lh;
                            cont.verts[dst + 2] = temp.verts[v + 2];

                            // Store portal direction and remove status to the
                            // fourth component.
                            cont.verts[dst + 3] = 0x0f;
                            if (nei != 0xff && nei >= 0xf8)
                                cont.verts[dst + 3] = nei - 0xf8;
                            if (shouldRemove)
                                cont.verts[dst + 3] |= 0x80;
                        }
                    }
                }
            }

            return lcset;
        }

        const uint VERTEX_BUCKET_COUNT2 = (1 << 8);

        private int ComputeVertexHash2(int x, int y, int z)
        {
            uint h1 = 0x8da6b343; // Large multiplicative constants;
            uint h2 = 0xd8163841; // here arbitrarily chosen primes
            uint h3 = 0xcb1ab31f;
            uint n = h1 * (uint)x + h2 * (uint)y + h3 * (uint)z;
            return (int)(n & (VERTEX_BUCKET_COUNT2 - 1));
        }

        private int AddVertex(int x, int y, int z, int[] verts, int[] firstVert, int[] nextVert, int nv)
        {
            int bucket = ComputeVertexHash2(x, 0, z);
            int i = firstVert[bucket];
            while (i != DT_TILECACHE_NULL_IDX)
            {
                int tv = i * 3;
                if (verts[tv] == x && verts[tv + 2] == z && (Math.Abs(verts[tv + 1] - y) <= 2))
                    return i;
                i = nextVert[i]; // next
            }

            // Could not find, create new.
            i = nv;
            int v = i * 3;
            verts[v] = x;
            verts[v + 1] = y;
            verts[v + 2] = z;
            nextVert[i] = firstVert[bucket];
            firstVert[bucket] = i;
            return i;
        }

        private void BuildMeshAdjacency(int[] polys, int npolys, int[] verts, int nverts, DtTileCacheContourSet lcset,
            int maxVertsPerPoly)
        {
            // Based on code by Eric Lengyel from:
            // http://www.terathon.com/code/edges.php

            int maxEdgeCount = npolys * maxVertsPerPoly;

            int[] firstEdge = new int[nverts + maxEdgeCount];
            int nextEdge = nverts;
            int edgeCount = 0;

            RcEdge[] edges = new RcEdge[maxEdgeCount];
            for (int i = 0; i < maxEdgeCount; i++)
            {
                edges[i] = new RcEdge();
            }

            for (int i = 0; i < nverts; i++)
                firstEdge[i] = DT_TILECACHE_NULL_IDX;

            for (int i = 0; i < npolys; ++i)
            {
                int t = i * maxVertsPerPoly * 2;
                for (int j = 0; j < maxVertsPerPoly; ++j)
                {
                    if (polys[t + j] == DT_TILECACHE_NULL_IDX)
                        break;
                    int v0 = polys[t + j];
                    int v1 = (j + 1 >= maxVertsPerPoly || polys[t + j + 1] == DT_TILECACHE_NULL_IDX)
                        ? polys[t]
                        : polys[t + j + 1];
                    if (v0 < v1)
                    {
                        RcEdge edge = edges[edgeCount];
                        edge.vert[0] = v0;
                        edge.vert[1] = v1;
                        edge.poly[0] = i;
                        edge.polyEdge[0] = j;
                        edge.poly[1] = i;
                        edge.polyEdge[1] = 0xff;
                        // Insert edge
                        firstEdge[nextEdge + edgeCount] = firstEdge[v0];
                        firstEdge[v0] = (short)edgeCount;
                        edgeCount++;
                    }
                }
            }

            for (int i = 0; i < npolys; ++i)
            {
                int t = i * maxVertsPerPoly * 2;
                for (int j = 0; j < maxVertsPerPoly; ++j)
                {
                    if (polys[t + j] == DT_TILECACHE_NULL_IDX)
                        break;
                    int v0 = polys[t + j];
                    int v1 = (j + 1 >= maxVertsPerPoly || polys[t + j + 1] == DT_TILECACHE_NULL_IDX)
                        ? polys[t]
                        : polys[t + j + 1];
                    if (v0 > v1)
                    {
                        bool found = false;
                        for (int e = firstEdge[v1]; e != DT_TILECACHE_NULL_IDX; e = firstEdge[nextEdge + e])
                        {
                            RcEdge edge = edges[e];
                            if (edge.vert[1] == v0 && edge.poly[0] == edge.poly[1])
                            {
                                edge.poly[1] = i;
                                edge.polyEdge[1] = j;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            // Matching edge not found, it is an open edge, add it.
                            RcEdge edge = edges[edgeCount];
                            edge.vert[0] = v1;
                            edge.vert[1] = v0;
                            edge.poly[0] = (short)i;
                            edge.polyEdge[0] = (short)j;
                            edge.poly[1] = (short)i;
                            edge.polyEdge[1] = 0xff;
                            // Insert edge
                            firstEdge[nextEdge + edgeCount] = firstEdge[v1];
                            firstEdge[v1] = (short)edgeCount;
                            edgeCount++;
                        }
                    }
                }
            }

            // Mark portal edges.
            for (int i = 0; i < lcset.nconts; ++i)
            {
                DtTileCacheContour cont = lcset.conts[i];
                if (cont.nverts < 3)
                    continue;

                for (int j = 0, k = cont.nverts - 1; j < cont.nverts; k = j++)
                {
                    int va = k * 4;
                    int vb = j * 4;
                    int dir = cont.verts[va + 3] & 0xf;
                    if (dir == 0xf)
                        continue;

                    if (dir == 0 || dir == 2)
                    {
                        // Find matching vertical edge
                        int x = cont.verts[va];
                        int zmin = cont.verts[va + 2];
                        int zmax = cont.verts[vb + 2];
                        if (zmin > zmax)
                        {
                            int tmp = zmin;
                            zmin = zmax;
                            zmax = tmp;
                        }

                        for (int m = 0; m < edgeCount; ++m)
                        {
                            RcEdge e = edges[m];
                            // Skip connected edges.
                            if (e.poly[0] != e.poly[1])
                                continue;
                            int eva = e.vert[0] * 3;
                            int evb = e.vert[1] * 3;
                            if (verts[eva] == x && verts[evb] == x)
                            {
                                int ezmin = verts[eva + 2];
                                int ezmax = verts[evb + 2];
                                if (ezmin > ezmax)
                                {
                                    int tmp = ezmin;
                                    ezmin = ezmax;
                                    ezmax = tmp;
                                }

                                if (OverlapRangeExl(zmin, zmax, ezmin, ezmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.polyEdge[1] = dir;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Find matching vertical edge
                        int z = cont.verts[va + 2];
                        int xmin = cont.verts[va];
                        int xmax = cont.verts[vb];
                        if (xmin > xmax)
                        {
                            int tmp = xmin;
                            xmin = xmax;
                            xmax = tmp;
                        }

                        for (int m = 0; m < edgeCount; ++m)
                        {
                            RcEdge e = edges[m];
                            // Skip connected edges.
                            if (e.poly[0] != e.poly[1])
                                continue;
                            int eva = e.vert[0] * 3;
                            int evb = e.vert[1] * 3;
                            if (verts[eva + 2] == z && verts[evb + 2] == z)
                            {
                                int exmin = verts[eva];
                                int exmax = verts[evb];
                                if (exmin > exmax)
                                {
                                    int tmp = exmin;
                                    exmin = exmax;
                                    exmax = tmp;
                                }

                                if (OverlapRangeExl(xmin, xmax, exmin, exmax))
                                {
                                    // Reuse the other polyedge to store dir.
                                    e.polyEdge[1] = dir;
                                }
                            }
                        }
                    }
                }
            }

            // Store adjacency
            for (int i = 0; i < edgeCount; ++i)
            {
                RcEdge e = edges[i];
                if (e.poly[0] != e.poly[1])
                {
                    int p0 = e.poly[0] * maxVertsPerPoly * 2;
                    int p1 = e.poly[1] * maxVertsPerPoly * 2;
                    polys[p0 + maxVertsPerPoly + e.polyEdge[0]] = e.poly[1];
                    polys[p1 + maxVertsPerPoly + e.polyEdge[1]] = e.poly[0];
                }
                else if (e.polyEdge[1] != 0xff)
                {
                    int p0 = e.poly[0] * maxVertsPerPoly * 2;
                    polys[p0 + maxVertsPerPoly + e.polyEdge[0]] = 0x8000 | (short)e.polyEdge[1];
                }
            }
        }

        private bool OverlapRangeExl(int amin, int amax, int bmin, int bmax)
        {
            return (amin >= bmax || amax <= bmin) ? false : true;
        }

        private int Prev(int i, int n)
        {
            return i - 1 >= 0 ? i - 1 : n - 1;
        }

        private int Next(int i, int n)
        {
            return i + 1 < n ? i + 1 : 0;
        }

        private int Area2(int[] verts, int a, int b, int c)
        {
            return (verts[b] - verts[a]) * (verts[c + 2] - verts[a + 2])
                   - (verts[c] - verts[a]) * (verts[b + 2] - verts[a + 2]);
        }

        // Returns true iff c is strictly to the left of the directed
        // line through a to b.
        private bool Left(int[] verts, int a, int b, int c)
        {
            return Area2(verts, a, b, c) < 0;
        }

        private bool LeftOn(int[] verts, int a, int b, int c)
        {
            return Area2(verts, a, b, c) <= 0;
        }

        private bool Collinear(int[] verts, int a, int b, int c)
        {
            return Area2(verts, a, b, c) == 0;
        }

        // Returns true iff ab properly intersects cd: they share
        // a point interior to both segments. The properness of the
        // intersection is ensured by using strict leftness.
        private bool IntersectProp(int[] verts, int a, int b, int c, int d)
        {
            // Eliminate improper cases.
            if (Collinear(verts, a, b, c) || Collinear(verts, a, b, d) || Collinear(verts, c, d, a)
                || Collinear(verts, c, d, b))
                return false;

            return (Left(verts, a, b, c) ^ Left(verts, a, b, d)) && (Left(verts, c, d, a) ^ Left(verts, c, d, b));
        }

        // Returns T iff (a,b,c) are collinear and point c lies
        // on the closed segment ab.
        private bool Between(int[] verts, int a, int b, int c)
        {
            if (!Collinear(verts, a, b, c))
                return false;
            // If ab not vertical, check betweenness on x; else on y.
            if (verts[a] != verts[b])
                return ((verts[a] <= verts[c]) && (verts[c] <= verts[b]))
                       || ((verts[a] >= verts[c]) && (verts[c] >= verts[b]));
            else
                return ((verts[a + 2] <= verts[c + 2]) && (verts[c + 2] <= verts[b + 2]))
                       || ((verts[a + 2] >= verts[c + 2]) && (verts[c + 2] >= verts[b + 2]));
        }

        // Returns true iff segments ab and cd intersect, properly or improperly.
        private bool Intersect(int[] verts, int a, int b, int c, int d)
        {
            if (IntersectProp(verts, a, b, c, d))
                return true;
            else if (Between(verts, a, b, c) || Between(verts, a, b, d) || Between(verts, c, d, a)
                     || Between(verts, c, d, b))
                return true;
            else
                return false;
        }

        private bool Vequal(int[] verts, int a, int b)
        {
            return verts[a] == verts[b] && verts[a + 2] == verts[b + 2];
        }

        // Returns T iff (v_i, v_j) is a proper internal *or* external
        // diagonal of P, *ignoring edges incident to v_i and v_j*.
        private bool Diagonalie(int i, int j, int n, int[] verts, int[] indices)
        {
            int d0 = (indices[i] & 0x7fff) * 4;
            int d1 = (indices[j] & 0x7fff) * 4;

            // For each edge (k,k+1) of P
            for (int k = 0; k < n; k++)
            {
                int k1 = Next(k, n);
                // Skip edges incident to i or j
                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    int p0 = (indices[k] & 0x7fff) * 4;
                    int p1 = (indices[k1] & 0x7fff) * 4;

                    if (Vequal(verts, d0, p0) || Vequal(verts, d1, p0) || Vequal(verts, d0, p1) || Vequal(verts, d1, p1))
                        continue;

                    if (Intersect(verts, d0, d1, p0, p1))
                        return false;
                }
            }

            return true;
        }

        // Returns true iff the diagonal (i,j) is strictly internal to the
        // polygon P in the neighborhood of the i endpoint.
        private bool InCone(int i, int j, int n, int[] verts, int[] indices)
        {
            int pi = (indices[i] & 0x7fff) * 4;
            int pj = (indices[j] & 0x7fff) * 4;
            int pi1 = (indices[Next(i, n)] & 0x7fff) * 4;
            int pin1 = (indices[Prev(i, n)] & 0x7fff) * 4;

            // If P[i] is a convex vertex [ i+1 left or on (i-1,i) ].
            if (LeftOn(verts, pin1, pi, pi1))
                return Left(verts, pi, pj, pin1) && Left(verts, pj, pi, pi1);
            // Assume (i-1,i,i+1) not collinear.
            // else P[i] is reflex.
            return !(LeftOn(verts, pi, pj, pi1) && LeftOn(verts, pj, pi, pin1));
        }

        // Returns T iff (v_i, v_j) is a proper internal
        // diagonal of P.
        private bool Diagonal(int i, int j, int n, int[] verts, int[] indices)
        {
            return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
        }

        private int Triangulate(int n, int[] verts, int[] indices, int[] tris)
        {
            int ntris = 0;
            int dst = 0; // tris;
            // The last bit of the index is used to indicate if the vertex can be
            // removed.
            for (int i = 0; i < n; i++)
            {
                int i1 = Next(i, n);
                int i2 = Next(i1, n);
                if (Diagonal(i, i2, n, verts, indices))
                    indices[i1] |= 0x8000;
            }

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int mi = 0; mi < n; mi++)
                {
                    int mi1 = Next(mi, n);
                    if ((indices[mi1] & 0x8000) != 0)
                    {
                        int p0 = (indices[mi] & 0x7fff) * 4;
                        int p2 = (indices[Next(mi1, n)] & 0x7fff) * 4;

                        int dx = verts[p2] - verts[p0];
                        int dz = verts[p2 + 2] - verts[p0 + 2];
                        int len = dx * dx + dz * dz;
                        if (minLen < 0 || len < minLen)
                        {
                            minLen = len;
                            mini = mi;
                        }
                    }
                }

                if (mini == -1)
                {
                    // Should not happen.
                    /*
                     * Printf("mini == -1 ntris=%d n=%d\n", ntris, n); for (int i = 0; i < n; i++) { Printf("%d ",
                     * indices[i] & 0x0fffffff); } Printf("\n");
                     */
                    return -ntris;
                }

                int i = mini;
                int i1 = Next(i, n);
                int i2 = Next(i1, n);

                tris[dst++] = indices[i] & 0x7fff;
                tris[dst++] = indices[i1] & 0x7fff;
                tris[dst++] = indices[i2] & 0x7fff;
                ntris++;

                // Removes P[i1] by copying P[i+1]...P[n-1] left one index.
                n--;
                for (int k = i1; k < n; k++)
                    indices[k] = indices[k + 1];

                if (i1 >= n)
                    i1 = 0;
                i = Prev(i1, n);
                // Update diagonal flags.
                if (Diagonal(Prev(i, n), i1, n, verts, indices))
                    indices[i] |= 0x8000;
                else
                    indices[i] &= 0x7fff;

                if (Diagonal(i, Next(i1, n), n, verts, indices))
                    indices[i1] |= 0x8000;
                else
                    indices[i1] &= 0x7fff;
            }

            // Append the remaining triangle.
            tris[dst++] = indices[0] & 0x7fff;
            tris[dst++] = indices[1] & 0x7fff;
            tris[dst++] = indices[2] & 0x7fff;
            ntris++;

            return ntris;
        }

        private int CountPolyVerts(int[] polys, int p, int maxVertsPerPoly)
        {
            for (int i = 0; i < maxVertsPerPoly; ++i)
                if (polys[p + i] == DT_TILECACHE_NULL_IDX)
                    return i;
            return maxVertsPerPoly;
        }

        private bool Uleft(int[] verts, int a, int b, int c)
        {
            return (verts[b] - verts[a]) * (verts[c + 2] - verts[a + 2])
                - (verts[c] - verts[a]) * (verts[b + 2] - verts[a + 2]) < 0;
        }

        private int GetPolyMergeValue(int[] polys, int pa, int pb, int[] verts, out int ea, out int eb, int maxVertsPerPoly)
        {
            ea = 0;
            eb = 0;

            int na = CountPolyVerts(polys, pa, maxVertsPerPoly);
            int nb = CountPolyVerts(polys, pb, maxVertsPerPoly);

            // If the merged polygon would be too big, do not merge.
            if (na + nb - 2 > maxVertsPerPoly)
                return -1;

            // Check if the polygons share an edge.
            ea = -1;
            eb = -1;

            for (int i = 0; i < na; ++i)
            {
                int va0 = polys[pa + i];
                int va1 = polys[pa + (i + 1) % na];
                if (va0 > va1)
                {
                    (va0, va1) = (va1, va0);
                }

                for (int j = 0; j < nb; ++j)
                {
                    int vb0 = polys[pb + j];
                    int vb1 = polys[pb + (j + 1) % nb];
                    if (vb0 > vb1)
                    {
                        (vb0, vb1) = (vb1, vb0);
                    }

                    if (va0 == vb0 && va1 == vb1)
                    {
                        ea = i;
                        eb = j;
                        break;
                    }
                }
            }

            // No common edge, cannot merge.
            if (ea == -1 || eb == -1)
                return -1;

            // Check to see if the merged polygon would be convex.
            int va, vb, vc;

            va = polys[pa + (ea + na - 1) % na];
            vb = polys[pa + ea];
            vc = polys[pb + (eb + 2) % nb];
            if (!Uleft(verts, va * 3, vb * 3, vc * 3))
                return -1;

            va = polys[pb + (eb + nb - 1) % nb];
            vb = polys[pb + eb];
            vc = polys[pa + (ea + 2) % na];
            if (!Uleft(verts, va * 3, vb * 3, vc * 3))
                return -1;

            va = polys[pa + ea];
            vb = polys[pa + (ea + 1) % na];

            int dx = verts[va * 3 + 0] - verts[vb * 3 + 0];
            int dy = verts[va * 3 + 2] - verts[vb * 3 + 2];

            return (dx * dx) + (dy * dy);
        }

        private void MergePolys(int[] polys, int pa, int pb, int ea, int eb, int maxVertsPerPoly)
        {
            int[] tmp = new int[maxVertsPerPoly * 2];

            int na = CountPolyVerts(polys, pa, maxVertsPerPoly);
            int nb = CountPolyVerts(polys, pb, maxVertsPerPoly);

            // Merge polygons.
            Array.Fill(tmp, DT_TILECACHE_NULL_IDX);
            int n = 0;
            // Add pa
            for (int i = 0; i < na - 1; ++i)
                tmp[n++] = polys[pa + (ea + 1 + i) % na];
            // Add pb
            for (int i = 0; i < nb - 1; ++i)
                tmp[n++] = polys[pb + (eb + 1 + i) % nb];
            Array.Copy(tmp, 0, polys, pa, maxVertsPerPoly);
        }

        private int PushFront(int v, List<int> arr)
        {
            arr.Insert(0, v);
            return arr.Count;
        }

        private int PushBack(int v, List<int> arr)
        {
            arr.Add(v);
            return arr.Count;
        }

        private bool CanRemoveVertex(DtTileCachePolyMesh mesh, int rem)
        {
            // Count number of polygons to remove.
            int maxVertsPerPoly = mesh.nvp;
            int numRemainingEdges = 0;
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int p = i * mesh.nvp * 2;
                int nv = CountPolyVerts(mesh.polys, p, maxVertsPerPoly);
                int numRemoved = 0;
                int numVerts = 0;
                for (int j = 0; j < nv; ++j)
                {
                    if (mesh.polys[p + j] == rem)
                    {
                        numRemoved++;
                    }

                    numVerts++;
                }

                if (numRemoved != 0)
                {
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
            }

            // There would be too few edges remaining to create a polygon.
            // This can happen for example when a tip of a triangle is marked
            // as deletion, but there are no other polys that share the vertex.
            // In this case, the vertex should not be removed.
            if (numRemainingEdges <= 2)
                return false;

            // Find edges which share the removed vertex.
            List<int> edges = new List<int>();
            int nedges = 0;

            for (int i = 0; i < mesh.npolys; ++i)
            {
                int p = i * mesh.nvp * 2;
                int nv = CountPolyVerts(mesh.polys, p, maxVertsPerPoly);

                // Collect edges which touches the removed vertex.
                for (int j = 0, k = nv - 1; j < nv; k = j++)
                {
                    if (mesh.polys[p + j] == rem || mesh.polys[p + k] == rem)
                    {
                        // Arrange edge so that a=rem.
                        int a = mesh.polys[p + j], b = mesh.polys[p + k];
                        if (b == rem)
                        {
                            int tmp = a;
                            a = b;
                            b = tmp;
                        }

                        // Check if the edge exists
                        bool exists = false;
                        for (int m = 0; m < nedges; ++m)
                        {
                            int e = m * 3;
                            if (edges[e + 1] == b)
                            {
                                // Exists, increment vertex share count.
                                edges[e + 2] = edges[e + 2] + 1;
                                exists = true;
                            }
                        }

                        // Add new edge.
                        if (!exists)
                        {
                            edges.Add(a);
                            edges.Add(b);
                            edges.Add(1);
                            nedges++;
                        }
                    }
                }
            }

            // There should be no more than 2 open edges.
            // This catches the case that two non-adjacent polygons
            // share the removed vertex. In that case, do not remove the vertex.
            int numOpenEdges = 0;
            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i * 3 + 2] < 2)
                    numOpenEdges++;
            }

            if (numOpenEdges > 2)
                return false;

            return true;
        }

        private void RemoveVertex(DtTileCachePolyMesh mesh, int rem, int maxTris)
        {
            // Count number of polygons to remove.
            int maxVertsPerPoly = mesh.nvp;
            int numRemovedVerts = 0;
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int p = i * maxVertsPerPoly * 2;
                int nv = CountPolyVerts(mesh.polys, p, maxVertsPerPoly);
                for (int j = 0; j < nv; ++j)
                {
                    if (mesh.polys[p + j] == rem)
                        numRemovedVerts++;
                }
            }

            int nedges = 0;
            List<int> edges = new List<int>();
            int nhole = 0;
            List<int> hole = new List<int>();
            List<int> harea = new List<int>();

            for (int i = 0; i < mesh.npolys; ++i)
            {
                int p = i * maxVertsPerPoly * 2;
                int nv = CountPolyVerts(mesh.polys, p, maxVertsPerPoly);
                bool hasRem = false;
                for (int j = 0; j < nv; ++j)
                    if (mesh.polys[p + j] == rem)
                        hasRem = true;
                if (hasRem)
                {
                    // Collect edges which does not touch the removed vertex.
                    for (int j = 0, k = nv - 1; j < nv; k = j++)
                    {
                        if (mesh.polys[p + j] != rem && mesh.polys[p + k] != rem)
                        {
                            edges.Add(mesh.polys[p + k]);
                            edges.Add(mesh.polys[p + j]);
                            edges.Add(mesh.areas[i]);
                            nedges++;
                        }
                    }

                    // Remove the polygon.
                    int p2 = (mesh.npolys - 1) * maxVertsPerPoly * 2;
                    Array.Copy(mesh.polys, p2, mesh.polys, p, maxVertsPerPoly);
                    Array.Fill(mesh.polys, DT_TILECACHE_NULL_IDX, p + maxVertsPerPoly, maxVertsPerPoly);
                    mesh.areas[i] = mesh.areas[mesh.npolys - 1];
                    mesh.npolys--;
                    --i;
                }
            }

            // Remove vertex.
            for (int i = rem; i < mesh.nverts - 1; ++i)
            {
                mesh.verts[i * 3 + 0] = mesh.verts[(i + 1) * 3 + 0];
                mesh.verts[i * 3 + 1] = mesh.verts[(i + 1) * 3 + 1];
                mesh.verts[i * 3 + 2] = mesh.verts[(i + 1) * 3 + 2];
            }

            mesh.nverts--;

            // Adjust indices to match the removed vertex layout.
            for (int i = 0; i < mesh.npolys; ++i)
            {
                int p = i * maxVertsPerPoly * 2;
                int nv = CountPolyVerts(mesh.polys, p, maxVertsPerPoly);
                for (int j = 0; j < nv; ++j)
                    if (mesh.polys[p + j] > rem)
                        mesh.polys[p + j]--;
            }

            for (int i = 0; i < nedges; ++i)
            {
                if (edges[i * 3] > rem)
                    edges[i * 3] = edges[i * 3] - 1;
                if (edges[i * 3 + 1] > rem)
                    edges[i * 3 + 1] = edges[i * 3 + 1] - 1;
            }

            if (nedges == 0)
                return;

            // Start with one vertex, keep appending connected
            // segments to the start and end of the hole.
            nhole = PushBack(edges[0], hole);
            PushBack(edges[2], harea);

            while (nedges != 0)
            {
                bool match = false;

                for (int i = 0; i < nedges; ++i)
                {
                    int ea = edges[i * 3];
                    int eb = edges[i * 3 + 1];
                    int a = edges[i * 3 + 2];
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        // The segment matches the beginning of the hole boundary.
                        nhole = PushFront(ea, hole);
                        PushFront(a, harea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        // The segment matches the end of the hole boundary.
                        nhole = PushBack(eb, hole);
                        PushBack(a, harea);
                        add = true;
                    }

                    if (add)
                    {
                        // The edge segment was added, remove it.
                        edges[i * 3] = edges[(nedges - 1) * 3];
                        edges[i * 3 + 1] = edges[(nedges - 1) * 3] + 1;
                        edges[i * 3 + 2] = edges[(nedges - 1) * 3] + 2;
                        --nedges;
                        match = true;
                        --i;
                    }
                }

                if (!match)
                    break;
            }

            int[] tris = new int[nhole * 3];
            int[] tverts = new int[nhole * 4];
            int[] tpoly = new int[nhole];

            // Generate temp vertex array for triangulation.
            for (int i = 0; i < nhole; ++i)
            {
                int pi = hole[i];
                tverts[i * 4 + 0] = mesh.verts[pi * 3 + 0];
                tverts[i * 4 + 1] = mesh.verts[pi * 3 + 1];
                tverts[i * 4 + 2] = mesh.verts[pi * 3 + 2];
                tverts[i * 4 + 3] = 0;
                tpoly[i] = i;
            }

            // Triangulate the hole.
            int ntris = Triangulate(nhole, tverts, tpoly, tris);
            if (ntris < 0)
            {
                // TODO: issue warning!
                ntris = -ntris;
            }

            int[] polys = new int[ntris * maxVertsPerPoly];
            int[] pareas = new int[ntris];

            // Build initial polygons.
            int npolys = 0;
            Array.Fill(polys, DT_TILECACHE_NULL_IDX, 0, ntris * maxVertsPerPoly);
            for (int j = 0; j < ntris; ++j)
            {
                int t = j * 3;
                if (tris[t] != tris[t + 1] && tris[t] != tris[t + 2] && tris[t + 1] != tris[t + 2])
                {
                    polys[npolys * maxVertsPerPoly + 0] = hole[tris[t]];
                    polys[npolys * maxVertsPerPoly + 1] = hole[tris[t + 1]];
                    polys[npolys * maxVertsPerPoly + 2] = hole[tris[t + 2]];
                    pareas[npolys] = harea[tris[t]];
                    npolys++;
                }
            }

            if (npolys == 0)
                return;

            // Merge polygons.
            if (maxVertsPerPoly > 3)
            {
                for (;;)
                {
                    // Find best polygons to merge.
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys - 1; ++j)
                    {
                        int pj = j * maxVertsPerPoly;
                        for (int k = j + 1; k < npolys; ++k)
                        {
                            int pk = k * maxVertsPerPoly;
                            int v = GetPolyMergeValue(polys, pj, pk, mesh.verts, out var ea, out var eb, maxVertsPerPoly);
                            if (v > bestMergeVal)
                            {
                                bestMergeVal = v;
                                bestPa = j;
                                bestPb = k;
                                bestEa = ea;
                                bestEb = eb;
                            }
                        }
                    }

                    if (bestMergeVal > 0)
                    {
                        // Found best, merge.
                        int pa = bestPa * maxVertsPerPoly;
                        int pb = bestPb * maxVertsPerPoly;
                        MergePolys(polys, pa, pb, bestEa, bestEb, maxVertsPerPoly);
                        Array.Copy(polys, (npolys - 1) * maxVertsPerPoly, polys, pb, maxVertsPerPoly);
                        pareas[bestPb] = pareas[npolys - 1];
                        npolys--;
                    }
                    else
                    {
                        // Could not merge any polygons, stop.
                        break;
                    }
                }
            }

            // Store polygons.
            for (int i = 0; i < npolys; ++i)
            {
                if (mesh.npolys >= maxTris)
                    break;
                int p = mesh.npolys * maxVertsPerPoly * 2;
                Array.Fill(mesh.polys, DT_TILECACHE_NULL_IDX, p, maxVertsPerPoly * 2);
                for (int j = 0; j < maxVertsPerPoly; ++j)
                    mesh.polys[p + j] = polys[i * maxVertsPerPoly + j];
                mesh.areas[mesh.npolys] = pareas[i];
                mesh.npolys++;
                if (mesh.npolys > maxTris)
                {
                    throw new Exception("Buffer too small");
                }
            }
        }

        public DtTileCachePolyMesh BuildTileCachePolyMesh(DtTileCacheContourSet lcset, int maxVertsPerPoly)
        {
            int maxVertices = 0;
            int maxTris = 0;
            int maxVertsPerCont = 0;
            for (int i = 0; i < lcset.nconts; ++i)
            {
                // Skip null contours.
                if (lcset.conts[i].nverts < 3)
                    continue;
                maxVertices += lcset.conts[i].nverts;
                maxTris += lcset.conts[i].nverts - 2;
                maxVertsPerCont = Math.Max(maxVertsPerCont, lcset.conts[i].nverts);
            }

            // TODO: warn about too many vertices?

            DtTileCachePolyMesh mesh = new DtTileCachePolyMesh(maxVertsPerPoly);

            int[] vflags = new int[maxVertices];

            mesh.verts = new int[maxVertices * 3];
            mesh.polys = new int[maxTris * maxVertsPerPoly * 2];
            mesh.areas = new int[maxTris];
            // Just allocate and clean the mesh flags array. The user is resposible
            // for filling it.
            mesh.flags = new int[maxTris];

            mesh.nverts = 0;
            mesh.npolys = 0;

            Array.Fill(mesh.polys, DT_TILECACHE_NULL_IDX);

            int[] firstVert = new int[VERTEX_BUCKET_COUNT2];
            for (int i = 0; i < VERTEX_BUCKET_COUNT2; ++i)
                firstVert[i] = DT_TILECACHE_NULL_IDX;

            int[] nextVert = new int[maxVertices];
            int[] indices = new int[maxVertsPerCont];
            int[] tris = new int[maxVertsPerCont * 3];
            int[] polys = new int[maxVertsPerCont * maxVertsPerPoly];

            for (int i = 0; i < lcset.nconts; ++i)
            {
                DtTileCacheContour cont = lcset.conts[i];

                // Skip null contours.
                if (cont.nverts < 3)
                    continue;

                // Triangulate contour
                for (int j = 0; j < cont.nverts; ++j)
                    indices[j] = j;

                int ntris = Triangulate(cont.nverts, cont.verts, indices, tris);
                if (ntris <= 0)
                {
                    // TODO: issue warning!
                    ntris = -ntris;
                }

                // Add and merge vertices.
                for (int j = 0; j < cont.nverts; ++j)
                {
                    int v = j * 4;
                    indices[j] = AddVertex(cont.verts[v], cont.verts[v + 1], cont.verts[v + 2], mesh.verts, firstVert,
                        nextVert, mesh.nverts);
                    mesh.nverts = Math.Max(mesh.nverts, indices[j] + 1);
                    if ((cont.verts[v + 3] & 0x80) != 0)
                    {
                        // This vertex should be removed.
                        vflags[indices[j]] = 1;
                    }
                }

                // Build initial polygons.
                int npolys = 0;
                Array.Fill(polys, DT_TILECACHE_NULL_IDX);
                for (int j = 0; j < ntris; ++j)
                {
                    int t = j * 3;
                    if (tris[t] != tris[t + 1] && tris[t] != tris[t + 2] && tris[t + 1] != tris[t + 2])
                    {
                        polys[npolys * maxVertsPerPoly + 0] = indices[tris[t]];
                        polys[npolys * maxVertsPerPoly + 1] = indices[tris[t + 1]];
                        polys[npolys * maxVertsPerPoly + 2] = indices[tris[t + 2]];
                        npolys++;
                    }
                }

                if (npolys == 0)
                    continue;

                // Merge polygons.
                if (maxVertsPerPoly > 3)
                {
                    for (;;)
                    {
                        // Find best polygons to merge.
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys - 1; ++j)
                        {
                            int pj = j * maxVertsPerPoly;
                            for (int k = j + 1; k < npolys; ++k)
                            {
                                int pk = k * maxVertsPerPoly;
                                int v = GetPolyMergeValue(polys, pj, pk, mesh.verts, out var ea, out var eb, maxVertsPerPoly);
                                if (v > bestMergeVal)
                                {
                                    bestMergeVal = v;
                                    bestPa = j;
                                    bestPb = k;
                                    bestEa = ea;
                                    bestEb = eb;
                                }
                            }
                        }

                        if (bestMergeVal > 0)
                        {
                            // Found best, merge.
                            int pa = bestPa * maxVertsPerPoly;
                            int pb = bestPb * maxVertsPerPoly;
                            MergePolys(polys, pa, pb, bestEa, bestEb, maxVertsPerPoly);
                            Array.Copy(polys, (npolys - 1) * maxVertsPerPoly, polys, pb, maxVertsPerPoly);
                            npolys--;
                        }
                        else
                        {
                            // Could not merge any polygons, stop.
                            break;
                        }
                    }
                }

                // Store polygons.
                for (int j = 0; j < npolys; ++j)
                {
                    int p = mesh.npolys * maxVertsPerPoly * 2;
                    int q = j * maxVertsPerPoly;
                    for (int k = 0; k < maxVertsPerPoly; ++k)
                        mesh.polys[p + k] = polys[q + k];
                    mesh.areas[mesh.npolys] = cont.area;
                    mesh.npolys++;
                    if (mesh.npolys > maxTris)
                        throw new Exception("Buffer too small");
                }
            }

            // Remove edge vertices.
            for (int i = 0; i < mesh.nverts; ++i)
            {
                if (vflags[i] != 0)
                {
                    if (!CanRemoveVertex(mesh, i))
                        continue;
                    RemoveVertex(mesh, i, maxTris);
                    // Remove vertex
                    // Note: mesh.nverts is already decremented inside
                    // RemoveVertex()!
                    for (int j = i; j < mesh.nverts; ++j)
                        vflags[j] = vflags[j + 1];
                    --i;
                }
            }

            // Calculate adjacency.
            BuildMeshAdjacency(mesh.polys, mesh.npolys, mesh.verts, mesh.nverts, lcset, maxVertsPerPoly);

            return mesh;
        }

        public void MarkCylinderArea(DtTileCacheLayer layer, RcVec3f orig, float cs, float ch, RcVec3f pos, float radius, float height, int areaId)
        {
            RcVec3f bmin = new RcVec3f();
            RcVec3f bmax = new RcVec3f();
            bmin.x = pos.x - radius;
            bmin.y = pos.y;
            bmin.z = pos.z - radius;
            bmax.x = pos.x + radius;
            bmax.y = pos.y + height;
            bmax.z = pos.z + radius;
            float r2 = Sqr(radius / cs + 0.5f);

            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float px = (pos.x - orig.x) * ics;
            float pz = (pos.z - orig.z) * ics;

            int minx = (int)Math.Floor((bmin.x - orig.x) * ics);
            int miny = (int)Math.Floor((bmin.y - orig.y) * ich);
            int minz = (int)Math.Floor((bmin.z - orig.z) * ics);
            int maxx = (int)Math.Floor((bmax.x - orig.x) * ics);
            int maxy = (int)Math.Floor((bmax.y - orig.y) * ich);
            int maxz = (int)Math.Floor((bmax.z - orig.z) * ics);

            if (maxx < 0)
                return;
            if (minx >= w)
                return;
            if (maxz < 0)
                return;
            if (minz >= h)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= w)
                maxx = w - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= h)
                maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float dx = x + 0.5f - px;
                    float dz = z + 0.5f - pz;
                    if (dx * dx + dz * dz > r2)
                        continue;
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                        continue;
                    layer.areas[x + z * w] = (short)areaId;
                }
            }
        }

        public void MarkBoxArea(DtTileCacheLayer layer, RcVec3f orig, float cs, float ch, RcVec3f bmin, RcVec3f bmax, int areaId)
        {
            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            int minx = (int)Math.Floor((bmin.x - orig.x) * ics);
            int miny = (int)Math.Floor((bmin.y - orig.y) * ich);
            int minz = (int)Math.Floor((bmin.z - orig.z) * ics);
            int maxx = (int)Math.Floor((bmax.x - orig.x) * ics);
            int maxy = (int)Math.Floor((bmax.y - orig.y) * ich);
            int maxz = (int)Math.Floor((bmax.z - orig.z) * ics);

            if (maxx < 0)
                return;
            if (minx >= w)
                return;
            if (maxz < 0)
                return;
            if (minz >= h)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= w)
                maxx = w - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= h)
                maxz = h - 1;

            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                        continue;
                    layer.areas[x + z * w] = (short)areaId;
                }
            }
        }

        public byte[] CompressTileCacheLayer(IRcCompressor comp, DtTileCacheLayer layer, RcByteOrder order, bool cCompatibility)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            DtTileCacheLayerHeaderWriter hw = new DtTileCacheLayerHeaderWriter();
            try
            {
                hw.Write(bw, layer.header, order, cCompatibility);
                int gridSize = layer.header.width * layer.header.height;
                byte[] buffer = new byte[gridSize * 3];
                for (int i = 0; i < gridSize; i++)
                {
                    buffer[i] = (byte)layer.heights[i];
                    buffer[gridSize + i] = (byte)layer.areas[i];
                    buffer[gridSize * 2 + i] = (byte)layer.cons[i];
                }

                var compressed = comp.Compress(buffer);
                bw.Write(compressed);
                return ms.ToArray();
            }
            catch (IOException e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public byte[] CompressTileCacheLayer(DtTileCacheLayerHeader header, int[] heights, int[] areas, int[] cons, RcByteOrder order, bool cCompatibility, IRcCompressor comp)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            DtTileCacheLayerHeaderWriter hw = new DtTileCacheLayerHeaderWriter();
            try
            {
                hw.Write(bw, header, order, cCompatibility);
                int gridSize = header.width * header.height;
                byte[] buffer = new byte[gridSize * 3];
                for (int i = 0; i < gridSize; i++)
                {
                    buffer[i] = (byte)heights[i];
                    buffer[gridSize + i] = (byte)areas[i];
                    buffer[gridSize * 2 + i] = (byte)cons[i];
                }

                var compressed = comp.Compress(buffer);
                bw.Write(compressed);
                return ms.ToArray();
            }
            catch (IOException e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public DtTileCacheLayer DecompressTileCacheLayer(IRcCompressor comp, byte[] compressed, RcByteOrder order, bool cCompatibility)
        {
            RcByteBuffer buf = new RcByteBuffer(compressed);
            buf.Order(order);
            DtTileCacheLayer layer = new DtTileCacheLayer();
            try
            {
                layer.header = reader.Read(buf, cCompatibility);
            }
            catch (IOException e)
            {
                throw new Exception(e.Message, e);
            }

            int gridSize = layer.header.width * layer.header.height;
            byte[] grids = comp.Decompress(compressed, buf.Position(), compressed.Length - buf.Position(), gridSize * 3);
            layer.heights = new short[gridSize];
            layer.areas = new short[gridSize];
            layer.cons = new short[gridSize];
            layer.regs = new short[gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                layer.heights[i] = (short)(grids[i] & 0xFF);
                layer.areas[i] = (short)(grids[i + gridSize] & 0xFF);
                layer.cons[i] = (short)(grids[i + gridSize * 2] & 0xFF);
            }

            return layer;
        }

        public void MarkBoxArea(DtTileCacheLayer layer, RcVec3f orig, float cs, float ch, RcVec3f center, RcVec3f extents,
            float[] rotAux, int areaId)
        {
            int w = layer.header.width;
            int h = layer.header.height;
            float ics = 1.0f / cs;
            float ich = 1.0f / ch;

            float cx = (center.x - orig.x) * ics;
            float cz = (center.z - orig.z) * ics;

            float maxr = 1.41f * Math.Max(extents.x, extents.z);
            int minx = (int)Math.Floor(cx - maxr * ics);
            int maxx = (int)Math.Floor(cx + maxr * ics);
            int minz = (int)Math.Floor(cz - maxr * ics);
            int maxz = (int)Math.Floor(cz + maxr * ics);
            int miny = (int)Math.Floor((center.y - extents.y - orig.y) * ich);
            int maxy = (int)Math.Floor((center.y + extents.y - orig.y) * ich);

            if (maxx < 0)
                return;
            if (minx >= w)
                return;
            if (maxz < 0)
                return;
            if (minz >= h)
                return;

            if (minx < 0)
                minx = 0;
            if (maxx >= w)
                maxx = w - 1;
            if (minz < 0)
                minz = 0;
            if (maxz >= h)
                maxz = h - 1;

            float xhalf = extents.x * ics + 0.5f;
            float zhalf = extents.z * ics + 0.5f;
            for (int z = minz; z <= maxz; ++z)
            {
                for (int x = minx; x <= maxx; ++x)
                {
                    float x2 = 2.0f * (x - cx);
                    float z2 = 2.0f * (z - cz);
                    float xrot = rotAux[1] * x2 + rotAux[0] * z2;
                    if (xrot > xhalf || xrot < -xhalf)
                        continue;
                    float zrot = rotAux[1] * z2 - rotAux[0] * x2;
                    if (zrot > zhalf || zrot < -zhalf)
                        continue;
                    int y = layer.heights[x + z * w];
                    if (y < miny || y > maxy)
                        continue;
                    layer.areas[x + z * w] = (short)areaId;
                }
            }
        }
    }
}