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

namespace DotRecast.Detour
{
    /**
 * Simple helper to find an intersection between a ray and a nav mesh
 */
    public static class DtNavMeshRaycast
    {
        public static bool Raycast(DtNavMesh mesh, RcVec3f src, RcVec3f dst, out float hitTime)
        {
            hitTime = 0.0f;
            for (int t = 0; t < mesh.GetMaxTiles(); ++t)
            {
                DtMeshTile tile = mesh.GetTile(t);
                if (tile != null && tile.data != null)
                {
                    if (Raycast(tile, src, dst, out hitTime))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool Raycast(DtMeshTile tile, RcVec3f sp, RcVec3f sq, out float hitTime)
        {
            hitTime = 0.0f;
            for (int i = 0; i < tile.data.header.polyCount; ++i)
            {
                DtPoly p = tile.data.polys[i];
                if (p.GetPolyType() == DtPoly.DT_POLYTYPE_OFFMESH_CONNECTION)
                {
                    continue;
                }

                DtPolyDetail pd = tile.data.detailMeshes[i];

                if (pd != null)
                {
                    RcVec3f[] verts = new RcVec3f[3];
                    for (int j = 0; j < pd.triCount; ++j)
                    {
                        int t = (pd.triBase + j) * 4;
                        for (int k = 0; k < 3; ++k)
                        {
                            int v = tile.data.detailTris[t + k];
                            if (v < p.vertCount)
                            {
                                verts[k].x = tile.data.verts[p.verts[v] * 3];
                                verts[k].y = tile.data.verts[p.verts[v] * 3 + 1];
                                verts[k].z = tile.data.verts[p.verts[v] * 3 + 2];
                            }
                            else
                            {
                                verts[k].x = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3];
                                verts[k].y = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 1];
                                verts[k].z = tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 2];
                            }
                        }

                        if (Intersections.IntersectSegmentTriangle(sp, sq, verts[0], verts[1], verts[2], out hitTime))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // FIXME: Use Poly if PolyDetail is unavailable
                }
            }

            return false;
        }
    }
}