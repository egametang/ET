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
using System.Threading;
using System.Threading.Tasks;
using DotRecast.Core;
using DotRecast.Recast.Geom;

namespace DotRecast.Recast
{
    public class RecastBuilder
    {
        private readonly IRecastBuilderProgressListener progressListener;

        public RecastBuilder()
        {
            progressListener = null;
        }

        public RecastBuilder(IRecastBuilderProgressListener progressListener)
        {
            this.progressListener = progressListener;
        }

        public List<RecastBuilderResult> BuildTiles(IInputGeomProvider geom, RcConfig cfg, TaskFactory taskFactory)
        {
            RcVec3f bmin = geom.GetMeshBoundsMin();
            RcVec3f bmax = geom.GetMeshBoundsMax();
            RcUtils.CalcTileCount(bmin, bmax, cfg.Cs, cfg.TileSizeX, cfg.TileSizeZ, out var tw, out var th);
            List<RecastBuilderResult> results = new List<RecastBuilderResult>();
            if (null != taskFactory)
            {
                BuildMultiThreadAsync(geom, cfg, bmin, bmax, tw, th, results, taskFactory, default);
            }
            else
            {
                BuildSingleThreadAsync(geom, cfg, bmin, bmax, tw, th, results);
            }

            return results;
        }


        public Task BuildTilesAsync(IInputGeomProvider geom, RcConfig cfg, int threads, List<RecastBuilderResult> results, TaskFactory taskFactory, CancellationToken cancellationToken)
        {
            RcVec3f bmin = geom.GetMeshBoundsMin();
            RcVec3f bmax = geom.GetMeshBoundsMax();
            RcUtils.CalcTileCount(bmin, bmax, cfg.Cs, cfg.TileSizeX, cfg.TileSizeZ, out var tw, out var th);
            Task task;
            if (1 < threads)
            {
                task = BuildMultiThreadAsync(geom, cfg, bmin, bmax, tw, th, results, taskFactory, cancellationToken);
            }
            else
            {
                task = BuildSingleThreadAsync(geom, cfg, bmin, bmax, tw, th, results);
            }

            return task;
        }

        private Task BuildSingleThreadAsync(IInputGeomProvider geom, RcConfig cfg, RcVec3f bmin, RcVec3f bmax,
            int tw, int th, List<RecastBuilderResult> results)
        {
            RcAtomicInteger counter = new RcAtomicInteger(0);
            for (int y = 0; y < th; ++y)
            {
                for (int x = 0; x < tw; ++x)
                {
                    results.Add(BuildTile(geom, cfg, bmin, bmax, x, y, counter, tw * th));
                }
            }

            return Task.CompletedTask;
        }

        private Task BuildMultiThreadAsync(IInputGeomProvider geom, RcConfig cfg, RcVec3f bmin, RcVec3f bmax,
            int tw, int th, List<RecastBuilderResult> results, TaskFactory taskFactory, CancellationToken cancellationToken)
        {
            RcAtomicInteger counter = new RcAtomicInteger(0);
            CountdownEvent latch = new CountdownEvent(tw * th);
            List<Task> tasks = new List<Task>();

            for (int x = 0; x < tw; ++x)
            {
                for (int y = 0; y < th; ++y)
                {
                    int tx = x;
                    int ty = y;
                    var task = taskFactory.StartNew(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        try
                        {
                            RecastBuilderResult tile = BuildTile(geom, cfg, bmin, bmax, tx, ty, counter, tw * th);
                            lock (results)
                            {
                                results.Add(tile);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }


                        latch.Signal();
                    }, cancellationToken);

                    tasks.Add(task);
                }
            }

            try
            {
                latch.Wait();
            }
            catch (ThreadInterruptedException)
            {
            }

            return Task.WhenAll(tasks.ToArray());
        }

        public RecastBuilderResult BuildTile(IInputGeomProvider geom, RcConfig cfg, RcVec3f bmin, RcVec3f bmax, int tx,
            int ty, RcAtomicInteger counter, int total)
        {
            RecastBuilderResult result = Build(geom, new RecastBuilderConfig(cfg, bmin, bmax, tx, ty));
            if (progressListener != null)
            {
                progressListener.OnProgress(counter.IncrementAndGet(), total);
            }

            return result;
        }

        public RecastBuilderResult Build(IInputGeomProvider geom, RecastBuilderConfig builderCfg)
        {
            RcConfig cfg = builderCfg.cfg;
            RcTelemetry ctx = new RcTelemetry();
            //
            // Step 1. Rasterize input polygon soup.
            //
            RcHeightfield solid = RecastVoxelization.BuildSolidHeightfield(geom, builderCfg, ctx);
            return Build(builderCfg.tileX, builderCfg.tileZ, geom, cfg, solid, ctx);
        }

        public RecastBuilderResult Build(int tileX, int tileZ, IInputGeomProvider geom, RcConfig cfg, RcHeightfield solid, RcTelemetry ctx)
        {
            FilterHeightfield(solid, cfg, ctx);
            RcCompactHeightfield chf = BuildCompactHeightfield(geom, cfg, ctx, solid);

            // Partition the heightfield so that we can use simple algorithm later
            // to triangulate the walkable areas.
            // There are 3 partitioning methods, each with some pros and cons:
            // 1) Watershed partitioning
            // - the classic Recast partitioning
            // - creates the nicest tessellation
            // - usually slowest
            // - partitions the heightfield into nice regions without holes or
            // overlaps
            // - the are some corner cases where this method creates produces holes
            // and overlaps
            // - holes may appear when a small obstacles is close to large open area
            // (triangulation can handle this)
            // - overlaps may occur if you have narrow spiral corridors (i.e
            // stairs), this make triangulation to fail
            // * generally the best choice if you precompute the navmesh, use this
            // if you have large open areas
            // 2) Monotone partioning
            // - fastest
            // - partitions the heightfield into regions without holes and overlaps
            // (guaranteed)
            // - creates long thin polygons, which sometimes causes paths with
            // detours
            // * use this if you want fast navmesh generation
            // 3) Layer partitoining
            // - quite fast
            // - partitions the heighfield into non-overlapping regions
            // - relies on the triangulation code to cope with holes (thus slower
            // than monotone partitioning)
            // - produces better triangles than monotone partitioning
            // - does not have the corner cases of watershed partitioning
            // - can be slow and create a bit ugly tessellation (still better than
            // monotone)
            // if you have large open areas with small obstacles (not a problem if
            // you use tiles)
            // * good choice to use for tiled navmesh with medium and small sized
            // tiles

            if (cfg.Partition == RcPartitionType.WATERSHED.Value)
            {
                // Prepare for region partitioning, by calculating distance field
                // along the walkable surface.
                RecastRegion.BuildDistanceField(ctx, chf);
                // Partition the walkable surface into simple regions without holes.
                RecastRegion.BuildRegions(ctx, chf, cfg.MinRegionArea, cfg.MergeRegionArea);
            }
            else if (cfg.Partition == RcPartitionType.MONOTONE.Value)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                RecastRegion.BuildRegionsMonotone(ctx, chf, cfg.MinRegionArea, cfg.MergeRegionArea);
            }
            else
            {
                // Partition the walkable surface into simple regions without holes.
                RecastRegion.BuildLayerRegions(ctx, chf, cfg.MinRegionArea);
            }

            //
            // Step 5. Trace and simplify region contours.
            //

            // Create contours.
            RcContourSet cset = RecastContour.BuildContours(ctx, chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen,
                RcConstants.RC_CONTOUR_TESS_WALL_EDGES);

            //
            // Step 6. Build polygons mesh from contours.
            //

            RcPolyMesh pmesh = RecastMesh.BuildPolyMesh(ctx, cset, cfg.MaxVertsPerPoly);

            //
            // Step 7. Create detail mesh which allows to access approximate height
            // on each polygon.
            //
            RcPolyMeshDetail dmesh = cfg.BuildMeshDetail
                ? RecastMeshDetail.BuildPolyMeshDetail(ctx, pmesh, chf, cfg.DetailSampleDist, cfg.DetailSampleMaxError)
                : null;
            return new RecastBuilderResult(tileX, tileZ, solid, chf, cset, pmesh, dmesh, ctx);
        }

        /*
         * Step 2. Filter walkable surfaces.
         */
        private void FilterHeightfield(RcHeightfield solid, RcConfig cfg, RcTelemetry ctx)
        {
            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.
            if (cfg.FilterLowHangingObstacles)
            {
                RecastFilter.FilterLowHangingWalkableObstacles(ctx, cfg.WalkableClimb, solid);
            }

            if (cfg.FilterLedgeSpans)
            {
                RecastFilter.FilterLedgeSpans(ctx, cfg.WalkableHeight, cfg.WalkableClimb, solid);
            }

            if (cfg.FilterWalkableLowHeightSpans)
            {
                RecastFilter.FilterWalkableLowHeightSpans(ctx, cfg.WalkableHeight, solid);
            }
        }

        /*
         * Step 3. Partition walkable surface to simple regions.
         */
        private RcCompactHeightfield BuildCompactHeightfield(IInputGeomProvider geom, RcConfig cfg, RcTelemetry ctx,
            RcHeightfield solid)
        {
            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            RcCompactHeightfield chf = RecastCompact.BuildCompactHeightfield(ctx, cfg.WalkableHeight, cfg.WalkableClimb, solid);

            // Erode the walkable area by agent radius.
            RecastArea.ErodeWalkableArea(ctx, cfg.WalkableRadius, chf);
            // (Optional) Mark areas.
            if (geom != null)
            {
                foreach (RcConvexVolume vol in geom.ConvexVolumes())
                {
                    RecastArea.MarkConvexPolyArea(ctx, vol.verts, vol.hmin, vol.hmax, vol.areaMod, chf);
                }
            }

            return chf;
        }

        public RcHeightfieldLayerSet BuildLayers(IInputGeomProvider geom, RecastBuilderConfig builderCfg)
        {
            RcTelemetry ctx = new RcTelemetry();
            RcHeightfield solid = RecastVoxelization.BuildSolidHeightfield(geom, builderCfg, ctx);
            FilterHeightfield(solid, builderCfg.cfg, ctx);
            RcCompactHeightfield chf = BuildCompactHeightfield(geom, builderCfg.cfg, ctx, solid);
            return RecastLayers.BuildHeightfieldLayers(ctx, chf, builderCfg.cfg.WalkableHeight);
        }
    }
}