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

namespace DotRecast.Recast.Geom
{
    public class RcChunkyTriMesh
    {
        private List<RcChunkyTriMeshNode> nodes;
        private int ntris;
        private int maxTrisPerChunk;

        private void CalcExtends(BoundsItem[] items, int imin, int imax, ref RcVec2f bmin, ref RcVec2f bmax)
        {
            bmin.x = items[imin].bmin.x;
            bmin.y = items[imin].bmin.y;

            bmax.x = items[imin].bmax.x;
            bmax.y = items[imin].bmax.y;

            for (int i = imin + 1; i < imax; ++i)
            {
                BoundsItem it = items[i];
                if (it.bmin.x < bmin.x)
                {
                    bmin.x = it.bmin.x;
                }

                if (it.bmin.y < bmin.y)
                {
                    bmin.y = it.bmin.y;
                }

                if (it.bmax.x > bmax.x)
                {
                    bmax.x = it.bmax.x;
                }

                if (it.bmax.y > bmax.y)
                {
                    bmax.y = it.bmax.y;
                }
            }
        }

        private int LongestAxis(float x, float y)
        {
            return y > x ? 1 : 0;
        }

        private void Subdivide(BoundsItem[] items, int imin, int imax, int trisPerChunk, List<RcChunkyTriMeshNode> nodes, int[] inTris)
        {
            int inum = imax - imin;

            RcChunkyTriMeshNode node = new RcChunkyTriMeshNode();
            nodes.Add(node);

            if (inum <= trisPerChunk)
            {
                // Leaf
                CalcExtends(items, imin, imax, ref node.bmin, ref node.bmax);

                // Copy triangles.
                node.i = nodes.Count;
                node.tris = new int[inum * 3];

                int dst = 0;
                for (int i = imin; i < imax; ++i)
                {
                    int src = items[i].i * 3;
                    node.tris[dst++] = inTris[src];
                    node.tris[dst++] = inTris[src + 1];
                    node.tris[dst++] = inTris[src + 2];
                }
            }
            else
            {
                // Split
                CalcExtends(items, imin, imax, ref node.bmin, ref node.bmax);

                int axis = LongestAxis(node.bmax.x - node.bmin.x, node.bmax.y - node.bmin.y);

                if (axis == 0)
                {
                    Array.Sort(items, imin, imax - imin, BoundsItemXComparer.Shared);
                    // Sort along x-axis
                }
                else if (axis == 1)
                {
                    Array.Sort(items, imin, imax - imin, BoundsItemYComparer.Shared);
                    // Sort along y-axis
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items, imin, isplit, trisPerChunk, nodes, inTris);
                // Right
                Subdivide(items, isplit, imax, trisPerChunk, nodes, inTris);

                // Negative index means escape.
                node.i = -nodes.Count;
            }
        }

        public RcChunkyTriMesh(float[] verts, int[] tris, int ntris, int trisPerChunk)
        {
            int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;

            nodes = new List<RcChunkyTriMeshNode>(nchunks);
            this.ntris = ntris;

            // Build tree
            BoundsItem[] items = new BoundsItem[ntris];

            for (int i = 0; i < ntris; i++)
            {
                int t = i * 3;
                BoundsItem it = items[i] = new BoundsItem();
                it.i = i;
                // Calc triangle XZ bounds.
                it.bmin.x = it.bmax.x = verts[tris[t] * 3 + 0];
                it.bmin.y = it.bmax.y = verts[tris[t] * 3 + 2];
                for (int j = 1; j < 3; ++j)
                {
                    int v = tris[t + j] * 3;
                    if (verts[v] < it.bmin.x)
                    {
                        it.bmin.x = verts[v];
                    }

                    if (verts[v + 2] < it.bmin.y)
                    {
                        it.bmin.y = verts[v + 2];
                    }

                    if (verts[v] > it.bmax.x)
                    {
                        it.bmax.x = verts[v];
                    }

                    if (verts[v + 2] > it.bmax.y)
                    {
                        it.bmax.y = verts[v + 2];
                    }
                }
            }

            Subdivide(items, 0, ntris, trisPerChunk, nodes, tris);

            // Calc max tris per node.
            maxTrisPerChunk = 0;
            foreach (RcChunkyTriMeshNode node in nodes)
            {
                bool isLeaf = node.i >= 0;
                if (!isLeaf)
                {
                    continue;
                }

                if (node.tris.Length / 3 > maxTrisPerChunk)
                {
                    maxTrisPerChunk = node.tris.Length / 3;
                }
            }
        }

        private bool CheckOverlapRect(float[] amin, float[] amax, RcVec2f bmin, RcVec2f bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax.x || amax[0] < bmin.x) ? false : overlap;
            overlap = (amin[1] > bmax.y || amax[1] < bmin.y) ? false : overlap;
            return overlap;
        }

        public List<RcChunkyTriMeshNode> GetChunksOverlappingRect(float[] bmin, float[] bmax)
        {
            // Traverse tree
            List<RcChunkyTriMeshNode> ids = new List<RcChunkyTriMeshNode>();
            int i = 0;
            while (i < nodes.Count)
            {
                RcChunkyTriMeshNode node = nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    ids.Add(node);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    i = -node.i;
                }
            }

            return ids;
        }

        public List<RcChunkyTriMeshNode> GetChunksOverlappingSegment(float[] p, float[] q)
        {
            // Traverse tree
            List<RcChunkyTriMeshNode> ids = new List<RcChunkyTriMeshNode>();
            int i = 0;
            while (i < nodes.Count)
            {
                RcChunkyTriMeshNode node = nodes[i];
                bool overlap = CheckOverlapSegment(p, q, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    ids.Add(node);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    i = -node.i;
                }
            }

            return ids;
        }

        private bool CheckOverlapSegment(float[] p, float[] q, RcVec2f bmin, RcVec2f bmax)
        {
            float EPSILON = 1e-6f;

            float tmin = 0;
            float tmax = 1;
            float[] d = new float[2];
            d[0] = q[0] - p[0];
            d[1] = q[1] - p[1];

            for (int i = 0; i < 2; i++)
            {
                if (Math.Abs(d[i]) < EPSILON)
                {
                    // Ray is parallel to slab. No hit if origin not within slab
                    if (p[i] < bmin.Get(i) || p[i] > bmax.Get(i))
                        return false;
                }
                else
                {
                    // Compute intersection t value of ray with near and far plane of slab
                    float ood = 1.0f / d[i];
                    float t1 = (bmin.Get(i) - p[i]) * ood;
                    float t2 = (bmax.Get(i) - p[i]) * ood;
                    if (t1 > t2)
                    {
                        (t1, t2) = (t2, t1);
                    }

                    if (t1 > tmin)
                        tmin = t1;
                    if (t2 < tmax)
                        tmax = t2;
                    if (tmin > tmax)
                        return false;
                }
            }

            return true;
        }
    }
}