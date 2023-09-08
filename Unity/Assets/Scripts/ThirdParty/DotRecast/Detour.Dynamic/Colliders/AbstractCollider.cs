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

using DotRecast.Core;
using DotRecast.Recast;

namespace DotRecast.Detour.Dynamic.Colliders
{
    public abstract class AbstractCollider : ICollider
    {
        protected readonly int area;
        protected readonly float flagMergeThreshold;
        protected readonly float[] _bounds;

        public AbstractCollider(int area, float flagMergeThreshold, float[] bounds)
        {
            this.area = area;
            this.flagMergeThreshold = flagMergeThreshold;
            this._bounds = bounds;
        }

        public float[] Bounds()
        {
            return _bounds;
        }

        public abstract void Rasterize(RcHeightfield hf, RcTelemetry telemetry);
    }
}