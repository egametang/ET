/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
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

namespace DotRecast.Recast
{
    public static class PolyUtils
    {
        // public static bool PointInPoly(float[] verts, RcVec3f p)
        // {
        //     bool c = false;
        //     int i, j;
        //     for (i = 0, j = verts.Length - 3; i < verts.Length; j = i, i += 3)
        //     {
        //         int vi = i;
        //         int vj = j;
        //         if (((verts[vi + 2] > p.z) != (verts[vj + 2] > p.z))
        //             && (p.x < (verts[vj] - verts[vi]) * (p.z - verts[vi + 2]) / (verts[vj + 2] - verts[vi + 2])
        //                 + verts[vi]))
        //             c = !c;
        //     }
        //
        //     return c;
        // }

        // TODO (graham): This is duplicated in the ConvexVolumeTool in RecastDemo
        /// Checks if a point is contained within a polygon
        ///
        /// @param[in]	numVerts	Number of vertices in the polygon
        /// @param[in]	verts		The polygon vertices
        /// @param[in]	point		The point to check
        /// @returns true if the point lies within the polygon, false otherwise.
        public static bool PointInPoly(float[] verts, RcVec3f point)
        {
            bool inPoly = false;
            for (int i = 0, j = verts.Length / 3 - 1; i < verts.Length / 3; j = i++)
            {
                RcVec3f vi = RcVec3f.Of(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]);
                RcVec3f vj = RcVec3f.Of(verts[j * 3], verts[j * 3 + 1], verts[j * 3 + 2]);
                if (vi.z > point.z == vj.z > point.z)
                {
                    continue;
                }

                if (point.x >= (vj.x - vi.x) * (point.z - vi.z) / (vj.z - vi.z) + vi.x)
                {
                    continue;
                }

                inPoly = !inPoly;
            }

            return inPoly;
        }

        /// Expands a convex polygon along its vertex normals by the given offset amount.
        /// Inserts extra vertices to bevel sharp corners.
        ///
        /// Helper function to offset convex polygons for rcMarkConvexPolyArea.
        ///
        /// @ingroup recast
        /// 
        /// @param[in]		verts		The vertices of the polygon [Form: (x, y, z) * @p numVerts]
        /// @param[in]		numVerts	The number of vertices in the polygon.
        /// @param[in]		offset		How much to offset the polygon by. [Units: wu]
        /// @param[out]		outVerts	The offset vertices (should hold up to 2 * @p numVerts) [Form: (x, y, z) * return value]
        /// @param[in]		maxOutVerts	The max number of vertices that can be stored to @p outVerts.
        /// @returns Number of vertices in the offset polygon or 0 if too few vertices in @p outVerts.
        public static int OffsetPoly(float[] verts, int numVerts, float offset, float[] outVerts, int maxOutVerts)
        {
            // Defines the limit at which a miter becomes a bevel.
            // Similar in behavior to https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/stroke-miterlimit
            const float MITER_LIMIT = 1.20f;

            int numOutVerts = 0;

            for (int vertIndex = 0; vertIndex < numVerts; vertIndex++)
            {
                int vertIndexA = (vertIndex + numVerts - 1) % numVerts;
                int vertIndexB = vertIndex;
                int vertIndexC = (vertIndex + 1) % numVerts;
                
                RcVec3f vertA = RcVec3f.Of(verts, vertIndexA * 3);
                RcVec3f vertB = RcVec3f.Of(verts, vertIndexB * 3);
                RcVec3f vertC = RcVec3f.Of(verts, vertIndexC * 3);
                
                // From A to B on the x/z plane
                RcVec3f prevSegmentDir = vertB.Subtract(vertA);
                prevSegmentDir.y = 0; // Squash onto x/z plane
                prevSegmentDir.SafeNormalize();
                
                // From B to C on the x/z plane
                RcVec3f currSegmentDir = vertC.Subtract(vertB);
                currSegmentDir.y = 0; // Squash onto x/z plane
                currSegmentDir.SafeNormalize();
                
                // The y component of the cross product of the two normalized segment directions.
                // The X and Z components of the cross product are both zero because the two
                // segment direction vectors fall within the x/z plane.
                float cross = currSegmentDir.x * prevSegmentDir.z - prevSegmentDir.x * currSegmentDir.z;

                // CCW perpendicular vector to AB.  The segment normal.
                float prevSegmentNormX = -prevSegmentDir.z;
                float prevSegmentNormZ = prevSegmentDir.x;

                // CCW perpendicular vector to BC.  The segment normal.
                float currSegmentNormX = -currSegmentDir.z;
                float currSegmentNormZ = currSegmentDir.x;

                // Average the two segment normals to get the proportional miter offset for B.
                // This isn't normalized because it's defining the distance and direction the corner will need to be
                // adjusted proportionally to the edge offsets to properly miter the adjoining edges.
                float cornerMiterX = (prevSegmentNormX + currSegmentNormX) * 0.5f;
                float cornerMiterZ = (prevSegmentNormZ + currSegmentNormZ) * 0.5f;
                float cornerMiterSqMag = RcMath.Sqr(cornerMiterX) + RcMath.Sqr(cornerMiterZ);
                
                // If the magnitude of the segment normal average is less than about .69444,
                // the corner is an acute enough angle that the result should be beveled.
                bool bevel = cornerMiterSqMag * MITER_LIMIT * MITER_LIMIT < 1.0f;
                
                // Scale the corner miter so it's proportional to how much the corner should be offset compared to the edges.
                if (cornerMiterSqMag > RcVec3f.EPSILON)
                {
                    float scale = 1.0f / cornerMiterSqMag;
                    cornerMiterX *= scale;
                    cornerMiterZ *= scale;
                }
                
                if (bevel && cross < 0.0f) // If the corner is convex and an acute enough angle, generate a bevel.
                {
                    if (numOutVerts + 2 > maxOutVerts)
                    {
                        return 0;
                    }

                    // Generate two bevel vertices at a distances from B proportional to the angle between the two segments.
                    // Move each bevel vertex out proportional to the given offset.
                    float d = (1.0f - (prevSegmentDir.x * currSegmentDir.x + prevSegmentDir.z * currSegmentDir.z)) * 0.5f;

                    outVerts[numOutVerts * 3 + 0] = vertB.x + (-prevSegmentNormX + prevSegmentDir.x * d) * offset;
                    outVerts[numOutVerts * 3 + 1] = vertB.y;
                    outVerts[numOutVerts * 3 + 2] = vertB.z + (-prevSegmentNormZ + prevSegmentDir.z * d) * offset;
                    numOutVerts++;

                    outVerts[numOutVerts * 3 + 0] = vertB.x + (-currSegmentNormX - currSegmentDir.x * d) * offset;
                    outVerts[numOutVerts * 3 + 1] = vertB.y;
                    outVerts[numOutVerts * 3 + 2] = vertB.z + (-currSegmentNormZ - currSegmentDir.z * d) * offset;
                    numOutVerts++;
                }
                else
                {
                    if (numOutVerts + 1 > maxOutVerts)
                    {
                        return 0;
                    }

                    // Move B along the miter direction by the specified offset.
                    outVerts[numOutVerts * 3 + 0] = vertB.x - cornerMiterX * offset;
                    outVerts[numOutVerts * 3 + 1] = vertB.y;
                    outVerts[numOutVerts * 3 + 2] = vertB.z - cornerMiterZ * offset;
                    numOutVerts++;
                }
            }

            return numOutVerts;
        }
    }
}