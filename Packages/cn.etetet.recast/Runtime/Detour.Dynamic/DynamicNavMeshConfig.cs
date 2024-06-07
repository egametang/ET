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

using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic
{
    public class DynamicNavMeshConfig
    {
        public readonly bool useTiles;
        public readonly int tileSizeX;
        public readonly int tileSizeZ;
        public readonly float cellSize;
        public int partition = RcPartitionType.WATERSHED.Value;
        public RcAreaModification walkableAreaModification = new RcAreaModification(1);
        public float walkableHeight;
        public float walkableSlopeAngle;
        public float walkableRadius;
        public float walkableClimb;
        public float minRegionArea;
        public float regionMergeArea;
        public float maxEdgeLen;
        public float maxSimplificationError;
        public int vertsPerPoly;
        public bool buildDetailMesh;
        public float detailSampleDistance;
        public float detailSampleMaxError;
        public bool filterLowHangingObstacles = true;
        public bool filterLedgeSpans = true;
        public bool filterWalkableLowHeightSpans = true;
        public bool enableCheckpoints = true;
        public bool keepIntermediateResults = false;

        public DynamicNavMeshConfig(bool useTiles, int tileSizeX, int tileSizeZ, float cellSize)
        {
            this.useTiles = useTiles;
            this.tileSizeX = tileSizeX;
            this.tileSizeZ = tileSizeZ;
            this.cellSize = cellSize;
        }
    }
}