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

using DotRecast.Core;

namespace DotRecast.Detour
{
    /** Provides high level information related to a dtMeshTile object. */
    public class DtMeshHeader
    {
        /** A magic number used to detect compatibility of navigation tile data. */
        public const int DT_NAVMESH_MAGIC = 'D' << 24 | 'N' << 16 | 'A' << 8 | 'V';

        /** A version number used to detect compatibility of navigation tile data. */
        public const int DT_NAVMESH_VERSION = 7;

        public const int DT_NAVMESH_VERSION_RECAST4J_FIRST = 0x8807;
        public const int DT_NAVMESH_VERSION_RECAST4J_NO_POLY_FIRSTLINK = 0x8808;
        public const int DT_NAVMESH_VERSION_RECAST4J_32BIT_BVTREE = 0x8809;
        public const int DT_NAVMESH_VERSION_RECAST4J_LAST = 0x8809;

        /** A magic number used to detect the compatibility of navigation tile states. */
        public const int DT_NAVMESH_STATE_MAGIC = 'D' << 24 | 'N' << 16 | 'M' << 8 | 'S';

        /** A version number used to detect compatibility of navigation tile states. */
        public const int DT_NAVMESH_STATE_VERSION = 1;

        /** Tile magic number. (Used to identify the data format.) */
        public int magic;

        /** Tile data format version number. */
        public int version;

        /** The x-position of the tile within the dtNavMesh tile grid. (x, y, layer) */
        public int x;

        /** The y-position of the tile within the dtNavMesh tile grid. (x, y, layer) */
        public int y;

        /** The layer of the tile within the dtNavMesh tile grid. (x, y, layer) */
        public int layer;

        /** The user defined id of the tile. */
        public int userId;

        /** The number of polygons in the tile. */
        public int polyCount;

        /** The number of vertices in the tile. */
        public int vertCount;

        /** The number of allocated links. */
        public int maxLinkCount;

        /** The number of sub-meshes in the detail mesh. */
        public int detailMeshCount;

        /** The number of unique vertices in the detail mesh. (In addition to the polygon vertices.) */
        public int detailVertCount;

        /** The number of triangles in the detail mesh. */
        public int detailTriCount;

        /** The number of bounding volume nodes. (Zero if bounding volumes are disabled.) */
        public int bvNodeCount;

        /** The number of off-mesh connections. */
        public int offMeshConCount;

        /** The index of the first polygon which is an off-mesh connection. */
        public int offMeshBase;

        /** The height of the agents using the tile. */
        public float walkableHeight;

        /** The radius of the agents using the tile. */
        public float walkableRadius;

        /** The maximum climb height of the agents using the tile. */
        public float walkableClimb;

        /** The minimum bounds of the tile's AABB. [(x, y, z)] */
        public RcVec3f bmin = new RcVec3f();

        /** The maximum bounds of the tile's AABB. [(x, y, z)] */
        public RcVec3f bmax = new RcVec3f();

        /** The bounding volume quantization factor. */
        public float bvQuantFactor;
    }
}