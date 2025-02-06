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
using System.Runtime.CompilerServices;

namespace DotRecast.Detour.Crowd
{
    public class DtProximityGrid
    {
        private readonly float _cellSize;
        private readonly float _invCellSize;
        private readonly Dictionary<long, List<DtCrowdAgent>> _items;

        public DtProximityGrid(float cellSize)
        {
            _cellSize = cellSize;
            _invCellSize = 1.0f / cellSize;
            _items = new Dictionary<long, List<DtCrowdAgent>>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CombineKey(int x, int y)
        {
            uint ux = (uint)x;
            uint uy = (uint)y;
            return ((long)ux << 32) | uy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecomposeKey(long key, out int x, out int y)
        {
            uint ux = (uint)(key >> 32);
            uint uy = (uint)key;
            x = (int)ux;
            y = (int)uy;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void AddItem(DtCrowdAgent agent, float minx, float miny, float maxx, float maxy)
        {
            int iminx = (int)Math.Floor(minx * _invCellSize);
            int iminy = (int)Math.Floor(miny * _invCellSize);
            int imaxx = (int)Math.Floor(maxx * _invCellSize);
            int imaxy = (int)Math.Floor(maxy * _invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    long key = CombineKey(x, y);
                    if (!_items.TryGetValue(key, out var ids))
                    {
                        ids = new List<DtCrowdAgent>();
                        _items.Add(key, ids);
                    }

                    ids.Add(agent);
                }
            }
        }

        // 해당 셀 사이즈의 크기로 x ~ y 영역을 찾아, 군집 에이전트를 가져오는 코드
        public int QueryItems(float minx, float miny, float maxx, float maxy, ref HashSet<DtCrowdAgent> result)
        {
            int iminx = (int)Math.Floor(minx * _invCellSize);
            int iminy = (int)Math.Floor(miny * _invCellSize);
            int imaxx = (int)Math.Floor(maxx * _invCellSize);
            int imaxy = (int)Math.Floor(maxy * _invCellSize);

            for (int y = iminy; y <= imaxy; ++y)
            {
                for (int x = iminx; x <= imaxx; ++x)
                {
                    long key = CombineKey(x, y);
                    if (_items.TryGetValue(key, out var ids))
                    {
                        for (int i = 0; i < ids.Count; ++i)
                        {
                            result.Add(ids[i]);
                        }
                    }
                }
            }

            return result.Count;
        }

        public IEnumerable<(long, int)> GetItemCounts()
        {
            return _items
                .Where(e => e.Value.Count > 0)
                .Select(e => (e.Key, e.Value.Count));
        }

        public float GetCellSize()
        {
            return _cellSize;
        }
    }
}