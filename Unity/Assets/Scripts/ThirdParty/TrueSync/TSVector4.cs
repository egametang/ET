/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

using System;

namespace TrueSync
{
    /// <summary>
    /// A vector structure.
    /// </summary>
    [Serializable]
    public struct TSVector4
    {

        private static FP ZeroEpsilonSq = TSMath.Epsilon;
        internal static TSVector4 InternalZero;

        /// <summary>The X component of the vector.</summary>
        public FP x;
        /// <summary>The Y component of the vector.</summary>
        public FP y;
        /// <summary>The Z component of the vector.</summary>
        public FP z;
        /// <summary>The W component of the vector.</summary>
        public FP w;

        #region Static readonly variables
        /// <summary>
        /// A vector with components (0,0,0,0);
        /// </summary>
        public static readonly TSVector4 zero;
        /// <summary>
        /// A vector with components (1,1,1,1);
        /// </summary>
        public static readonly TSVector4 one;
        /// <summary>
        /// A vector with components 
        /// (FP.MinValue,FP.MinValue,FP.MinValue);
        /// </summary>
        public static readonly TSVector4 MinValue;
        /// <summary>
        /// A vector with components 
        /// (FP.MaxValue,FP.MaxValue,FP.MaxValue);
        /// </summary>
        public static readonly TSVector4 MaxValue;
        #endregion

        #region Private static constructor
        static TSVector4()
        {
            one = new TSVector4(1, 1, 1, 1);
            zero = new TSVector4(0, 0, 0, 0);
            MinValue = new TSVector4(FP.MinValue);
            MaxValue = new TSVector4(FP.MaxValue);
            InternalZero = zero;
        }
        #endregion

        public static TSVector4 Abs(TSVector4 other)
        {
            return new TSVector4(FP.Abs(other.x), FP.Abs(other.y), FP.Abs(other.z), FP.Abs(other.z));
        }

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        /// <returns>Returns the squared length of the vector.</returns>
        public FP sqrMagnitude
        {
            get
            {
                return (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z) + (this.w * this.w));
            }
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        /// <returns>Returns the length of the vector.</returns>
        public FP magnitude
        {
            get
            {
                FP num = sqrMagnitude;
                return FP.Sqrt(num);
            }
        }

        public static TSVector4 ClampMagnitude(TSVector4 vector, FP maxLength)
        {
            return Normalize(vector) * maxLength;
        }

        /// <summary>
        /// Gets a normalized version of the vector.
        /// </summary>
        /// <returns>Returns a normalized version of the vector.</returns>
        public TSVector4 normalized
        {
            get
            {
                TSVector4 result = new TSVector4(this.x, this.y, this.z, this.w);
                result.Normalize();

                return result;
            }
        }

        /// <summary>
        /// Constructor initializing a new instance of the structure
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        /// <param name="w">The W component of the vector.</param>
        public TSVector4(int x, int y, int z, int w)
        {
            this.x = (FP)x;
            this.y = (FP)y;
            this.z = (FP)z;
            this.w = (FP)w;
        }

        public TSVector4(FP x, FP y, FP z, FP w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public void Scale(TSVector4 other)
        {
            this.x = x * other.x;
            this.y = y * other.y;
            this.z = z * other.z;
            this.w = w * other.w;
        }

        /// <summary>
        /// Sets all vector component to specific values.
        /// </summary>
        /// <param name="x">The X component of the vector.</param>
        /// <param name="y">The Y component of the vector.</param>
        /// <param name="z">The Z component of the vector.</param>
        /// <param name="w">The W component of the vector.</param>
        public void Set(FP x, FP y, FP z, FP w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Constructor initializing a new instance of the structure
        /// </summary>
        /// <param name="xyz">All components of the vector are set to xyz</param>
        public TSVector4(FP xyzw)
        {
            this.x = xyzw;
            this.y = xyzw;
            this.z = xyzw;
            this.w = xyzw;
        }

        public static TSVector4 Lerp(TSVector4 from, TSVector4 to, FP percent)
        {
            return from + (to - from) * percent;
        }

        /// <summary>
        /// Builds a string from the JVector.
        /// </summary>
        /// <returns>A string containing all three components.</returns>
        #region public override string ToString()
        public override string ToString()
        {
            return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", x.AsFloat(), y.AsFloat(), z.AsFloat(), w.AsFloat());
        }
        #endregion

        /// <summary>
        /// Tests if an object is equal to this vector.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>Returns true if they are euqal, otherwise false.</returns>
        #region public override bool Equals(object obj)
        public override bool Equals(object obj)
        {
            if (!(obj is TSVector4)) return false;
            TSVector4 other = (TSVector4)obj;

            return (((x == other.x) && (y == other.y)) && (z == other.z) && (w == other.w));
        }
        #endregion

        /// <summary>
        /// Multiplies each component of the vector by the same components of the provided vector.
        /// </summary>
        public static TSVector4 Scale(TSVector4 vecA, TSVector4 vecB)
        {
            TSVector4 result;
            result.x = vecA.x * vecB.x;
            result.y = vecA.y * vecB.y;
            result.z = vecA.z * vecB.z;
            result.w = vecA.w * vecB.w;

            return result;
        }

        /// <summary>
        /// Tests if two JVector are equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns true if both values are equal, otherwise false.</returns>
        #region public static bool operator ==(JVector value1, JVector value2)
        public static bool operator ==(TSVector4 value1, TSVector4 value2)
        {
            return (((value1.x == value2.x) && (value1.y == value2.y)) && (value1.z == value2.z) && (value1.w == value2.w));
        }
        #endregion

        /// <summary>
        /// Tests if two JVector are not equal.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>Returns false if both values are equal, otherwise true.</returns>
        #region public static bool operator !=(JVector value1, JVector value2)
        public static bool operator !=(TSVector4 value1, TSVector4 value2)
        {
            if ((value1.x == value2.x) && (value1.y == value2.y) && (value1.z == value2.z))
            {
                return (value1.w != value2.w);
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Gets a vector with the minimum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A vector with the minimum x,y and z values of both vectors.</returns>
        #region public static JVector Min(JVector value1, JVector value2)

        public static TSVector4 Min(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result;
            TSVector4.Min(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Gets a vector with the minimum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the minimum x,y and z values of both vectors.</param>
        public static void Min(ref TSVector4 value1, ref TSVector4 value2, out TSVector4 result)
        {
            result.x = (value1.x < value2.x) ? value1.x : value2.x;
            result.y = (value1.y < value2.y) ? value1.y : value2.y;
            result.z = (value1.z < value2.z) ? value1.z : value2.z;
            result.w = (value1.w < value2.w) ? value1.w : value2.w;
        }
        #endregion

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A vector with the maximum x,y and z values of both vectors.</returns>
        #region public static JVector Max(JVector value1, JVector value2)
        public static TSVector4 Max(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result;
            TSVector4.Max(ref value1, ref value2, out result);
            return result;
        }

        public static FP Distance(TSVector4 v1, TSVector4 v2)
        {
            return FP.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z) + (v1.w - v2.w) * (v1.w - v2.w));
        }

        /// <summary>
        /// Gets a vector with the maximum x,y and z values of both vectors.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="result">A vector with the maximum x,y and z values of both vectors.</param>
        public static void Max(ref TSVector4 value1, ref TSVector4 value2, out TSVector4 result)
        {
            result.x = (value1.x > value2.x) ? value1.x : value2.x;
            result.y = (value1.y > value2.y) ? value1.y : value2.y;
            result.z = (value1.z > value2.z) ? value1.z : value2.z;
            result.w = (value1.w > value2.w) ? value1.w : value2.w;
        }
        #endregion

        /// <summary>
        /// Sets the length of the vector to zero.
        /// </summary>
        #region public void MakeZero()
        public void MakeZero()
        {
            x = FP.Zero;
            y = FP.Zero;
            z = FP.Zero;
            w = FP.Zero;
        }
        #endregion

        /// <summary>
        /// Checks if the length of the vector is zero.
        /// </summary>
        /// <returns>Returns true if the vector is zero, otherwise false.</returns>
        #region public bool IsZero()
        public bool IsZero()
        {
            return (this.sqrMagnitude == FP.Zero);
        }

        /// <summary>
        /// Checks if the length of the vector is nearly zero.
        /// </summary>
        /// <returns>Returns true if the vector is nearly zero, otherwise false.</returns>
        public bool IsNearlyZero()
        {
            return (this.sqrMagnitude < ZeroEpsilonSq);
        }
        #endregion

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <returns>The transformed vector.</returns>
        #region public static JVector Transform(JVector position, JMatrix matrix)
#if !DISABLE_FP_SIN_COS_TAN
        public static TSVector4 Transform(TSVector4 position, TSMatrix4x4 matrix)
        {
            TSVector4 result;
            TSVector4.Transform(ref position, ref matrix, out result);
            return result;
        }

        public static TSVector4 Transform(TSVector position, TSMatrix4x4 matrix)
        {
            TSVector4 result;
            TSVector4.Transform(ref position, ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <param name="matrix">The transform matrix.</param>
        /// <param name="result">The transformed vector.</param>
        public static void Transform(ref TSVector vector, ref TSMatrix4x4 matrix, out TSVector4 result)
        {
            result.x = vector.x * matrix.M11 + vector.y * matrix.M12 + vector.z * matrix.M13 + matrix.M14;
            result.y = vector.x * matrix.M21 + vector.y * matrix.M22 + vector.z * matrix.M23 + matrix.M24;
            result.z = vector.x * matrix.M31 + vector.y * matrix.M32 + vector.z * matrix.M33 + matrix.M34;
            result.w = vector.x * matrix.M41 + vector.y * matrix.M42 + vector.z * matrix.M43 + matrix.M44;
        }

        public static void Transform(ref TSVector4 vector, ref TSMatrix4x4 matrix, out TSVector4 result)
        {
            result.x = vector.x * matrix.M11 + vector.y * matrix.M12 + vector.z * matrix.M13 + vector.w * matrix.M14;
            result.y = vector.x * matrix.M21 + vector.y * matrix.M22 + vector.z * matrix.M23 + vector.w * matrix.M24;
            result.z = vector.x * matrix.M31 + vector.y * matrix.M32 + vector.z * matrix.M33 + vector.w * matrix.M34;
            result.w = vector.x * matrix.M41 + vector.y * matrix.M42 + vector.z * matrix.M43 + vector.w * matrix.M44;
        }
#endif
        #endregion

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>
        #region public static FP Dot(JVector vector1, JVector vector2)
        public static FP Dot(TSVector4 vector1, TSVector4 vector2)
        {
            return TSVector4.Dot(ref vector1, ref vector2);
        }


        /// <summary>
        /// Calculates the dot product of both vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>Returns the dot product of both vectors.</returns>
        public static FP Dot(ref TSVector4 vector1, ref TSVector4 vector2)
        {
            return ((vector1.x * vector2.x) + (vector1.y * vector2.y)) + (vector1.z * vector2.z) + (vector1.w * vector2.w);
        }
        #endregion

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The sum of both vectors.</returns>
        #region public static void Add(JVector value1, JVector value2)
        public static TSVector4 Add(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result;
            TSVector4.Add(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Adds to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The sum of both vectors.</param>
        public static void Add(ref TSVector4 value1, ref TSVector4 value2, out TSVector4 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
            result.z = value1.z + value2.z;
            result.w = value1.w + value2.w;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static TSVector4 Divide(TSVector4 value1, FP scaleFactor)
        {
            TSVector4 result;
            TSVector4.Divide(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the scaled vector.</param>
        public static void Divide(ref TSVector4 value1, FP scaleFactor, out TSVector4 result)
        {
            result.x = value1.x / scaleFactor;
            result.y = value1.y / scaleFactor;
            result.z = value1.z / scaleFactor;
            result.w = value1.w / scaleFactor;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The difference of both vectors.</returns>
        #region public static JVector Subtract(JVector value1, JVector value2)
        public static TSVector4 Subtract(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result;
            TSVector4.Subtract(ref value1, ref value2, out result);
            return result;
        }

        /// <summary>
        /// Subtracts to vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="result">The difference of both vectors.</param>
        public static void Subtract(ref TSVector4 value1, ref TSVector4 value2, out TSVector4 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
            result.z = value1.z - value2.z;
            result.w = value1.w - value2.w;
        }
        #endregion

        /// <summary>
        /// Gets the hashcode of the vector.
        /// </summary>
        /// <returns>Returns the hashcode of the vector.</returns>
        #region public override int GetHashCode()
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }
        #endregion

        /// <summary>
        /// Inverses the direction of the vector.
        /// </summary>
        #region public static JVector Negate(JVector value)
        public void Negate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
            this.w = -this.w;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <returns>The negated vector.</returns>
        public static TSVector4 Negate(TSVector4 value)
        {
            TSVector4 result;
            TSVector4.Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Inverses the direction of a vector.
        /// </summary>
        /// <param name="value">The vector to inverse.</param>
        /// <param name="result">The negated vector.</param>
        public static void Negate(ref TSVector4 value, out TSVector4 result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
            result.w = -value.w;
        }
        #endregion

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <returns>A normalized vector.</returns>
        #region public static JVector Normalize(JVector value)
        public static TSVector4 Normalize(TSVector4 value)
        {
            TSVector4 result;
            TSVector4.Normalize(ref value, out result);
            return result;
        }

        /// <summary>
        /// Normalizes this vector.
        /// </summary>
        public void Normalize()
        {
            FP num2 = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z) + (this.w * this.w);
            FP num = FP.One / FP.Sqrt(num2);
            this.x *= num;
            this.y *= num;
            this.z *= num;
            this.w *= num;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="value">The vector which should be normalized.</param>
        /// <param name="result">A normalized vector.</param>
        public static void Normalize(ref TSVector4 value, out TSVector4 result)
        {
            FP num2 = ((value.x * value.x) + (value.y * value.y)) + (value.z * value.z) + (value.w * value.w);
            FP num = FP.One / FP.Sqrt(num2);
            result.x = value.x * num;
            result.y = value.y * num;
            result.z = value.z * num;
            result.w = value.w * num;
        }
        #endregion

        #region public static void Swap(ref JVector vector1, ref JVector vector2)

        /// <summary>
        /// Swaps the components of both vectors.
        /// </summary>
        /// <param name="vector1">The first vector to swap with the second.</param>
        /// <param name="vector2">The second vector to swap with the first.</param>
        public static void Swap(ref TSVector4 vector1, ref TSVector4 vector2)
        {
            FP temp;

            temp = vector1.x;
            vector1.x = vector2.x;
            vector2.x = temp;

            temp = vector1.y;
            vector1.y = vector2.y;
            vector2.y = temp;

            temp = vector1.z;
            vector1.z = vector2.z;
            vector2.z = temp;

            temp = vector1.w;
            vector1.w = vector2.w;
            vector2.w = temp;
        }
        #endregion

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the multiplied vector.</returns>
        #region public static JVector Multiply(JVector value1, FP scaleFactor)
        public static TSVector4 Multiply(TSVector4 value1, FP scaleFactor)
        {
            TSVector4 result;
            TSVector4.Multiply(ref value1, scaleFactor, out result);
            return result;
        }

        /// <summary>
        /// Multiply a vector with a factor.
        /// </summary>
        /// <param name="value1">The vector to multiply.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="result">Returns the multiplied vector.</param>
        public static void Multiply(ref TSVector4 value1, FP scaleFactor, out TSVector4 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
            result.w = value1.w * scaleFactor;
        }
        #endregion

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>Returns the dot product of both.</returns>
        #region public static FP operator *(JVector value1, JVector value2)
        public static FP operator *(TSVector4 value1, TSVector4 value2)
        {
            return TSVector4.Dot(ref value1, ref value2);
        }
        #endregion

        /// <summary>
        /// Multiplies a vector by a scale factor.
        /// </summary>
        /// <param name="value1">The vector to scale.</param>
        /// <param name="value2">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        #region public static JVector operator *(JVector value1, FP value2)
        public static TSVector4 operator *(TSVector4 value1, FP value2)
        {
            TSVector4 result;
            TSVector4.Multiply(ref value1, value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Multiplies a vector by a scale factor.
        /// </summary>
        /// <param name="value2">The vector to scale.</param>
        /// <param name="value1">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        #region public static JVector operator *(FP value1, JVector value2)
        public static TSVector4 operator *(FP value1, TSVector4 value2)
        {
            TSVector4 result;
            TSVector4.Multiply(ref value2, value1, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The difference of both vectors.</returns>
        #region public static JVector operator -(JVector value1, JVector value2)
        public static TSVector4 operator -(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result; TSVector4.Subtract(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The sum of both vectors.</returns>
        #region public static JVector operator +(JVector value1, JVector value2)
        public static TSVector4 operator +(TSVector4 value1, TSVector4 value2)
        {
            TSVector4 result; TSVector4.Add(ref value1, ref value2, out result);
            return result;
        }
        #endregion

        /// <summary>
        /// Divides a vector by a factor.
        /// </summary>
        /// <param name="value1">The vector to divide.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <returns>Returns the scaled vector.</returns>
        public static TSVector4 operator /(TSVector4 value1, FP value2)
        {
            TSVector4 result;
            TSVector4.Divide(ref value1, value2, out result);
            return result;
        }

        public TSVector2 ToTSVector2()
        {
            return new TSVector2(this.x, this.y);
        }

        public TSVector ToTSVector()
        {
            return new TSVector(this.x, this.y, this.z);
        }
    }

}