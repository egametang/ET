/*
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
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
using static DotRecast.Core.RcMath;
using static DotRecast.Recast.RcConstants;


namespace DotRecast.Recast
{
    public static class RecastCompact
    {
        private const int MAX_LAYERS = RC_NOT_CONNECTED - 1;
        private const int MAX_HEIGHT = RcConstants.SPAN_MAX_HEIGHT;

        /// @par
        ///
        /// This is just the beginning of the process of fully building a compact heightfield.
        /// Various filters may be applied, then the distance field and regions built.
        /// E.g: #rcBuildDistanceField and #rcBuildRegions
        ///
        /// See the #rcConfig documentation for more information on the configuration parameters.
        ///
        /// @see rcAllocCompactHeightfield, rcHeightfield, rcCompactHeightfield, rcConfig
        public static RcCompactHeightfield BuildCompactHeightfield(RcTelemetry ctx, int walkableHeight, int walkableClimb,
            RcHeightfield hf)
        {
            using var timer = ctx.ScopedTimer(RcTimerLabel.RC_TIMER_BUILD_COMPACTHEIGHTFIELD);

            RcCompactHeightfield chf = new RcCompactHeightfield();
            int w = hf.width;
            int h = hf.height;
            int spanCount = GetHeightFieldSpanCount(hf);

            // Fill in header.
            chf.width = w;
            chf.height = h;
            chf.borderSize = hf.borderSize;
            chf.spanCount = spanCount;
            chf.walkableHeight = walkableHeight;
            chf.walkableClimb = walkableClimb;
            chf.maxRegions = 0;
            chf.bmin = hf.bmin;
            chf.bmax = hf.bmax;
            chf.bmax.y += walkableHeight * hf.ch;
            chf.cs = hf.cs;
            chf.ch = hf.ch;
            chf.cells = new RcCompactCell[w * h];
            chf.spans = new RcCompactSpan[spanCount];
            chf.areas = new int[spanCount];

            for (int i = 0; i < chf.spans.Length; i++)
            {
                chf.spans[i] = new RcCompactSpan();
            }

            // Fill in cells and spans.
            int idx = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcSpan s = hf.spans[x + y * w];
                    // If there are no spans at this cell, just leave the data to index=0, count=0.
                    if (s == null)
                        continue;
                    
                    int tmpIdx = idx;
                    int tmpCount = 0;
                    while (s != null)
                    {
                        if (s.area != RC_NULL_AREA)
                        {
                            int bot = s.smax;
                            int top = s.next != null ? (int)s.next.smin : MAX_HEIGHT;
                            chf.spans[idx].y = Clamp(bot, 0, MAX_HEIGHT);
                            chf.spans[idx].h = Clamp(top - bot, 0, MAX_HEIGHT);
                            chf.areas[idx] = s.area;
                            idx++;
                            tmpCount++;
                        }

                        s = s.next;
                    }

                    chf.cells[x + y * w] = new RcCompactCell(tmpIdx, tmpCount);
                }
            }

            // Find neighbour connections.
            int tooHighNeighbour = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    RcCompactCell c = chf.cells[x + y * w];
                    for (int i = c.index, ni = c.index + c.count; i < ni; ++i)
                    {
                        RcCompactSpan s = chf.spans[i];

                        for (int dir = 0; dir < 4; ++dir)
                        {
                            RecastCommon.SetCon(s, dir, RC_NOT_CONNECTED);
                            int nx = x + RecastCommon.GetDirOffsetX(dir);
                            int ny = y + RecastCommon.GetDirOffsetY(dir);
                            // First check that the neighbour cell is in bounds.
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                continue;

                            // Iterate over all neighbour spans and check if any of the is
                            // accessible from current cell.
                            RcCompactCell nc = chf.cells[nx + ny * w];
                            for (int k = nc.index, nk = nc.index + nc.count; k < nk; ++k)
                            {
                                RcCompactSpan ns = chf.spans[k];
                                int bot = Math.Max(s.y, ns.y);
                                int top = Math.Min(s.y + s.h, ns.y + ns.h);

                                // Check that the gap between the spans is walkable,
                                // and that the climb height between the gaps is not too high.
                                if ((top - bot) >= walkableHeight && Math.Abs(ns.y - s.y) <= walkableClimb)
                                {
                                    // Mark direction as walkable.
                                    int lidx = k - nc.index;
                                    if (lidx < 0 || lidx > MAX_LAYERS)
                                    {
                                        tooHighNeighbour = Math.Max(tooHighNeighbour, lidx);
                                        continue;
                                    }

                                    RecastCommon.SetCon(s, dir, lidx);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (tooHighNeighbour > MAX_LAYERS)
            {
                throw new Exception("rcBuildCompactHeightfield: Heightfield has too many layers " + tooHighNeighbour
                                                                                                  + " (max: " + MAX_LAYERS + ")");
            }

            return chf;
        }

        private static int GetHeightFieldSpanCount(RcHeightfield hf)
        {
            int w = hf.width;
            int h = hf.height;
            int spanCount = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    for (RcSpan s = hf.spans[x + y * w]; s != null; s = s.next)
                    {
                        if (s.area != RC_NULL_AREA)
                            spanCount++;
                    }
                }
            }

            return spanCount;
        }
    }
}