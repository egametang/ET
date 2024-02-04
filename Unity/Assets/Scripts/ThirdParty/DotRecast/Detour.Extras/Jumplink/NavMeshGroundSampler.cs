using System;
using DotRecast.Core;

using DotRecast.Recast;

namespace DotRecast.Detour.Extras.Jumplink
{
    class NavMeshGroundSampler : AbstractGroundSampler
    {
        public override void Sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es)
        {
            DtNavMeshQuery navMeshQuery = CreateNavMesh(result, acfg.agentRadius, acfg.agentHeight, acfg.agentClimb);
            SampleGround(acfg, es, (RcVec3f pt, float heightRange, out float height) => GetNavMeshHeight(navMeshQuery, pt, acfg.cellSize, heightRange, out height));
        }

        private DtNavMeshQuery CreateNavMesh(RecastBuilderResult r, float agentRadius, float agentHeight, float agentClimb)
        {
            DtNavMeshCreateParams option = new DtNavMeshCreateParams();
            option.verts = r.GetMesh().verts;
            option.vertCount = r.GetMesh().nverts;
            option.polys = r.GetMesh().polys;
            option.polyAreas = r.GetMesh().areas;
            option.polyFlags = r.GetMesh().flags;
            option.polyCount = r.GetMesh().npolys;
            option.nvp = r.GetMesh().nvp;
            option.detailMeshes = r.GetMeshDetail().meshes;
            option.detailVerts = r.GetMeshDetail().verts;
            option.detailVertsCount = r.GetMeshDetail().nverts;
            option.detailTris = r.GetMeshDetail().tris;
            option.detailTriCount = r.GetMeshDetail().ntris;
            option.walkableRadius = agentRadius;
            option.walkableHeight = agentHeight;
            option.walkableClimb = agentClimb;
            option.bmin = r.GetMesh().bmin;
            option.bmax = r.GetMesh().bmax;
            option.cs = r.GetMesh().cs;
            option.ch = r.GetMesh().ch;
            option.buildBvTree = true;
            return new DtNavMeshQuery(new DtNavMesh(DtNavMeshBuilder.CreateNavMeshData(option), option.nvp, 0));
        }


        private bool GetNavMeshHeight(DtNavMeshQuery navMeshQuery, RcVec3f pt, float cs, float heightRange, out float height)
        {
            height = default;

            RcVec3f halfExtents = new RcVec3f { x = cs, y = heightRange, z = cs };
            float maxHeight = pt.y + heightRange;
            RcAtomicBoolean found = new RcAtomicBoolean();
            RcAtomicFloat minHeight = new RcAtomicFloat(pt.y);

            navMeshQuery.QueryPolygons(pt, halfExtents, DtQueryNoOpFilter.Shared, new PolyQueryInvoker((tile, poly, refs) =>
            {
                var status = navMeshQuery.GetPolyHeight(refs, pt, out var h);
                if (status.Succeeded())
                {
                    if (h > minHeight.Get() && h < maxHeight)
                    {
                        minHeight.Exchange(h);
                        found.Set(true);
                    }
                }
            }));

            if (found.Get())
            {
                height = minHeight.Get();
                return true;
            }

            height = pt.y;
            return false;
        }
    }
}