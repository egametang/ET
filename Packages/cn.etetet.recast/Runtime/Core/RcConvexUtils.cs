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

using System.Collections.Generic;

namespace DotRecast.Core
{
    public static class RcConvexUtils
    {
        // Calculates convex hull on xz-plane of points on 'pts',
        // stores the indices of the resulting hull in 'out' and
        // returns number of points on hull.
        public static List<int> Convexhull(List<RcVec3f> pts)
        {
            int npts = pts.Count;
            List<int> @out = new List<int>();
            // Find lower-leftmost point.
            int hull = 0;
            for (int i = 1; i < npts; ++i)
            {
                if (Cmppt(pts[i], pts[hull]))
                {
                    hull = i;
                }
            }

            // Gift wrap hull.
            int endpt = 0;
            do
            {
                @out.Add(hull);
                endpt = 0;
                for (int j = 1; j < npts; ++j)
                {
                    RcVec3f a = pts[hull];
                    RcVec3f b = pts[endpt];
                    RcVec3f c = pts[j];
                    if (hull == endpt || Left(a, b, c))
                    {
                        endpt = j;
                    }
                }

                hull = endpt;
            } while (endpt != @out[0]);

            return @out;
        }

        // Returns true if 'a' is more lower-left than 'b'.
        private static bool Cmppt(RcVec3f a, RcVec3f b)
        {
            if (a.x < b.x)
            {
                return true;
            }

            if (a.x > b.x)
            {
                return false;
            }

            if (a.z < b.z)
            {
                return true;
            }

            if (a.z > b.z)
            {
                return false;
            }

            return false;
        }

        // Returns true if 'c' is left of line 'a'-'b'.
        private static bool Left(RcVec3f a, RcVec3f b, RcVec3f c)
        {
            float u1 = b.x - a.x;
            float v1 = b.z - a.z;
            float u2 = c.x - a.x;
            float v2 = c.z - a.z;
            return u1 * v2 - v1 * u2 < 0;
        }
    }
}