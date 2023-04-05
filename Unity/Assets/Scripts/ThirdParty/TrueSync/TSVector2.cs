#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors
 * Alan McGovern

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License

using System;

namespace TrueSync {

    [Serializable]
    public struct TSVector2 : IEquatable<TSVector2>
    {
#region Private Fields

        private static TSVector2 zeroVector = new TSVector2(0, 0);
        private static TSVector2 oneVector = new TSVector2(1, 1);

        private static TSVector2 rightVector = new TSVector2(1, 0);
        private static TSVector2 leftVector = new TSVector2(-1, 0);

        private static TSVector2 upVector = new TSVector2(0, 1);
        private static TSVector2 downVector = new TSVector2(0, -1);

        #endregion Private Fields

        #region Public Fields

        public FP x;
        public FP y;

        #endregion Public Fields

#region Properties

        public static TSVector2 zero
        {
            get { return zeroVector; }
        }

        public static TSVector2 one
        {
            get { return oneVector; }
        }

        public static TSVector2 right
        {
            get { return rightVector; }
        }

        public static TSVector2 left {
            get { return leftVector; }
        }

        public static TSVector2 up
        {
            get { return upVector; }
        }

        public static TSVector2 down {
            get { return downVector; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor foe standard 2D vector.
        /// </summary>
        /// <param name="x">
        /// A <see cref="System.Single"/>
        /// </param>
        /// <param name="y">
        /// A <see cref="System.Single"/>
        /// </param>
        public TSVector2(FP x, FP y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Constructor for "square" vector.
        /// </summary>
        /// <param name="value">
        /// A <see cref="System.Single"/>
        /// </param>
        public TSVector2(FP value)
        {
            x = value;
            y = value;
        }

        public void Set(FP x, FP y) {
            this.x = x;
            this.y = y;
        }

        #endregion Constructors

        #region Public Methods

        public static void Reflect(ref TSVector2 vector, ref TSVector2 normal, out TSVector2 result)
        {
            FP dot = Dot(vector, normal);
            result.x = vector.x - ((2f*dot)*normal.x);
            result.y = vector.y - ((2f*dot)*normal.y);
        }

        public static TSVector2 Reflect(TSVector2 vector, TSVector2 normal)
        {
            TSVector2 result;
            Reflect(ref vector, ref normal, out result);
            return result;
        }

        public static TSVector2 Add(TSVector2 value1, TSVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        public static void Add(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
        }

        public static TSVector2 Barycentric(TSVector2 value1, TSVector2 value2, TSVector2 value3, FP amount1, FP amount2)
        {
            return new TSVector2(
                TSMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                TSMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        public static void Barycentric(ref TSVector2 value1, ref TSVector2 value2, ref TSVector2 value3, FP amount1,
                                       FP amount2, out TSVector2 result)
        {
            result = new TSVector2(
                TSMath.Barycentric(value1.x, value2.x, value3.x, amount1, amount2),
                TSMath.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        public static TSVector2 CatmullRom(TSVector2 value1, TSVector2 value2, TSVector2 value3, TSVector2 value4, FP amount)
        {
            return new TSVector2(
                TSMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                TSMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static void CatmullRom(ref TSVector2 value1, ref TSVector2 value2, ref TSVector2 value3, ref TSVector2 value4,
                                      FP amount, out TSVector2 result)
        {
            result = new TSVector2(
                TSMath.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                TSMath.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static TSVector2 Clamp(TSVector2 value1, TSVector2 min, TSVector2 max)
        {
            return new TSVector2(
                TSMath.Clamp(value1.x, min.x, max.x),
                TSMath.Clamp(value1.y, min.y, max.y));
        }

        public static void Clamp(ref TSVector2 value1, ref TSVector2 min, ref TSVector2 max, out TSVector2 result)
        {
            result = new TSVector2(
                TSMath.Clamp(value1.x, min.x, max.x),
                TSMath.Clamp(value1.y, min.y, max.y));
        }

        /// <summary>
        /// Returns FP precison distanve between two vectors
        /// </summary>
        /// <param name="value1">
        /// A <see cref="TSVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="TSVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Single"/>
        /// </returns>
        public static FP Distance(TSVector2 value1, TSVector2 value2)
        {
            FP result;
            DistanceSquared(ref value1, ref value2, out result);
            return (FP) FP.Sqrt(result);
        }


        public static void Distance(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = (FP) FP.Sqrt(result);
        }

        public static FP DistanceSquared(TSVector2 value1, TSVector2 value2)
        {
            FP result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        public static void DistanceSquared(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            result = (value1.x - value2.x)*(value1.x - value2.x) + (value1.y - value2.y)*(value1.y - value2.y);
        }

        /// <summary>
        /// Devide first vector with the secund vector
        /// </summary>
        /// <param name="value1">
        /// A <see cref="TSVector2"/>
        /// </param>
        /// <param name="value2">
        /// A <see cref="TSVector2"/>
        /// </param>
        /// <returns>
        /// A <see cref="TSVector2"/>
        /// </returns>
        public static TSVector2 Divide(TSVector2 value1, TSVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        public static void Divide(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x/value2.x;
            result.y = value1.y/value2.y;
        }

        public static TSVector2 Divide(TSVector2 value1, FP divider)
        {
            FP factor = 1/divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        public static void Divide(ref TSVector2 value1, FP divider, out TSVector2 result)
        {
            FP factor = 1/divider;
            result.x = value1.x*factor;
            result.y = value1.y*factor;
        }

        public static FP Dot(TSVector2 value1, TSVector2 value2)
        {
            return value1.x*value2.x + value1.y*value2.y;
        }

        public static void Dot(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            result = value1.x*value2.x + value1.y*value2.y;
        }

        public override bool Equals(object obj)
        {
            return (obj is TSVector2) ? this == ((TSVector2) obj) : false;
        }

        public bool Equals(TSVector2 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int) (x + y);
        }

        public static TSVector2 Hermite(TSVector2 value1, TSVector2 tangent1, TSVector2 value2, TSVector2 tangent2, FP amount)
        {
            TSVector2 result = new TSVector2();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(ref TSVector2 value1, ref TSVector2 tangent1, ref TSVector2 value2, ref TSVector2 tangent2,
                                   FP amount, out TSVector2 result)
        {
            result.x = TSMath.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
            result.y = TSMath.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        }

        public FP magnitude {
            get {
                FP result;
                DistanceSquared(ref this, ref zeroVector, out result);
                return FP.Sqrt(result);
            }
        }

        public static TSVector2 ClampMagnitude(TSVector2 vector, FP maxLength) {
            return Normalize(vector) * maxLength;
        }

        public FP LengthSquared()
        {
            FP result;
            DistanceSquared(ref this, ref zeroVector, out result);
            return result;
        }

        public static TSVector2 Lerp(TSVector2 value1, TSVector2 value2, FP amount) {
            amount = TSMath.Clamp(amount, 0, 1);

            return new TSVector2(
                TSMath.Lerp(value1.x, value2.x, amount),
                TSMath.Lerp(value1.y, value2.y, amount));
        }

        public static TSVector2 LerpUnclamped(TSVector2 value1, TSVector2 value2, FP amount)
        {
            return new TSVector2(
                TSMath.Lerp(value1.x, value2.x, amount),
                TSMath.Lerp(value1.y, value2.y, amount));
        }

        public static void LerpUnclamped(ref TSVector2 value1, ref TSVector2 value2, FP amount, out TSVector2 result)
        {
            result = new TSVector2(
                TSMath.Lerp(value1.x, value2.x, amount),
                TSMath.Lerp(value1.y, value2.y, amount));
        }

        public static TSVector2 Max(TSVector2 value1, TSVector2 value2)
        {
            return new TSVector2(
                TSMath.Max(value1.x, value2.x),
                TSMath.Max(value1.y, value2.y));
        }

        public static void Max(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = TSMath.Max(value1.x, value2.x);
            result.y = TSMath.Max(value1.y, value2.y);
        }

        public static TSVector2 Min(TSVector2 value1, TSVector2 value2)
        {
            return new TSVector2(
                TSMath.Min(value1.x, value2.x),
                TSMath.Min(value1.y, value2.y));
        }

        public static void Min(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = TSMath.Min(value1.x, value2.x);
            result.y = TSMath.Min(value1.y, value2.y);
        }

        public void Scale(TSVector2 other) {
            this.x = x * other.x;
            this.y = y * other.y;
        }

        public static TSVector2 Scale(TSVector2 value1, TSVector2 value2) {
            TSVector2 result;
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;

            return result;
        }

        public static TSVector2 Multiply(TSVector2 value1, TSVector2 value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

        public static TSVector2 Multiply(TSVector2 value1, FP scaleFactor)
        {
            value1.x *= scaleFactor;
            value1.y *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref TSVector2 value1, FP scaleFactor, out TSVector2 result)
        {
            result.x = value1.x*scaleFactor;
            result.y = value1.y*scaleFactor;
        }

        public static void Multiply(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x*value2.x;
            result.y = value1.y*value2.y;
        }

        public static TSVector2 Negate(TSVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        public static void Negate(ref TSVector2 value, out TSVector2 result)
        {
            result.x = -value.x;
            result.y = -value.y;
        }

        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        public static TSVector2 Normalize(TSVector2 value)
        {
            Normalize(ref value, out value);
            return value;
        }

        public TSVector2 normalized {
            get {
                TSVector2 result;
                TSVector2.Normalize(ref this, out result);

                return result;
            }
        }

        public static void Normalize(ref TSVector2 value, out TSVector2 result)
        {
            FP factor;
            DistanceSquared(ref value, ref zeroVector, out factor);
            factor = 1f/(FP) FP.Sqrt(factor);
            result.x = value.x*factor;
            result.y = value.y*factor;
        }

        public static TSVector2 SmoothStep(TSVector2 value1, TSVector2 value2, FP amount)
        {
            return new TSVector2(
                TSMath.SmoothStep(value1.x, value2.x, amount),
                TSMath.SmoothStep(value1.y, value2.y, amount));
        }

        public static void SmoothStep(ref TSVector2 value1, ref TSVector2 value2, FP amount, out TSVector2 result)
        {
            result = new TSVector2(
                TSMath.SmoothStep(value1.x, value2.x, amount),
                TSMath.SmoothStep(value1.y, value2.y, amount));
        }

        public static TSVector2 Subtract(TSVector2 value1, TSVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        public static void Subtract(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
        }

        public static FP Angle(TSVector2 a, TSVector2 b) {
            return FP.Acos(a.normalized * b.normalized) * FP.Rad2Deg;
        }

        public TSVector ToTSVector() {
            return new TSVector(this.x, this.y, 0);
        }

        public override string ToString() {
            return string.Format("({0:f1}, {1:f1})", x.AsFloat(), y.AsFloat());
        }

        #endregion Public Methods

#region Operators

        public static TSVector2 operator -(TSVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }


        public static bool operator ==(TSVector2 value1, TSVector2 value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }


        public static bool operator !=(TSVector2 value1, TSVector2 value2)
        {
            return value1.x != value2.x || value1.y != value2.y;
        }


        public static TSVector2 operator +(TSVector2 value1, TSVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }


        public static TSVector2 operator -(TSVector2 value1, TSVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }


        public static FP operator *(TSVector2 value1, TSVector2 value2)
        {
            return TSVector2.Dot(value1, value2);
        }


        public static TSVector2 operator *(TSVector2 value, FP scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }


        public static TSVector2 operator *(FP scaleFactor, TSVector2 value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }


        public static TSVector2 operator /(TSVector2 value1, TSVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }


        public static TSVector2 operator /(TSVector2 value1, FP divider)
        {
            FP factor = 1/divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        #endregion Operators
    }
}