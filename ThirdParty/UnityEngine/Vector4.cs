using System;
using System.Globalization;

namespace UnityEngine
{
    [Serializable]
    public struct Vector4: IEquatable<Vector4>
    {
        public static readonly Vector4 zero = new Vector4();
        public static readonly Vector4 one = new Vector4(1f, 1f, 1f, 1f);
        public float x;
        public float y;
        public float z;
        public float w;
        
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector2 value, float z, float w)
        {
            this.x = value.x;
            this.y = value.y;
            this.z = z;
            this.w = w;
        }

        public Vector4(Vector3 value, float w)
        {
            this.x = value.x;
            this.y = value.y;
            this.z = value.z;
            this.w = w;
        }

        public Vector4(float value)
        {
            this.x = this.y = this.z = this.w = value;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format(currentCulture, "{0}, {1}, {2}, {3}", (object) this.x.ToString(currentCulture),
                                 (object) this.y.ToString(currentCulture),
                                 (object) this.z.ToString(currentCulture),
                                 (object) this.w.ToString(currentCulture));
        }

        public bool Equals(Vector4 other)
        {
            if (this.x == (double) other.x && this.y == (double) other.y && this.z == (double) other.z)
                return this.w == (double) other.w;
            return false;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Vector4)
                flag = this.Equals((Vector4) obj);
            return flag;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() + this.y.GetHashCode() + this.z.GetHashCode() + this.w.GetHashCode();
        }

        public float Length()
        {
            return (float) Math.Sqrt(this.x * (double) this.x + this.y * (double) this.y + this.z * (double) this.z +
                                     this.w * (double) this.w);
        }

        public float LengthSquared()
        {
            return (float) (this.x * (double) this.x + this.y * (double) this.y + this.z * (double) this.z +
                this.w * (double) this.w);
        }

        public static float Distance(Vector4 value1, Vector4 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = value1.w - value2.w;
            return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3 +
                                     num4 * (double) num4);
        }

        public static void Distance(ref Vector4 value1, ref Vector4 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = value1.w - value2.w;
            float num5 = (float) (num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3 +
                num4 * (double) num4);
            result = (float) Math.Sqrt(num5);
        }

        public static float DistanceSquared(Vector4 value1, Vector4 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = value1.w - value2.w;
            return (float) (num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3 +
                num4 * (double) num4);
        }

        public static void DistanceSquared(ref Vector4 value1, ref Vector4 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = value1.w - value2.w;
            result = (float) (num1 * (double) num1 + num2 * (double) num2 + num3 * (double) num3 +
                num4 * (double) num4);
        }

        public static float Dot(Vector4 vector1, Vector4 vector2)
        {
            return (float) (vector1.x * (double) vector2.x + vector1.y * (double) vector2.y +
                vector1.z * (double) vector2.z + vector1.w * (double) vector2.w);
        }

        public static void Dot(ref Vector4 vector1, ref Vector4 vector2, out float result)
        {
            result = (float) (vector1.x * (double) vector2.x + vector1.y * (double) vector2.y +
                vector1.z * (double) vector2.z + vector1.w * (double) vector2.w);
        }

        public void Normalize()
        {
            float num1 = (float) (this.x * (double) this.x + this.y * (double) this.y + this.z * (double) this.z +
                this.w * (double) this.w);
            if (num1 < (double) Mathf.Epsilon)
                return;
            float num2 = 1f / (float) Math.Sqrt(num1);
            this.x *= num2;
            this.y *= num2;
            this.z *= num2;
            this.w *= num2;
        }

        public static Vector4 Normalize(Vector4 vector)
        {
            float num1 = (float) (vector.x * (double) vector.x + vector.y * (double) vector.y +
                vector.z * (double) vector.z + vector.w * (double) vector.w);
            if (num1 < (double) Mathf.Epsilon)
                return vector;
            float num2 = 1f / (float) Math.Sqrt(num1);
            Vector4 vector4;
            vector4.x = vector.x * num2;
            vector4.y = vector.y * num2;
            vector4.z = vector.z * num2;
            vector4.w = vector.w * num2;
            return vector4;
        }

        public static void Normalize(ref Vector4 vector, out Vector4 result)
        {
            float num1 = (float) (vector.x * (double) vector.x + vector.y * (double) vector.y +
                vector.z * (double) vector.z + vector.w * (double) vector.w);
            if (num1 < (double) Mathf.Epsilon)
            {
                result = vector;
            }
            else
            {
                float num2 = 1f / (float) Math.Sqrt(num1);
                result.x = vector.x * num2;
                result.y = vector.y * num2;
                result.z = vector.z * num2;
                result.w = vector.w * num2;
            }
        }

        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            vector4.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
            vector4.z = (double) value1.z < (double) value2.z? value1.z : value2.z;
            vector4.w = (double) value1.w < (double) value2.w? value1.w : value2.w;
            return vector4;
        }

        public static void Min(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
            result.z = (double) value1.z < (double) value2.z? value1.z : value2.z;
            result.w = (double) value1.w < (double) value2.w? value1.w : value2.w;
        }

        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            vector4.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
            vector4.z = (double) value1.z > (double) value2.z? value1.z : value2.z;
            vector4.w = (double) value1.w > (double) value2.w? value1.w : value2.w;
            return vector4;
        }

        public static void Max(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
            result.z = (double) value1.z > (double) value2.z? value1.z : value2.z;
            result.w = (double) value1.w > (double) value2.w? value1.w : value2.w;
        }

        public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max)
        {
            float x = value1.x;
            float num1 = (double) x > (double) max.x? max.x : x;
            float num2 = (double) num1 < (double) min.x? min.x : num1;
            float y = value1.y;
            float num3 = (double) y > (double) max.y? max.y : y;
            float num4 = (double) num3 < (double) min.y? min.y : num3;
            float z = value1.z;
            float num5 = (double) z > (double) max.z? max.z : z;
            float num6 = (double) num5 < (double) min.z? min.z : num5;
            float w = value1.w;
            float num7 = (double) w > (double) max.w? max.w : w;
            float num8 = (double) num7 < (double) min.w? min.w : num7;
            Vector4 vector4;
            vector4.x = num2;
            vector4.y = num4;
            vector4.z = num6;
            vector4.w = num8;
            return vector4;
        }

        public static void Clamp(ref Vector4 value1, ref Vector4 min, ref Vector4 max, out Vector4 result)
        {
            float x = value1.x;
            float num1 = (double) x > (double) max.x? max.x : x;
            float num2 = (double) num1 < (double) min.x? min.x : num1;
            float y = value1.y;
            float num3 = (double) y > (double) max.y? max.y : y;
            float num4 = (double) num3 < (double) min.y? min.y : num3;
            float z = value1.z;
            float num5 = (double) z > (double) max.z? max.z : z;
            float num6 = (double) num5 < (double) min.z? min.z : num5;
            float w = value1.w;
            float num7 = (double) w > (double) max.w? max.w : w;
            float num8 = (double) num7 < (double) min.w? min.w : num7;
            result.x = num2;
            result.y = num4;
            result.z = num6;
            result.w = num8;
        }

        public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount)
        {
            Vector4 vector4;
            vector4.x = value1.x + (value2.x - value1.x) * amount;
            vector4.y = value1.y + (value2.y - value1.y) * amount;
            vector4.z = value1.z + (value2.z - value1.z) * amount;
            vector4.w = value1.w + (value2.w - value1.w) * amount;
            return vector4;
        }

        public static void Lerp(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result)
        {
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
            result.w = value1.w + (value2.w - value1.w) * amount;
        }

        public static Vector4 SmoothStep(Vector4 value1, Vector4 value2, float amount)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) (amount * (double) amount * (3.0 - 2.0 * amount));
            Vector4 vector4;
            vector4.x = value1.x + (value2.x - value1.x) * amount;
            vector4.y = value1.y + (value2.y - value1.y) * amount;
            vector4.z = value1.z + (value2.z - value1.z) * amount;
            vector4.w = value1.w + (value2.w - value1.w) * amount;
            return vector4;
        }

        public static void SmoothStep(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) (amount * (double) amount * (3.0 - 2.0 * amount));
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
            result.w = value1.w + (value2.w - value1.w) * amount;
        }

        public static Vector4 Hermite(Vector4 value1, Vector4 tangent1, Vector4 value2, Vector4 tangent2, float amount)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float) (2.0 * num2 - 3.0 * num1 + 1.0);
            float num4 = (float) (-2.0 * num2 + 3.0 * num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            Vector4 vector4;
            vector4.x = (float) (value1.x * (double) num3 + value2.x * (double) num4 + tangent1.x * (double) num5 +
                tangent2.x * (double) num6);
            vector4.y = (float) (value1.y * (double) num3 + value2.y * (double) num4 + tangent1.y * (double) num5 +
                tangent2.y * (double) num6);
            vector4.z = (float) (value1.z * (double) num3 + value2.z * (double) num4 + tangent1.z * (double) num5 +
                tangent2.z * (double) num6);
            vector4.w = (float) (value1.w * (double) num3 + value2.w * (double) num4 + tangent1.w * (double) num5 +
                tangent2.w * (double) num6);
            return vector4;
        }

        public static void Hermite(
            ref Vector4 value1, ref Vector4 tangent1, ref Vector4 value2, ref Vector4 tangent2, float amount, out Vector4 result)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float) (2.0 * num2 - 3.0 * num1 + 1.0);
            float num4 = (float) (-2.0 * num2 + 3.0 * num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            result.x = (float) (value1.x * (double) num3 + value2.x * (double) num4 + tangent1.x * (double) num5 +
                tangent2.x * (double) num6);
            result.y = (float) (value1.y * (double) num3 + value2.y * (double) num4 + tangent1.y * (double) num5 +
                tangent2.y * (double) num6);
            result.z = (float) (value1.z * (double) num3 + value2.z * (double) num4 + tangent1.z * (double) num5 +
                tangent2.z * (double) num6);
            result.w = (float) (value1.w * (double) num3 + value2.w * (double) num4 + tangent1.w * (double) num5 +
                tangent2.w * (double) num6);
        }

        public static Vector4 Project(Vector4 vector, Vector4 onNormal)
        {
            return onNormal * Vector4.Dot(vector, onNormal) / Vector4.Dot(onNormal, onNormal);
        }

        public static void Project(ref Vector4 vector, ref Vector4 onNormal, out Vector4 result)
        {
            result = onNormal * Vector4.Dot(vector, onNormal) / Vector4.Dot(onNormal, onNormal);
        }

        public static Vector4 Negate(Vector4 value)
        {
            Vector4 vector4;
            vector4.x = -value.x;
            vector4.y = -value.y;
            vector4.z = -value.z;
            vector4.w = -value.w;
            return vector4;
        }

        public static void Negate(ref Vector4 value, out Vector4 result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
            result.w = -value.w;
        }

        public static Vector4 Add(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x + value2.x;
            vector4.y = value1.y + value2.y;
            vector4.z = value1.z + value2.z;
            vector4.w = value1.w + value2.w;
            return vector4;
        }

        public static void Add(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
            result.z = value1.z + value2.z;
            result.w = value1.w + value2.w;
        }

        public static Vector4 Sub(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x - value2.x;
            vector4.y = value1.y - value2.y;
            vector4.z = value1.z - value2.z;
            vector4.w = value1.w - value2.w;
            return vector4;
        }

        public static void Sub(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
            result.z = value1.z - value2.z;
            result.w = value1.w - value2.w;
        }

        public static Vector4 Multiply(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x * value2.x;
            vector4.y = value1.y * value2.y;
            vector4.z = value1.z * value2.z;
            vector4.w = value1.w * value2.w;
            return vector4;
        }

        public static void Multiply(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
            result.z = value1.z * value2.z;
            result.w = value1.w * value2.w;
        }

        public static Vector4 Multiply(Vector4 value1, float scaleFactor)
        {
            Vector4 vector4;
            vector4.x = value1.x * scaleFactor;
            vector4.y = value1.y * scaleFactor;
            vector4.z = value1.z * scaleFactor;
            vector4.w = value1.w * scaleFactor;
            return vector4;
        }

        public static void Multiply(ref Vector4 value1, float scaleFactor, out Vector4 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
            result.w = value1.w * scaleFactor;
        }

        public static Vector4 Divide(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x / value2.x;
            vector4.y = value1.y / value2.y;
            vector4.z = value1.z / value2.z;
            vector4.w = value1.w / value2.w;
            return vector4;
        }

        public static void Divide(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
            result.z = value1.z / value2.z;
            result.w = value1.w / value2.w;
        }

        public static Vector4 Divide(Vector4 value1, float divider)
        {
            float num = 1f / divider;
            Vector4 vector4;
            vector4.x = value1.x * num;
            vector4.y = value1.y * num;
            vector4.z = value1.z * num;
            vector4.w = value1.w * num;
            return vector4;
        }

        public static void Divide(ref Vector4 value1, float divider, out Vector4 result)
        {
            float num = 1f / divider;
            result.x = value1.x * num;
            result.y = value1.y * num;
            result.z = value1.z * num;
            result.w = value1.w * num;
        }

        public static Vector4 operator -(Vector4 value)
        {
            Vector4 vector4;
            vector4.x = -value.x;
            vector4.y = -value.y;
            vector4.z = -value.z;
            vector4.w = -value.w;
            return vector4;
        }

        public static bool operator ==(Vector4 value1, Vector4 value2)
        {
            if (value1.x == (double) value2.x && value1.y == (double) value2.y && value1.z == (double) value2.z)
                return value1.w == (double) value2.w;
            return false;
        }

        public static bool operator !=(Vector4 value1, Vector4 value2)
        {
            if (value1.x == (double) value2.x && value1.y == (double) value2.y && value1.z == (double) value2.z)
                return value1.w != (double) value2.w;
            return true;
        }

        public static Vector4 operator +(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x + value2.x;
            vector4.y = value1.y + value2.y;
            vector4.z = value1.z + value2.z;
            vector4.w = value1.w + value2.w;
            return vector4;
        }

        public static Vector4 operator -(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x - value2.x;
            vector4.y = value1.y - value2.y;
            vector4.z = value1.z - value2.z;
            vector4.w = value1.w - value2.w;
            return vector4;
        }

        public static Vector4 operator *(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x * value2.x;
            vector4.y = value1.y * value2.y;
            vector4.z = value1.z * value2.z;
            vector4.w = value1.w * value2.w;
            return vector4;
        }

        public static Vector4 operator *(Vector4 value1, float scaleFactor)
        {
            Vector4 vector4;
            vector4.x = value1.x * scaleFactor;
            vector4.y = value1.y * scaleFactor;
            vector4.z = value1.z * scaleFactor;
            vector4.w = value1.w * scaleFactor;
            return vector4;
        }

        public static Vector4 operator *(float scaleFactor, Vector4 value1)
        {
            Vector4 vector4;
            vector4.x = value1.x * scaleFactor;
            vector4.y = value1.y * scaleFactor;
            vector4.z = value1.z * scaleFactor;
            vector4.w = value1.w * scaleFactor;
            return vector4;
        }

        public static Vector4 operator /(Vector4 value1, Vector4 value2)
        {
            Vector4 vector4;
            vector4.x = value1.x / value2.x;
            vector4.y = value1.y / value2.y;
            vector4.z = value1.z / value2.z;
            vector4.w = value1.w / value2.w;
            return vector4;
        }

        public static Vector4 operator /(Vector4 value1, float divider)
        {
            float num = 1f / divider;
            Vector4 vector4;
            vector4.x = value1.x * num;
            vector4.y = value1.y * num;
            vector4.z = value1.z * num;
            vector4.w = value1.w * num;
            return vector4;
        }
    }
}