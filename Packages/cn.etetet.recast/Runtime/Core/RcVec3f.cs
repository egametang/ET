/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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
using System.Runtime.CompilerServices;

namespace DotRecast.Core
{
    public struct RcVec3f
    {
        public float x;
        public float y;
        public float z;

        public static RcVec3f Zero { get; } = new RcVec3f(0.0f, 0.0f, 0.0f);
        public static RcVec3f One { get; } = new RcVec3f(1.0f);
        public static RcVec3f UnitX { get; } = new RcVec3f(1.0f, 0.0f, 0.0f);
        public static RcVec3f UnitY { get; } = new RcVec3f(0.0f, 1.0f, 0.0f);
        public static RcVec3f UnitZ { get; } = new RcVec3f(0.0f, 0.0f, 1.0f);

        public static RcVec3f Of(float[] f)
        {
            return Of(f, 0);
        }

        public static RcVec3f Of(float[] f, int idx)
        {
            return Of(f[idx + 0], f[idx + 1], f[idx + 2]);
        }

        public static RcVec3f Of(float x, float y, float z)
        {
            return new RcVec3f(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(float f)
        {
            x = f;
            y = f;
            z = f;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RcVec3f(float[] f)
        {
            x = f[0];
            y = f[1];
            z = f[2];
        }

        public float this[int index]
        {
            get => GetElement(index);
            set => SetElement(index, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetElement(int index)
        {
            switch (index)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
                default: throw new IndexOutOfRangeException($"{index}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetElement(int index, float value)
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float a, float b, float c)
        {
            x = a;
            y = b;
            z = c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in)
        {
            Set(@in, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float[] @in, int i)
        {
            x = @in[i];
            y = @in[i + 1];
            z = @in[i + 2];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RcVec3f Subtract(RcVec3f right)
        {
            return new RcVec3f(
                x - right.x,
                y - right.y,
                z - right.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RcVec3f Add(RcVec3f v2)
        {
            return new RcVec3f(
                x + v2.x,
                y + v2.y,
                z + v2.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly RcVec3f Scale(float scale)
        {
            return new RcVec3f(
                x * scale,
                y * scale,
                z * scale
            );
        }


        /// Derives the dot product of two vectors on the xz-plane. (@p u . @p v)
        /// @param[in] u A vector [(x, y, z)]
        /// @param[in] v A vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D(RcVec3f v)
        {
            return x * v.x + z * v.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Dot2D(float[] v, int vi)
        {
            return x * v[vi] + z * v[vi + 2];
        }


        public override bool Equals(object obj)
        {
            if (!(obj is RcVec3f))
                return false;

            return Equals((RcVec3f)obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RcVec3f other)
        {
            return x.Equals(other.x) &&
                   y.Equals(other.y) &&
                   z.Equals(other.z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            int hash = x.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, y.GetHashCode());
            hash = RcHashCodes.CombineHashCodes(hash, z.GetHashCode());
            return hash;
        }

        /// Normalizes the vector.
        /// @param[in,out] v The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float d = (float)(1.0f / Math.Sqrt(RcMath.Sqr(x) + RcMath.Sqr(y) + RcMath.Sqr(z)));
            if (d != 0)
            {
                x *= d;
                y *= d;
                z *= d;
            }
        }

        public const float EPSILON = 1e-6f;

        /// Normalizes the vector if the length is greater than zero.
        /// If the magnitude is zero, the vector is unchanged.
        /// @param[in,out]	v	The vector to normalize. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SafeNormalize()
        {
            float sqMag = RcMath.Sqr(x) + RcMath.Sqr(y) + RcMath.Sqr(z);
            if (sqMag > EPSILON)
            {
                float inverseMag = 1.0f / (float)Math.Sqrt(sqMag);
                x *= inverseMag;
                y *= inverseMag;
                z *= inverseMag;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(float[] @in, int i)
        {
            x = Math.Min(x, @in[i]);
            y = Math.Min(y, @in[i + 1]);
            z = Math.Min(z, @in[i + 2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Min(RcVec3f b)
        {
            x = Math.Min(x, b.x);
            y = Math.Min(y, b.y);
            z = Math.Min(z, b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(RcVec3f b)
        {
            x = Math.Max(x, b.x);
            y = Math.Max(y, b.y);
            z = Math.Max(z, b.z);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Max(float[] @in, int i)
        {
            x = Math.Max(x, @in[i]);
            y = Math.Max(y, @in[i + 1]);
            z = Math.Max(z, @in[i + 2]);
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RcVec3f left, RcVec3f right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RcVec3f left, RcVec3f right)
        {
            return !left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator -(RcVec3f left, RcVec3f right)
        {
            return left.Subtract(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator +(RcVec3f left, RcVec3f right)
        {
            return left.Add(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(RcVec3f left, RcVec3f right)
        {
            return new RcVec3f(
                left.x * right.x,
                left.y * right.y,
                left.z * right.z
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(RcVec3f left, float right)
        {
            return left * new RcVec3f(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f operator *(float left, RcVec3f right)
        {
            return right * left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Cross(RcVec3f v1, RcVec3f v2)
        {
            return new RcVec3f(
                (v1.y * v2.z) - (v1.z * v2.y),
                (v1.z * v2.x) - (v1.x * v2.z),
                (v1.x * v2.y) - (v1.y * v2.x)
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Lerp(RcVec3f v1, RcVec3f v2, float t)
        {
            return new RcVec3f(
                v1.x + (v2.x - v1.x) * t,
                v1.y + (v2.y - v1.y) * t,
                v1.z + (v2.z - v1.z) * t
            );
        }

        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.x - v1.x;
            float dy = v2.y - v1.y;
            float dz = v2.z - v1.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(RcVec3f v1, RcVec3f v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y)
                                 + (v1.z * v2.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, float[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float[] v1, RcVec3f v2)
        {
            return v1[0] * v2.x + v1[1] * v2.y + v1[2] * v2.z;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PerpXZ(RcVec3f a, RcVec3f b)
        {
            return (a.x * b.z) - (a.z * b.x);
        }

        /// Performs a scaled vector addition. (@p v1 + (@p v2 * @p s))
        /// @param[out] dest The result vector. [(x, y, z)]
        /// @param[in] v1 The base vector. [(x, y, z)]
        /// @param[in] v2 The vector to scale and add to @p v1. [(x, y, z)]
        /// @param[in] s The amount to scale @p v2 by before adding to @p v1.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Mad(RcVec3f v1, RcVec3f v2, float s)
        {
            return new RcVec3f()
            {
                x = v1.x + (v2.x * s),
                y = v1.y + (v2.y * s),
                z = v1.z + (v2.z * s),
            };
        }

        /// Performs a linear interpolation between two vectors. (@p v1 toward @p
        /// v2)
        /// @param[out] dest The result vector. [(x, y, x)]
        /// @param[in] v1 The starting vector.
        /// @param[in] v2 The destination vector.
        /// @param[in] t The interpolation factor. [Limits: 0 <= value <= 1.0]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RcVec3f Lerp(float[] verts, int v1, int v2, float t)
        {
            return new RcVec3f(
                verts[v1 + 0] + (verts[v2 + 0] - verts[v1 + 0]) * t,
                verts[v1 + 1] + (verts[v2 + 1] - verts[v1 + 1]) * t,
                verts[v1 + 2] + (verts[v2 + 2] - verts[v1 + 2]) * t
            );
        }


        /// Returns the distance between two points.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the two points.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(RcVec3f v1, float[] v2, int i)
        {
            float dx = v2[i] - v1.x;
            float dy = v2[i + 1] - v1.y;
            float dz = v2[i + 2] - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.x - v1.x;
            float dy = v2.y - v1.y;
            float dz = v2.z - v1.z;
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(float[] v, int i, int j)
        {
            float dx = v[i] - v[j];
            float dy = v[i + 1] - v[j + 1];
            float dz = v[i + 2] - v[j + 2];
            return dx * dx + dy * dy + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dy = v2[1] - v1[1];
            float dz = v2[2] - v1[2];
            return dx * dx + dy * dy + dz * dz;
        }

        /// Derives the distance between the specified points on the xz-plane.
        /// @param[in] v1 A point. [(x, y, z)]
        /// @param[in] v2 A point. [(x, y, z)]
        /// @return The distance between the point on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2D(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2D(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return dx * dx + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(RcVec3f v1, RcVec3f v2)
        {
            float dx = v2.x - v1.x;
            float dz = v2.z - v1.z;
            return dx * dx + dz * dz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dist2DSqr(RcVec3f p, float[] verts, int i)
        {
            float dx = verts[i] - p.x;
            float dz = verts[i + 2] - p.z;
            return dx * dx + dz * dz;
        }

        /// Derives the xz-plane 2D perp product of the two vectors. (uz*vx - ux*vz)
        /// @param[in] u The LHV vector [(x, y, z)]
        /// @param[in] v The RHV vector [(x, y, z)]
        /// @return The dot product on the xz-plane.
        ///
        /// The vectors are projected onto the xz-plane, so the y-values are
        /// ignored.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Perp2D(RcVec3f u, RcVec3f v)
        {
            return u.z * v.x - u.x * v.z;
        }

        /// Derives the square of the scalar length of the vector. (len * len)
        /// @param[in] v The vector. [(x, y, z)]
        /// @return The square of the scalar length of the vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LenSqr(RcVec3f v)
        {
            return v.x * v.x + v.y * v.y + v.z * v.z;
        }


        /// Checks that the specified vector's components are all finite.
        /// @param[in] v A point. [(x, y, z)]
        /// @return True if all of the point's components are finite, i.e. not NaN
        /// or any of the infinities.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(RcVec3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        }

        /// Checks that the specified vector's 2D components are finite.
        /// @param[in] v A point. [(x, y, z)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite2D(RcVec3f v)
        {
            return float.IsFinite(v.x) && float.IsFinite(v.z);
        }


        public static void Copy(ref RcVec3f @out, float[] @in, int i)
        {
            Copy(ref @out, 0, @in, i);
        }

        public static void Copy(float[] @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Copy(float[] @out, int n, RcVec3f @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Copy(ref RcVec3f @out, int n, float[] @in, int m)
        {
            @out[n] = @in[m];
            @out[n + 1] = @in[m + 1];
            @out[n + 2] = @in[m + 2];
        }

        public static void Add(ref RcVec3f e0, RcVec3f a, float[] verts, int i)
        {
            e0.x = a.x + verts[i];
            e0.y = a.y + verts[i + 1];
            e0.z = a.z + verts[i + 2];
        }


        public static void Sub(ref RcVec3f e0, float[] verts, int i, int j)
        {
            e0.x = verts[i] - verts[j];
            e0.y = verts[i + 1] - verts[j + 1];
            e0.z = verts[i + 2] - verts[j + 2];
        }


        public static void Sub(ref RcVec3f e0, RcVec3f i, float[] verts, int j)
        {
            e0.x = i.x - verts[j];
            e0.y = i.y - verts[j + 1];
            e0.z = i.z - verts[j + 2];
        }


        public static void Cross(float[] dest, float[] v1, float[] v2)
        {
            dest[0] = v1[1] * v2[2] - v1[2] * v2[1];
            dest[1] = v1[2] * v2[0] - v1[0] * v2[2];
            dest[2] = v1[0] * v2[1] - v1[1] * v2[0];
        }

        public static void Cross(float[] dest, RcVec3f v1, RcVec3f v2)
        {
            dest[0] = v1.y * v2.z - v1.z * v2.y;
            dest[1] = v1.z * v2.x - v1.x * v2.z;
            dest[2] = v1.x * v2.y - v1.y * v2.x;
        }

        public static void Cross(ref RcVec3f dest, RcVec3f v1, RcVec3f v2)
        {
            dest.x = v1.y * v2.z - v1.z * v2.y;
            dest.y = v1.z * v2.x - v1.x * v2.z;
            dest.z = v1.x * v2.y - v1.y * v2.x;
        }


        public static void Normalize(float[] v)
        {
            float d = (float)(1.0f / Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]));
            v[0] *= d;
            v[1] *= d;
            v[2] *= d;
        }

        public static void Normalize(ref RcVec3f v)
        {
            float d = (float)(1.0f / Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
            v.x *= d;
            v.y *= d;
            v.z *= d;
        }
    }
}