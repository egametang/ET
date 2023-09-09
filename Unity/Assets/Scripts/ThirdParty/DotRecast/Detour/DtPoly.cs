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

namespace DotRecast.Detour
{
    /** Defines a polygon within a MeshTile object. */
    public class DtPoly
    {
        /** The polygon is a standard convex polygon that is part of the surface of the mesh. */
        public const int DT_POLYTYPE_GROUND = 0;

        /** The polygon is an off-mesh connection consisting of two vertices. */
        public const int DT_POLYTYPE_OFFMESH_CONNECTION = 1;
        
        public readonly int index;

        /** The indices of the polygon's vertices. The actual vertices are located in MeshTile::verts. */
        public readonly int[] verts;

        /** Packed data representing neighbor polygons references and flags for each edge. */
        public readonly int[] neis;

        /** The user defined polygon flags. */
        public int flags;

        /** The number of vertices in the polygon. */
        public int vertCount;

        /// The bit packed area id and polygon type.
        /// @note Use the structure's set and get methods to access this value.
        public int areaAndtype;

        public DtPoly(int index, int maxVertsPerPoly)
        {
            this.index = index;
            verts = new int[maxVertsPerPoly];
            neis = new int[maxVertsPerPoly];
        }

        /** Sets the user defined area id. [Limit: &lt; {@link org.recast4j.detour.NavMesh#DT_MAX_AREAS}] */
        public void SetArea(int a)
        {
            areaAndtype = (areaAndtype & 0xc0) | (a & 0x3f);
        }

        /** Sets the polygon type. (See: #dtPolyTypes.) */
        public void SetPolyType(int t)
        {
            areaAndtype = (areaAndtype & 0x3f) | (t << 6);
        }

        /** Gets the user defined area id. */
        public int GetArea()
        {
            return areaAndtype & 0x3f;
        }

        /** Gets the polygon type. (See: #dtPolyTypes) */
        public int GetPolyType()
        {
            return areaAndtype >> 6;
        }
    }
}