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

using DotRecast.Core;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.Extras
{
    public class BVTreeBuilder
    {
        public void Build(DtMeshData data)
        {
            data.bvTree = new DtBVNode[data.header.polyCount * 2];
            data.header.bvNodeCount = data.bvTree.Length == 0
                ? 0
                : CreateBVTree(data, data.bvTree, data.header.bvQuantFactor);
        }

        private static int CreateBVTree(DtMeshData data, DtBVNode[] nodes, float quantFactor)
        {
            BVItem[] items = new BVItem[data.header.polyCount];
            for (int i = 0; i < data.header.polyCount; i++)
            {
                BVItem it = new BVItem();
                items[i] = it;
                it.i = i;
                RcVec3f bmin = new RcVec3f();
                RcVec3f bmax = new RcVec3f();
                bmin.Set(data.verts, data.polys[i].verts[0] * 3);
                bmax.Set(data.verts, data.polys[i].verts[0] * 3);
                for (int j = 1; j < data.polys[i].vertCount; j++)
                {
                    bmin.Min(data.verts, data.polys[i].verts[j] * 3);
                    bmax.Max(data.verts, data.polys[i].verts[j] * 3);
                }

                it.bmin[0] = Clamp((int)((bmin.x - data.header.bmin.x) * quantFactor), 0, 0x7fffffff);
                it.bmin[1] = Clamp((int)((bmin.y - data.header.bmin.y) * quantFactor), 0, 0x7fffffff);
                it.bmin[2] = Clamp((int)((bmin.z - data.header.bmin.z) * quantFactor), 0, 0x7fffffff);
                it.bmax[0] = Clamp((int)((bmax.x - data.header.bmin.x) * quantFactor), 0, 0x7fffffff);
                it.bmax[1] = Clamp((int)((bmax.y - data.header.bmin.y) * quantFactor), 0, 0x7fffffff);
                it.bmax[2] = Clamp((int)((bmax.z - data.header.bmin.z) * quantFactor), 0, 0x7fffffff);
            }

            return DtNavMeshBuilder.Subdivide(items, data.header.polyCount, 0, data.header.polyCount, 0, nodes);
        }
    }
}