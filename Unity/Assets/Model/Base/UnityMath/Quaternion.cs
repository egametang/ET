using System;
using System.Globalization;

namespace PF
{
    [Serializable]
    public struct Quaternion: IEquatable<Quaternion>
    {
        private static Quaternion _identity = new Quaternion(0.0f, 0.0f, 0.0f, 1f);
        public float W;
        public float X;
        public float Y;
        public float Z;

        public static Quaternion identity
        {
            get
            {
                return Quaternion._identity;
            }
        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Quaternion(float angle, Vector3 rkAxis)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            this.X = rkAxis.x * num2;
            this.Y = rkAxis.y * num2;
            this.Z = rkAxis.z * num2;
            this.W = num3;
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
            this.X = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            this.Y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            this.Z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            this.W = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
        }
        
#if !SERVER
        public static implicit operator UnityEngine.Quaternion(Quaternion q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }
        
        public static implicit operator Quaternion(UnityEngine.Quaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
#endif

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return string.Format((IFormatProvider) currentCulture, "({0}, {1}, {2}, {3})", (object) this.X.ToString((IFormatProvider) currentCulture),
                                 (object) this.Y.ToString((IFormatProvider) currentCulture),
                                 (object) this.Z.ToString((IFormatProvider) currentCulture),
                                 (object) this.W.ToString((IFormatProvider) currentCulture));
        }

        public bool Equals(Quaternion other)
        {
            if ((double) this.X == (double) other.X && (double) this.Y == (double) other.Y && (double) this.Z == (double) other.Z)
                return (double) this.W == (double) other.W;
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
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
        }

        public float LengthSquared()
        {
            return (float) ((double) this.X * (double) this.X + (double) this.Y * (double) this.Y + (double) this.Z * (double) this.Z +
                (double) this.W * (double) this.W);
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y + (double) this.Z * (double) this.Z +
                                     (double) this.W * (double) this.W);
        }

        public void Normalize()
        {
            float num = 1f / (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y +
                                               (double) this.Z * (double) this.Z + (double) this.W * (double) this.W);
            this.X *= num;
            this.Y *= num;
            this.Z *= num;
            this.W *= num;
        }

        public static Quaternion Normalize(Quaternion quaternion)
        {
            float num = 1f / (float) Math.Sqrt((double) quaternion.X * (double) quaternion.X + (double) quaternion.Y * (double) quaternion.Y +
                                               (double) quaternion.Z * (double) quaternion.Z + (double) quaternion.W * (double) quaternion.W);
            Quaternion quaternion1;
            quaternion1.X = quaternion.X * num;
            quaternion1.Y = quaternion.Y * num;
            quaternion1.Z = quaternion.Z * num;
            quaternion1.W = quaternion.W * num;
            return quaternion1;
        }

        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float) Math.Sqrt((double) quaternion.X * (double) quaternion.X + (double) quaternion.Y * (double) quaternion.Y +
                                               (double) quaternion.Z * (double) quaternion.Z + (double) quaternion.W * (double) quaternion.W);
            result.X = quaternion.X * num;
            result.Y = quaternion.Y * num;
            result.Z = quaternion.Z * num;
            result.W = quaternion.W * num;
        }

        public static Quaternion Inverse(Quaternion quaternion)
        {
            float num = 1f / (float) ((double) quaternion.X * (double) quaternion.X + (double) quaternion.Y * (double) quaternion.Y +
                (double) quaternion.Z * (double) quaternion.Z + (double) quaternion.W * (double) quaternion.W);
            Quaternion quaternion1;
            quaternion1.X = -quaternion.X * num;
            quaternion1.Y = -quaternion.Y * num;
            quaternion1.Z = -quaternion.Z * num;
            quaternion1.W = quaternion.W * num;
            return quaternion1;
        }

        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            float num = 1f / (float) ((double) quaternion.X * (double) quaternion.X + (double) quaternion.Y * (double) quaternion.Y +
                (double) quaternion.Z * (double) quaternion.Z + (double) quaternion.W * (double) quaternion.W);
            result.X = -quaternion.X * num;
            result.Y = -quaternion.Y * num;
            result.Z = -quaternion.Z * num;
            result.W = quaternion.W * num;
        }

        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            Quaternion quaternion;
            quaternion.X = axis.x * num2;
            quaternion.Y = axis.y * num2;
            quaternion.Z = axis.z * num2;
            quaternion.W = num3;
            return quaternion;
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            float num1 = angle * 0.5f;
            float num2 = (float) Math.Sin((double) num1);
            float num3 = (float) Math.Cos((double) num1);
            result.X = axis.x * num2;
            result.Y = axis.y * num2;
            result.Z = axis.z * num2;
            result.W = num3;
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
            quaternion.X = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            quaternion.Y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            quaternion.Z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            quaternion.W = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
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
            float x = q.X * 2.0F;
            float y = q.Y * 2.0F;
            float z = q.Z * 2.0F;
            float xx = q.X * x;
            float yy = q.Y * y;
            float zz = q.Z * z;
            float xy = q.X * y;
            float xz = q.X * z;
            float yz = q.Y * z;
            float wx = q.W * x;
            float wy = q.W * y;
            float wz = q.W * z;

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
                q.W = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;  // 1/(4w)
                q.X = (kRot.Get(2, 1) - kRot.Get(1, 2)) * fRoot;
                q.Y = (kRot.Get(0, 2) - kRot.Get(2, 0)) * fRoot;
                q.Z = (kRot.Get(1, 0) - kRot.Get(0, 1)) * fRoot;
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
                float[] apkQuat = new float[3] { q.X, q.Y, q.Z };

                apkQuat[i] = 0.5f * fRoot;
                fRoot = 0.5f / fRoot;
                q.W = (kRot.Get(k, j) - kRot.Get(j, k)) * fRoot;
                apkQuat[j] = (kRot.Get(j, i) + kRot.Get(i, j)) * fRoot;
                apkQuat[k] = (kRot.Get(k, i) + kRot.Get(i, k)) * fRoot;

                q.X = apkQuat[0];
                q.Y = apkQuat[1];
                q.Z = apkQuat[2];
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
            result.X = (float) ((double) num9 * (double) num5 * (double) num3 + (double) num8 * (double) num6 * (double) num2);
            result.Y = (float) ((double) num8 * (double) num6 * (double) num3 - (double) num9 * (double) num5 * (double) num2);
            result.Z = (float) ((double) num9 * (double) num6 * (double) num2 - (double) num8 * (double) num5 * (double) num3);
            result.W = (float) ((double) num9 * (double) num6 * (double) num3 + (double) num8 * (double) num5 * (double) num2);
        }

        public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            Quaternion quaternion = new Quaternion();
            if ((double) num1 > 0.0)
            {
                float num2 = (float) Math.Sqrt((double) num1 + 1.0);
                quaternion.W = num2 * 0.5f;
                float num3 = 0.5f / num2;
                quaternion.X = (matrix.m21 - matrix.m12) * num3;
                quaternion.Y = (matrix.m02 - matrix.m20) * num3;
                quaternion.Z = (matrix.m10 - matrix.m01) * num3;
                return quaternion;
            }

            if ((double) matrix.m00 >= (double) matrix.m11 && (double) matrix.m00 >= (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m00 - (double) matrix.m11 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.X = 0.5f * num2;
                quaternion.Y = (matrix.m10 + matrix.m01) * num3;
                quaternion.Z = (matrix.m20 + matrix.m02) * num3;
                quaternion.W = (matrix.m21 - matrix.m12) * num3;
                return quaternion;
            }

            if ((double) matrix.m11 > (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m11 - (double) matrix.m00 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                quaternion.X = (matrix.m01 + matrix.m10) * num3;
                quaternion.Y = 0.5f * num2;
                quaternion.Z = (matrix.m12 + matrix.m21) * num3;
                quaternion.W = (matrix.m02 - matrix.m20) * num3;
                return quaternion;
            }

            float num4 = (float) Math.Sqrt(1.0 + (double) matrix.m22 - (double) matrix.m00 - (double) matrix.m11);
            float num5 = 0.5f / num4;
            quaternion.X = (matrix.m02 + matrix.m20) * num5;
            quaternion.Y = (matrix.m12 + matrix.m21) * num5;
            quaternion.Z = 0.5f * num4;
            quaternion.W = (matrix.m10 - matrix.m01) * num5;
            return quaternion;
        }

        public static void CreateFromRotationMatrix(ref Matrix4x4 matrix, out Quaternion result)
        {
            float num1 = matrix.m00 + matrix.m11 + matrix.m22;
            if ((double) num1 > 0.0)
            {
                float num2 = (float) Math.Sqrt((double) num1 + 1.0);
                result.W = num2 * 0.5f;
                float num3 = 0.5f / num2;
                result.X = (matrix.m21 - matrix.m12) * num3;
                result.Y = (matrix.m02 - matrix.m20) * num3;
                result.Z = (matrix.m10 - matrix.m01) * num3;
            }
            else if ((double) matrix.m00 >= (double) matrix.m11 && (double) matrix.m00 >= (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m00 - (double) matrix.m11 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                result.X = 0.5f * num2;
                result.Y = (matrix.m10 + matrix.m01) * num3;
                result.Z = (matrix.m20 + matrix.m02) * num3;
                result.W = (matrix.m21 - matrix.m12) * num3;
            }
            else if ((double) matrix.m11 > (double) matrix.m22)
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m11 - (double) matrix.m00 - (double) matrix.m22);
                float num3 = 0.5f / num2;
                result.X = (matrix.m01 + matrix.m10) * num3;
                result.Y = 0.5f * num2;
                result.Z = (matrix.m12 + matrix.m21) * num3;
                result.W = (matrix.m02 - matrix.m20) * num3;
            }
            else
            {
                float num2 = (float) Math.Sqrt(1.0 + (double) matrix.m22 - (double) matrix.m00 - (double) matrix.m11);
                float num3 = 0.5f / num2;
                result.X = (matrix.m02 + matrix.m20) * num3;
                result.Y = (matrix.m12 + matrix.m21) * num3;
                result.Z = 0.5f * num2;
                result.W = (matrix.m10 - matrix.m01) * num3;
            }
        }

        public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
        {
            return (float) ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W);
        }

        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            result = (float) ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W);
        }

        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = (float) ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W);
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
            quaternion.X = (float) ((double) num3 * (double) quaternion1.X + (double) num4 * (double) quaternion2.X);
            quaternion.Y = (float) ((double) num3 * (double) quaternion1.Y + (double) num4 * (double) quaternion2.Y);
            quaternion.Z = (float) ((double) num3 * (double) quaternion1.Z + (double) num4 * (double) quaternion2.Z);
            quaternion.W = (float) ((double) num3 * (double) quaternion1.W + (double) num4 * (double) quaternion2.W);
            return quaternion;
        }

        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = (float) ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W);
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

            result.X = (float) ((double) num3 * (double) quaternion1.X + (double) num4 * (double) quaternion2.X);
            result.Y = (float) ((double) num3 * (double) quaternion1.Y + (double) num4 * (double) quaternion2.Y);
            result.Z = (float) ((double) num3 * (double) quaternion1.Z + (double) num4 * (double) quaternion2.Z);
            result.W = (float) ((double) num3 * (double) quaternion1.W + (double) num4 * (double) quaternion2.W);
        }

        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            Quaternion quaternion = new Quaternion();
            if ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W >= 0.0)
            {
                quaternion.X = (float) ((double) num2 * (double) quaternion1.X + (double) num1 * (double) quaternion2.X);
                quaternion.Y = (float) ((double) num2 * (double) quaternion1.Y + (double) num1 * (double) quaternion2.Y);
                quaternion.Z = (float) ((double) num2 * (double) quaternion1.Z + (double) num1 * (double) quaternion2.Z);
                quaternion.W = (float) ((double) num2 * (double) quaternion1.W + (double) num1 * (double) quaternion2.W);
            }
            else
            {
                quaternion.X = (float) ((double) num2 * (double) quaternion1.X - (double) num1 * (double) quaternion2.X);
                quaternion.Y = (float) ((double) num2 * (double) quaternion1.Y - (double) num1 * (double) quaternion2.Y);
                quaternion.Z = (float) ((double) num2 * (double) quaternion1.Z - (double) num1 * (double) quaternion2.Z);
                quaternion.W = (float) ((double) num2 * (double) quaternion1.W - (double) num1 * (double) quaternion2.W);
            }

            float num3 = 1f / (float) Math.Sqrt((double) quaternion.X * (double) quaternion.X + (double) quaternion.Y * (double) quaternion.Y +
                                                (double) quaternion.Z * (double) quaternion.Z + (double) quaternion.W * (double) quaternion.W);
            quaternion.X *= num3;
            quaternion.Y *= num3;
            quaternion.Z *= num3;
            quaternion.W *= num3;
            return quaternion;
        }

        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            float num1 = amount;
            float num2 = 1f - num1;
            if ((double) quaternion1.X * (double) quaternion2.X + (double) quaternion1.Y * (double) quaternion2.Y +
                (double) quaternion1.Z * (double) quaternion2.Z + (double) quaternion1.W * (double) quaternion2.W >= 0.0)
            {
                result.X = (float) ((double) num2 * (double) quaternion1.X + (double) num1 * (double) quaternion2.X);
                result.Y = (float) ((double) num2 * (double) quaternion1.Y + (double) num1 * (double) quaternion2.Y);
                result.Z = (float) ((double) num2 * (double) quaternion1.Z + (double) num1 * (double) quaternion2.Z);
                result.W = (float) ((double) num2 * (double) quaternion1.W + (double) num1 * (double) quaternion2.W);
            }
            else
            {
                result.X = (float) ((double) num2 * (double) quaternion1.X - (double) num1 * (double) quaternion2.X);
                result.Y = (float) ((double) num2 * (double) quaternion1.Y - (double) num1 * (double) quaternion2.Y);
                result.Z = (float) ((double) num2 * (double) quaternion1.Z - (double) num1 * (double) quaternion2.Z);
                result.W = (float) ((double) num2 * (double) quaternion1.W - (double) num1 * (double) quaternion2.W);
            }

            float num3 = 1f / (float) Math.Sqrt((double) result.X * (double) result.X + (double) result.Y * (double) result.Y +
                                                (double) result.Z * (double) result.Z + (double) result.W * (double) result.W);
            result.X *= num3;
            result.Y *= num3;
            result.Z *= num3;
            result.W *= num3;
        }

        public void Conjugate()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }

        public static Quaternion Conjugate(Quaternion value)
        {
            Quaternion quaternion;
            quaternion.X = -value.X;
            quaternion.Y = -value.Y;
            quaternion.Z = -value.Z;
            quaternion.W = value.W;
            return quaternion;
        }

        public static void Conjugate(ref Quaternion value, out Quaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
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
            quaternion1.X = -quaternion.X;
            quaternion1.Y = -quaternion.Y;
            quaternion1.Z = -quaternion.Z;
            quaternion1.W = -quaternion.W;
            return quaternion1;
        }

        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            result.X = -quaternion.X;
            result.Y = -quaternion.Y;
            result.Z = -quaternion.Z;
            result.W = -quaternion.W;
        }

        public static Quaternion Sub(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.X = quaternion1.X - quaternion2.X;
            quaternion.Y = quaternion1.Y - quaternion2.Y;
            quaternion.Z = quaternion1.Z - quaternion2.Z;
            quaternion.W = quaternion1.W - quaternion2.W;
            return quaternion;
        }

        public static void Sub(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            result.X = quaternion1.X - quaternion2.X;
            result.Y = quaternion1.Y - quaternion2.Y;
            result.Z = quaternion1.Z - quaternion2.Z;
            result.W = quaternion1.W - quaternion2.W;
        }

        public static Vector3 Rotate(Quaternion rotation, Vector3 vector3)
        {
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
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
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            result.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) vector3.x + ((double) num7 - (double) num12) * (double) vector3.y +
                ((double) num8 + (double) num11) * (double) vector3.z);
            result.y = (float) (((double) num7 + (double) num12) * (double) vector3.x + (1.0 - ((double) num4 + (double) num6)) * (double) vector3.y +
                ((double) num9 - (double) num10) * (double) vector3.z);
            result.z = (float) (((double) num8 - (double) num11) * (double) vector3.x + ((double) num9 + (double) num10) * (double) vector3.y +
                (1.0 - ((double) num4 + (double) num5)) * (double) vector3.z);
        }

        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x1 = quaternion1.X;
            float y1 = quaternion1.Y;
            float z1 = quaternion1.Z;
            float w1 = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            Quaternion quaternion;
            quaternion.X = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            quaternion.Y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            quaternion.Z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            quaternion.W = w1 * w2 - num4;
            return quaternion;
        }

        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            float x1 = quaternion1.X;
            float y1 = quaternion1.Y;
            float z1 = quaternion1.Z;
            float w1 = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            result.X = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            result.Y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            result.Z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            result.W = w1 * w2 - num4;
        }

        public static Quaternion operator -(Quaternion quaternion)
        {
            Quaternion quaternion1;
            quaternion1.X = -quaternion.X;
            quaternion1.Y = -quaternion.Y;
            quaternion1.Z = -quaternion.Z;
            quaternion1.W = -quaternion.W;
            return quaternion1;
        }

        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            if ((double) quaternion1.X == (double) quaternion2.X && (double) quaternion1.Y == (double) quaternion2.Y &&
                (double) quaternion1.Z == (double) quaternion2.Z)
                return (double) quaternion1.W == (double) quaternion2.W;
            return false;
        }

        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            if ((double) quaternion1.X == (double) quaternion2.X && (double) quaternion1.Y == (double) quaternion2.Y &&
                (double) quaternion1.Z == (double) quaternion2.Z)
                return (double) quaternion1.W != (double) quaternion2.W;
            return true;
        }

        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            Quaternion quaternion;
            quaternion.X = quaternion1.X - quaternion2.X;
            quaternion.Y = quaternion1.Y - quaternion2.Y;
            quaternion.Z = quaternion1.Z - quaternion2.Z;
            quaternion.W = quaternion1.W - quaternion2.W;
            return quaternion;
        }

        public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
        {
            float x1 = quaternion1.X;
            float y1 = quaternion1.Y;
            float z1 = quaternion1.Z;
            float w1 = quaternion1.W;
            float x2 = quaternion2.X;
            float y2 = quaternion2.Y;
            float z2 = quaternion2.Z;
            float w2 = quaternion2.W;
            float num1 = (float) ((double) y1 * (double) z2 - (double) z1 * (double) y2);
            float num2 = (float) ((double) z1 * (double) x2 - (double) x1 * (double) z2);
            float num3 = (float) ((double) x1 * (double) y2 - (double) y1 * (double) x2);
            float num4 = (float) ((double) x1 * (double) x2 + (double) y1 * (double) y2 + (double) z1 * (double) z2);
            Quaternion quaternion;
            quaternion.X = (float) ((double) x1 * (double) w2 + (double) x2 * (double) w1) + num1;
            quaternion.Y = (float) ((double) y1 * (double) w2 + (double) y2 * (double) w1) + num2;
            quaternion.Z = (float) ((double) z1 * (double) w2 + (double) z2 * (double) w1) + num3;
            quaternion.W = w1 * w2 - num4;
            return quaternion;
        }
        
        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            Vector3 vector3;
            vector3.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y + ((double) num8 + (double) num11) * (double) point.z);
            vector3.y = (float) (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y + ((double) num9 - (double) num10) * (double) point.z);
            vector3.z = (float) (((double) num8 - (double) num11) * (double) point.x + ((double) num9 + (double) num10) * (double) point.y + (1.0 - ((double) num4 + (double) num5)) * (double) point.z);
            return vector3;
        }
    }
}