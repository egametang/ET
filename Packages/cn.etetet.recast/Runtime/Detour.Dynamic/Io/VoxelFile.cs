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
using System.Collections.Generic;
using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic.Io
{
    public class VoxelFile
    {
        public static readonly RcByteOrder PREFERRED_BYTE_ORDER = RcByteOrder.BIG_ENDIAN;
        public const int MAGIC = 'V' << 24 | 'O' << 16 | 'X' << 8 | 'L';
        public const int VERSION_EXPORTER_MASK = 0xF000;
        public const int VERSION_COMPRESSION_MASK = 0x0F00;
        public const int VERSION_EXPORTER_RECAST4J = 0x1000;
        public const int VERSION_COMPRESSION_LZ4 = 0x0100;
        public int version;
        public int partition = RcPartitionType.WATERSHED.Value;
        public bool filterLowHangingObstacles = true;
        public bool filterLedgeSpans = true;
        public bool filterWalkableLowHeightSpans = true;
        public float walkableRadius;
        public float walkableHeight;
        public float walkableClimb;
        public float walkableSlopeAngle;
        public float cellSize;
        public float maxSimplificationError;
        public float maxEdgeLen;
        public float minRegionArea;
        public float regionMergeArea;
        public int vertsPerPoly;
        public bool buildMeshDetail;
        public float detailSampleDistance;
        public float detailSampleMaxError;
        public bool useTiles;
        public int tileSizeX;
        public int tileSizeZ;
        public RcVec3f rotation = new RcVec3f();
        public float[] bounds = new float[6];
        public readonly List<VoxelTile> tiles = new List<VoxelTile>();

        public void AddTile(VoxelTile tile)
        {
            tiles.Add(tile);
        }

        public RcConfig GetConfig(VoxelTile tile, RcAreaModification walkbableAreaMod, bool buildMeshDetail)
        {
            return new RcConfig(useTiles, tileSizeX, tileSizeZ,
                tile.borderSize,
                RcPartitionType.OfValue(partition),
                cellSize, tile.cellHeight,
                walkableSlopeAngle, walkableHeight, walkableRadius, walkableClimb,
                minRegionArea, regionMergeArea,
                maxEdgeLen, maxSimplificationError,
                vertsPerPoly,
                detailSampleDistance, detailSampleMaxError,
                filterLowHangingObstacles, filterLedgeSpans, filterWalkableLowHeightSpans,
                walkbableAreaMod, buildMeshDetail);
        }

        public static VoxelFile From(RcConfig config, List<RecastBuilderResult> results)
        {
            VoxelFile f = new VoxelFile();
            f.version = 1;
            f.partition = config.Partition;
            f.filterLowHangingObstacles = config.FilterLowHangingObstacles;
            f.filterLedgeSpans = config.FilterLedgeSpans;
            f.filterWalkableLowHeightSpans = config.FilterWalkableLowHeightSpans;
            f.walkableRadius = config.WalkableRadiusWorld;
            f.walkableHeight = config.WalkableHeightWorld;
            f.walkableClimb = config.WalkableClimbWorld;
            f.walkableSlopeAngle = config.WalkableSlopeAngle;
            f.cellSize = config.Cs;
            f.maxSimplificationError = config.MaxSimplificationError;
            f.maxEdgeLen = config.MaxEdgeLenWorld;
            f.minRegionArea = config.MinRegionAreaWorld;
            f.regionMergeArea = config.MergeRegionAreaWorld;
            f.vertsPerPoly = config.MaxVertsPerPoly;
            f.buildMeshDetail = config.BuildMeshDetail;
            f.detailSampleDistance = config.DetailSampleDist;
            f.detailSampleMaxError = config.DetailSampleMaxError;
            f.useTiles = config.UseTiles;
            f.tileSizeX = config.TileSizeX;
            f.tileSizeZ = config.TileSizeZ;
            f.bounds = new float[]
            {
                float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity,
                float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity
            };
            foreach (RecastBuilderResult r in results)
            {
                f.tiles.Add(new VoxelTile(r.tileX, r.tileZ, r.GetSolidHeightfield()));
                f.bounds[0] = Math.Min(f.bounds[0], r.GetSolidHeightfield().bmin.x);
                f.bounds[1] = Math.Min(f.bounds[1], r.GetSolidHeightfield().bmin.y);
                f.bounds[2] = Math.Min(f.bounds[2], r.GetSolidHeightfield().bmin.z);
                f.bounds[3] = Math.Max(f.bounds[3], r.GetSolidHeightfield().bmax.x);
                f.bounds[4] = Math.Max(f.bounds[4], r.GetSolidHeightfield().bmax.y);
                f.bounds[5] = Math.Max(f.bounds[5], r.GetSolidHeightfield().bmax.z);
            }

            return f;
        }

        public static VoxelFile From(DynamicNavMesh mesh)
        {
            VoxelFile f = new VoxelFile();
            f.version = 1;
            DynamicNavMeshConfig config = mesh.config;
            f.partition = config.partition;
            f.filterLowHangingObstacles = config.filterLowHangingObstacles;
            f.filterLedgeSpans = config.filterLedgeSpans;
            f.filterWalkableLowHeightSpans = config.filterWalkableLowHeightSpans;
            f.walkableRadius = config.walkableRadius;
            f.walkableHeight = config.walkableHeight;
            f.walkableClimb = config.walkableClimb;
            f.walkableSlopeAngle = config.walkableSlopeAngle;
            f.cellSize = config.cellSize;
            f.maxSimplificationError = config.maxSimplificationError;
            f.maxEdgeLen = config.maxEdgeLen;
            f.minRegionArea = config.minRegionArea;
            f.regionMergeArea = config.regionMergeArea;
            f.vertsPerPoly = config.vertsPerPoly;
            f.buildMeshDetail = config.buildDetailMesh;
            f.detailSampleDistance = config.detailSampleDistance;
            f.detailSampleMaxError = config.detailSampleMaxError;
            f.useTiles = config.useTiles;
            f.tileSizeX = config.tileSizeX;
            f.tileSizeZ = config.tileSizeZ;
            f.bounds = new float[]
            {
                float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity,
                float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity
            };
            foreach (VoxelTile vt in mesh.VoxelTiles())
            {
                RcHeightfield heightfield = vt.Heightfield();
                f.tiles.Add(new VoxelTile(vt.tileX, vt.tileZ, heightfield));
                f.bounds[0] = Math.Min(f.bounds[0], vt.boundsMin.x);
                f.bounds[1] = Math.Min(f.bounds[1], vt.boundsMin.y);
                f.bounds[2] = Math.Min(f.bounds[2], vt.boundsMin.z);
                f.bounds[3] = Math.Max(f.bounds[3], vt.boundsMax.x);
                f.bounds[4] = Math.Max(f.bounds[4], vt.boundsMax.y);
                f.bounds[5] = Math.Max(f.bounds[5], vt.boundsMax.z);
            }

            return f;
        }
    }
}