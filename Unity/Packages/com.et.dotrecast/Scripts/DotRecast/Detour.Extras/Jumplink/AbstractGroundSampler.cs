using System;
using DotRecast.Core;
using DotRecast.Recast;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    public abstract class AbstractGroundSampler : IGroundSampler
    {
        public delegate bool ComputeNavMeshHeight(RcVec3f pt, float cellSize, out float height);

        protected void SampleGround(JumpLinkBuilderConfig acfg, EdgeSampler es, ComputeNavMeshHeight heightFunc)
        {
            float cs = acfg.cellSize;
            float dist = (float)Math.Sqrt(RcVec3f.Dist2DSqr(es.start.p, es.start.q));
            int ngsamples = Math.Max(2, (int)Math.Ceiling(dist / cs));

            SampleGroundSegment(heightFunc, es.start, ngsamples);
            foreach (GroundSegment end in es.end)
            {
                SampleGroundSegment(heightFunc, end, ngsamples);
            }
        }

        public abstract void Sample(JumpLinkBuilderConfig acfg, RecastBuilderResult result, EdgeSampler es);

        protected void SampleGroundSegment(ComputeNavMeshHeight heightFunc, GroundSegment seg, int nsamples)
        {
            seg.gsamples = new GroundSample[nsamples];

            for (int i = 0; i < nsamples; ++i)
            {
                float u = i / (float)(nsamples - 1);

                GroundSample s = new GroundSample();
                seg.gsamples[i] = s;
                RcVec3f pt = RcVec3f.Lerp(seg.p, seg.q, u);
                bool success = heightFunc.Invoke(pt, seg.height, out var height);
                s.p.x = pt.x;
                s.p.y = height;
                s.p.z = pt.z;

                if (!success)
                {
                    continue;
                }

                s.validHeight = true;
            }
        }
    }
}