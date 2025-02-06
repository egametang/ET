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
using DotRecast.Core;

namespace DotRecast.Detour
{
    /**
 * Convex-convex intersection based on "Computational Geometry in C" by Joseph O'Rourke
 */
    public static class ConvexConvexIntersection
    {
        private static readonly float EPSILON = 0.0001f;

        public static float[] Intersect(float[] p, float[] q)
        {
            int n = p.Length / 3;
            int m = q.Length / 3;
            float[] inters = new float[Math.Max(m, n) * 3 * 3];
            int ii = 0;
            /* Initialize variables. */
            RcVec3f a = new RcVec3f();
            RcVec3f b = new RcVec3f();
            RcVec3f a1 = new RcVec3f();
            RcVec3f b1 = new RcVec3f();

            int aa = 0;
            int ba = 0;
            int ai = 0;
            int bi = 0;

            InFlag f = InFlag.Unknown;
            bool FirstPoint = true;
            RcVec3f ip = new RcVec3f();
            RcVec3f iq = new RcVec3f();

            do
            {
                a.Set(p, 3 * (ai % n));
                b.Set(q, 3 * (bi % m));
                a1.Set(p, 3 * ((ai + n - 1) % n)); // prev a
                b1.Set(q, 3 * ((bi + m - 1) % m)); // prev b

                RcVec3f A = a.Subtract(a1);
                RcVec3f B = b.Subtract(b1);

                float cross = B.x * A.z - A.x * B.z; // TriArea2D({0, 0}, A, B);
                float aHB = DtUtils.TriArea2D(b1, b, a);
                float bHA = DtUtils.TriArea2D(a1, a, b);
                if (Math.Abs(cross) < EPSILON)
                {
                    cross = 0f;
                }

                bool parallel = cross == 0f;
                Intersection code = parallel ? ParallelInt(a1, a, b1, b, ref ip, ref iq) : SegSegInt(a1, a, b1, b, ref ip, ref iq);

                if (code == Intersection.Single)
                {
                    if (FirstPoint)
                    {
                        FirstPoint = false;
                        aa = ba = 0;
                    }

                    ii = AddVertex(inters, ii, ip);
                    f = InOut(f, aHB, bHA);
                }

                /*-----Advance rules-----*/

                /* Special case: A & B overlap and oppositely oriented. */
                if (code == Intersection.Overlap && A.Dot2D(B) < 0)
                {
                    ii = AddVertex(inters, ii, ip);
                    ii = AddVertex(inters, ii, iq);
                    break;
                }

                /* Special case: A & B parallel and separated. */
                if (parallel && aHB < 0f && bHA < 0f)
                {
                    return null;
                }
                /* Special case: A & B collinear. */
                else if (parallel && Math.Abs(aHB) < EPSILON && Math.Abs(bHA) < EPSILON)
                {
                    /* Advance but do not output point. */
                    if (f == InFlag.Pin)
                    {
                        ba++;
                        bi++;
                    }
                    else
                    {
                        aa++;
                        ai++;
                    }
                }
                /* Generic cases. */
                else if (cross >= 0)
                {
                    if (bHA > 0)
                    {
                        if (f == InFlag.Pin)
                        {
                            ii = AddVertex(inters, ii, a);
                        }

                        aa++;
                        ai++;
                    }
                    else
                    {
                        if (f == InFlag.Qin)
                        {
                            ii = AddVertex(inters, ii, b);
                        }

                        ba++;
                        bi++;
                    }
                }
                else
                {
                    if (aHB > 0)
                    {
                        if (f == InFlag.Qin)
                        {
                            ii = AddVertex(inters, ii, b);
                        }

                        ba++;
                        bi++;
                    }
                    else
                    {
                        if (f == InFlag.Pin)
                        {
                            ii = AddVertex(inters, ii, a);
                        }

                        aa++;
                        ai++;
                    }
                }
                /* Quit when both adv. indices have cycled, or one has cycled twice. */
            } while ((aa < n || ba < m) && aa < 2 * n && ba < 2 * m);

            /* Deal with special cases: not implemented. */
            if (f == InFlag.Unknown)
            {
                return null;
            }

            float[] copied = new float[ii];
            Array.Copy(inters, copied, ii);
            return copied;
        }

        private static int AddVertex(float[] inters, int ii, float[] p)
        {
            if (ii > 0)
            {
                if (inters[ii - 3] == p[0] && inters[ii - 2] == p[1] && inters[ii - 1] == p[2])
                {
                    return ii;
                }

                if (inters[0] == p[0] && inters[1] == p[1] && inters[2] == p[2])
                {
                    return ii;
                }
            }

            inters[ii] = p[0];
            inters[ii + 1] = p[1];
            inters[ii + 2] = p[2];
            return ii + 3;
        }

        private static int AddVertex(float[] inters, int ii, RcVec3f p)
        {
            if (ii > 0)
            {
                if (inters[ii - 3] == p.x && inters[ii - 2] == p.y && inters[ii - 1] == p.z)
                {
                    return ii;
                }

                if (inters[0] == p.x && inters[1] == p.y && inters[2] == p.z)
                {
                    return ii;
                }
            }

            inters[ii] = p.x;
            inters[ii + 1] = p.y;
            inters[ii + 2] = p.z;
            return ii + 3;
        }


        private static InFlag InOut(InFlag inflag, float aHB, float bHA)
        {
            if (aHB > 0)
            {
                return InFlag.Pin;
            }
            else if (bHA > 0)
            {
                return InFlag.Qin;
            }

            return inflag;
        }

        private static Intersection SegSegInt(RcVec3f a, RcVec3f b, RcVec3f c, RcVec3f d, ref RcVec3f p, ref RcVec3f q)
        {
            if (DtUtils.IntersectSegSeg2D(a, b, c, d, out var s, out var t))
            {
                if (s >= 0.0f && s <= 1.0f && t >= 0.0f && t <= 1.0f)
                {
                    p.x = a.x + (b.x - a.x) * s;
                    p.y = a.y + (b.y - a.y) * s;
                    p.z = a.z + (b.z - a.z) * s;
                    return Intersection.Single;
                }
            }

            return Intersection.None;
        }

        private static Intersection ParallelInt(RcVec3f a, RcVec3f b, RcVec3f c, RcVec3f d, ref RcVec3f p, ref RcVec3f q)
        {
            if (Between(a, b, c) && Between(a, b, d))
            {
                p = c;
                q = d;
                return Intersection.Overlap;
            }

            if (Between(c, d, a) && Between(c, d, b))
            {
                p = a;
                q = b;
                return Intersection.Overlap;
            }

            if (Between(a, b, c) && Between(c, d, b))
            {
                p = c;
                q = b;
                return Intersection.Overlap;
            }

            if (Between(a, b, c) && Between(c, d, a))
            {
                p = c;
                q = a;
                return Intersection.Overlap;
            }

            if (Between(a, b, d) && Between(c, d, b))
            {
                p = d;
                q = b;
                return Intersection.Overlap;
            }

            if (Between(a, b, d) && Between(c, d, a))
            {
                p = d;
                q = a;
                return Intersection.Overlap;
            }

            return Intersection.None;
        }

        private static bool Between(RcVec3f a, RcVec3f b, RcVec3f c)
        {
            if (Math.Abs(a.x - b.x) > Math.Abs(a.z - b.z))
            {
                return ((a.x <= c.x) && (c.x <= b.x)) || ((a.x >= c.x) && (c.x >= b.x));
            }
            else
            {
                return ((a.z <= c.z) && (c.z <= b.z)) || ((a.z >= c.z) && (c.z >= b.z));
            }
        }
    }
}