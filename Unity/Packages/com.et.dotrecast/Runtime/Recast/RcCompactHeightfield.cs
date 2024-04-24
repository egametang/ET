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

using DotRecast.Core;

namespace DotRecast.Recast
{
    /** A compact, static heightfield representing unobstructed space. */
    public class RcCompactHeightfield
    {
        /** The width of the heightfield. (Along the x-axis in cell units.) */
        public int width;

        /** The height of the heightfield. (Along the z-axis in cell units.) */
        public int height;

        /** The number of spans in the heightfield. */
        public int spanCount;

        /** The walkable height used during the build of the field. (See: RecastConfig::walkableHeight) */
        public int walkableHeight;

        /** The walkable climb used during the build of the field. (See: RecastConfig::walkableClimb) */
        public int walkableClimb;

        /** The AABB border size used during the build of the field. (See: RecastConfig::borderSize) */
        public int borderSize;

        /** The maximum distance value of any span within the field. */
        public int maxDistance;

        /** The maximum region id of any span within the field. */
        public int maxRegions;

        /** The minimum bounds in world space. [(x, y, z)] */
        public RcVec3f bmin = new RcVec3f();

        /** The maximum bounds in world space. [(x, y, z)] */
        public RcVec3f bmax = new RcVec3f();

        /** The size of each cell. (On the xz-plane.) */
        public float cs;

        /** The height of each cell. (The minimum increment along the y-axis.) */
        public float ch;

        /** Array of cells. [Size: #width*#height] */
        public RcCompactCell[] cells;

        /** Array of spans. [Size: #spanCount] */
        public RcCompactSpan[] spans;

        /** Array containing border distance data. [Size: #spanCount] */
        public int[] dist;

        /** Array containing area id data. [Size: #spanCount] */
        public int[] areas;
    }
}