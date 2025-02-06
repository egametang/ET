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
    public class TrimeshCollider : AbstractCollider
    {
        private readonly float[] vertices;
        private readonly int[] triangles;

        public TrimeshCollider(float[] vertices, int[] triangles, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, ComputeBounds(vertices))
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }

        public TrimeshCollider(float[] vertices, int[] triangles, float[] bounds, int area, float flagMergeThreshold) :
            base(area, flagMergeThreshold, bounds)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }

        public static float[] ComputeBounds(float[] vertices)
        {
            float[] bounds = new float[] { vertices[0], vertices[1], vertices[2], vertices[0], vertices[1], vertices[2] };
            for (int i = 3; i < vertices.Length; i += 3)
            {
                bounds[0] = Math.Min(bounds[0], vertices[i]);
                bounds[1] = Math.Min(bounds[1], vertices[i + 1]);
                bounds[2] = Math.Min(bounds[2], vertices[i + 2]);
                bounds[3] = Math.Max(bounds[3], vertices[i]);
                bounds[4] = Math.Max(bounds[4], vertices[i + 1]);
                bounds[5] = Math.Max(bounds[5], vertices[i + 2]);
            }

            return bounds;
        }

        public override void Rasterize(RcHeightfield hf, RcTelemetry telemetry)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                RecastRasterization.RasterizeTriangle(hf, vertices, triangles[i], triangles[i + 1], triangles[i + 2], area,
                    (int)Math.Floor(flagMergeThreshold / hf.ch), telemetry);
            }
        }
    }
}
