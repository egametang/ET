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

namespace DotRecast.Core
{
    public static class Intersections
    {
        public static bool IntersectSegmentTriangle(RcVec3f sp, RcVec3f sq, RcVec3f a, RcVec3f b, RcVec3f c, out float t)
        {
            t = 0;
            float v, w;
            RcVec3f ab = b.Subtract(a);
            RcVec3f ac = c.Subtract(a);
            RcVec3f qp = sp.Subtract(sq);

            // Compute triangle normal. Can be precalculated or cached if
            // intersecting multiple segments against the same triangle
            RcVec3f norm = RcVec3f.Cross(ab, ac);

            // Compute denominator d. If d <= 0, segment is parallel to or points
            // away from triangle, so exit early
            float d = RcVec3f.Dot(qp, norm);
            if (d <= 0.0f)
            {
                return false;
            }

            // Compute intersection t value of pq with plane of triangle. A ray
            // intersects iff 0 <= t. Segment intersects iff 0 <= t <= 1. Delay
            // dividing by d until intersection has been found to pierce triangle
            RcVec3f ap = sp.Subtract(a);
            t = RcVec3f.Dot(ap, norm);
            if (t < 0.0f)
            {
                return false;
            }

            if (t > d)
            {
                return false; // For segment; exclude this code line for a ray test
            }

            // Compute barycentric coordinate components and test if within bounds
            RcVec3f e = RcVec3f.Cross(qp, ap);
            v = RcVec3f.Dot(ac, e);
            if (v < 0.0f || v > d)
            {
                return false;
            }

            w = -RcVec3f.Dot(ab, e);
            if (w < 0.0f || v + w > d)
            {
                return false;
            }

            // Segment/ray intersects triangle. Perform delayed division
            t /= d;

            return true;
        }

        public static bool IsectSegAABB(RcVec3f sp, RcVec3f sq, RcVec3f amin, RcVec3f amax, out float tmin, out float tmax)
        {
            const float EPS = 1e-6f;

            RcVec3f d = new RcVec3f();
            d.x = sq.x - sp.x;
            d.y = sq.y - sp.y;
            d.z = sq.z - sp.z;
            tmin = 0.0f;
            tmax = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(d[i]) < EPS)
                {
                    if (sp[i] < amin[i] || sp[i] > amax[i])
                    {
                        return false;
                    }
                }
                else
                {
                    float ood = 1.0f / d[i];
                    float t1 = (amin[i] - sp[i]) * ood;
                    float t2 = (amax[i] - sp[i]) * ood;

                    if (t1 > t2)
                    {
                        (t1, t2) = (t2, t1);
                    }

                    if (t1 > tmin)
                    {
                        tmin = t1;
                    }

                    if (t2 < tmax)
                    {
                        tmax = t2;
                    }

                    if (tmin > tmax)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}