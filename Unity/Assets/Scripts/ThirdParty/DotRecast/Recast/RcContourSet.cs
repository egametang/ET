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

using System.Collections.Generic;
using DotRecast.Core;

namespace DotRecast.Recast
{
    /** Represents a group of related contours. */
    public class RcContourSet
    {
        /** A list of the contours in the set. */
        public List<RcContour> conts = new List<RcContour>();

        /** The minimum bounds in world space. [(x, y, z)] */
        public RcVec3f bmin = new RcVec3f();

        /** The maximum bounds in world space. [(x, y, z)] */
        public RcVec3f bmax = new RcVec3f();

        /** The size of each cell. (On the xz-plane.) */
        public float cs;

        /** The height of each cell. (The minimum increment along the y-axis.) */
        public float ch;

        /** The width of the set. (Along the x-axis in cell units.) */
        public int width;

        /** The height of the set. (Along the z-axis in cell units.) */
        public int height;

        /** The AABB border size used to generate the source data from which the contours were derived. */
        public int borderSize;

        /** The max edge error that this contour set was simplified with. */
        public float maxError;
    }
}