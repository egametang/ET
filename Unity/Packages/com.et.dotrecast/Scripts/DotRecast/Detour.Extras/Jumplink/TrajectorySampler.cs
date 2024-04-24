using System;
using DotRecast.Core;
using DotRecast.Recast;
using static DotRecast.Core.RcMath;

namespace DotRecast.Detour.Extras.Jumplink
{
    class TrajectorySampler
    {
        public void Sample(JumpLinkBuilderConfig acfg, RcHeightfield heightfield, EdgeSampler es)
        {
            int nsamples = es.start.gsamples.Length;
            for (int i = 0; i < nsamples; ++i)
            {
                GroundSample ssmp = es.start.gsamples[i];
                foreach (GroundSegment end in es.end)
                {
                    GroundSample esmp = end.gsamples[i];
                    if (!ssmp.validHeight || !esmp.validHeight)
                    {
                        continue;
                    }

                    if (!SampleTrajectory(acfg, heightfield, ssmp.p, esmp.p, es.trajectory))
                    {
                        continue;
                    }

                    ssmp.validTrajectory = true;
                    esmp.validTrajectory = true;
                }
            }
        }

        private bool SampleTrajectory(JumpLinkBuilderConfig acfg, RcHeightfield solid, RcVec3f pa, RcVec3f pb, Trajectory tra)
        {
            float cs = Math.Min(acfg.cellSize, acfg.cellHeight);
            float d = RcVec3f.Dist2D(pa, pb) + Math.Abs(pa.y - pb.y);
            int nsamples = Math.Max(2, (int)Math.Ceiling(d / cs));
            for (int i = 0; i < nsamples; ++i)
            {
                float u = (float)i / (float)(nsamples - 1);
                RcVec3f p = tra.Apply(pa, pb, u);
                if (CheckHeightfieldCollision(solid, p.x, p.y + acfg.groundTolerance, p.y + acfg.agentHeight, p.z))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckHeightfieldCollision(RcHeightfield solid, float x, float ymin, float ymax, float z)
        {
            int w = solid.width;
            int h = solid.height;
            float cs = solid.cs;
            float ch = solid.ch;
            RcVec3f orig = solid.bmin;
            int ix = (int)Math.Floor((x - orig.x) / cs);
            int iz = (int)Math.Floor((z - orig.z) / cs);

            if (ix < 0 || iz < 0 || ix > w || iz > h)
            {
                return false;
            }

            RcSpan s = solid.spans[ix + iz * w];
            if (s == null)
            {
                return false;
            }

            while (s != null)
            {
                float symin = orig.y + s.smin * ch;
                float symax = orig.y + s.smax * ch;
                if (OverlapRange(ymin, ymax, symin, symax))
                {
                    return true;
                }

                s = s.next;
            }

            return false;
        }

        private bool OverlapRange(float amin, float amax, float bmin, float bmax)
        {
            return (amin > bmax || amax < bmin) ? false : true;
        }
    }
}