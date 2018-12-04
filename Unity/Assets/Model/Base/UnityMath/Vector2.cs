using System;
using System.Globalization;

namespace PF
{
    [Serializable]
    public struct Vector2: IEquatable<Vector2>
    {
        private static readonly Vector2 _zero = new Vector2();
        private static readonly Vector2 _one = new Vector2(1f, 1f);
        private const float epsilon = 1E-05f;
        public float x;
        public float y;
#if !SERVER
        public static implicit operator UnityEngine.Vector2(Vector2 v2)
        {
            return new UnityEngine.Vector3(v2.x, v2.y);
        }
        
        public static implicit operator Vector2(UnityEngine.Vector2 v2)
        {
            return new Vector2(v2.x, v2.y);
        }
#endif
        
        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0.0f);
        }

        public static Vector2 zero
        {
            get
            {
                return Vector2._zero;
            }
        }

        public static Vector2 one
        {
            get
            {
                return Vector2._one;
            }
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(float value)
        {
            this.x = this.y = value;
        }

        public float this[int index]
        {
            get
            {
                if (index == 0)
                    return this.x;
                if (index == 1)
                    return this.y;
                throw new IndexOutOfRangeException("Invalid Vector2 index!");
            }
            set
            {
                if (index != 0)
                {
                    if (index != 1)
                        throw new IndexOutOfRangeException("Invalid Vector2 index!");
                    this.y = value;
                }
                else
                    this.x = value;
            }
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format((IFormatProvider) currentCulture, "{0}, {1}",
                                 new object[2]
                                 {
                                     (object) this.x.ToString((IFormatProvider) currentCulture),
                                     (object) this.y.ToString((IFormatProvider) currentCulture)
                                 });
        }

        public bool Equals(Vector2 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Vector2)
                flag = this.Equals((Vector2) obj);
            return flag;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() + this.y.GetHashCode();
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y);
        }

        public float LengthSquared()
        {
            return (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y);
        }
        
        public float magnitude
        {
            get
            {
                return this.Length();
            }
        }
        
        public float sqrMagnitude
        {
            get
            {
                return this.LengthSquared();
            }
        }

        public static float Distance(Vector2 value1, Vector2 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
            result = (float) Math.Sqrt((double) num3);
        }

        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            result = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public void Normalize()
        {
            float num1 = (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y);
            if ((double) num1 < 9.99999974737875E-06)
                return;
            float num2 = 1f / (float) Math.Sqrt((double) num1);
            this.x *= num2;
            this.y *= num2;
        }

        public static Vector2 Normalize(Vector2 value)
        {
            float num1 = (float) ((double) value.x * (double) value.x + (double) value.y * (double) value.y);
            if ((double) num1 < 9.99999974737875E-06)
                return value;
            float num2 = 1f / (float) Math.Sqrt((double) num1);
            Vector2 vector2;
            vector2.x = value.x * num2;
            vector2.y = value.y * num2;
            return vector2;
        }

        public static void Normalize(ref Vector2 value, out Vector2 result)
        {
            float num1 = (float) ((double) value.x * (double) value.x + (double) value.y * (double) value.y);
            if ((double) num1 < 9.99999974737875E-06)
            {
                result = value;
            }
            else
            {
                float num2 = 1f / (float) Math.Sqrt((double) num1);
                result.x = value.x * num2;
                result.y = value.y * num2;
            }
        }
        
        public Vector2 normalized
        {
            get
            {
                return Vector2.Normalize(this);
            }
        }

        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
        {
            float num = (float) ((double) vector.x * (double) normal.x + (double) vector.y * (double) normal.y);
            Vector2 vector2;
            vector2.x = vector.x - 2f * num * normal.x;
            vector2.y = vector.y - 2f * num * normal.y;
            return vector2;
        }

        public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
        {
            float num = (float) ((double) vector.x * (double) normal.x + (double) vector.y * (double) normal.y);
            result.x = vector.x - 2f * num * normal.x;
            result.y = vector.y - 2f * num * normal.y;
        }

        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            vector2.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
            return vector2;
        }

        public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
        }

        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            vector2.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
            return vector2;
        }

        public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
        }

        public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
        {
            float x = value1.x;
            float num1 = (double) x > (double) max.x? max.x : x;
            float num2 = (double) num1 < (double) min.x? min.x : num1;
            float y = value1.y;
            float num3 = (double) y > (double) max.y? max.y : y;
            float num4 = (double) num3 < (double) min.y? min.y : num3;
            Vector2 vector2;
            vector2.x = num2;
            vector2.y = num4;
            return vector2;
        }

        public static void Clamp(ref Vector2 value1, ref Vector2 min, ref Vector2 max, out Vector2 result)
        {
            float x = value1.x;
            float num1 = (double) x > (double) max.x? max.x : x;
            float num2 = (double) num1 < (double) min.x? min.x : num1;
            float y = value1.y;
            float num3 = (double) y > (double) max.y? max.y : y;
            float num4 = (double) num3 < (double) min.y? min.y : num3;
            result.x = num2;
            result.y = num4;
        }

        public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount)
        {
            Vector2 vector2;
            vector2.x = value1.x + (value2.x - value1.x) * amount;
            vector2.y = value1.y + (value2.y - value1.y) * amount;
            return vector2;
        }

        public static void Lerp(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result)
        {
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
        }

        public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) ((double) amount * (double) amount * (3.0 - 2.0 * (double) amount));
            Vector2 vector2;
            vector2.x = value1.x + (value2.x - value1.x) * amount;
            vector2.y = value1.y + (value2.y - value1.y) * amount;
            return vector2;
        }

        public static void SmoothStep(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) ((double) amount * (double) amount * (3.0 - 2.0 * (double) amount));
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
        }

        public static Vector2 Negate(Vector2 value)
        {
            Vector2 vector2;
            vector2.x = -value.x;
            vector2.y = -value.y;
            return vector2;
        }

        public static void Negate(ref Vector2 value, out Vector2 result)
        {
            result.x = -value.x;
            result.y = -value.y;
        }

        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return (float) ((double) value1.x * (double) value2.x + (double) value1.y * (double) value2.y);
        }

        public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            result = (float) ((double) value1.x * (double) value2.x + (double) value1.y * (double) value2.y);
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            from.Normalize();
            to.Normalize();
            float result;
            Vector2.Dot(ref from, ref to, out result);
            return MathHelper.ACos(MathHelper.Clamp(result, -1f, 1f)) * 57.29578f;
        }

        public static void Angle(ref Vector2 from, ref Vector2 to, out float result)
        {
            from.Normalize();
            to.Normalize();
            float result1;
            Vector2.Dot(ref from, ref to, out result1);
            result = MathHelper.ACos(MathHelper.Clamp(result1, -1f, 1f)) * 57.29578f;
        }

        public static Vector2 Add(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x + value2.x;
            vector2.y = value1.y + value2.y;
            return vector2;
        }

        public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
        }

        public static Vector2 Sub(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x - value2.x;
            vector2.y = value1.y - value2.y;
            return vector2;
        }

        public static void Sub(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
        }

        public static Vector2 Multiply(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x * value2.x;
            vector2.y = value1.y * value2.y;
            return vector2;
        }

        public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
        }

        public static Vector2 Multiply(Vector2 value1, float scaleFactor)
        {
            Vector2 vector2;
            vector2.x = value1.x * scaleFactor;
            vector2.y = value1.y * scaleFactor;
            return vector2;
        }

        public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
        }

        public static Vector2 Divide(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x / value2.x;
            vector2.y = value1.y / value2.y;
            return vector2;
        }

        public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
        }

        public static Vector2 Divide(Vector2 value1, float divider)
        {
            float num = 1f / divider;
            Vector2 vector2;
            vector2.x = value1.x * num;
            vector2.y = value1.y * num;
            return vector2;
        }

        public static void Divide(ref Vector2 value1, float divider, out Vector2 result)
        {
            float num = 1f / divider;
            result.x = value1.x * num;
            result.y = value1.y * num;
        }

        public static Vector2 operator -(Vector2 value)
        {
            Vector2 vector2;
            vector2.x = -value.x;
            vector2.y = -value.y;
            return vector2;
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return (double) (lhs - rhs).sqrMagnitude < 9.99999943962493E-11;
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return !(lhs == rhs);
        }

        public static Vector2 operator +(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x + value2.x;
            vector2.y = value1.y + value2.y;
            return vector2;
        }

        public static Vector2 operator -(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x - value2.x;
            vector2.y = value1.y - value2.y;
            return vector2;
        }

        public static Vector2 operator *(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x * value2.x;
            vector2.y = value1.y * value2.y;
            return vector2;
        }

        public static Vector2 operator *(Vector2 value, float scaleFactor)
        {
            Vector2 vector2;
            vector2.x = value.x * scaleFactor;
            vector2.y = value.y * scaleFactor;
            return vector2;
        }

        public static Vector2 operator *(float scaleFactor, Vector2 value)
        {
            Vector2 vector2;
            vector2.x = value.x * scaleFactor;
            vector2.y = value.y * scaleFactor;
            return vector2;
        }

        public static Vector2 operator /(Vector2 value1, Vector2 value2)
        {
            Vector2 vector2;
            vector2.x = value1.x / value2.x;
            vector2.y = value1.y / value2.y;
            return vector2;
        }

        public static Vector2 operator /(Vector2 value1, float divider)
        {
            float num = 1f / divider;
            Vector2 vector2;
            vector2.x = value1.x * num;
            vector2.y = value1.y * num;
            return vector2;
        }
    }
}