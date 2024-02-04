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

namespace DotRecast.Detour
{
    using static DotRecast.Core.RcMath;

    public static class DtNavMeshBuilder
    {
        const int MESH_NULL_IDX = 0xffff;


        private static int[][] CalcExtends(BVItem[] items, int nitems, int imin, int imax)
        {
            int[] bmin = new int[3];
            int[] bmax = new int[3];
            bmin[0] = items[imin].bmin[0];
            bmin[1] = items[imin].bmin[1];
            bmin[2] = items[imin].bmin[2];

            bmax[0] = items[imin].bmax[0];
            bmax[1] = items[imin].bmax[1];
            bmax[2] = items[imin].bmax[2];

            for (int i = imin + 1; i < imax; ++i)
            {
                BVItem it = items[i];
                if (it.bmin[0] < bmin[0])
                    bmin[0] = it.bmin[0];
                if (it.bmin[1] < bmin[1])
                    bmin[1] = it.bmin[1];
                if (it.bmin[2] < bmin[2])
                    bmin[2] = it.bmin[2];

                if (it.bmax[0] > bmax[0])
                    bmax[0] = it.bmax[0];
                if (it.bmax[1] > bmax[1])
                    bmax[1] = it.bmax[1];
                if (it.bmax[2] > bmax[2])
                    bmax[2] = it.bmax[2];
            }

            return new int[][] { bmin, bmax };
        }

        private static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }

            if (z > maxVal)
            {
                axis = 2;
                maxVal = z;
            }

            return axis;
        }

        public static int Subdivide(BVItem[] items, int nitems, int imin, int imax, int curNode, DtBVNode[] nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            DtBVNode node = new DtBVNode();
            nodes[curNode++] = node;

            if (inum == 1)
            {
                // Leaf
                node.bmin[0] = items[imin].bmin[0];
                node.bmin[1] = items[imin].bmin[1];
                node.bmin[2] = items[imin].bmin[2];

                node.bmax[0] = items[imin].bmax[0];
                node.bmax[1] = items[imin].bmax[1];
                node.bmax[2] = items[imin].bmax[2];

                node.i = items[imin].i;
            }
            else
            {
                // Split
                int[][] minmax = CalcExtends(items, nitems, imin, imax);
                node.bmin = minmax[0];
                node.bmax = minmax[1];

                int axis = LongestAxis(node.bmax[0] - node.bmin[0], node.bmax[1] - node.bmin[1],
                    node.bmax[2] - node.bmin[2]);

                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, BVItemXComparer.Shared);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, BVItemYComparer.Shared);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, BVItemZComparer.Shared);
                }

                int isplit = imin + inum / 2;

                // Left
                curNode = Subdivide(items, nitems, imin, isplit, curNode, nodes);
                // Right
                curNode = Subdivide(items, nitems, isplit, imax, curNode, nodes);

                int iescape = curNode - icur;
                // Negative index means escape.
                node.i = -iescape;
            }

            return curNode;
        }

        private static int CreateBVTree(DtNavMeshCreateParams option, DtBVNode[] nodes)
        {
            // Build tree
            float quantFactor = 1 / option.cs;
            BVItem[] items = new BVItem[option.polyCount];
            for (int i = 0; i < option.polyCount; i++)
            {
                BVItem it = new BVItem();
                items[i] = it;
                it.i = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (option.detailMeshes != null)
                {
                    int vb = option.detailMeshes[i * 4 + 0];
                    int ndv = option.detailMeshes[i * 4 + 1];
                    RcVec3f bmin = new RcVec3f();
                    RcVec3f bmax = new RcVec3f();
                    int dv = vb * 3;
                    bmin.Set(option.detailVerts, dv);
                    bmax.Set(option.detailVerts, dv);
                    for (int j = 1; j < ndv; j++)
                    {
                        bmin.Min(option.detailVerts, dv + j * 3);
                        bmax.Max(option.detailVerts, dv + j * 3);
                    }

                    // BV-tree uses cs for all dimensions
                    it.bmin[0] = Clamp((int)((bmin.x - option.bmin.x) * quantFactor), 0, int.MaxValue);
                    it.bmin[1] = Clamp((int)((bmin.y - option.bmin.y) * quantFactor), 0, int.MaxValue);
                    it.bmin[2] = Clamp((int)((bmin.z - option.bmin.z) * quantFactor), 0, int.MaxValue);

                    it.bmax[0] = Clamp((int)((bmax.x - option.bmin.x) * quantFactor), 0, int.MaxValue);
                    it.bmax[1] = Clamp((int)((bmax.y - option.bmin.y) * quantFactor), 0, int.MaxValue);
                    it.bmax[2] = Clamp((int)((bmax.z - option.bmin.z) * quantFactor), 0, int.MaxValue);
                }
                else
                {
                    int p = i * option.nvp * 2;
                    it.bmin[0] = it.bmax[0] = option.verts[option.polys[p] * 3 + 0];
                    it.bmin[1] = it.bmax[1] = option.verts[option.polys[p] * 3 + 1];
                    it.bmin[2] = it.bmax[2] = option.verts[option.polys[p] * 3 + 2];

                    for (int j = 1; j < option.nvp; ++j)
                    {
                        if (option.polys[p + j] == MESH_NULL_IDX)
                            break;
                        int x = option.verts[option.polys[p + j] * 3 + 0];
                        int y = option.verts[option.polys[p + j] * 3 + 1];
                        int z = option.verts[option.polys[p + j] * 3 + 2];

                        if (x < it.bmin[0])
                            it.bmin[0] = x;
                        if (y < it.bmin[1])
                            it.bmin[1] = y;
                        if (z < it.bmin[2])
                            it.bmin[2] = z;

                        if (x > it.bmax[0])
                            it.bmax[0] = x;
                        if (y > it.bmax[1])
                            it.bmax[1] = y;
                        if (z > it.bmax[2])
                            it.bmax[2] = z;
                    }

                    // Remap y
                    it.bmin[1] = (int)Math.Floor(it.bmin[1] * option.ch * quantFactor);
                    it.bmax[1] = (int)Math.Ceiling(it.bmax[1] * option.ch * quantFactor);
                }
            }

            return Subdivide(items, option.polyCount, 0, option.polyCount, 0, nodes);
        }

        const int XP = 1 << 0;
        const int ZP = 1 << 1;
        const int XM = 1 << 2;
        const int ZM = 1 << 3;

        public static int ClassifyOffMeshPoint(RcVec3f pt, RcVec3f bmin, RcVec3f bmax)
        {
            int outcode = 0;
            outcode |= (pt.x >= bmax.x) ? XP : 0;
            outcode |= (pt.z >= bmax.z) ? ZP : 0;
            outcode |= (pt.x < bmin.x) ? XM : 0;
            outcode |= (pt.z < bmin.z) ? ZM : 0;

            switch (outcode)
            {
                case XP:
                    return 0;
                case XP | ZP:
                    return 1;
                case ZP:
                    return 2;
                case XM | ZP:
                    return 3;
                case XM:
                    return 4;
                case XM | ZM:
                    return 5;
                case ZM:
                    return 6;
                case XP | ZM:
                    return 7;
            }

            return 0xff;
        }

        /**
     * Builds navigation mesh tile data from the provided tile creation data.
     *
     * @param option
     *            Tile creation data.
     *
     * @return created tile data
     */
        public static DtMeshData CreateNavMeshData(DtNavMeshCreateParams option)
        {
            if (option.vertCount >= 0xffff)
                return null;
            if (option.vertCount == 0 || option.verts == null)
                return null;
            if (option.polyCount == 0 || option.polys == null)
                return null;

            int nvp = option.nvp;

            // Classify off-mesh connection points. We store only the connections
            // whose start point is inside the tile.
            int[] offMeshConClass = null;
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (option.offMeshConCount > 0)
            {
                offMeshConClass = new int[option.offMeshConCount * 2];

                // Find tight heigh bounds, used for culling out off-mesh start
                // locations.
                float hmin = float.MaxValue;
                float hmax = -float.MaxValue;

                if (option.detailVerts != null && option.detailVertsCount != 0)
                {
                    for (int i = 0; i < option.detailVertsCount; ++i)
                    {
                        float h = option.detailVerts[i * 3 + 1];
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                else
                {
                    for (int i = 0; i < option.vertCount; ++i)
                    {
                        int iv = i * 3;
                        float h = option.bmin.y + option.verts[iv + 1] * option.ch;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }

                hmin -= option.walkableClimb;
                hmax += option.walkableClimb;
                RcVec3f bmin = new RcVec3f();
                RcVec3f bmax = new RcVec3f();
                bmin = option.bmin;
                bmax = option.bmax;
                bmin.y = hmin;
                bmax.y = hmax;

                for (int i = 0; i < option.offMeshConCount; ++i)
                {
                    var p0 = RcVec3f.Of(option.offMeshConVerts, (i * 2 + 0) * 3);
                    var p1 = RcVec3f.Of(option.offMeshConVerts, (i * 2 + 1) * 3);

                    offMeshConClass[i * 2 + 0] = ClassifyOffMeshPoint(p0, bmin, bmax);
                    offMeshConClass[i * 2 + 1] = ClassifyOffMeshPoint(p1, bmin, bmax);

                    // Zero out off-mesh start positions which are not even
                    // potentially touching the mesh.
                    if (offMeshConClass[i * 2 + 0] == 0xff)
                    {
                        if (p0.y < bmin.y || p0.y > bmax.y)
                            offMeshConClass[i * 2 + 0] = 0;
                    }

                    // Count how many links should be allocated for off-mesh
                    // connections.
                    if (offMeshConClass[i * 2 + 0] == 0xff)
                        offMeshConLinkCount++;
                    if (offMeshConClass[i * 2 + 1] == 0xff)
                        offMeshConLinkCount++;

                    if (offMeshConClass[i * 2 + 0] == 0xff)
                        storedOffMeshConCount++;
                }
            }

            // Off-mesh connections are stored as polygons, adjust values.
            int totPolyCount = option.polyCount + storedOffMeshConCount;
            int totVertCount = option.vertCount + storedOffMeshConCount * 2;

            // Find portal edges which are at tile borders.
            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < option.polyCount; ++i)
            {
                int p = i * 2 * nvp;
                for (int j = 0; j < nvp; ++j)
                {
                    if (option.polys[p + j] == MESH_NULL_IDX)
                        break;
                    edgeCount++;

                    if ((option.polys[p + nvp + j] & 0x8000) != 0)
                    {
                        int dir = option.polys[p + nvp + j] & 0xf;
                        if (dir != 0xf)
                            portalCount++;
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            // Find unique detail vertices.
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (option.detailMeshes != null)
            {
                // Has detail mesh, count unique detail vertex count and use input
                // detail tri count.
                detailTriCount = option.detailTriCount;
                for (int i = 0; i < option.polyCount; ++i)
                {
                    int p = i * nvp * 2;
                    int ndv = option.detailMeshes[i * 4 + 1];
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (option.polys[p + j] == MESH_NULL_IDX)
                            break;
                        nv++;
                    }

                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                // No input detail mesh, build detail mesh from nav polys.
                uniqueDetailVertCount = 0; // No extra detail verts.
                detailTriCount = 0;
                for (int i = 0; i < option.polyCount; ++i)
                {
                    int p = i * nvp * 2;
                    int nv = 0;
                    for (int j = 0; j < nvp; ++j)
                    {
                        if (option.polys[p + j] == MESH_NULL_IDX)
                            break;
                        nv++;
                    }

                    detailTriCount += nv - 2;
                }
            }

            int bvTreeSize = option.buildBvTree ? option.polyCount * 2 : 0;
            DtMeshHeader header = new DtMeshHeader();
            float[] navVerts = new float[3 * totVertCount];
            DtPoly[] navPolys = new DtPoly[totPolyCount];
            DtPolyDetail[] navDMeshes = new DtPolyDetail[option.polyCount];
            float[] navDVerts = new float[3 * uniqueDetailVertCount];
            int[] navDTris = new int[4 * detailTriCount];
            DtBVNode[] navBvtree = new DtBVNode[bvTreeSize];
            DtOffMeshConnection[] offMeshCons = new DtOffMeshConnection[storedOffMeshConCount];

            // Store header
            header.magic = DtMeshHeader.DT_NAVMESH_MAGIC;
            header.version = DtMeshHeader.DT_NAVMESH_VERSION;
            header.x = option.tileX;
            header.y = option.tileZ;
            header.layer = option.tileLayer;
            header.userId = option.userId;
            header.polyCount = totPolyCount;
            header.vertCount = totVertCount;
            header.maxLinkCount = maxLinkCount;
            header.bmin = option.bmin;
            header.bmax = option.bmax;
            header.detailMeshCount = option.polyCount;
            header.detailVertCount = uniqueDetailVertCount;
            header.detailTriCount = detailTriCount;
            header.bvQuantFactor = 1.0f / option.cs;
            header.offMeshBase = option.polyCount;
            header.walkableHeight = option.walkableHeight;
            header.walkableRadius = option.walkableRadius;
            header.walkableClimb = option.walkableClimb;
            header.offMeshConCount = storedOffMeshConCount;
            header.bvNodeCount = bvTreeSize;

            int offMeshVertsBase = option.vertCount;
            int offMeshPolyBase = option.polyCount;

            // Store vertices
            // Mesh vertices
            for (int i = 0; i < option.vertCount; ++i)
            {
                int iv = i * 3;
                int v = i * 3;
                navVerts[v] = option.bmin.x + option.verts[iv] * option.cs;
                navVerts[v + 1] = option.bmin.y + option.verts[iv + 1] * option.ch;
                navVerts[v + 2] = option.bmin.z + option.verts[iv + 2] * option.cs;
            }

            // Off-mesh link vertices.
            int n = 0;
            for (int i = 0; i < option.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    int linkv = i * 2 * 3;
                    int v = (offMeshVertsBase + n * 2) * 3;
                    Array.Copy(option.offMeshConVerts, linkv, navVerts, v, 6);
                    n++;
                }
            }

            // Store polygons
            // Mesh polys
            int src = 0;
            for (int i = 0; i < option.polyCount; ++i)
            {
                DtPoly p = new DtPoly(i, nvp);
                navPolys[i] = p;
                p.vertCount = 0;
                p.flags = option.polyFlags[i];
                p.SetArea(option.polyAreas[i]);
                p.SetPolyType(DtPoly.DT_POLYTYPE_GROUND);
                for (int j = 0; j < nvp; ++j)
                {
                    if (option.polys[src + j] == MESH_NULL_IDX)
                        break;
                    p.verts[j] = option.polys[src + j];
                    if ((option.polys[src + nvp + j] & 0x8000) != 0)
                    {
                        // Border or portal edge.
                        int dir = option.polys[src + nvp + j] & 0xf;
                        if (dir == 0xf) // Border
                            p.neis[j] = 0;
                        else if (dir == 0) // Portal x-
                            p.neis[j] = DtNavMesh.DT_EXT_LINK | 4;
                        else if (dir == 1) // Portal z+
                            p.neis[j] = DtNavMesh.DT_EXT_LINK | 2;
                        else if (dir == 2) // Portal x+
                            p.neis[j] = DtNavMesh.DT_EXT_LINK | 0;
                        else if (dir == 3) // Portal z-
                            p.neis[j] = DtNavMesh.DT_EXT_LINK | 6;
                    }
                    else
                    {
                        // Normal connection
                        p.neis[j] = option.polys[src + nvp + j] + 1;
                    }

                    p.vertCount++;
                }

                src += nvp * 2;
            }

            // Off-mesh connection vertices.
            n = 0;
            for (int i = 0; i < option.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    DtPoly p = new DtPoly(offMeshPolyBase + n, nvp);
                    navPolys[offMeshPolyBase + n] = p;
                    p.vertCount = 2;
                    p.verts[0] = offMeshVertsBase + n * 2;
                    p.verts[1] = offMeshVertsBase + n * 2 + 1;
                    p.flags = option.offMeshConFlags[i];
                    p.SetArea(option.offMeshConAreas[i]);
                    p.SetPolyType(DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION);
                    n++;
                }
            }

            // Store detail meshes and vertices.
            // The nav polygon vertices are stored as the first vertices on each
            // mesh.
            // We compress the mesh data by skipping them and using the navmesh
            // coordinates.
            if (option.detailMeshes != null)
            {
                int vbase = 0;
                for (int i = 0; i < option.polyCount; ++i)
                {
                    DtPolyDetail dtl = new DtPolyDetail();
                    navDMeshes[i] = dtl;
                    int vb = option.detailMeshes[i * 4 + 0];
                    int ndv = option.detailMeshes[i * 4 + 1];
                    int nv = navPolys[i].vertCount;
                    dtl.vertBase = vbase;
                    dtl.vertCount = (ndv - nv);
                    dtl.triBase = option.detailMeshes[i * 4 + 2];
                    dtl.triCount = option.detailMeshes[i * 4 + 3];
                    // Copy vertices except the first 'nv' verts which are equal to
                    // nav poly verts.
                    if (ndv - nv != 0)
                    {
                        Array.Copy(option.detailVerts, (vb + nv) * 3, navDVerts, vbase * 3, 3 * (ndv - nv));
                        vbase += ndv - nv;
                    }
                }

                // Store triangles.
                Array.Copy(option.detailTris, 0, navDTris, 0, 4 * option.detailTriCount);
            }
            else
            {
                // Create dummy detail mesh by triangulating polys.
                int tbase = 0;
                for (int i = 0; i < option.polyCount; ++i)
                {
                    DtPolyDetail dtl = new DtPolyDetail();
                    navDMeshes[i] = dtl;
                    int nv = navPolys[i].vertCount;
                    dtl.vertBase = 0;
                    dtl.vertCount = 0;
                    dtl.triBase = tbase;
                    dtl.triCount = (nv - 2);
                    // Triangulate polygon (local indices).
                    for (int j = 2; j < nv; ++j)
                    {
                        int t = tbase * 4;
                        navDTris[t + 0] = 0;
                        navDTris[t + 1] = (j - 1);
                        navDTris[t + 2] = j;
                        // Bit for each edge that belongs to poly boundary.
                        navDTris[t + 3] = (1 << 2);
                        if (j == 2)
                            navDTris[t + 3] |= (1 << 0);
                        if (j == nv - 1)
                            navDTris[t + 3] |= (1 << 4);
                        tbase++;
                    }
                }
            }

            // Store and create BVtree.
            // TODO: take detail mesh into account! use byte per bbox extent?
            if (option.buildBvTree)
            {
                // Do not set header.bvNodeCount set to make it work look exactly the same as in original Detour
                header.bvNodeCount = CreateBVTree(option, navBvtree);
            }

            // Store Off-Mesh connections.
            n = 0;
            for (int i = 0; i < option.offMeshConCount; ++i)
            {
                // Only store connections which start from this tile.
                if (offMeshConClass[i * 2 + 0] == 0xff)
                {
                    DtOffMeshConnection con = new DtOffMeshConnection();
                    offMeshCons[n] = con;
                    con.poly = (offMeshPolyBase + n);
                    // Copy connection end-points.
                    int endPts = i * 2 * 3;
                    Array.Copy(option.offMeshConVerts, endPts, con.pos, 0, 6);
                    con.rad = option.offMeshConRad[i];
                    con.flags = option.offMeshConDir[i] != 0 ? DtNavMesh.DT_OFFMESH_CON_BIDIR : 0;
                    con.side = offMeshConClass[i * 2 + 1];
                    if (option.offMeshConUserID != null)
                        con.userId = option.offMeshConUserID[i];
                    n++;
                }
            }

            DtMeshData nmd = new DtMeshData();
            nmd.header = header;
            nmd.verts = navVerts;
            nmd.polys = navPolys;
            nmd.detailMeshes = navDMeshes;
            nmd.detailVerts = navDVerts;
            nmd.detailTris = navDTris;
            nmd.bvTree = navBvtree;
            nmd.offMeshCons = offMeshCons;
            return nmd;
        }
    }
}