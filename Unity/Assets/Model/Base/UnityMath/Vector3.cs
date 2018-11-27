using System;
using System.Globalization;

namespace PF
{
    [Serializable]
    public struct Vector3: IEquatable<Vector3>
    {
        private const float k1OverSqrt2 = 0.7071068f;
        private const float epsilon = 1E-05f;
        private static readonly Vector3 _zero = new Vector3();
        private static readonly Vector3 _one = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 _up = new Vector3(0.0f, 1f, 0.0f);
        private static readonly Vector3 _down = new Vector3(0.0f, -1f, 0.0f);
        private static readonly Vector3 _right = new Vector3(1f, 0.0f, 0.0f);
        private static readonly Vector3 _left = new Vector3(-1f, 0.0f, 0.0f);
        private static readonly Vector3 _forward = new Vector3(0.0f, 0.0f, 1f);
        private static readonly Vector3 _backward = new Vector3(0.0f, 0.0f, -1f);
        public float x;
        public float y;
        public float z;
#if !SERVER
        public static implicit operator UnityEngine.Vector3(Vector3 v3)
        {
            return new UnityEngine.Vector3(v3.x, v3.y, v3.z);
        }
        
        public static implicit operator Vector3(UnityEngine.Vector3 v3)
        {
            return new Vector3(v3.x, v3.y, v3.z);
        }
#endif

        public static Vector3 zero
        {
            get
            {
                return Vector3._zero;
            }
        }

        public static Vector3 one
        {
            get
            {
                return Vector3._one;
            }
        }

        public static Vector3 up
        {
            get
            {
                return Vector3._up;
            }
        }

        public static Vector3 down
        {
            get
            {
                return Vector3._down;
            }
        }

        public static Vector3 right
        {
            get
            {
                return Vector3._right;
            }
        }

        public static Vector3 left
        {
            get
            {
                return Vector3._left;
            }
        }

        public static Vector3 forward
        {
            get
            {
                return Vector3._forward;
            }
        }

        public static Vector3 back
        {
            get
            {
                return Vector3._backward;
            }
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(float value)
        {
            this.x = this.y = this.z = value;
        }

        public Vector3(Vector2 value, float z)
        {
            this.x = value.x;
            this.y = value.y;
            this.z = z;
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format((IFormatProvider) currentCulture, "({0}, {1}, {2})", (object) this.x.ToString((IFormatProvider) currentCulture),
                                 (object) this.y.ToString((IFormatProvider) currentCulture),
                                 (object) this.z.ToString((IFormatProvider) currentCulture));
        }

        public bool Equals(Vector3 other)
        {
            if ((double) this.x == (double) other.x && (double) this.y == (double) other.y)
                return (double) this.z == (double) other.z;
            return false;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Vector3)
                flag = this.Equals((Vector3) obj);
            return flag;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() + this.y.GetHashCode() + this.z.GetHashCode();
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z);
        }

        public float LengthSquared()
        {
            return (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z);
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

        public static float Distance(Vector3 value1, Vector3 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            float num4 = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
            result = (float) Math.Sqrt((double) num4);
        }

        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            float num1 = value1.x - value2.x;
            float num2 = value1.y - value2.y;
            float num3 = value1.z - value2.z;
            result = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
        }

        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return (float) ((double) vector1.x * (double) vector2.x + (double) vector1.y * (double) vector2.y +
                (double) vector1.z * (double) vector2.z);
        }

        public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
        {
            result = (float) ((double) vector1.x * (double) vector2.x + (double) vector1.y * (double) vector2.y +
                (double) vector1.z * (double) vector2.z);
        }

        public void Normalize()
        {
            float num1 = (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z);
            if ((double) num1 < (double) Vector3.epsilon)
                return;
            float num2 = 1f / (float) Math.Sqrt((double) num1);
            this.x *= num2;
            this.y *= num2;
            this.z *= num2;
        }
        
        public Vector3 normalized
        {
            get
            {
                return Vector3.Normalize(this);
            }
        }

        public static Vector3 Normalize(Vector3 value)
        {
            float num1 = (float) ((double) value.x * (double) value.x + (double) value.y * (double) value.y + (double) value.z * (double) value.z);
            if ((double) num1 < (double) Vector3.epsilon)
                return value;
            float num2 = 1f / (float) Math.Sqrt((double) num1);
            Vector3 vector3;
            vector3.x = value.x * num2;
            vector3.y = value.y * num2;
            vector3.z = value.z * num2;
            return vector3;
        }

        public static void Normalize(ref Vector3 value, out Vector3 result)
        {
            float num1 = (float) ((double) value.x * (double) value.x + (double) value.y * (double) value.y + (double) value.z * (double) value.z);
            if ((double) num1 < (double) Vector3.epsilon)
            {
                result = value;
            }
            else
            {
                float num2 = 1f / (float) Math.Sqrt((double) num1);
                result.x = value.x * num2;
                result.y = value.y * num2;
                result.z = value.z * num2;
            }
        }

        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            Vector3 vector3;
            vector3.x = (float) ((double) vector1.y * (double) vector2.z - (double) vector1.z * (double) vector2.y);
            vector3.y = (float) ((double) vector1.z * (double) vector2.x - (double) vector1.x * (double) vector2.z);
            vector3.z = (float) ((double) vector1.x * (double) vector2.y - (double) vector1.y * (double) vector2.x);
            return vector3;
        }

        public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
        {
            float num1 = (float) ((double) vector1.y * (double) vector2.z - (double) vector1.z * (double) vector2.y);
            float num2 = (float) ((double) vector1.z * (double) vector2.x - (double) vector1.x * (double) vector2.z);
            float num3 = (float) ((double) vector1.x * (double) vector2.y - (double) vector1.y * (double) vector2.x);
            result.x = num1;
            result.y = num2;
            result.z = num3;
        }

        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            float num =
                    (float) ((double) vector.x * (double) normal.x + (double) vector.y * (double) normal.y + (double) vector.z * (double) normal.z);
            Vector3 vector3;
            vector3.x = vector.x - 2f * num * normal.x;
            vector3.y = vector.y - 2f * num * normal.y;
            vector3.z = vector.z - 2f * num * normal.z;
            return vector3;
        }

        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
        {
            float num =
                    (float) ((double) vector.x * (double) normal.x + (double) vector.y * (double) normal.y + (double) vector.z * (double) normal.z);
            result.x = vector.x - 2f * num * normal.x;
            result.y = vector.y - 2f * num * normal.y;
            result.z = vector.z - 2f * num * normal.z;
        }

        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            vector3.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
            vector3.z = (double) value1.z < (double) value2.z? value1.z : value2.z;
            return vector3;
        }

        public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = (double) value1.x < (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y < (double) value2.y? value1.y : value2.y;
            result.z = (double) value1.z < (double) value2.z? value1.z : value2.z;
        }

        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            vector3.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
            vector3.z = (double) value1.z > (double) value2.z? value1.z : value2.z;
            return vector3;
        }

        public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = (double) value1.x > (double) value2.x? value1.x : value2.x;
            result.y = (double) value1.y > (double) value2.y? value1.y : value2.y;
            result.z = (double) value1.z > (double) value2.z? value1.z : value2.z;
        }

        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
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
            Vector3 vector3;
            vector3.x = num2;
            vector3.y = num4;
            vector3.z = num6;
            return vector3;
        }

        public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
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
            result.x = num2;
            result.y = num4;
            result.z = num6;
        }

        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            Vector3 vector3;
            vector3.x = value1.x + (value2.x - value1.x) * amount;
            vector3.y = value1.y + (value2.y - value1.y) * amount;
            vector3.z = value1.z + (value2.z - value1.z) * amount;
            return vector3;
        }

        public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
        }

        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) ((double) amount * (double) amount * (3.0 - 2.0 * (double) amount));
            Vector3 vector3;
            vector3.x = value1.x + (value2.x - value1.x) * amount;
            vector3.y = value1.y + (value2.y - value1.y) * amount;
            vector3.z = value1.z + (value2.z - value1.z) * amount;
            return vector3;
        }

        public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            amount = (double) amount > 1.0? 1f : ((double) amount < 0.0? 0.0f : amount);
            amount = (float) ((double) amount * (double) amount * (3.0 - 2.0 * (double) amount));
            result.x = value1.x + (value2.x - value1.x) * amount;
            result.y = value1.y + (value2.y - value1.y) * amount;
            result.z = value1.z + (value2.z - value1.z) * amount;
        }

        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float) (2.0 * (double) num2 - 3.0 * (double) num1 + 1.0);
            float num4 = (float) (-2.0 * (double) num2 + 3.0 * (double) num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            Vector3 vector3;
            vector3.x = (float) ((double) value1.x * (double) num3 + (double) value2.x * (double) num4 + (double) tangent1.x * (double) num5 +
                (double) tangent2.x * (double) num6);
            vector3.y = (float) ((double) value1.y * (double) num3 + (double) value2.y * (double) num4 + (double) tangent1.y * (double) num5 +
                (double) tangent2.y * (double) num6);
            vector3.z = (float) ((double) value1.z * (double) num3 + (double) value2.z * (double) num4 + (double) tangent1.z * (double) num5 +
                (double) tangent2.z * (double) num6);
            return vector3;
        }

        public static void Hermite(
            ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
        {
            float num1 = amount * amount;
            float num2 = amount * num1;
            float num3 = (float) (2.0 * (double) num2 - 3.0 * (double) num1 + 1.0);
            float num4 = (float) (-2.0 * (double) num2 + 3.0 * (double) num1);
            float num5 = num2 - 2f * num1 + amount;
            float num6 = num2 - num1;
            result.x = (float) ((double) value1.x * (double) num3 + (double) value2.x * (double) num4 + (double) tangent1.x * (double) num5 +
                (double) tangent2.x * (double) num6);
            result.y = (float) ((double) value1.y * (double) num3 + (double) value2.y * (double) num4 + (double) tangent1.y * (double) num5 +
                (double) tangent2.y * (double) num6);
            result.z = (float) ((double) value1.z * (double) num3 + (double) value2.z * (double) num4 + (double) tangent1.z * (double) num5 +
                (double) tangent2.z * (double) num6);
        }

        public static Vector3 Negate(Vector3 value)
        {
            Vector3 vector3;
            vector3.x = -value.x;
            vector3.y = -value.y;
            vector3.z = -value.z;
            return vector3;
        }

        public static void Negate(ref Vector3 value, out Vector3 result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
        }

        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x + value2.x;
            vector3.y = value1.y + value2.y;
            vector3.z = value1.z + value2.z;
            return vector3;
        }

        public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
            result.z = value1.z + value2.z;
        }

        public static Vector3 Sub(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x - value2.x;
            vector3.y = value1.y - value2.y;
            vector3.z = value1.z - value2.z;
            return vector3;
        }

        public static void Sub(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
            result.z = value1.z - value2.z;
        }

        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x * value2.x;
            vector3.y = value1.y * value2.y;
            vector3.z = value1.z * value2.z;
            return vector3;
        }

        public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
            result.z = value1.z * value2.z;
        }

        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            Vector3 vector3;
            vector3.x = value1.x * scaleFactor;
            vector3.y = value1.y * scaleFactor;
            vector3.z = value1.z * scaleFactor;
            return vector3;
        }

        public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
        }

        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x / value2.x;
            vector3.y = value1.y / value2.y;
            vector3.z = value1.z / value2.z;
            return vector3;
        }

        public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
            result.z = value1.z / value2.z;
        }

        public static Vector3 Divide(Vector3 value1, float divider)
        {
            float num = 1f / divider;
            Vector3 vector3;
            vector3.x = value1.x * num;
            vector3.y = value1.y * num;
            vector3.z = value1.z * num;
            return vector3;
        }

        public static void Divide(ref Vector3 value1, float divider, out Vector3 result)
        {
            float num = 1f / divider;
            result.x = value1.x * num;
            result.y = value1.y * num;
            result.z = value1.z * num;
        }

        private static float magnitudeStatic(ref Vector3 inV)
        {
            return (float) Math.Sqrt((double) Vector3.Dot(inV, inV));
        }

        private static Vector3 orthoNormalVectorFast(ref Vector3 n)
        {
            Vector3 vector3;
            if ((double) Math.Abs(n.z) > (double) Vector3.k1OverSqrt2)
            {
                float num = 1f / (float) Math.Sqrt((double) n.y * (double) n.y + (double) n.z * (double) n.z);
                vector3.x = 0.0f;
                vector3.y = -n.z * num;
                vector3.z = n.y * num;
            }
            else
            {
                float num = 1f / (float) Math.Sqrt((double) n.x * (double) n.x + (double) n.y * (double) n.y);
                vector3.x = -n.y * num;
                vector3.y = n.x * num;
                vector3.z = 0.0f;
            }

            return vector3;
        }

        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent)
        {
            float num1 = Vector3.magnitudeStatic(ref normal);
            if ((double) num1 > (double) Vector3.epsilon)
                normal /= num1;
            else
                normal = new Vector3(1f, 0.0f, 0.0f);
            float num2 = Vector3.Dot(normal, tangent);
            tangent -= num2 * normal;
            float num3 = Vector3.magnitudeStatic(ref tangent);
            if ((double) num3 < (double) Vector3.epsilon)
                tangent = Vector3.orthoNormalVectorFast(ref normal);
            else
                tangent /= num3;
        }

        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal)
        {
            float num1 = Vector3.magnitudeStatic(ref normal);
            if ((double) num1 > (double) Vector3.epsilon)
                normal /= num1;
            else
                normal = new Vector3(1f, 0.0f, 0.0f);
            float num2 = Vector3.Dot(normal, tangent);
            tangent -= num2 * normal;
            float num3 = Vector3.magnitudeStatic(ref tangent);
            if ((double) num3 > (double) Vector3.epsilon)
                tangent /= num3;
            else
                tangent = Vector3.orthoNormalVectorFast(ref normal);
            float num4 = Vector3.Dot(tangent, binormal);
            float num5 = Vector3.Dot(normal, binormal);
            binormal -= num5 * normal + num4 * tangent;
            float num6 = Vector3.magnitudeStatic(ref binormal);
            if ((double) num6 > (double) Vector3.epsilon)
                binormal /= num6;
            else
                binormal = Vector3.Cross(normal, tangent);
        }

        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
            return onNormal * Vector3.Dot(vector, onNormal) / Vector3.Dot(onNormal, onNormal);
        }

        public static void Project(ref Vector3 vector, ref Vector3 onNormal, out Vector3 result)
        {
            result = onNormal * Vector3.Dot(vector, onNormal) / Vector3.Dot(onNormal, onNormal);
        }

        public static float Angle(Vector3 from, Vector3 to)
        {
            from.Normalize();
            to.Normalize();
            float result;
            Vector3.Dot(ref from, ref to, out result);
            return MathHelper.ACos(MathHelper.Clamp(result, -1f, 1f)) * 57.29578f;
        }

        public static void Angle(ref Vector3 from, ref Vector3 to, out float result)
        {
            from.Normalize();
            to.Normalize();
            float result1;
            Vector3.Dot(ref from, ref to, out result1);
            result = MathHelper.ACos(MathHelper.Clamp(result1, -1f, 1f)) * 57.29578f;
        }

        public static Vector3 operator -(Vector3 value)
        {
            Vector3 vector3;
            vector3.x = -value.x;
            vector3.y = -value.y;
            vector3.z = -value.z;
            return vector3;
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return (double) (lhs - rhs).sqrMagnitude < 9.99999943962493E-11;
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return !(lhs == rhs);
        }

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x + value2.x;
            vector3.y = value1.y + value2.y;
            vector3.z = value1.z + value2.z;
            return vector3;
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x - value2.x;
            vector3.y = value1.y - value2.y;
            vector3.z = value1.z - value2.z;
            return vector3;
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x * value2.x;
            vector3.y = value1.y * value2.y;
            vector3.z = value1.z * value2.z;
            return vector3;
        }

        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            Vector3 vector3;
            vector3.x = value.x * scaleFactor;
            vector3.y = value.y * scaleFactor;
            vector3.z = value.z * scaleFactor;
            return vector3;
        }

        public static Vector3 operator *(float scaleFactor, Vector3 value)
        {
            Vector3 vector3;
            vector3.x = value.x * scaleFactor;
            vector3.y = value.y * scaleFactor;
            vector3.z = value.z * scaleFactor;
            return vector3;
        }

        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            Vector3 vector3;
            vector3.x = value1.x / value2.x;
            vector3.y = value1.y / value2.y;
            vector3.z = value1.z / value2.z;
            return vector3;
        }

        public static Vector3 operator /(Vector3 value, float divider)
        {
            float num = 1f / divider;
            Vector3 vector3;
            vector3.x = value.x * num;
            vector3.y = value.y * num;
            vector3.z = value.z * num;
            return vector3;
        }
    }
}