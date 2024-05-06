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
using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic.Colliders
{
    public class BoxCollider : AbstractCollider
    {
        private readonly RcVec3f center;
        private readonly RcVec3f[] halfEdges;

        public BoxCollider(RcVec3f center, RcVec3f[] halfEdges, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, Bounds(center, halfEdges))
        {
            this.center = center;
            this.halfEdges = halfEdges;
        }

        private static float[] Bounds(RcVec3f center, RcVec3f[] halfEdges)
        {
            float[] bounds = new float[]
            {
                float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity,
                float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity
            };
            for (int i = 0; i < 8; ++i)
            {
                float s0 = (i & 1) != 0 ? 1f : -1f;
                float s1 = (i & 2) != 0 ? 1f : -1f;
                float s2 = (i & 4) != 0 ? 1f : -1f;
                float vx = center.x + s0 * halfEdges[0].x + s1 * halfEdges[1].x + s2 * halfEdges[2].x;
                float vy = center.y + s0 * halfEdges[0].y + s1 * halfEdges[1].y + s2 * halfEdges[2].y;
                float vz = center.z + s0 * halfEdges[0].z + s1 * halfEdges[1].z + s2 * halfEdges[2].z;
                bounds[0] = Math.Min(bounds[0], vx);
                bounds[1] = Math.Min(bounds[1], vy);
                bounds[2] = Math.Min(bounds[2], vz);
                bounds[3] = Math.Max(bounds[3], vx);
                bounds[4] = Math.Max(bounds[4], vy);
                bounds[5] = Math.Max(bounds[5], vz);
            }

            return bounds;
        }

        public override void Rasterize(RcHeightfield hf, RcTelemetry telemetry)
        {
            RecastFilledVolumeRasterization.RasterizeBox(
                hf, center, halfEdges, area, (int)Math.Floor(flagMergeThreshold / hf.ch), telemetry);
        }

        public static RcVec3f[] GetHalfEdges(RcVec3f up, RcVec3f forward, RcVec3f extent)
        {
            RcVec3f[] halfEdges =
            {
                RcVec3f.Zero,
                RcVec3f.Of(up.x, up.y, up.z),
                RcVec3f.Zero
            };
            RcVec3f.Normalize(ref halfEdges[1]);
            RcVec3f.Cross(ref halfEdges[0], up, forward);
            RcVec3f.Normalize(ref halfEdges[0]);
            RcVec3f.Cross(ref halfEdges[2], halfEdges[0], up);
            RcVec3f.Normalize(ref halfEdges[2]);
            halfEdges[0].x *= extent.x;
            halfEdges[0].y *= extent.x;
            halfEdges[0].z *= extent.x;
            halfEdges[1].x *= extent.y;
            halfEdges[1].y *= extent.y;
            halfEdges[1].z *= extent.y;
            halfEdges[2].x *= extent.z;
            halfEdges[2].y *= extent.z;
            halfEdges[2].z *= extent.z;
            return halfEdges;
        }
    }
}
