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
    using static RecastCommon;
    using static RcConstants;

    public static class RecastLayers
    {
        const int RC_MAX_LAYERS = RcConstants.RC_NOT_CONNECTED;
        const int RC_MAX_NEIS = 16;


        private static void AddUnique(List<int> a, int v)
        {
            if (!a.Contains(v))
            {
                a.Add(v);
            }
        }

        private static bool Contains(List<int> a, int v)
        {
            return a.Contains(v);
        }

        private static bool OverlapRange(int amin, int amax, int bmin, int bmax)
        {
            return (amin > bmax || amax < bmin) ? false : true;
        }

        public static RcHeightfieldLayerSet BuildHeightfieldLayers(RcTelemetry ctx, RcCompactHeightfield chf, int walkableHeight)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_LAYERS);
            
            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;
            int[] srcReg = new int[chf.spanCount];
            Array.Fill(srcReg, 0xFF);
            int nsweeps = chf.width; // Math.Max(chf.width, chf.height);
            RcSweepSpan[] sweeps = new RcSweepSpan[nsweeps];
            for (int i = 0; i < sweeps.Length; i++)
            {
                sweeps[i] = new RcSweepSpan();
            }

            // Partition walkable area into monotone regions.
            int[] prevCount = new int[256];
            int regId = 0;
            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                Array.Fill(prevCount, 0, 0, (regId) - (0));
                int sweepId = 0;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (chf.areas[i] == RC_NULL_AREA)
                            continue;
                        int sid = 0xFF;
                        // -x

                        if (GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(0);
                            int ay = y + GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 0);
                            if (chf.areas[ai] != RC_NULL_AREA && srcReg[ai] != 0xff)
                                sid = srcReg[ai];
                        }

                        if (sid == 0xff)
                        {
                            sid = sweepId++;
                            sweeps[sid].nei = 0xff;
                            sweeps[sid].ns = 0;
                        }

                        // -y
                        if (GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + GetDirOffsetX(3);
                            int ay = y + GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + GetCon(s, 3);
                            int nr = srcReg[ai];
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
                                    // This is hit if there is nore than one
                                    // neighbour.
                                    // Invalidate the neighbour.
                                    sweeps[sid].nei = 0xff;
                                }
                            }
                        }

                        srcReg[i] = sid;
                    }
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
                            throw new Exception("rcBuildHeightfieldLayers: Region ID overflow.");
                        }

                        sweeps[i].id = regId++;
                    }
                }

                // Remap local sweep ids to region ids.
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (srcReg[i] != 0xff)
                            srcReg[i] = sweeps[srcReg[i]].id;
                    }
                }
            }

            int nregs = regId;
            RcLayerRegion[] regs = new RcLayerRegion[nregs];

            // Construct regions
            for (int i = 0; i < nregs; ++i)
            {
                regs[i] = new RcLayerRegion(i);
            }

            // Find region neighbours and overlapping regions.
            List<int> lregs = new List<int>();
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    lregs.Clear();

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        int ri = srcReg[i];
                        if (ri == 0xff)
                            continue;

                        regs[ri].ymin = Math.Min(regs[ri].ymin, s.y);
                        regs[ri].ymax = Math.Max(regs[ri].ymax, s.y);

                        // Collect all region layers.
                        lregs.Add(ri);

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + GetDirOffsetX(dir);
                                int ay = y + GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai != 0xff && rai != ri)
                                    AddUnique(regs[ri].neis, rai);
                            }
                        }
                    }

                    // Update overlapping regions.
                    for (int i = 0; i < lregs.Count - 1; ++i)
                    {
                        for (int j = i + 1; j < lregs.Count; ++j)
                        {
                            if (lregs[i] != lregs[j])
                            {
                                RcLayerRegion ri = regs[lregs[i]];
                                RcLayerRegion rj = regs[lregs[j]];
                                AddUnique(ri.layers, lregs[j]);
                                AddUnique(rj.layers, lregs[i]);
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 0;

            List<int> stack = new List<int>();

            for (int i = 0; i < nregs; ++i)
            {
                RcLayerRegion root = regs[i];
                // Skip already visited.
                if (root.layerId != 0xff)
                    continue;

                // Start search.
                root.layerId = layerId;
                root.@base = true;

                stack.Add(i);

                while (stack.Count != 0)
                {
                    // Pop front
                    int pop = stack[0]; // TODO : 여기에 stack 처럼 작동하게 했는데, 스택인지는 모르겠음
                    stack.RemoveAt(0);
                    RcLayerRegion reg = regs[pop];

                    foreach (int nei in reg.neis)
                    {
                        RcLayerRegion regn = regs[nei];
                        // Skip already visited.
                        if (regn.layerId != 0xff)
                            continue;
                        // Skip if the neighbour is overlapping root region.
                        if (Contains(root.layers, nei))
                            continue;
                        // Skip if the height range would become too large.
                        int ymin = Math.Min(root.ymin, regn.ymin);
                        int ymax = Math.Max(root.ymax, regn.ymax);
                        if ((ymax - ymin) >= 255)
                            continue;

                        // Deepen
                        stack.Add(nei);

                        // Mark layer id
                        regn.layerId = layerId;
                        // Merge current layers to root.
                        foreach (int layer in regn.layers)
                            AddUnique(root.layers, layer);
                        root.ymin = Math.Min(root.ymin, regn.ymin);
                        root.ymax = Math.Max(root.ymax, regn.ymax);
                    }
                }

                layerId++;
            }

            // Merge non-overlapping regions that are close in height.
            int mergeHeight = walkableHeight * 4;

            for (int i = 0; i < nregs; ++i)
            {
                RcLayerRegion ri = regs[i];
                if (!ri.@base)
                    continue;

                int newId = ri.layerId;

                for (;;)
                {
                    int oldId = 0xff;

                    for (int j = 0; j < nregs; ++j)
                    {
                        if (i == j)
                            continue;
                        RcLayerRegion rj = regs[j];
                        if (!rj.@base)
                            continue;

                        // Skip if the regions are not close to each other.
                        if (!OverlapRange(ri.ymin, ri.ymax + mergeHeight, rj.ymin, rj.ymax + mergeHeight))
                            continue;
                        // Skip if the height range would become too large.
                        int ymin = Math.Min(ri.ymin, rj.ymin);
                        int ymax = Math.Max(ri.ymax, rj.ymax);
                        if ((ymax - ymin) >= 255)
                            continue;

                        // Make sure that there is no overlap when merging 'ri' and
                        // 'rj'.
                        bool overlap = false;
                        // Iterate over all regions which have the same layerId as
                        // 'rj'
                        for (int k = 0; k < nregs; ++k)
                        {
                            if (regs[k].layerId != rj.layerId)
                                continue;
                            // Check if region 'k' is overlapping region 'ri'
                            // Index to 'regs' is the same as region id.
                            if (Contains(ri.layers, k))
                            {
                                overlap = true;
                                break;
                            }
                        }

                        // Cannot merge of regions overlap.
                        if (overlap)
                            continue;

                        // Can merge i and j.
                        oldId = rj.layerId;
                        break;
                    }

                    // Could not find anything to merge with, stop.
                    if (oldId == 0xff)
                        break;

                    // Merge
                    for (int j = 0; j < nregs; ++j)
                    {
                        RcLayerRegion rj = regs[j];
                        if (rj.layerId == oldId)
                        {
                            rj.@base = false;
                            // Remap layerIds.
                            rj.layerId = newId;
                            // Add overlaid layers from 'rj' to 'ri'.
                            foreach (int layer in rj.layers)
                                AddUnique(ri.layers, layer);
                            // Update height bounds.
                            ri.ymin = Math.Min(ri.ymin, rj.ymin);
                            ri.ymax = Math.Max(ri.ymax, rj.ymax);
                        }
                    }
                }
            }

            // Compact layerIds
            int[] remap = new int[256];

            // Find number of unique layers.
            layerId = 0;
            for (int i = 0; i < nregs; ++i)
                remap[regs[i].layerId] = 1;
            for (int i = 0; i < 256; ++i)
            {
                if (remap[i] != 0)
                    remap[i] = layerId++;
                else
                    remap[i] = 0xff;
            }

            // Remap ids.
            for (int i = 0; i < nregs; ++i)
                regs[i].layerId = remap[regs[i].layerId];

            // No layers, return empty.
            if (layerId == 0)
            {
                // ctx.Stop(RC_TIMER_BUILD_LAYERS);
                return null;
            }

            // Create layers.
            // RcAssert(lset.layers == 0);

            int lw = w - borderSize * 2;
            int lh = h - borderSize * 2;

            // Build contracted bbox for layers.
            RcVec3f bmin = chf.bmin;
            RcVec3f bmax = chf.bmax;
            bmin.x += borderSize * chf.cs;
            bmin.z += borderSize * chf.cs;
            bmax.x -= borderSize * chf.cs;
            bmax.z -= borderSize * chf.cs;

            RcHeightfieldLayerSet lset = new RcHeightfieldLayerSet();
            lset.layers = new RcHeightfieldLayer[layerId];
            for (int i = 0; i < lset.layers.Length; i++)
            {
                lset.layers[i] = new RcHeightfieldLayer();
            }

            // Store layers.
            for (int i = 0; i < lset.layers.Length; ++i)
            {
                int curId = i;

                RcHeightfieldLayer layer = lset.layers[i];

                int gridSize = lw * lh;

                layer.heights = new int[gridSize];
                Array.Fill(layer.heights, 0xFF);
                layer.areas = new int[gridSize];
                layer.cons = new int[gridSize];

                // Find layer height bounds.
                int hmin = 0, hmax = 0;
                for (int j = 0; j < nregs; ++j)
                {
                    if (regs[j].@base && regs[j].layerId == curId)
                    {
                        hmin = regs[j].ymin;
                        hmax = regs[j].ymax;
                    }
                }

                layer.width = lw;
                layer.height = lh;
                layer.cs = chf.cs;
                layer.ch = chf.ch;

                // Adjust the bbox to fit the heightfield.
                layer.bmin = bmin;
                layer.bmax = bmax;
                layer.bmin.y = bmin.y + hmin * chf.ch;
                layer.bmax.y = bmin.y + hmax * chf.ch;
                layer.hmin = hmin;
                layer.hmax = hmax;

                // Update usable data region.
                layer.minx = layer.width;
                layer.maxx = 0;
                layer.miny = layer.height;
                layer.maxy = 0;

                // Copy height and area from compact heightfield.
                for (int y = 0; y < lh; ++y)
                {
                    for (int x = 0; x < lw; ++x)
                    {
                        int cx = borderSize + x;
                        int cy = borderSize + y;
                        RcCompactCell c = chf.cells[cx + cy * w];
                        for (int j = c.index, nj = c.index + c.count; j < nj; ++j)
                        {
                            RcCompactSpan s = chf.spans[j];
                            // Skip unassigned regions.
                            if (srcReg[j] == 0xff)
                                continue;
                            // Skip of does nto belong to current layer.
                            int lid = regs[srcReg[j]].layerId;
                            if (lid != curId)
                                continue;

                            // Update data bounds.
                            layer.minx = Math.Min(layer.minx, x);
                            layer.maxx = Math.Max(layer.maxx, x);
                            layer.miny = Math.Min(layer.miny, y);
                            layer.maxy = Math.Max(layer.maxy, y);

                            // Store height and area type.
                            int idx = x + y * lw;
                            layer.heights[idx] = (char)(s.y - hmin);
                            layer.areas[idx] = chf.areas[j];

                            // Check connection.
                            char portal = (char)0;
                            char con = (char)0;
                            for (int dir = 0; dir < 4; ++dir)
                            {
                                if (GetCon(s, dir) != RC_NOT_CONNECTED)
                                {
                                    int ax = cx + GetDirOffsetX(dir);
                                    int ay = cy + GetDirOffsetY(dir);
                                    int ai = chf.cells[ax + ay * w].index + GetCon(s, dir);
                                    int alid = srcReg[ai] != 0xff ? regs[srcReg[ai]].layerId : 0xff;
                                    // Portal mask
                                    if (chf.areas[ai] != RC_NULL_AREA && lid != alid)
                                    {
                                        portal |= (char)(1 << dir);
                                        // Update height so that it matches on both
                                        // sides of the portal.
                                        RcCompactSpan @as = chf.spans[ai];
                                        if (@as.y > hmin)
                                            layer.heights[idx] = Math.Max(layer.heights[idx], (char)(@as.y - hmin));
                                    }

                                    // Valid connection mask
                                    if (chf.areas[ai] != RC_NULL_AREA && lid == alid)
                                    {
                                        int nx = ax - borderSize;
                                        int ny = ay - borderSize;
                                        if (nx >= 0 && ny >= 0 && nx < lw && ny < lh)
                                            con |= (char)(1 << dir);
                                    }
                                }
                            }

                            layer.cons[idx] = (portal << 4) | con;
                        }
                    }
                }

                if (layer.minx > layer.maxx)
                    layer.minx = layer.maxx = 0;
                if (layer.miny > layer.maxy)
                    layer.miny = layer.maxy = 0;
            }

            // ctx->StopTimer(RC_TIMER_BUILD_LAYERS);
            return lset;
        }
    }
}