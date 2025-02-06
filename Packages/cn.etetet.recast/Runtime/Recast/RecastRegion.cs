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
using System.Linq;
using DotRecast.Core;

namespace DotRecast.Recast
{
    using static RcConstants;

    public static class RecastRegion
    {
        const int RC_NULL_NEI = 0xffff;


        public static int CalculateDistanceField(RcCompactHeightfield chf, int[] src)
        {
            int maxDist;
            int w = chf.width;
            int h = chf.height;

            // Init distance and points.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                src[i] = 0xffff;
            }

            // Mark boundary cells.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        int area = chf.areas[i];

                        int nc = 0;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastCommon.GetDirOffsetX(dir);
                                int ay = y + RecastCommon.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                                if (area == chf.areas[ai])
                                {
                                    nc++;
                                }
                            }
                        }

                        if (nc != 4)
                        {
                            src[i] = 0;
                        }
                    }
                }
            }

            // Pass 1
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        if (RecastCommon.GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            // (-1,0)
                            int ax = x + RecastCommon.GetDirOffsetX(0);
                            int ay = y + RecastCommon.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 0);
                            RcCompactSpan @as = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,-1)
                            if (RecastCommon.GetCon(@as, 3) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(3);
                                int aay = ay + RecastCommon.GetDirOffsetY(3);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 3);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }

                        if (RecastCommon.GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            // (0,-1)
                            int ax = x + RecastCommon.GetDirOffsetX(3);
                            int ay = y + RecastCommon.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 3);
                            RcCompactSpan @as = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,-1)
                            if (RecastCommon.GetCon(@as, 2) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(2);
                                int aay = ay + RecastCommon.GetDirOffsetY(2);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 2);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            // Pass 2
            for (int y = h - 1; y >= 0; --y)
            {
                for (int x = w - 1; x >= 0; --x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        if (RecastCommon.GetCon(s, 2) != RC_NOT_CONNECTED)
                        {
                            // (1,0)
                            int ax = x + RecastCommon.GetDirOffsetX(2);
                            int ay = y + RecastCommon.GetDirOffsetY(2);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 2);
                            RcCompactSpan @as = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (1,1)
                            if (RecastCommon.GetCon(@as, 1) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(1);
                                int aay = ay + RecastCommon.GetDirOffsetY(1);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 1);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }

                        if (RecastCommon.GetCon(s, 1) != RC_NOT_CONNECTED)
                        {
                            // (0,1)
                            int ax = x + RecastCommon.GetDirOffsetX(1);
                            int ay = y + RecastCommon.GetDirOffsetY(1);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 1);
                            RcCompactSpan @as = chf.spans[ai];
                            if (src[ai] + 2 < src[i])
                            {
                                src[i] = src[ai] + 2;
                            }

                            // (-1,1)
                            if (RecastCommon.GetCon(@as, 0) != RC_NOT_CONNECTED)
                            {
                                int aax = ax + RecastCommon.GetDirOffsetX(0);
                                int aay = ay + RecastCommon.GetDirOffsetY(0);
                                int aai = chf.cells[aax + aay * w].index + RecastCommon.GetCon(@as, 0);
                                if (src[aai] + 3 < src[i])
                                {
                                    src[i] = src[aai] + 3;
                                }
                            }
                        }
                    }
                }
            }

            maxDist = 0;
            for (int i = 0; i < chf.spanCount; ++i)
            {
                maxDist = Math.Max(src[i], maxDist);
            }

            return maxDist;
        }

        private static int[] BoxBlur(RcCompactHeightfield chf, int thr, int[] src)
        {
            int w = chf.width;
            int h = chf.height;
            int[] dst = new int[chf.spanCount];

            thr *= 2;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        int cd = src[i];
                        if (cd <= thr)
                        {
                            dst[i] = cd;
                            continue;
                        }

                        int d = cd;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastCommon.GetDirOffsetX(dir);
                                int ay = y + RecastCommon.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                                d += src[ai];

                                RcCompactSpan @as = chf.spans[ai];
                                int dir2 = (dir + 1) & 0x3;
                                if (RecastCommon.GetCon(@as, dir2) != RC_NOT_CONNECTED)
                                {
                                    int ax2 = ax + RecastCommon.GetDirOffsetX(dir2);
                                    int ay2 = ay + RecastCommon.GetDirOffsetY(dir2);
                                    int ai2 = chf.cells[ax2 + ay2 * w].index + RecastCommon.GetCon(@as, dir2);
                                    d += src[ai2];
                                }
                                else
                                {
                                    d += cd;
                                }
                            }
                            else
                            {
                                d += cd * 2;
                            }
                        }

                        dst[i] = ((d + 5) / 9);
                    }
                }
            }

            return dst;
        }

        private static bool FloodRegion(int x, int y, int i, int level, int r, RcCompactHeightfield chf, int[] srcReg,
            int[] srcDist, List<int> stack)
        {
            int w = chf.width;

            int area = chf.areas[i];

            // Flood fill mark region.
            stack.Clear();
            stack.Add(x);
            stack.Add(y);
            stack.Add(i);
            srcReg[i] = r;
            srcDist[i] = 0;

            int lev = level >= 2 ? level - 2 : 0;
            int count = 0;

            while (stack.Count > 0)
            {
                int ci = stack[^1];
                stack.RemoveAt(stack.Count - 1);

                int cy = stack[^1];
                stack.RemoveAt(stack.Count - 1);

                int cx = stack[^1];
                stack.RemoveAt(stack.Count - 1);


                RcCompactSpan cs = chf.spans[ci];

                // Check if any of the neighbours already have a valid region set.
                int ar = 0;
                for (int dir = 0; dir < 4; ++dir)
                {
                    // 8 connected
                    if (RecastCommon.GetCon(cs, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + RecastCommon.GetDirOffsetX(dir);
                        int ay = cy + RecastCommon.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(cs, dir);
                        if (chf.areas[ai] != area)
                        {
                            continue;
                        }

                        int nr = srcReg[ai];
                        if ((nr & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        if (nr != 0 && nr != r)
                        {
                            ar = nr;
                            break;
                        }

                        RcCompactSpan @as = chf.spans[ai];

                        int dir2 = (dir + 1) & 0x3;
                        if (RecastCommon.GetCon(@as, dir2) != RC_NOT_CONNECTED)
                        {
                            int ax2 = ax + RecastCommon.GetDirOffsetX(dir2);
                            int ay2 = ay + RecastCommon.GetDirOffsetY(dir2);
                            int ai2 = chf.cells[ax2 + ay2 * w].index + RecastCommon.GetCon(@as, dir2);
                            if (chf.areas[ai2] != area)
                            {
                                continue;
                            }

                            int nr2 = srcReg[ai2];
                            if (nr2 != 0 && nr2 != r)
                            {
                                ar = nr2;
                                break;
                            }
                        }
                    }
                }

                if (ar != 0)
                {
                    srcReg[ci] = 0;
                    continue;
                }

                count++;

                // Expand neighbours.
                for (int dir = 0; dir < 4; ++dir)
                {
                    if (RecastCommon.GetCon(cs, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = cx + RecastCommon.GetDirOffsetX(dir);
                        int ay = cy + RecastCommon.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(cs, dir);
                        if (chf.areas[ai] != area)
                        {
                            continue;
                        }

                        if (chf.dist[ai] >= lev && srcReg[ai] == 0)
                        {
                            srcReg[ai] = r;
                            srcDist[ai] = 0;
                            stack.Add(ax);
                            stack.Add(ay);
                            stack.Add(ai);
                        }
                    }
                }
            }

            return count > 0;
        }

        private static int[] ExpandRegions(int maxIter, int level, RcCompactHeightfield chf, int[] srcReg, int[] srcDist,
            List<int> stack, bool fillStack)
        {
            int w = chf.width;
            int h = chf.height;

            if (fillStack)
            {
                // Find cells revealed by the raised level.
                stack.Clear();
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        RcCompactCell c = chf.cells[x + y * w];
                        for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                        {
                            if (chf.dist[i] >= level && srcReg[i] == 0 && chf.areas[i] != RC_NULL_AREA)
                            {
                                stack.Add(x);
                                stack.Add(y);
                                stack.Add(i);
                            }
                        }
                    }
                }
            }
            else // use cells in the input stack
            {
                // mark all cells which already have a region
                for (int j = 0; j < stack.Count; j += 3)
                {
                    int i = stack[j + 2];
                    if (srcReg[i] != 0)
                    {
                        stack[j + 2] = -1;
                    }
                }
            }

            List<int> dirtyEntries = new List<int>();
            int iter = 0;
            while (stack.Count > 0)
            {
                int failed = 0;
                dirtyEntries.Clear();

                for (int j = 0; j < stack.Count; j += 3)
                {
                    int x = stack[j + 0];
                    int y = stack[j + 1];
                    int i = stack[j + 2];
                    if (i < 0)
                    {
                        failed++;
                        continue;
                    }

                    int r = srcReg[i];
                    int d2 = 0xffff;
                    int area = chf.areas[i];
                    RcCompactSpan s = chf.spans[i];
                    for (int dir = 0; dir < 4; ++dir)
                    {
                        if (RecastCommon.GetCon(s, dir) == RC_NOT_CONNECTED)
                        {
                            continue;
                        }

                        int ax = x + RecastCommon.GetDirOffsetX(dir);
                        int ay = y + RecastCommon.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                        if (chf.areas[ai] != area)
                        {
                            continue;
                        }

                        if (srcReg[ai] > 0 && (srcReg[ai] & RC_BORDER_REG) == 0)
                        {
                            if (srcDist[ai] + 2 < d2)
                            {
                                r = srcReg[ai];
                                d2 = srcDist[ai] + 2;
                            }
                        }
                    }

                    if (r != 0)
                    {
                        stack[j + 2] = -1; // mark as used
                        dirtyEntries.Add(i);
                        dirtyEntries.Add(r);
                        dirtyEntries.Add(d2);
                    }
                    else
                    {
                        failed++;
                    }
                }

                // Copy entries that differ between src and dst to keep them in sync.
                for (int i = 0; i < dirtyEntries.Count; i += 3)
                {
                    int idx = dirtyEntries[i];
                    srcReg[idx] = dirtyEntries[i + 1];
                    srcDist[idx] = dirtyEntries[i + 2];
                }

                if (failed * 3 == stack.Count())
                {
                    break;
                }

                if (level > 0)
                {
                    ++iter;
                    if (iter >= maxIter)
                    {
                        break;
                    }
                }
            }

            return srcReg;
        }

        private static void SortCellsByLevel(int startLevel, RcCompactHeightfield chf, int[] srcReg, int nbStacks,
            List<List<int>> stacks, int loglevelsPerStack) // the levels per stack (2 in our case) as a bit shift
        {
            int w = chf.width;
            int h = chf.height;
            startLevel = startLevel >> loglevelsPerStack;

            for (int j = 0; j < nbStacks; ++j)
            {
                stacks[j].Clear();
            }

            // put all cells in the level range into the appropriate stacks
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (chf.areas[i] == RC_NULL_AREA || srcReg[i] != 0)
                        {
                            continue;
                        }

                        int level = chf.dist[i] >> loglevelsPerStack;
                        int sId = startLevel - level;
                        if (sId >= nbStacks)
                        {
                            continue;
                        }

                        if (sId < 0)
                        {
                            sId = 0;
                        }

                        stacks[sId].Add(x);
                        stacks[sId].Add(y);
                        stacks[sId].Add(i);
                    }
                }
            }
        }

        private static void AppendStacks(List<int> srcStack, List<int> dstStack, int[] srcReg)
        {
            for (int j = 0; j < srcStack.Count; j += 3)
            {
                int i = srcStack[j + 2];
                if ((i < 0) || (srcReg[i] != 0))
                {
                    continue;
                }

                dstStack.Add(srcStack[j]);
                dstStack.Add(srcStack[j + 1]);
                dstStack.Add(srcStack[j + 2]);
            }
        }


        private static void RemoveAdjacentNeighbours(RcRegion reg)
        {
            // Remove adjacent duplicates.
            for (int i = 0; i < reg.connections.Count && reg.connections.Count > 1;)
            {
                int ni = (i + 1) % reg.connections.Count;
                if (reg.connections[i] == reg.connections[ni])
                {
                    reg.connections.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        private static void ReplaceNeighbour(RcRegion reg, int oldId, int newId)
        {
            bool neiChanged = false;
            for (int i = 0; i < reg.connections.Count; ++i)
            {
                if (reg.connections[i] == oldId)
                {
                    reg.connections[i] = newId;
                    neiChanged = true;
                }
            }

            for (int i = 0; i < reg.floors.Count; ++i)
            {
                if (reg.floors[i] == oldId)
                {
                    reg.floors[i] = newId;
                }
            }

            if (neiChanged)
            {
                RemoveAdjacentNeighbours(reg);
            }
        }

        private static bool CanMergeWithRegion(RcRegion rega, RcRegion regb)
        {
            if (rega.areaType != regb.areaType)
            {
                return false;
            }

            int n = 0;
            for (int i = 0; i < rega.connections.Count; ++i)
            {
                if (rega.connections[i] == regb.id)
                {
                    n++;
                }
            }

            if (n > 1)
            {
                return false;
            }

            for (int i = 0; i < rega.floors.Count; ++i)
            {
                if (rega.floors[i] == regb.id)
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddUniqueFloorRegion(RcRegion reg, int n)
        {
            if (!reg.floors.Contains(n))
            {
                reg.floors.Add(n);
            }
        }

        private static bool MergeRegions(RcRegion rega, RcRegion regb)
        {
            int aid = rega.id;
            int bid = regb.id;

            // Duplicate current neighbourhood.
            List<int> acon = new List<int>(rega.connections);
            List<int> bcon = regb.connections;

            // Find insertion point on A.
            int insa = -1;
            for (int i = 0; i < acon.Count; ++i)
            {
                if (acon[i] == bid)
                {
                    insa = i;
                    break;
                }
            }

            if (insa == -1)
            {
                return false;
            }

            // Find insertion point on B.
            int insb = -1;
            for (int i = 0; i < bcon.Count; ++i)
            {
                if (bcon[i] == aid)
                {
                    insb = i;
                    break;
                }
            }

            if (insb == -1)
            {
                return false;
            }

            // Merge neighbours.
            rega.connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            RemoveAdjacentNeighbours(rega);

            for (int j = 0; j < regb.floors.Count; ++j)
            {
                AddUniqueFloorRegion(rega, regb.floors[j]);
            }

            rega.spanCount += regb.spanCount;
            regb.spanCount = 0;
            regb.connections.Clear();

            return true;
        }

        private static bool IsRegionConnectedToBorder(RcRegion reg)
        {
            // Region is connected to border if
            // one of the neighbours is null id.
            return reg.connections.Contains(0);
        }

        private static bool IsSolidEdge(RcCompactHeightfield chf, int[] srcReg, int x, int y, int i, int dir)
        {
            RcCompactSpan s = chf.spans[i];
            int r = 0;
            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + RecastCommon.GetDirOffsetX(dir);
                int ay = y + RecastCommon.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(s, dir);
                r = srcReg[ai];
            }

            if (r == srcReg[i])
            {
                return false;
            }

            return true;
        }

        private static void WalkContour(int x, int y, int i, int dir, RcCompactHeightfield chf, int[] srcReg,
            List<int> cont)
        {
            int startDir = dir;
            int starti = i;

            RcCompactSpan ss = chf.spans[i];
            int curReg = 0;
            if (RecastCommon.GetCon(ss, dir) != RC_NOT_CONNECTED)
            {
                int ax = x + RecastCommon.GetDirOffsetX(dir);
                int ay = y + RecastCommon.GetDirOffsetY(dir);
                int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(ss, dir);
                curReg = srcReg[ai];
            }

            cont.Add(curReg);

            int iter = 0;
            while (++iter < 40000)
            {
                RcCompactSpan s = chf.spans[i];

                if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                {
                    // Choose the edge corner
                    int r = 0;
                    if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                    {
                        int ax = x + RecastCommon.GetDirOffsetX(dir);
                        int ay = y + RecastCommon.GetDirOffsetY(dir);
                        int ai = chf.cells[ax + ay * chf.width].index + RecastCommon.GetCon(s, dir);
                        r = srcReg[ai];
                    }

                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Add(curReg);
                    }

                    dir = (dir + 1) & 0x3; // Rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + RecastCommon.GetDirOffsetX(dir);
                    int ny = y + RecastCommon.GetDirOffsetY(dir);
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

            // Remove adjacent duplicates.
            if (cont.Count > 1)
            {
                for (int j = 0; j < cont.Count;)
                {
                    int nj = (j + 1) % cont.Count;
                    if (cont[j] == cont[nj])
                    {
                        cont.RemoveAt(j);
                    }
                    else
                    {
                        ++j;
                    }
                }
            }
        }

        private static int MergeAndFilterRegions(RcTelemetry ctx, int minRegionArea, int mergeRegionSize, int maxRegionId,
            RcCompactHeightfield chf, int[] srcReg, List<int> overlaps)
        {
            int w = chf.width;
            int h = chf.height;

            int nreg = maxRegionId + 1;
            RcRegion[] regions = new RcRegion[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new RcRegion(i);
            }

            // Find edge of a region and find connections around the contour.
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        int r = srcReg[i];
                        if (r == 0 || r >= nreg)
                        {
                            continue;
                        }

                        RcRegion reg = regions[r];
                        reg.spanCount++;

                        // Update floors.
                        for (int j = c.index; j < ni; ++j)
                        {
                            if (i == j)
                            {
                                continue;
                            }

                            int floorId = srcReg[j];
                            if (floorId == 0 || floorId >= nreg)
                            {
                                continue;
                            }

                            if (floorId == r)
                            {
                                reg.overlap = true;
                            }

                            AddUniqueFloorRegion(reg, floorId);
                        }

                        // Have found contour
                        if (reg.connections.Count > 0)
                        {
                            continue;
                        }

                        reg.areaType = chf.areas[i];

                        // Check if this cell is next to a border.
                        int ndir = -1;
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (IsSolidEdge(chf, srcReg, x, y, i, dir))
                            {
                                ndir = dir;
                                break;
                            }
                        }

                        if (ndir != -1)
                        {
                            // The cell is at border.
                            // Walk around the contour to find all the neighbours.
                            WalkContour(x, y, i, ndir, chf, srcReg, reg.connections);
                        }
                    }
                }
            }

            // Remove too small regions.
            List<int> stack = new List<int>(32);
            List<int> trace = new List<int>(32);
            for (int i = 0; i < nreg; ++i)
            {
                RcRegion reg = regions[i];
                if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
                {
                    continue;
                }

                if (reg.spanCount == 0)
                {
                    continue;
                }

                if (reg.visited)
                {
                    continue;
                }

                // Count the total size of all the connected regions.
                // Also keep track of the regions connects to a tile border.
                bool connectsToBorder = false;
                int spanCount = 0;
                stack.Clear();
                trace.Clear();

                reg.visited = true;
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop
                    int ri = stack[^1];
                    stack.RemoveAt(stack.Count - 1);

                    RcRegion creg = regions[ri];

                    spanCount += creg.spanCount;
                    trace.Add(ri);

                    for (int j = 0; j < creg.connections.Count; ++j)
                    {
                        if ((creg.connections[j] & RC_BORDER_REG) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }

                        RcRegion neireg = regions[creg.connections[j]];
                        if (neireg.visited)
                        {
                            continue;
                        }

                        if (neireg.id == 0 || (neireg.id & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        // Visit
                        stack.Add(neireg.id);
                        neireg.visited = true;
                    }
                }

                // If the accumulated regions size is too small, remove it.
                // Do not remove areas which connect to tile borders
                // as their size cannot be estimated correctly and removing them
                // can potentially remove necessary areas.
                if (spanCount < minRegionArea && !connectsToBorder)
                {
                    // Kill all visited regions.
                    for (int j = 0; j < trace.Count; ++j)
                    {
                        regions[trace[j]].spanCount = 0;
                        regions[trace[j]].id = 0;
                    }
                }
            }

            // Merge too small regions to neighbour regions.
            int mergeCount = 0;
            do
            {
                mergeCount = 0;
                for (int i = 0; i < nreg; ++i)
                {
                    RcRegion reg = regions[i];
                    if (reg.id == 0 || (reg.id & RC_BORDER_REG) != 0)
                    {
                        continue;
                    }

                    if (reg.overlap)
                    {
                        continue;
                    }

                    if (reg.spanCount == 0)
                    {
                        continue;
                    }

                    // Check to see if the region should be merged.
                    if (reg.spanCount > mergeRegionSize && IsRegionConnectedToBorder(reg))
                    {
                        continue;
                    }

                    // Small region with more than 1 connection.
                    // Or region which is not connected to a border at all.
                    // Find smallest neighbour region that connects to this one.
                    int smallest = 0xfffffff;
                    int mergeId = reg.id;
                    for (int j = 0; j < reg.connections.Count; ++j)
                    {
                        if ((reg.connections[j] & RC_BORDER_REG) != 0)
                        {
                            continue;
                        }

                        RcRegion mreg = regions[reg.connections[j]];
                        if (mreg.id == 0 || (mreg.id & RC_BORDER_REG) != 0 || mreg.overlap)
                        {
                            continue;
                        }

                        if (mreg.spanCount < smallest && CanMergeWithRegion(reg, mreg) && CanMergeWithRegion(mreg, reg))
                        {
                            smallest = mreg.spanCount;
                            mergeId = mreg.id;
                        }
                    }

                    // Found new id.
                    if (mergeId != reg.id)
                    {
                        int oldId = reg.id;
                        RcRegion target = regions[mergeId];

                        // Merge neighbours.
                        if (MergeRegions(target, reg))
                        {
                            // Fixup regions pointing to current region.
                            for (int j = 0; j < nreg; ++j)
                            {
                                if (regions[j].id == 0 || (regions[j].id & RC_BORDER_REG) != 0)
                                {
                                    continue;
                                }

                                // If another region was already merged into current region
                                // change the nid of the previous region too.
                                if (regions[j].id == oldId)
                                {
                                    regions[j].id = mergeId;
                                }

                                // Replace the current region with the new one if the
                                // current regions is neighbour.
                                ReplaceNeighbour(regions[j], oldId, mergeId);
                            }

                            mergeCount++;
                        }
                    }
                }
            } while (mergeCount > 0);

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    continue; // Skip nil regions.
                }

                if ((regions[i].id & RC_BORDER_REG) != 0)
                {
                    continue; // Skip external regions.
                }

                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }

                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }

            maxRegionId = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            // Return regions that we found to be overlapping.
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].overlap)
                {
                    overlaps.Add(regions[i].id);
                }
            }

            return maxRegionId;
        }

        private static void AddUniqueConnection(RcRegion reg, int n)
        {
            if (!reg.connections.Contains(n))
            {
                reg.connections.Add(n);
            }
        }

        private static int MergeAndFilterLayerRegions(RcTelemetry ctx, int minRegionArea, int maxRegionId,
            RcCompactHeightfield chf, int[] srcReg, List<int> overlaps)
        {
            int w = chf.width;
            int h = chf.height;

            int nreg = maxRegionId + 1;
            RcRegion[] regions = new RcRegion[nreg];

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions[i] = new RcRegion(i);
            }

            // Find region neighbours and overlapping regions.
            List<int> lregs = new List<int>(32);
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
                        if (ri == 0 || ri >= nreg)
                        {
                            continue;
                        }

                        RcRegion reg = regions[ri];

                        reg.spanCount++;
                        reg.areaType = chf.areas[i];
                        reg.ymin = Math.Min(reg.ymin, s.y);
                        reg.ymax = Math.Max(reg.ymax, s.y);
                        // Collect all region layers.
                        lregs.Add(ri);

                        // Update neighbours
                        for (int dir = 0; dir < 4; ++dir)
                        {
                            if (RecastCommon.GetCon(s, dir) != RC_NOT_CONNECTED)
                            {
                                int ax = x + RecastCommon.GetDirOffsetX(dir);
                                int ay = y + RecastCommon.GetDirOffsetY(dir);
                                int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, dir);
                                int rai = srcReg[ai];
                                if (rai > 0 && rai < nreg && rai != ri)
                                {
                                    AddUniqueConnection(reg, rai);
                                }

                                if ((rai & RC_BORDER_REG) != 0)
                                {
                                    reg.connectsToBorder = true;
                                }
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
                                RcRegion ri = regions[lregs[i]];
                                RcRegion rj = regions[lregs[j]];
                                AddUniqueFloorRegion(ri, lregs[j]);
                                AddUniqueFloorRegion(rj, lregs[i]);
                            }
                        }
                    }
                }
            }

            // Create 2D layers from regions.
            int layerId = 1;

            for (int i = 0; i < nreg; ++i)
            {
                regions[i].id = 0;
            }

            // Merge montone regions to create non-overlapping areas.
            List<int> stack = new List<int>(32);
            for (int i = 1; i < nreg; ++i)
            {
                RcRegion root = regions[i];
                // Skip already visited.
                if (root.id != 0)
                {
                    continue;
                }

                // Start search.
                root.id = layerId;

                stack.Clear();
                stack.Add(i);

                while (stack.Count > 0)
                {
                    // Pop front
                    var idx = stack[0];
                    stack.RemoveAt(0);
                    RcRegion reg = regions[idx];

                    int ncons = reg.connections.Count;
                    for (int j = 0; j < ncons; ++j)
                    {
                        int nei = reg.connections[j];
                        RcRegion regn = regions[nei];
                        // Skip already visited.
                        if (regn.id != 0)
                        {
                            continue;
                        }

                        // Skip if different area type, do not connect regions with different area type.
                        if (reg.areaType != regn.areaType)
                        {
                            continue;
                        }

                        // Skip if the neighbour is overlapping root region.
                        bool overlap = false;
                        for (int k = 0; k < root.floors.Count; k++)
                        {
                            if (root.floors[k] == nei)
                            {
                                overlap = true;
                                break;
                            }
                        }

                        if (overlap)
                        {
                            continue;
                        }

                        // Deepen
                        stack.Add(nei);

                        // Mark layer id
                        regn.id = layerId;
                        // Merge current layers to root.
                        for (int k = 0; k < regn.floors.Count; ++k)
                        {
                            AddUniqueFloorRegion(root, regn.floors[k]);
                        }

                        root.ymin = Math.Min(root.ymin, regn.ymin);
                        root.ymax = Math.Max(root.ymax, regn.ymax);
                        root.spanCount += regn.spanCount;
                        regn.spanCount = 0;
                        root.connectsToBorder = root.connectsToBorder || regn.connectsToBorder;
                    }
                }

                layerId++;
            }

            // Remove small regions
            for (int i = 0; i < nreg; ++i)
            {
                if (regions[i].spanCount > 0 && regions[i].spanCount < minRegionArea && !regions[i].connectsToBorder)
                {
                    int reg = regions[i].id;
                    for (int j = 0; j < nreg; ++j)
                    {
                        if (regions[j].id == reg)
                        {
                            regions[j].id = 0;
                        }
                    }
                }
            }

            // Compress region Ids.
            for (int i = 0; i < nreg; ++i)
            {
                regions[i].remap = false;
                if (regions[i].id == 0)
                {
                    continue; // Skip nil regions.
                }

                if ((regions[i].id & RC_BORDER_REG) != 0)
                {
                    continue; // Skip external regions.
                }

                regions[i].remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < nreg; ++i)
            {
                if (!regions[i].remap)
                {
                    continue;
                }

                int oldId = regions[i].id;
                int newId = ++regIdGen;
                for (int j = i; j < nreg; ++j)
                {
                    if (regions[j].id == oldId)
                    {
                        regions[j].id = newId;
                        regions[j].remap = false;
                    }
                }
            }

            maxRegionId = regIdGen;

            // Remap regions.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                if ((srcReg[i] & RC_BORDER_REG) == 0)
                {
                    srcReg[i] = regions[srcReg[i]].id;
                }
            }

            return maxRegionId;
        }

        /// @par
        ///
        /// This is usually the second to the last step in creating a fully built
        /// compact heightfield. This step is required before regions are built
        /// using #rcBuildRegions or #rcBuildRegionsMonotone.
        ///
        /// After this step, the distance data is available via the rcCompactHeightfield::maxDistance
        /// and rcCompactHeightfield::dist fields.
        ///
        /// @see rcCompactHeightfield, rcBuildRegions, rcBuildRegionsMonotone
        public static void BuildDistanceField(RcTelemetry ctx, RcCompactHeightfield chf)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD);

            int[] src = new int[chf.spanCount];

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_DIST);
            int maxDist = CalculateDistanceField(chf, src);
            chf.maxDistance = maxDist;
            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_DIST);

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_BLUR);

            // Blur
            src = BoxBlur(chf, 1, src);

            // Store distance.
            chf.dist = src;

            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_DISTANCEFIELD_BLUR);
        }

        private static void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, RcCompactHeightfield chf,
            int[] srcReg)
        {
            int w = chf.width;
            for (int y = miny; y < maxy; ++y)
            {
                for (int x = minx; x < maxx; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (chf.areas[i] != RC_NULL_AREA)
                        {
                            srcReg[i] = regId;
                        }
                    }
                }
            }
        }

        /// @par
        ///
        /// Non-null regions will consist of connected, non-overlapping walkable spans that form a single contour.
        /// Contours will form simple polygons.
        ///
        /// If multiple regions form an area that is smaller than @p minRegionArea, then all spans will be
        /// re-assigned to the zero (null) region.
        ///
        /// Partitioning can result in smaller than necessary regions. @p mergeRegionArea helps
        /// reduce unnecessarily small regions.
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// The region data will be available via the rcCompactHeightfield::maxRegions
        /// and rcCompactSpan::reg fields.
        ///
        /// @warning The distance field must be created using #rcBuildDistanceField before attempting to build regions.
        ///
        /// @see rcCompactHeightfield, rcCompactSpan, rcBuildDistanceField, rcBuildRegionsMonotone, rcConfig
        public static void BuildRegionsMonotone(RcTelemetry ctx, RcCompactHeightfield chf, int minRegionArea,
            int mergeRegionArea)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS);

            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;
            int id = 1;

            int[] srcReg = new int[chf.spanCount];

            int nsweeps = Math.Max(chf.width, chf.height);
            RcSweepSpan[] sweeps = new RcSweepSpan[nsweeps];
            for (int i = 0; i < sweeps.Length; i++)
            {
                sweeps[i] = new RcSweepSpan();
            }

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
            }

            int[] prev = new int[1024];

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                if (prev.Length < id * 2)
                {
                    prev = new int[id * 2];
                }
                else
                {
                    Array.Fill(prev, 0, 0, (id) - (0));
                }

                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (chf.areas[i] == RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (RecastCommon.GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastCommon.GetDirOffsetX(0);
                            int ay = y + RecastCommon.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].rid = previd;
                            sweeps[previd].ns = 0;
                            sweeps[previd].nei = 0;
                        }

                        // -y
                        if (RecastCommon.GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastCommon.GetDirOffsetX(3);
                            int ay = y + RecastCommon.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].nei == 0 || sweeps[previd].nei == nr)
                                {
                                    sweeps[previd].nei = nr;
                                    sweeps[previd].ns++;
                                    if (prev.Length <= nr)
                                    {
                                        Array.Resize(ref prev, prev.Length * 2);
                                    }

                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].nei = RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].nei != RC_NULL_NEI && sweeps[i].nei != 0 && prev[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        sweeps[i].id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].id;
                        }
                    }
                }
            }

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);
            // Merge regions and filter out small regions.
            List<int> overlaps = new List<int>();
            chf.maxRegions = MergeAndFilterRegions(ctx, minRegionArea, mergeRegionArea, id, chf, srcReg, overlaps);

            // Monotone partitioning does not generate overlapping regions.
            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

            // Store the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }
        }

        /// @par
        ///
        /// Non-null regions will consist of connected, non-overlapping walkable spans that form a single contour.
        /// Contours will form simple polygons.
        ///
        /// If multiple regions form an area that is smaller than @p minRegionArea, then all spans will be
        /// re-assigned to the zero (null) region.
        ///
        /// Watershed partitioning can result in smaller than necessary regions, especially in diagonal corridors.
        /// @p mergeRegionArea helps reduce unnecessarily small regions.
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// The region data will be available via the rcCompactHeightfield::maxRegions
        /// and rcCompactSpan::reg fields.
        ///
        /// @warning The distance field must be created using #rcBuildDistanceField before attempting to build regions.
        ///
        /// @see rcCompactHeightfield, rcCompactSpan, rcBuildDistanceField, rcBuildRegionsMonotone, rcConfig
        public static void BuildRegions(RcTelemetry ctx, RcCompactHeightfield chf, int minRegionArea,
            int mergeRegionArea)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS);

            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_WATERSHED);

            int LOG_NB_STACKS = 3;
            int NB_STACKS = 1 << LOG_NB_STACKS;
            List<List<int>> lvlStacks = new List<List<int>>();
            for (int i = 0; i < NB_STACKS; ++i)
            {
                lvlStacks.Add(new List<int>(1024));
            }

            List<int> stack = new List<int>(1024);

            int[] srcReg = new int[chf.spanCount];
            int[] srcDist = new int[chf.spanCount];

            int regionId = 1;
            int level = (chf.maxDistance + 1) & ~1;

            // TODO: Figure better formula, expandIters defines how much the
            // watershed "overflows" and simplifies the regions. Tying it to
            // agent radius was usually good indication how greedy it could be.
            // readonly int expandIters = 4 + walkableRadius * 2;
            int expandIters = 8;

            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, regionId | RC_BORDER_REG, chf, srcReg);
                regionId++;
                PaintRectRegion(w - bw, w, 0, h, regionId | RC_BORDER_REG, chf, srcReg);
                regionId++;
                PaintRectRegion(0, w, 0, bh, regionId | RC_BORDER_REG, chf, srcReg);
                regionId++;
                PaintRectRegion(0, w, h - bh, h, regionId | RC_BORDER_REG, chf, srcReg);
                regionId++;
            }

            chf.borderSize = borderSize;

            int sId = -1;
            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;
                sId = (sId + 1) & (NB_STACKS - 1);

                // ctx->StartTimer(RC_TIMER_DIVIDE_TO_LEVELS);

                if (sId == 0)
                {
                    SortCellsByLevel(level, chf, srcReg, NB_STACKS, lvlStacks, 1);
                }
                else
                {
                    AppendStacks(lvlStacks[sId - 1], lvlStacks[sId], srcReg); // copy left overs from last level
                }

                // ctx->StopTimer(RC_TIMER_DIVIDE_TO_LEVELS);

                ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_EXPAND);

                // Expand current regions until no empty connected cells found.
                ExpandRegions(expandIters, level, chf, srcReg, srcDist, lvlStacks[sId], false);

                ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_EXPAND);

                ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FLOOD);

                // Mark new regions with IDs.
                for (int j = 0; j < lvlStacks[sId].Count; j += 3)
                {
                    int x = lvlStacks[sId][j];
                    int y = lvlStacks[sId][j + 1];
                    int i = lvlStacks[sId][j + 2];
                    if (i >= 0 && srcReg[i] == 0)
                    {
                        if (FloodRegion(x, y, i, level, regionId, chf, srcReg, srcDist, stack))
                        {
                            regionId++;
                        }
                    }
                }

                ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FLOOD);
            }

            // Expand current regions until no empty connected cells found.
            ExpandRegions(expandIters * 8, 0, chf, srcReg, srcDist, stack, true);

            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_WATERSHED);

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

            // Merge regions and filter out small regions.
            List<int> overlaps = new List<int>();
            chf.maxRegions = MergeAndFilterRegions(ctx, minRegionArea, mergeRegionArea, regionId, chf, srcReg, overlaps);

            // If overlapping regions were found during merging, split those regions.
            if (overlaps.Count > 0)
            {
                ctx.Warn("rcBuildRegions: " + overlaps.Count + " overlapping regions.");
            }

            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

            // Write the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }
        }

        public static void BuildLayerRegions(RcTelemetry ctx, RcCompactHeightfield chf, int minRegionArea)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS);

            int w = chf.width;
            int h = chf.height;
            int borderSize = chf.borderSize;
            int id = 1;

            int[] srcReg = new int[chf.spanCount];
            int nsweeps = Math.Max(chf.width, chf.height);
            RcSweepSpan[] sweeps = new RcSweepSpan[nsweeps];
            for (int i = 0; i < sweeps.Length; i++)
            {
                sweeps[i] = new RcSweepSpan();
            }

            // Mark border regions.
            if (borderSize > 0)
            {
                // Make sure border will not overflow.
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);
                // Paint regions
                PaintRectRegion(0, bw, 0, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(w - bw, w, 0, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(0, w, 0, bh, id | RC_BORDER_REG, chf, srcReg);
                id++;
                PaintRectRegion(0, w, h - bh, h, id | RC_BORDER_REG, chf, srcReg);
                id++;
            }

            int[] prev = new int[1024];

            // Sweep one line at a time.
            for (int y = borderSize; y < h - borderSize; ++y)
            {
                // Collect spans from this row.
                if (prev.Length <= id * 2)
                {
                    prev = new int[id * 2];
                }
                else
                {
                    Array.Fill(prev, 0, 0, (id) - (0));
                }

                int rid = 1;

                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];
                        if (chf.areas[i] == RC_NULL_AREA)
                        {
                            continue;
                        }

                        // -x
                        int previd = 0;
                        if (RecastCommon.GetCon(s, 0) != RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastCommon.GetDirOffsetX(0);
                            int ay = y + RecastCommon.GetDirOffsetY(0);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 0);
                            if ((srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                previd = srcReg[ai];
                            }
                        }

                        if (previd == 0)
                        {
                            previd = rid++;
                            sweeps[previd].rid = previd;
                            sweeps[previd].ns = 0;
                            sweeps[previd].nei = 0;
                        }

                        // -y
                        if (RecastCommon.GetCon(s, 3) != RC_NOT_CONNECTED)
                        {
                            int ax = x + RecastCommon.GetDirOffsetX(3);
                            int ay = y + RecastCommon.GetDirOffsetY(3);
                            int ai = chf.cells[ax + ay * w].index + RecastCommon.GetCon(s, 3);
                            if (srcReg[ai] != 0 && (srcReg[ai] & RC_BORDER_REG) == 0 && chf.areas[i] == chf.areas[ai])
                            {
                                int nr = srcReg[ai];
                                if (sweeps[previd].nei == 0 || sweeps[previd].nei == nr)
                                {
                                    sweeps[previd].nei = nr;
                                    sweeps[previd].ns++;
                                    if (prev.Length <= nr)
                                    {
                                        Array.Resize(ref prev, prev.Length * 2);
                                    }

                                    prev[nr]++;
                                }
                                else
                                {
                                    sweeps[previd].nei = RC_NULL_NEI;
                                }
                            }
                        }

                        srcReg[i] = previd;
                    }
                }

                // Create unique ID.
                for (int i = 1; i < rid; ++i)
                {
                    if (sweeps[i].nei != RC_NULL_NEI && sweeps[i].nei != 0 && prev[sweeps[i].nei] == sweeps[i].ns)
                    {
                        sweeps[i].id = sweeps[i].nei;
                    }
                    else
                    {
                        sweeps[i].id = id++;
                    }
                }

                // Remap IDs
                for (int x = borderSize; x < w - borderSize; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];

                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        if (srcReg[i] > 0 && srcReg[i] < rid)
                        {
                            srcReg[i] = sweeps[srcReg[i]].id;
                        }
                    }
                }
            }

            ctx.StartTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

            // Merge monotone regions to layers and remove small regions.
            List<int> overlaps = new List<int>();
            chf.maxRegions = MergeAndFilterLayerRegions(ctx, minRegionArea, id, chf, srcReg, overlaps);

            ctx.StopTimer(RcTimerLabel.RC_TIMER_BUILD_REGIONS_FILTER);

            // Store the result out.
            for (int i = 0; i < chf.spanCount; ++i)
            {
                chf.spans[i].reg = srcReg[i];
            }
        }
    }
}