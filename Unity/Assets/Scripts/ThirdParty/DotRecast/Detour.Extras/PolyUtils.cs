/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

namespace DotRecast.Detour.Extras
{
    public static class PolyUtils
    {
        /**
     * Find edge shared by 2 polygons within the same tile
     */
        public static int FindEdge(DtPoly node, DtPoly neighbour, DtMeshData tile, DtMeshData neighbourTile)
        {
            // Compare indices first assuming there are no duplicate vertices
            for (int i = 0; i < node.vertCount; i++)
            {
                int j = (i + 1) % node.vertCount;
                for (int k = 0; k < neighbour.vertCount; k++)
                {
                    int l = (k + 1) % neighbour.vertCount;
                    if ((node.verts[i] == neighbour.verts[l] && node.verts[j] == neighbour.verts[k])
                        || (node.verts[i] == neighbour.verts[k] && node.verts[j] == neighbour.verts[l]))
                    {
                        return i;
                    }
                }
            }

            // Fall back to comparing actual positions in case of duplicate vertices
            for (int i = 0; i < node.vertCount; i++)
            {
                int j = (i + 1) % node.vertCount;
                for (int k = 0; k < neighbour.vertCount; k++)
                {
                    int l = (k + 1) % neighbour.vertCount;
                    if ((SamePosition(tile.verts, node.verts[i], neighbourTile.verts, neighbour.verts[l])
                         && SamePosition(tile.verts, node.verts[j], neighbourTile.verts, neighbour.verts[k]))
                        || (SamePosition(tile.verts, node.verts[i], neighbourTile.verts, neighbour.verts[k])
                            && SamePosition(tile.verts, node.verts[j], neighbourTile.verts, neighbour.verts[l])))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static bool SamePosition(float[] verts, int v, float[] verts2, int v2)
        {
            for (int i = 0; i < 3; i++)
            {
                if (verts[3 * v + i] != verts2[3 * v2 + 1])
                {
                    return false;
                }
            }

            return true;
        }

        /**
     * Find edge closest to the given coordinate
     */
        public static int FindEdge(DtPoly node, DtMeshData tile, float value, int comp)
        {
            float error = float.MaxValue;
            int edge = 0;
            for (int i = 0; i < node.vertCount; i++)
            {
                int j = (i + 1) % node.vertCount;
                float v1 = tile.verts[3 * node.verts[i] + comp] - value;
                float v2 = tile.verts[3 * node.verts[j] + comp] - value;
                float d = v1 * v1 + v2 * v2;
                if (d < error)
                {
                    error = d;
                    edge = i;
                }
            }

            return edge;
        }
    }
}