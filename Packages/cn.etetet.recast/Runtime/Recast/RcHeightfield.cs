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
    /** Represents a heightfield layer within a layer set. */
    public class RcHeightfield
    {
        /** The width of the heightfield. (Along the x-axis in cell units.) */
        public readonly int width;

        /** The height of the heightfield. (Along the z-axis in cell units.) */
        public readonly int height;

        /** The minimum bounds in world space. [(x, y, z)] */
        public readonly RcVec3f bmin;

        /** The maximum bounds in world space. [(x, y, z)] */
        public RcVec3f bmax;

        /** The size of each cell. (On the xz-plane.) */
        public readonly float cs;

        /** The height of each cell. (The minimum increment along the y-axis.) */
        public readonly float ch;

        /** Heightfield of spans (width*height). */
        public readonly RcSpan[] spans;

        /** Border size in cell units */
        public readonly int borderSize;

        public RcHeightfield(int width, int height, RcVec3f bmin, RcVec3f bmax, float cs, float ch, int borderSize)
        {
            this.width = width;
            this.height = height;
            this.bmin = bmin;
            this.bmax = bmax;
            this.cs = cs;
            this.ch = ch;
            this.borderSize = borderSize;
            spans = new RcSpan[width * height];
        }
    }
}