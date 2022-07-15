using System;
using System.Globalization;

namespace UnityEngine
{
    [Serializable]
    public struct Quaternion: IEquatable<Quaternion>
    {
        public static readonly Quaternion identity = new Quaternion(0.0f, 0.0f, 0.0f, 1f);
        public float w;
        public float x;
        public float y;
        public float z;


        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(float angle, Vector3 rkAxis)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            this.x = rkAxis.x * num2;
            this.y = rkAxis.y * num2;
            this.z = rkAxis.z * num2;
            this.w = num3;
        }

        public Quaternion(Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            Matrix4x4 identityM = Matrix4x4.identity;
            identityM[0, 0] = xaxis.x;
            identityM[1, 0] = xaxis.y;
            identityM[2, 0] = xaxis.z;
            identityM[0, 1] = yaxis.x;
            identityM[1, 1] = yaxis.y;
            identityM[2, 1] = yaxis.z;
            identityM[0, 2] = zaxis.x;
            identityM[1, 2] = zaxis.y;
            identityM[2, 2] = zaxis.z;
            Quaternion.CreateFromRotationMatrix(ref identityM, out this);
        }

        public Quaternion(float yaw, float pitch, float roll)
        {
            float num1 = roll * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            float num4 = pitch * 0.5f;
            float num5 = (float) Math.Sin((double) num4);
            float num6 = (float) Math.Cos((double) num4);
            float num7 = yaw * 0.5f;
            float num8 = (float) Math.Sin((double) num7);
            float num9 = (float) Math.Cos((double) num7);
            this.x = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            this.y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            this.z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            this.w = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
        }
        

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format((IFormatProvider) currentCulture, "({0}, {1}, {2}, {3})", (object) this.x.ToString((IFormatProvider) currentCulture),
                                 (object) this.y.ToString((IFormatProvider) currentCulture),
                                 (object) this.z.ToString((IFormatProvider) currentCulture),
                                 (object) this.w.ToString((IFormatProvider) currentCulture));
        }

        public bool Equals(Quaternion other)
        {
            if ((double) this.x == (double) other.x && (double) this.y == (double) other.y && (double) this.z == (double) other.z)
                return (double) this.w == (double) other.w;
            return false;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Quaternion)
                flag = this.Equals((Quaternion) obj);
            return flag;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() + this.y.GetHashCode() + this.z.GetHashCode() + this.w.GetHashCode();
        }

        public float LengthSquared()
        {
            return (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z +
                (double) this.w * (double) this.w);
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z +
                                     (double) this.w * (double) this.w);
        }

        public void Normalize()
        {
            float num = 1f / (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y +
                                               (double) this.z * (double) this.z + (double) this.w * (double) this.w);
            this.x *= num;
            this.y *= num;
            this.z *= num;
            this.w *= num;
        }

        public static Quaternion Normalize(Quaternion quaternion)
        {
            float num = 1f / (float) Math.Sqrt((double) quaternion.x * (double) quaternion.x + (double) quaternion.y * (double) quaternion.y +
                                               (double) quaternion.z * (double) quaternion.z + (double) quaternion.w * (double) quaternion.w);
            Quaternion quaternion1;
            quaternion1.x = quaternion.x * num;
            quaternion1.y = quaternion.y * num;
            quaternion1.z = quaternion.z * num;
            quaternion1.w = quaternion.w * num;
            return quaternion1;
        }

        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float) Math.Sqrt((double) quaternion.x * (double) quaternion.x + (double) quaternion.y * (double) quaternion.y +
                                               (double) quaternion.z * (double) quaternion.z + (double) quaternion.w * (double) quaternion.w);
            result.x = quaternion.x * num;
            result.y = quaternion.y * num;
            result.z = quaternion.z * num;
            result.w = quaternion.w * num;
        }

        public static Quaternion Inverse(Quaternion quaternion)
        {
            float num = 1f / (float) ((double) quaternion.x * (double) quaternion.x + (double) quaternion.y * (double) quaternion.y +
                (double) quaternion.z * (double) quaternion.z + (double) quaternion.w * (double) quaternion.w);
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x * num;
            quaternion1.y = -quaternion.y * num;
            quaternion1.z = -quaternion.z * num;
            quaternion1.w = quaternion.w * num;
            return quaternion1;
        }

        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float) ((double) quaternion.x * (double) quaternion.x + (double) quaternion.y * (double) quaternion.y +
                (double) quaternion.z * (double) quaternion.z + (double) quaternion.w * (double) quaternion.w);
            result.x = -quaternion.x * num;
            result.y = -quaternion.y * num;
            result.z = -quaternion.z * num;
            result.w = quaternion.w * num;
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            Quaternion quaternion;
            quaternion.x = axis.x * num2;
            quaternion.y = axis.y * num2;
            quaternion.z = axis.z * num2;
            quaternion.w = num3;
            return quaternion;
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            result.x = axis.x * num2;
            result.y = axis.y * num2;
            result.z = axis.z * num2;
            result.w = num3;
        }

        public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float num1 = roll * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            float num4 = pitch * 0.5f;
            float num5 = (float) Math.Sin((double) num4);
            float num6 = (float) Math.Cos((double) num4);
            float num7 = yaw * 0.5f;
            float num8 = (float) Math.Sin((double) num7);
            float num9 = (float) Math.Cos((double) num7);
            Quaternion quaternion;
            quaternion.x = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            quaternion.y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            quaternion.z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            quaternion.w = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
            return quaternion;
        }
        
        public static Quaternion Euler(Vector3 eulerAngle)
        {
            //角度转弧度
            eulerAngle = Mathf.Deg2Rad(eulerAngle);

            float cX = (float)Math.Cos(eulerAngle.x / 2.0f);
            float sX = (float)Math.Sin(eulerAngle.x / 2.0f);

            float cY = (float)Math.Cos(eulerAngle.y / 2.0f);
            float sY = (float)Math.Sin(eulerAngle.y / 2.0f);

            float cZ = (float)Math.Cos(eulerAngle.z / 2.0f);
            float sZ = (float)Math.Sin(eulerAngle.z / 2.0f);

            Quaternion qX = new Quaternion(sX, 0, 0, cX);
            Quaternion qY = new Quaternion(0, sY, 0, cY);
            Quaternion qZ = new Quaternion(0, 0, sZ, cZ);

            Quaternion q = (qY * qX) * qZ;

            return q;
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            return Euler(new Vector3(x, y, z));
        }
        
        private static Matrix3x3 QuaternionToMatrix(Quaternion q)
        {
            // Precalculate coordinate products
            float x = q.x * 2.0F;
            float y = q.y * 2.0F;
            float z = q.z * 2.0F;
            float xx = q.x * x;
            float yy = q.y * y;
            float zz = q.z * z;
            float xy = q.x * y;
            float xz = q.x * z;
            float yz = q.y * z;
            float wx = q.w * x;
            float wy = q.w * y;
            float wz = q.w * z;

            // Calculate 3x3 matrix from orthonormal basis
            Matrix3x3 m = Matrix3x3.identity;

            m.Data[0] = 1.0f - (yy + zz);
            m.Data[1] = xy + wz;
            m.Data[2] = xz - wy;

            m.Data[3] = xy - wz;
            m.Data[4] = 1.0f - (xx + zz);
            m.Data[5] = yz + wx;

            m.Data[6] = xz + wy;
            m.Data[7] = yz - wx;
            m.Data[8] = 1.0f - (xx + yy);

            return m;
        }
        
        public static Vector3 QuaternionToEuler(Quaternion quat)
        {
            Matrix3x3 m = QuaternionToMatrix(quat);
            Vector3 euler = MatrixToEuler(m);

            //弧度转角度
            return Mathf.Rad2Deg(euler);
        }
        
        private static Vector3 MakePositive(Vector3 euler)
        {
            const float negativeFlip = -0.0001F;
            const float positiveFlip = ((float)Math.PI * 2.0F) - 0.0001F;

            if (euler.x < negativeFlip)
                euler.x += 2.0f * (float)Math.PI;
            else if (euler.x > positiveFlip)
                euler.x -= 2.0f * (float)Math.PI;

            if (euler.y < negativeFlip)
                euler.y += 2.0f * (float)Math.PI;
            else if (euler.y > positiveFlip)
                euler.y -= 2.0f * (float)Math.PI;

            if (euler.z < negativeFlip)
                euler.z += 2.0f * (float)Math.PI;
            else if (euler.z > positiveFlip)
                euler.z -= 2.0f * (float)Math.PI;

            return euler;
        }
        
        
        private static Vector3 MatrixToEuler(Matrix3x3 matrix)
        {
            // from http://www.geometrictools.com/Documentation/EulerAngles.pdf
            // YXZ order
            Vector3 v = Vector3.zero;
            if (matrix.Data[7] < 0.999F) // some fudge for imprecision
            {
                if (matrix.Data[7] > -0.999F) // some fudge for imprecision
                {
                    v.x = Mathf.Asin(-matrix.Data[7]);
                    v.y = Mathf.Atan2(matrix.Data[6], matrix.Data[8]);
                    v.z = Mathf.Atan2(matrix.Data[1], matrix.Data[4]);
                    MakePositive(v);
                }
                else
                {
                    // WARNING.  Not unique.  YA - ZA = atan2(r01,r00)
                    v.x = (float)Math.PI * 0.5F;
                    v.y = Mathf.Atan2(matrix.Data[3], matrix.Data[0]);
                    v.z = 0.0F;
                    MakePositive(v);
                }
            }
            else
            {
                // WARNING.  Not unique.  YA + ZA = atan2(-r01,r00)
                v.x = -(float)Math.PI * 0.5F;
                v.y = Mathf.Atan2(-matrix.Data[3], matrix.Data[0]);
                v.z = 0.0F;
                MakePositive(v);
            }

            return v; //返回的是弧度值
        }
        
        private static Quaternion MatrixToQuaternion(Matrix3x3 kRot)
        {
            Quaternion q = new Quaternion();

            // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
            // article "Quaternionf Calculus and Fast Animation".

            float fTrace = kRot.Get(0, 0) + kRot.Get(1, 1) + kRot.Get(2, 2);
            float fRoot;

            if (fTrace > 0.0f)
            {
                // |w| > 1/2, mafy as well choose w > 1/2
                fRoot = Mathf.Sqrt(fTrace + 1.0f);  // 2w
                q.w = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;  // 1/(4w)
                q.x = (kRot.Get(2, 1) - kRot.Get(1, 2)) * fRoot;
                q.y = (kRot.Get(0, 2) - kRot.Get(2, 0)) * fRoot;
                q.z = (kRot.Get(1, 0) - kRot.Get(0, 1)) * fRoot;
            }
            else
            {
                // |w| <= 1/2
                int[] s_iNext = new int[3] { 1, 2, 0 };
                int i = 0;
                if (kRot.Get(1, 1) > kRot.Get(0, 0))
                    i = 1;
                if (kRot.Get(2, 2) > kRot.Get(i, i))
                    i = 2;
                int j = s_iNext[i];
                int k = s_iNext[j];

                fRoot = Mathf.Sqrt(kRot.Get(i, i) - kRot.Get(j, j) - kRot.Get(k, k) + 1.0f);
                float[] apkQuat = new float[3] { q.x, q.y, q.z };

                apkQuat[i] = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;
                q.w = (kRot.Get(k, j) - kRot.Get(j, k)) * fRoot;
                apkQuat[j] = (kRot.Get(j, i) + kRot.Get(i, j)) * fRoot;
                apkQuat[k] = (kRot.Get(k, i) + kRot.Get(i, k)) * fRoot;

                q.x = apkQuat[0];
                q.y = apkQuat[1];
                q.z = apkQuat[2];
            }
            q = Quaternion.Normalize(q);

            return q;
        }
        
        public static Quaternion FromToRotation(Vector3 a, Vector3 b)
        {
            //return UnityEngine.Quaternion.FromToRotation(a, b);
            Vector3 start = a.normalized;
            Vector3 dest = b.normalized;
            float cosTheta = Vector3.Dot(start, dest);
            Vector3 rotationAxis;
            Quaternion quaternion;
            if (cosTheta < -1 + 0.001f)
            {
                rotationAxis = Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), start);
                if (rotationAxis.sqrMagnitude < 0.01f)
                {
                    rotationAxis = Vector3.Cross(new Vector3(1.0f, 0.0f, 0.0f), start);
                }
                rotationAxis.Normalize();
                quaternion = new Quaternion((float) Math.PI, rotationAxis);
                quaternion.Normalize();
                return quaternion;
            }

            rotationAxis = Vector3.Cross(start, dest);
            float s = (float)Math.Sqrt((1 + cosTheta) * 2);
            float invs = 1 / s;
            
            quaternion = new Quaternion(rotationAxis.x * invs, rotationAxis.y * invs, rotationAxis.z * invs, s * 0.5f);
            quaternion.Normalize();
            return quaternion;
        }
        
        public static bool LookRotationToQuaternion(Vector3 viewVec, Vector3 upVec, out Quaternion quat)
        {
            quat = Quaternion.identity;

            // Generates a Right handed Quat from a look rotation. Returns if conversion was successful.
            Matrix3x3 m;
            if (!Matrix3x3.LookRotationToMatrix(viewVec, upVec, out m))
                return false;
            quat = MatrixToQuaternion(m);
            return true;
        }

        public static Quaternion LookRotation(Vector3 viewVec, Vector3 upVec)
        {
            Quaternion q;
            bool ret = LookRotationToQuaternion(viewVec, upVec, out q);
            if (!ret)
            {
                throw new Exception("Look fail!");
            }

            return q;
        }

        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
        {
            float num1 = roll * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            float num4 = pitch * 0.5f;
            float num5 = (float) Math.Sin((double) num4);
            float num6 = (float) Math.Cos((double) num4);
            float num7 = yaw * 0.5f;
            float num8 = (float) Math.Sin((double) num7);
            float num9 = (float) Math.Cos((double) num7);
            result.x = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            result.y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            result.z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            result.w = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            Quaternion quaternion = new Quaternion();
            if ((double) num1 > 0.0)
            {
                float num2 = (float) Math.Sqrt((double) num1 + 1.0);
                quaternion.w = num2 * 0.5f;
                float num3 = 0.5f / num2;
                quaternion.x = (matrix.m21 - matrix.m12) * num3;
                quaternion.y = (matrix.m02 - matrix.m20) * num3;
                quaternion.z = (matrix.m10 - matrix.m01) * num3;
                return quaternion;
            }

            if ((double) matrix.m00 >= (double) matrix.m11 && (double) matrix.m00 >= (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m00 - (double) matrix.m11 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.x = 0.5f * num2;
                quaternion.y = (matrix.m10 + matrix.m01) * num3;
                quaternion.z = (matrix.m20 + matrix.m02) * num3;
                quaternion.w = (matrix.m21 - matrix.m12) * num3;
                return quaternion;
            }

            if ((double) matrix.m11 > (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m11 - (double) matrix.m00 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.x = (matrix.m01 + matrix.m10) * num3;
                quaternion.y = 0.5f * num2;
                quaternion.z = (matrix.m12 + matrix.m21) * num3;
                quaternion.w = (matrix.m02 - matrix.m20) * num3;
                return quaternion;
            }

            float num4 = (float) Math.Sqrt(1.0 + (double) matrix.m22 - (double) matrix.m00 - (double) matrix.m11);
            float num5 = 0.5f / num4;
            quaternion.x = (matrix.m02 + matrix.m20) * num5;
            quaternion.y = (matrix.m12 + matrix.m21) * num5;
            quaternion.z = 0.5f * num4;
            quaternion.w = (matrix.m10 - matrix.m01) * num5;
            return quaternion;
        }

        public static void CreateFromRotationMatrix(ref Matrix4x4 matrix, out Quaternion result)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            if ((double) num1 > 0.0)
            {
                float num2 = (float) Math.Sqrt((double) num1 + 1.0);
                result.w = num2 * 0.5f;
                float num3 = 0.5f / num2;
                result.x = (matrix.m21 - matrix.m12) * num3;
                result.y = (matrix.m02 - matrix.m20) * num3;
                result.z = (matrix.m10 - matrix.m01) * num3;
            }
            else if ((double) matrix.m00 >= (double) matrix.m11 && (double) matrix.m00 >= (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m00 - (double) matrix.m11 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                result.x = 0.5f * num2;
                result.y = (matrix.m10 + matrix.m01) * num3;
                result.z = (matrix.m20 + matrix.m02) * num3;
                result.w = (matrix.m21 - matrix.m12) * num3;
            }
            else if ((double) matrix.m11 > (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m11 - (double) matrix.m00 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                result.x = (matrix.m01 + matrix.m10) * num3;
                result.y = 0.5f * num2;
                result.z = (matrix.m12 + matrix.m21) * num3;
                result.w = (matrix.m02 - matrix.m20) * num3;
            }
            else
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m22 - (double) matrix.m00 - (double) matrix.m11);
                float num3 = 0.5f / num2;
                result.x = (matrix.m02 + matrix.m20) * num3;
                result.y = (matrix.m12 + matrix.m21) * num3;
                result.z = 0.5f * num2;
                result.w = (matrix.m10 - matrix.m01) * num3;
            }
        }

        public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
        {
            return (float) ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w);
        }

        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            result = (float) ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w);
        }

        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = (float) ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w);
            bool flag = false;
            if ((double) num2 < 0.0)
            {
                flag = true;
                num2 = -num2;
            }

            float num3;
            float num4;
            if ((double) num2 > 0.999998986721039)
            {
                num3 = 1f - num1;
                num4 = flag? -num1 : num1;
            }
            else
            {
                float num5 = (float) Math.Acos((double) num2);
                float num6 = (float) (1.0 / Math.Sin((double) num5));
                num3 = (float) Math.Sin((1.0 - (double) num1) * (double) num5) * num6;
                num4 = flag? (float) -Math.Sin((double) num1 * (double) num5) * num6 : (float) Math.Sin((double) num1 * (double) num5) * num6;
            }

            Quaternion quaternion;
            quaternion.x = (float) ((double) num3 * (double) quaternion1.x + (double) num4 * (double) quaternion2.x);
            quaternion.y = (float) ((double) num3 * (double) quaternion1.y + (double) num4 * (double) quaternion2.y);
            quaternion.z = (float) ((double) num3 * (double) quaternion1.z + (double) num4 * (double) quaternion2.z);
            quaternion.w = (float) ((double) num3 * (double) quaternion1.w + (double) num4 * (double) quaternion2.w);
            return quaternion;
        }

        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = (float) ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w);
            bool flag = false;
            if ((double) num2 < 0.0)
            {
                flag = true;
                num2 = -num2;
            }

            float num3;
            float num4;
            if ((double) num2 > 0.999998986721039)
            {
                num3 = 1f - num1;
                num4 = flag? -num1 : num1;
            }
            else
            {
                float num5 = (float) Math.Acos((double) num2);
                float num6 = (float) (1.0 / Math.Sin((double) num5));
                num3 = (float) Math.Sin((1.0 - (double) num1) * (double) num5) * num6;
                num4 = flag? (float) -Math.Sin((double) num1 * (double) num5) * num6 : (float) Math.Sin((double) num1 * (double) num5) * num6;
            }

            result.x = (float) ((double) num3 * (double) quaternion1.x + (double) num4 * (double) quaternion2.x);
            result.y = (float) ((double) num3 * (double) quaternion1.y + (double) num4 * (double) quaternion2.y);
            result.z = (float) ((double) num3 * (double) quaternion1.z + (double) num4 * (double) quaternion2.z);
            result.w = (float) ((double) num3 * (double) quaternion1.w + (double) num4 * (double) quaternion2.w);
        }

        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            Quaternion quaternion = new Quaternion();
            if ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w >= 0.0)
            {
                quaternion.x = (float) ((double) num2 * (double) quaternion1.x + (double) num1 * (double) quaternion2.x);
                quaternion.y = (float) ((double) num2 * (double) quaternion1.y + (double) num1 * (double) quaternion2.y);
                quaternion.z = (float) ((double) num2 * (double) quaternion1.z + (double) num1 * (double) quaternion2.z);
                quaternion.w = (float) ((double) num2 * (double) quaternion1.w + (double) num1 * (double) quaternion2.w);
            }
            else
            {
                quaternion.x = (float) ((double) num2 * (double) quaternion1.x - (double) num1 * (double) quaternion2.x);
                quaternion.y = (float) ((double) num2 * (double) quaternion1.y - (double) num1 * (double) quaternion2.y);
                quaternion.z = (float) ((double) num2 * (double) quaternion1.z - (double) num1 * (double) quaternion2.z);
                quaternion.w = (float) ((double) num2 * (double) quaternion1.w - (double) num1 * (double) quaternion2.w);
            }

            float num3 = 1f / (float) Math.Sqrt((double) quaternion.x * (double) quaternion.x + (double) quaternion.y * (double) quaternion.y +
                                                (double) quaternion.z * (double) quaternion.z + (double) quaternion.w * (double) quaternion.w);
            quaternion.x *= num3;
            quaternion.y *= num3;
            quaternion.z *= num3;
            quaternion.w *= num3;
            return quaternion;
        }

        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            if ((double) quaternion1.x * (double) quaternion2.x + (double) quaternion1.y * (double) quaternion2.y +
                (double) quaternion1.z * (double) quaternion2.z + (double) quaternion1.w * (double) quaternion2.w >= 0.0)
            {
                result.x = (float) ((double) num2 * (double) quaternion1.x + (double) num1 * (double) quaternion2.x);
                result.y = (float) ((double) num2 * (double) quaternion1.y + (double) num1 * (double) quaternion2.y);
                result.z = (float) ((double) num2 * (double) quaternion1.z + (double) num1 * (double) quaternion2.z);
                result.w = (float) ((double) num2 * (double) quaternion1.w + (double) num1 * (double) quaternion2.w);
            }
            else
            {
                result.x = (float) ((double) num2 * (double) quaternion1.x - (double) num1 * (double) quaternion2.x);
                result.y = (float) ((double) num2 * (double) quaternion1.y - (double) num1 * (double) quaternion2.y);
                result.z = (float) ((double) num2 * (double) quaternion1.z - (double) num1 * (double) quaternion2.z);
                result.w = (float) ((double) num2 * (double) quaternion1.w - (double) num1 * (double) quaternion2.w);
            }

            float num3 = 1f / (float) Math.Sqrt((double) result.x * (double) result.x + (double) result.y * (double) result.y +
                                                (double) result.z * (double) result.z + (double) result.w * (double) result.w);
            result.x *= num3;
            result.y *= num3;
            result.z *= num3;
            result.w *= num3;
        }

        public void Conjugate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        public static Quaternion Conjugate(Quaternion value)
        {
            Quaternion quaternion;
            quaternion.x = -value.x;
            quaternion.y = -value.y;
            quaternion.z = -value.z;
            quaternion.w = value.w;
            return quaternion;
        }

        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.x = -value.x;
            result.y = -value.y;
            result.z = -value.z;
            result.w = value.w;
        }

        private static float Angle(Quaternion a, Quaternion b)
        {
            return (float) (Math.Acos((double) Math.Min(Math.Abs(Quaternion.Dot(a, b)), 1f)) * 2.0 * 57.2957801818848);
        }

        private static void Angle(ref Quaternion a, ref Quaternion b, out float result)
        {
            result = (float) (Math.Acos((double) Math.Min(Math.Abs(Quaternion.Dot(a, b)), 1f)) * 2.0 * 57.2957801818848);
        }

        public static Quaternion Negate(Quaternion quaternion)
        {
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x;
            quaternion1.y = -quaternion.y;
            quaternion1.z = -quaternion.z;
            quaternion1.w = -quaternion.w;
            return quaternion1;
        }

        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.x = -quaternion.x;
            result.y = -quaternion.y;
            result.z = -quaternion.z;
            result.w = -quaternion.w;
        }

        public static Quaternion Sub(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.x = quaternion1.x - quaternion2.x;
            quaternion.y = quaternion1.y - quaternion2.y;
            quaternion.z = quaternion1.z - quaternion2.z;
            quaternion.w = quaternion1.w - quaternion2.w;
            return quaternion;
        }

        public static void Sub(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }

        public static Vector3 Rotate(Quaternion rotation, Vector3 vector3)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num1;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 vector3_1;
            vector3_1.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) vector3.x +
                ((double) num7 - (double) num12) * (double) vector3.y + ((double) num8 + (double) num11) * (double) vector3.z);
            vector3_1.y = (float) (((double) num7 + (double) num12) * (double) vector3.x +
                (1.0 - ((double) num4 + (double) num6)) * (double) vector3.y + ((double) num9 - (double) num10) * (double) vector3.z);
            vector3_1.z = (float) (((double) num8 - (double) num11) * (double) vector3.x + ((double) num9 + (double) num10) * (double) vector3.y +
                (1.0 - ((double) num4 + (double) num5)) * (double) vector3.z);
            return vector3_1;
        }

        public static void Rotate(ref Quaternion rotation, ref Vector3 vector3, out Vector3 result)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num1;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            result.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) vector3.x + ((double) num7 - (double) num12) * (double) vector3.y +
                ((double) num8 + (double) num11) * (double) vector3.z);
            result.y = (float) (((double) num7 + (double) num12) * (double) vector3.x + (1.0 - ((double) num4 + (double) num6)) * (double) vector3.y +
                ((double) num9 - (double) num10) * (double) vector3.z);
            result.z = (float) (((double) num8 - (double) num11) * (double) vector3.x + ((double) num9 + (double) num10) * (double) vector3.y +
                (1.0 - ((double) num4 + (double) num5)) * (double) vector3.z);
        }

        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x1 = quaternion1.x;
            float y1 = quaternion1.y;
            float z1 = quaternion1.z;
            float w1 = quaternion1.w;
            float x2 = quaternion2.x;
            float y2 = quaternion2.y;
            float z2 = quaternion2.z;
            float w2 = quaternion2.w;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            Quaternion quaternion;
            quaternion.x = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            quaternion.y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            quaternion.z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            quaternion.w = w1 * w2 - num4;
            return quaternion;
        }

        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x1 = quaternion1.x;
            float y1 = quaternion1.y;
            float z1 = quaternion1.z;
            float w1 = quaternion1.w;
            float x2 = quaternion2.x;
            float y2 = quaternion2.y;
            float z2 = quaternion2.z;
            float w2 = quaternion2.w;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            result.x = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            result.y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            result.z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            result.w = w1 * w2 - num4;
        }

        public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion quaternion1;
            quaternion1.x = -quaternion.x;
            quaternion1.y = -quaternion.y;
            quaternion1.z = -quaternion.z;
            quaternion1.w = -quaternion.w;
            return quaternion1;
        }

        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            if ((double) quaternion1.x == (double) quaternion2.x && (double) quaternion1.y == (double) quaternion2.y &&
                (double) quaternion1.z == (double) quaternion2.z)
                return (double) quaternion1.w == (double) quaternion2.w;
            return false;
        }

        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            if ((double) quaternion1.x == (double) quaternion2.x && (double) quaternion1.y == (double) quaternion2.y &&
                (double) quaternion1.z == (double) quaternion2.z)
                return (double) quaternion1.w != (double) quaternion2.w;
            return true;
        }

        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.x = quaternion1.x - quaternion2.x;
            quaternion.y = quaternion1.y - quaternion2.y;
            quaternion.z = quaternion1.z - quaternion2.z;
            quaternion.w = quaternion1.w - quaternion2.w;
            return quaternion;
        }

        public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x1 = quaternion1.x;
            float y1 = quaternion1.y;
            float z1 = quaternion1.z;
            float w1 = quaternion1.w;
            float x2 = quaternion2.x;
            float y2 = quaternion2.y;
            float z2 = quaternion2.z;
            float w2 = quaternion2.w;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            Quaternion quaternion;
            quaternion.x = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            quaternion.y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            quaternion.z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            quaternion.w = w1 * w2 - num4;
            return quaternion;
        }
        
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num1 = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num1;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num1;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 vector3;
            vector3.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y + ((double) num8 + (double) num11) * (double) point.z);
            vector3.y = (float) (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y + ((double) num9 - (double) num10) * (double) point.z);
            vector3.z = (float) (((double) num8 - (double) num11) * (double) point.x + ((double) num9 + (double) num10) * (double) point.y + (1.0 - ((double) num4 + (double) num5)) * (double) point.z);
            return vector3;
        }
    }
}