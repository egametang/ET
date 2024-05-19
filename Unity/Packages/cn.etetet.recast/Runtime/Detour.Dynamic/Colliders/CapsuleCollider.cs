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
    public class CapsuleCollider : AbstractCollider
    {
        private readonly RcVec3f start;
        private readonly RcVec3f end;
        private readonly float radius;

        public CapsuleCollider(RcVec3f start, RcVec3f end, float radius, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, Bounds(start, end, radius))
        {
            this.start = start;
            this.end = end;
            this.radius = radius;
        }

        public override void Rasterize(RcHeightfield hf, RcTelemetry telemetry)
        {
            RecastFilledVolumeRasterization.RasterizeCapsule(hf, start, end, radius, area, (int)Math.Floor(flagMergeThreshold / hf.ch),
                telemetry);
        }

        private static float[] Bounds(RcVec3f start, RcVec3f end, float radius)
        {
            return new float[]
            {
                Math.Min(start.x, end.x) - radius, Math.Min(start.y, end.y) - radius,
                Math.Min(start.z, end.z) - radius, Math.Max(start.x, end.x) + radius, Math.Max(start.y, end.y) + radius,
                Math.Max(start.z, end.z) + radius
            };
        }
    }
}