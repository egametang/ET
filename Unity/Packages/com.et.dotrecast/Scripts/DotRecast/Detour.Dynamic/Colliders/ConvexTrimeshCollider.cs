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
    public class ConvexTrimeshCollider : AbstractCollider
    {
        private readonly float[] vertices;
        private readonly int[] triangles;

        public ConvexTrimeshCollider(float[] vertices, int[] triangles, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, TrimeshCollider.ComputeBounds(vertices))
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }

        public ConvexTrimeshCollider(float[] vertices, int[] triangles, float[] bounds, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, bounds)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }

        public override void Rasterize(RcHeightfield hf, RcTelemetry telemetry)
        {
            RecastFilledVolumeRasterization.RasterizeConvex(hf, vertices, triangles, area,
                (int)Math.Floor(flagMergeThreshold / hf.ch), telemetry);
        }
    }
}