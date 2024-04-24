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

namespace DotRecast.Recast
{
    /// Specifies a configuration to use when performing Recast builds.
    /// @ingroup recast
    public class RcConfig
    {
        public readonly int Partition;

        public readonly bool UseTiles;

        /** The width/depth size of tile's on the xz-plane. [Limit: &gt;= 0] [Units: vx] **/
        public readonly int TileSizeX;

        public readonly int TileSizeZ;

        /** The xz-plane cell size to use for fields. [Limit: &gt; 0] [Units: wu] **/
        public readonly float Cs;

        /** The y-axis cell size to use for fields. [Limit: &gt; 0] [Units: wu] **/
        public readonly float Ch;

        /** The maximum slope that is considered walkable. [Limits: 0 &lt;= value &lt; 90] [Units: Degrees] **/
        public readonly float WalkableSlopeAngle;

        /**
         * Minimum floor to 'ceiling' height that will still allow the floor area to be considered walkable. [Limit: &gt;= 3]
         * [Units: vx]
         **/
        public readonly int WalkableHeight;

        /** Maximum ledge height that is considered to still be traversable. [Limit: &gt;=0] [Units: vx] **/
        public readonly int WalkableClimb;

        /**
         * The distance to erode/shrink the walkable area of the heightfield away from obstructions. [Limit: &gt;=0] [Units:
         * vx]
         **/
        public readonly int WalkableRadius;

        /** The maximum allowed length for contour edges along the border of the mesh. [Limit: &gt;=0] [Units: vx] **/
        public readonly int MaxEdgeLen;

        /**
         * The maximum distance a simplfied contour's border edges should deviate the original raw contour. [Limit: &gt;=0]
         * [Units: vx]
         **/
        public readonly float MaxSimplificationError;

        /** The minimum number of cells allowed to form isolated island areas. [Limit: &gt;=0] [Units: vx] **/
        public readonly int MinRegionArea;

        /**
         * Any regions with a span count smaller than this value will, if possible, be merged with larger regions. [Limit:&gt;=0] [Units: vx]
         **/
        public readonly int MergeRegionArea;

        /**
         * The maximum number of vertices allowed for polygons generated during the contour to polygon conversion process.
         * [Limit: &gt;= 3]
         **/
        public readonly int MaxVertsPerPoly;

        /**
         * Sets the sampling distance to use when generating the detail mesh. (For height detail only.) [Limits: 0 or >= 0.9] [Units: wu]
         **/
        public readonly float DetailSampleDist;

        /**
         * The maximum distance the detail mesh surface should deviate from heightfield data. (For height detail only.)
         * [Limit: &gt;=0] [Units: wu]
         **/
        public readonly float DetailSampleMaxError;

        public readonly RcAreaModification WalkableAreaMod;
        public readonly bool FilterLowHangingObstacles;
        public readonly bool FilterLedgeSpans;
        public readonly bool FilterWalkableLowHeightSpans;

        /** Set to false to disable building detailed mesh **/
        public readonly bool BuildMeshDetail;

        /** The size of the non-navigable border around the heightfield. [Limit: &gt;=0] [Units: vx] **/
        public readonly int BorderSize;

        /** Set of original settings passed in world units */
        public readonly float MinRegionAreaWorld;

        public readonly float MergeRegionAreaWorld;
        public readonly float WalkableHeightWorld;
        public readonly float WalkableClimbWorld;
        public readonly float WalkableRadiusWorld;
        public readonly float MaxEdgeLenWorld;

        /**
         * Non-tiled build configuration
         */
        public RcConfig(
            RcPartition partitionType,
            float cellSize, float cellHeight,
            float agentMaxSlope, float agentHeight, float agentRadius, float agentMaxClimb,
            int regionMinSize, int regionMergeSize,
            float edgeMaxLen, float edgeMaxError,
            int vertsPerPoly,
            float detailSampleDist, float detailSampleMaxError,
            bool filterLowHangingObstacles, bool filterLedgeSpans, bool filterWalkableLowHeightSpans,
            RcAreaModification walkableAreaMod, bool buildMeshDetail)
            : this(false, 0, 0, 0,
                partitionType,
                cellSize, cellHeight,
                agentMaxSlope, agentHeight, agentRadius, agentMaxClimb,
                regionMinSize * regionMinSize * cellSize * cellSize, regionMergeSize * regionMergeSize * cellSize * cellSize,
                edgeMaxLen, edgeMaxError,
                vertsPerPoly,
                detailSampleDist, detailSampleMaxError,
                filterLowHangingObstacles, filterLedgeSpans, filterWalkableLowHeightSpans,
                walkableAreaMod, buildMeshDetail)
        {
            // Note: area = size*size in [Units: wu]
        }

        public RcConfig(
            bool useTiles, int tileSizeX, int tileSizeZ,
            int borderSize,
            RcPartition partition,
            float cellSize, float cellHeight,
            float agentMaxSlope, float agentHeight, float agentRadius, float agentMaxClimb,
            float minRegionArea, float mergeRegionArea,
            float edgeMaxLen, float edgeMaxError, int vertsPerPoly,
            float detailSampleDist, float detailSampleMaxError,
            bool filterLowHangingObstacles, bool filterLedgeSpans, bool filterWalkableLowHeightSpans,
            RcAreaModification walkableAreaMod, bool buildMeshDetail)
        {
            UseTiles = useTiles;
            TileSizeX = tileSizeX;
            TileSizeZ = tileSizeZ;
            BorderSize = borderSize;
            Partition = RcPartitionType.Of(partition).Value;
            Cs = cellSize;
            Ch = cellHeight;
            WalkableSlopeAngle = agentMaxSlope;
            WalkableHeight = (int)Math.Ceiling(agentHeight / Ch);
            WalkableHeightWorld = agentHeight;
            WalkableClimb = (int)Math.Floor(agentMaxClimb / Ch);
            WalkableClimbWorld = agentMaxClimb;
            WalkableRadius = (int)Math.Ceiling(agentRadius / Cs);
            WalkableRadiusWorld = agentRadius;
            MinRegionArea = (int)Math.Round(minRegionArea / (Cs * Cs));
            MinRegionAreaWorld = minRegionArea;
            MergeRegionArea = (int)Math.Round(mergeRegionArea / (Cs * Cs));
            MergeRegionAreaWorld = mergeRegionArea;
            MaxEdgeLen = (int)(edgeMaxLen / cellSize);
            MaxEdgeLenWorld = edgeMaxLen;
            MaxSimplificationError = edgeMaxError;
            MaxVertsPerPoly = vertsPerPoly;
            DetailSampleDist = detailSampleDist < 0.9f ? 0 : cellSize * detailSampleDist;
            DetailSampleMaxError = cellHeight * detailSampleMaxError;
            WalkableAreaMod = walkableAreaMod;
            FilterLowHangingObstacles = filterLowHangingObstacles;
            FilterLedgeSpans = filterLedgeSpans;
            FilterWalkableLowHeightSpans = filterWalkableLowHeightSpans;
            BuildMeshDetail = buildMeshDetail;
        }

        public static int CalcBorder(float agentRadius, float cs)
        {
            return 3 + (int)Math.Ceiling(agentRadius / cs);
        }
    }
}