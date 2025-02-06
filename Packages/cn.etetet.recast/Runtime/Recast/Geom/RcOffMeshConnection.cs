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

namespace DotRecast.Recast.Geom
{
    public class RcOffMeshConnection
    {
        public readonly float[] verts;
        public readonly float radius;

        public readonly bool bidir;
        public readonly int area;

        public readonly int flags;
        public readonly int userId;

        public RcOffMeshConnection(RcVec3f start, RcVec3f end, float radius, bool bidir, int area, int flags)
        {
            verts = new float[6];
            verts[0] = start.x;
            verts[1] = start.y;
            verts[2] = start.z;
            verts[3] = end.x;
            verts[4] = end.y;
            verts[5] = end.z;
            this.radius = radius;
            this.bidir = bidir;
            this.area = area;
            this.flags = flags;
        }
    }
}