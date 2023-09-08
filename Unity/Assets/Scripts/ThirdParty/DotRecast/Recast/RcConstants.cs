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

namespace DotRecast.Recast
{
    public static class RcConstants
    {
        /// Represents the null area.
        /// When a data element is given this value it is considered to no longer be 
        /// assigned to a usable area.  (E.g. It is un-walkable.)
        public const int RC_NULL_AREA = 0;
        
        /// The default area id used to indicate a walkable polygon. 
        /// This is also the maximum allowed area id, and the only non-null area id 
        /// recognized by some steps in the build process. 
        public const int RC_WALKABLE_AREA = 63;

        /// The value returned by #rcGetCon if the specified direction is not connected
        /// to another span. (Has no neighbor.)
        public const int RC_NOT_CONNECTED = 0x3f;
        
        /// Defines the number of bits allocated to rcSpan::smin and rcSpan::smax.
        public const int SPAN_HEIGHT_BITS = 20;

        /// Defines the maximum value for rcSpan::smin and rcSpan::smax.
        public const int SPAN_MAX_HEIGHT = (1 << SPAN_HEIGHT_BITS) - 1;

        /// Heighfield border flag.
        /// If a heightfield region ID has this bit set, then the region is a border
        /// region and its spans are considered unwalkable.
        /// (Used during the region and contour build process.)
        /// @see rcCompactSpan::reg
        public const int RC_BORDER_REG = 0x8000;

        /// Polygon touches multiple regions.
        /// If a polygon has this region ID it was merged with or created
        /// from polygons of different regions during the polymesh
        /// build step that removes redundant border vertices.
        /// (Used during the polymesh and detail polymesh build processes)
        /// @see rcPolyMesh::regs
        public const int RC_MULTIPLE_REGS = 0;

        // Border vertex flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// a tile border. If a contour vertex's region ID has this bit set, the
        /// vertex will later be removed in order to match the segments and vertices
        /// at tile boundaries.
        /// (Used during the build process.)
        /// @see rcCompactSpan::reg, #rcContour::verts, #rcContour::rverts
        public const int RC_BORDER_VERTEX = 0x10000;

        /// Area border flag.
        /// If a region ID has this bit set, then the associated element lies on
        /// the border of an area.
        /// (Used during the region and contour build process.)
        /// @see rcCompactSpan::reg, #rcContour::verts, #rcContour::rverts
        public const int RC_AREA_BORDER = 0x20000;

        /// Applied to the region id field of contour vertices in order to extract the region id.
        /// The region id field of a vertex may have several flags applied to it. So the
        /// fields value can't be used directly.
        /// @see rcContour::verts, rcContour::rverts
        public const int RC_CONTOUR_REG_MASK = 0xffff;

        /// A value which indicates an invalid index within a mesh.
        /// @note This does not necessarily indicate an error.
        /// @see rcPolyMesh::polys
        public const int RC_MESH_NULL_IDX = 0xffff;

        public const int RC_CONTOUR_TESS_WALL_EDGES = 0x01;

        /// < Tessellate solid (impassable) edges during contour
        /// simplification.
        public const int RC_CONTOUR_TESS_AREA_EDGES = 0x02;


        public const int RC_LOG_WARNING = 1;
    }
}