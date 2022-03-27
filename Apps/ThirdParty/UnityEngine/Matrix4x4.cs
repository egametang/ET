using System;
using System.Globalization;

namespace UnityEngine
{
    [Serializable]
    public struct Matrix4x4: IEquatable<Matrix4x4>
    {
        public static readonly Matrix4x4 identity = new Matrix4x4(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;
        
        public bool isIdentity
        {
            get
            {
                return this.m00 == 1f && this.m11 == 1f && this.m22 == 1f && this.m33 == 1f && // Check diagonal element first for early out.
                this.m12 == 0.0f && this.m13 == 0.0f && this.m13 == 0.0f && this.m21 == 0.0f  && this.m23 == 0.0f && this.m23 == 0.0f  && this.m31 == 0.0f  && this.m32 == 0.0f  && this.m33 == 0.0f;
            }
        }

        public Vector3 up
        {
            get
            {
                Vector3 vector3;
                vector3.x = this.m01;
                vector3.y = this.m11;
                vector3.z = this.m21;
                return vector3;
            }
            set
            {
                this.m01 = value.x;
                this.m11 = value.y;
                this.m21 = value.z;
            }
        }

        public Vector3 down
        {
            get
            {
                Vector3 vector3;
                vector3.x = -this.m01;
                vector3.y = -this.m11;
                vector3.z = -this.m21;
                return vector3;
            }
            set
            {
                this.m01 = -value.x;
                this.m11 = -value.y;
                this.m21 = -value.z;
            }
        }

        public Vector3 right
        {
            get
            {
                Vector3 vector3;
                vector3.x = this.m00;
                vector3.y = this.m10;
                vector3.z = this.m20;
                return vector3;
            }
            set
            {
                this.m00 = value.x;
                this.m10 = value.y;
                this.m20 = value.z;
            }
        }

        public Vector3 left
        {
            get
            {
                Vector3 vector3;
                vector3.x = -this.m00;
                vector3.y = -this.m10;
                vector3.z = -this.m20;
                return vector3;
            }
            set
            {
                this.m00 = -value.x;
                this.m10 = -value.y;
                this.m20 = -value.z;
            }
        }

        public Vector3 forward
        {
            get
            {
                Vector3 vector3;
                vector3.x = -this.m02;
                vector3.y = -this.m12;
                vector3.z = -this.m22;
                return vector3;
            }
            set
            {
                this.m02 = -value.x;
                this.m12 = -value.y;
                this.m22 = -value.z;
            }
        }

        public Vector3 back
        {
            get
            {
                Vector3 vector3;
                vector3.x = this.m02;
                vector3.y = this.m12;
                vector3.z = this.m22;
                return vector3;
            }
            set
            {
                this.m02 = value.x;
                this.m12 = value.y;
                this.m22 = value.z;
            }
        }

        public unsafe float this[int row, int col]
        {
            get
            {
                fixed (float* numPtr = &this.m00)
                    return numPtr[row * 4 + col];
            }
            set
            {
                fixed (float* numPtr = &this.m00)
                    numPtr[row * 4 + col] = value;
            }
        }

        public unsafe float this[int index]
        {
            get
            {
                fixed (float* numPtr = &this.m00)
                    return numPtr[index];
            }
            set
            {
                fixed (float* numPtr = &this.m00)
                    numPtr[index] = value;
            }
        }

        public Vector4 GetRow(int index)
        {
            Vector4 vector4;
            vector4.x = this[index, 0];
            vector4.y = this[index, 1];
            vector4.z = this[index, 2];
            vector4.w = this[index, 3];
            return vector4;
        }

        public void SetRow(int index, Vector4 value)
        {
            this[index, 0] = value.x;
            this[index, 1] = value.y;
            this[index, 2] = value.z;
            this[index, 3] = value.w;
        }

        public Vector4 GetColumn(int index)
        {
            Vector4 vector4;
            vector4.x = this[0, index];
            vector4.y = this[1, index];
            vector4.z = this[2, index];
            vector4.w = this[3, index];
            return vector4;
        }

        public void SetColumn(int index, Vector4 value)
        {
            this[0, index] = value.x;
            this[1, index] = value.y;
            this[2, index] = value.z;
            this[3, index] = value.w;
        }

        public Matrix4x4(
            float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            this.m00 = m00;
            this.m01 = m01;
            this.m02 = m02;
            this.m03 = m03;
            this.m10 = m10;
            this.m11 = m11;
            this.m12 = m12;
            this.m13 = m13;
            this.m20 = m20;
            this.m21 = m21;
            this.m22 = m22;
            this.m23 = m23;
            this.m30 = m30;
            this.m31 = m31;
            this.m32 = m32;
            this.m33 = m33;
        }

        public static Matrix4x4 CreateTranslation(Vector3 position)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = 1f;
            matrix44.m01 = 0.0f;
            matrix44.m02 = 0.0f;
            matrix44.m03 = position.x;
            matrix44.m10 = 0.0f;
            matrix44.m11 = 1f;
            matrix44.m12 = 0.0f;
            matrix44.m13 = position.y;
            matrix44.m20 = 0.0f;
            matrix44.m21 = 0.0f;
            matrix44.m22 = 1f;
            matrix44.m23 = position.z;
            matrix44.m30 = 0.0f;
            matrix44.m31 = 0.0f;
            matrix44.m32 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public Matrix4x4 inverse
        {
            get
            {
                return Matrix4x4.Invert(this);
            }
        }

        public static void CreateTranslation(ref Vector3 position, out Matrix4x4 matrix)
        {
            matrix.m00 = 1f;
            matrix.m01 = 0.0f;
            matrix.m02 = 0.0f;
            matrix.m03 = position.x;
            matrix.m10 = 0.0f;
            matrix.m11 = 1f;
            matrix.m12 = 0.0f;
            matrix.m13 = position.y;
            matrix.m20 = 0.0f;
            matrix.m21 = 0.0f;
            matrix.m22 = 1f;
            matrix.m23 = position.z;
            matrix.m30 = 0.0f;
            matrix.m31 = 0.0f;
            matrix.m32 = 0.0f;
            matrix.m33 = 1f;
        }

        public static Matrix4x4 CreateScale(Vector3 scales)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = scales.x;
            matrix44.m01 = 0.0f;
            matrix44.m02 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m10 = 0.0f;
            matrix44.m11 = scales.y;
            matrix44.m12 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m20 = 0.0f;
            matrix44.m21 = 0.0f;
            matrix44.m22 = scales.z;
            matrix44.m23 = 0.0f;
            matrix44.m30 = 0.0f;
            matrix44.m31 = 0.0f;
            matrix44.m32 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            Matrix4x4 m1 = CreateTranslation(pos);
            Matrix4x4 m2 = CreateFromQuaternion(q);
            Matrix4x4 m3 = CreateScale(s);
            return m1 * m2 * m3;
        }

        public static Matrix4x4 Scale(Vector3 scales)
        {
            Matrix4x4 m1;
            CreateScale(ref scales, out m1);
            return m1;
        }

        public static void CreateScale(ref Vector3 scales, out Matrix4x4 matrix)
        {
            matrix.m00 = scales.x;
            matrix.m01 = 0.0f;
            matrix.m02 = 0.0f;
            matrix.m03 = 0.0f;
            matrix.m10 = 0.0f;
            matrix.m11 = scales.y;
            matrix.m12 = 0.0f;
            matrix.m13 = 0.0f;
            matrix.m20 = 0.0f;
            matrix.m21 = 0.0f;
            matrix.m22 = scales.z;
            matrix.m23 = 0.0f;
            matrix.m30 = 0.0f;
            matrix.m31 = 0.0f;
            matrix.m32 = 0.0f;
            matrix.m33 = 1f;
        }

        public static Matrix4x4 CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = 2f / width;
            matrix44.m10 = matrix44.m20 = matrix44.m30 = 0.0f;
            matrix44.m11 = 2f / height;
            matrix44.m01 = matrix44.m21 = matrix44.m31 = 0.0f;
            matrix44.m22 = (float) (1.0 / ((double) zNearPlane - (double) zFarPlane));
            matrix44.m02 = matrix44.m12 = matrix44.m32 = 0.0f;
            matrix44.m03 = matrix44.m13 = 0.0f;
            matrix44.m23 = zNearPlane / (zNearPlane - zFarPlane);
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix4x4 matrix)
        {
            matrix.m00 = 2f / width;
            matrix.m10 = matrix.m20 = matrix.m30 = 0.0f;
            matrix.m11 = 2f / height;
            matrix.m01 = matrix.m21 = matrix.m31 = 0.0f;
            matrix.m22 = (float) (1.0 / ((double) zNearPlane - (double) zFarPlane));
            matrix.m02 = matrix.m12 = matrix.m32 = 0.0f;
            matrix.m03 = matrix.m13 = 0.0f;
            matrix.m23 = zNearPlane / (zNearPlane - zFarPlane);
            matrix.m33 = 1f;
        }

        public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            Vector3 vector3_1 = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3_1));
            Vector3 vector1 = Vector3.Cross(vector3_1, vector3_2);
            Matrix4x4 matrix44;
            matrix44.m00 = vector3_2.x;
            matrix44.m10 = vector1.x;
            matrix44.m20 = vector3_1.x;
            matrix44.m30 = 0.0f;
            matrix44.m01 = vector3_2.y;
            matrix44.m11 = vector1.y;
            matrix44.m21 = vector3_1.y;
            matrix44.m31 = 0.0f;
            matrix44.m02 = vector3_2.z;
            matrix44.m12 = vector1.z;
            matrix44.m22 = vector3_1.z;
            matrix44.m32 = 0.0f;
            matrix44.m03 = -Vector3.Dot(vector3_2, cameraPosition);
            matrix44.m13 = -Vector3.Dot(vector1, cameraPosition);
            matrix44.m23 = -Vector3.Dot(vector3_1, cameraPosition);
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix4x4 matrix)
        {
            Vector3 vector3_1 = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3_1));
            Vector3 vector1 = Vector3.Cross(vector3_1, vector3_2);
            matrix.m00 = vector3_2.x;
            matrix.m10 = vector1.x;
            matrix.m20 = vector3_1.x;
            matrix.m30 = 0.0f;
            matrix.m01 = vector3_2.y;
            matrix.m11 = vector1.y;
            matrix.m21 = vector3_1.y;
            matrix.m31 = 0.0f;
            matrix.m02 = vector3_2.z;
            matrix.m12 = vector1.z;
            matrix.m22 = vector3_1.z;
            matrix.m32 = 0.0f;
            matrix.m03 = -Vector3.Dot(vector3_2, cameraPosition);
            matrix.m13 = -Vector3.Dot(vector1, cameraPosition);
            matrix.m23 = -Vector3.Dot(vector3_1, cameraPosition);
            matrix.m33 = 1f;
        }

        public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
        {
            float num1 = quaternion.x * quaternion.x;
            float num2 = quaternion.y * quaternion.y;
            float num3 = quaternion.z * quaternion.z;
            float num4 = quaternion.x * quaternion.y;
            float num5 = quaternion.z * quaternion.w;
            float num6 = quaternion.z * quaternion.x;
            float num7 = quaternion.y * quaternion.w;
            float num8 = quaternion.y * quaternion.z;
            float num9 = quaternion.x * quaternion.w;
            Matrix4x4 matrix44;
            matrix44.m00 = (float) (1.0 - 2.0 * ((double) num2 + (double) num3));
            matrix44.m10 = (float) (2.0 * ((double) num4 + (double) num5));
            matrix44.m20 = (float) (2.0 * ((double) num6 - (double) num7));
            matrix44.m30 = 0.0f;
            matrix44.m01 = (float) (2.0 * ((double) num4 - (double) num5));
            matrix44.m11 = (float) (1.0 - 2.0 * ((double) num3 + (double) num1));
            matrix44.m21 = (float) (2.0 * ((double) num8 + (double) num9));
            matrix44.m31 = 0.0f;
            matrix44.m02 = (float) (2.0 * ((double) num6 + (double) num7));
            matrix44.m12 = (float) (2.0 * ((double) num8 - (double) num9));
            matrix44.m22 = (float) (1.0 - 2.0 * ((double) num2 + (double) num1));
            matrix44.m32 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m23 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix4x4 matrix)
        {
            float num1 = quaternion.x * quaternion.x;
            float num2 = quaternion.y * quaternion.y;
            float num3 = quaternion.z * quaternion.z;
            float num4 = quaternion.x * quaternion.y;
            float num5 = quaternion.z * quaternion.w;
            float num6 = quaternion.z * quaternion.x;
            float num7 = quaternion.y * quaternion.w;
            float num8 = quaternion.y * quaternion.z;
            float num9 = quaternion.x * quaternion.w;
            matrix.m00 = (float) (1.0 - 2.0 * ((double) num2 + (double) num3));
            matrix.m10 = (float) (2.0 * ((double) num4 + (double) num5));
            matrix.m20 = (float) (2.0 * ((double) num6 - (double) num7));
            matrix.m30 = 0.0f;
            matrix.m01 = (float) (2.0 * ((double) num4 - (double) num5));
            matrix.m11 = (float) (1.0 - 2.0 * ((double) num3 + (double) num1));
            matrix.m21 = (float) (2.0 * ((double) num8 + (double) num9));
            matrix.m31 = 0.0f;
            matrix.m02 = (float) (2.0 * ((double) num6 + (double) num7));
            matrix.m12 = (float) (2.0 * ((double) num8 - (double) num9));
            matrix.m22 = (float) (1.0 - 2.0 * ((double) num2 + (double) num1));
            matrix.m32 = 0.0f;
            matrix.m03 = 0.0f;
            matrix.m13 = 0.0f;
            matrix.m23 = 0.0f;
            matrix.m33 = 1f;
        }

        public static Matrix4x4 CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion result;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out result);
            return Matrix4x4.CreateFromQuaternion(result);
        }

        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix4x4 result)
        {
            Quaternion result1;
            Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out result1);
            result = Matrix4x4.CreateFromQuaternion(result1);
        }

        public static Matrix4x4 CreateRotationX(float radians)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            Matrix4x4 matrix44;
            matrix44.m00 = 1f;
            matrix44.m10 = 0.0f;
            matrix44.m20 = 0.0f;
            matrix44.m30 = 0.0f;
            matrix44.m01 = 0.0f;
            matrix44.m11 = num1;
            matrix44.m21 = num2;
            matrix44.m31 = 0.0f;
            matrix44.m02 = 0.0f;
            matrix44.m12 = -num2;
            matrix44.m22 = num1;
            matrix44.m32 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m23 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateRotationX(float radians, out Matrix4x4 result)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            result.m00 = 1f;
            result.m10 = 0.0f;
            result.m20 = 0.0f;
            result.m30 = 0.0f;
            result.m01 = 0.0f;
            result.m11 = num1;
            result.m21 = num2;
            result.m31 = 0.0f;
            result.m02 = 0.0f;
            result.m12 = -num2;
            result.m22 = num1;
            result.m32 = 0.0f;
            result.m03 = 0.0f;
            result.m13 = 0.0f;
            result.m23 = 0.0f;
            result.m33 = 1f;
        }

        public static Matrix4x4 CreateRotationY(float radians)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            Matrix4x4 matrix44;
            matrix44.m00 = num1;
            matrix44.m10 = 0.0f;
            matrix44.m20 = -num2;
            matrix44.m30 = 0.0f;
            matrix44.m01 = 0.0f;
            matrix44.m11 = 1f;
            matrix44.m21 = 0.0f;
            matrix44.m31 = 0.0f;
            matrix44.m02 = num2;
            matrix44.m12 = 0.0f;
            matrix44.m22 = num1;
            matrix44.m32 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m23 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateRotationY(float radians, out Matrix4x4 result)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            result.m00 = num1;
            result.m10 = 0.0f;
            result.m20 = -num2;
            result.m30 = 0.0f;
            result.m01 = 0.0f;
            result.m11 = 1f;
            result.m21 = 0.0f;
            result.m31 = 0.0f;
            result.m02 = num2;
            result.m12 = 0.0f;
            result.m22 = num1;
            result.m32 = 0.0f;
            result.m03 = 0.0f;
            result.m13 = 0.0f;
            result.m23 = 0.0f;
            result.m33 = 1f;
        }

        public static Matrix4x4 CreateRotationZ(float radians)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            Matrix4x4 matrix44;
            matrix44.m00 = num1;
            matrix44.m10 = num2;
            matrix44.m20 = 0.0f;
            matrix44.m30 = 0.0f;
            matrix44.m01 = -num2;
            matrix44.m11 = num1;
            matrix44.m21 = 0.0f;
            matrix44.m31 = 0.0f;
            matrix44.m02 = 0.0f;
            matrix44.m12 = 0.0f;
            matrix44.m22 = 1f;
            matrix44.m32 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m23 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateRotationZ(float radians, out Matrix4x4 result)
        {
            float num1 = (float) Math.Cos((double) radians);
            float num2 = (float) Math.Sin((double) radians);
            result.m00 = num1;
            result.m10 = num2;
            result.m20 = 0.0f;
            result.m30 = 0.0f;
            result.m01 = -num2;
            result.m11 = num1;
            result.m21 = 0.0f;
            result.m31 = 0.0f;
            result.m02 = 0.0f;
            result.m12 = 0.0f;
            result.m22 = 1f;
            result.m32 = 0.0f;
            result.m03 = 0.0f;
            result.m13 = 0.0f;
            result.m23 = 0.0f;
            result.m33 = 1f;
        }

        public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, float angle)
        {
            float x = axis.x;
            float y = axis.y;
            float z = axis.z;
            float num1 = (float) Math.Sin((double) angle);
            float num2 = (float) Math.Cos((double) angle);
            float num3 = x * x;
            float num4 = y * y;
            float num5 = z * z;
            float num6 = x * y;
            float num7 = x * z;
            float num8 = y * z;
            Matrix4x4 matrix44;
            matrix44.m00 = num3 + num2 * (1f - num3);
            matrix44.m10 = (float) ((double) num6 - (double) num2 * (double) num6 + (double) num1 * (double) z);
            matrix44.m20 = (float) ((double) num7 - (double) num2 * (double) num7 - (double) num1 * (double) y);
            matrix44.m30 = 0.0f;
            matrix44.m01 = (float) ((double) num6 - (double) num2 * (double) num6 - (double) num1 * (double) z);
            matrix44.m11 = num4 + num2 * (1f - num4);
            matrix44.m21 = (float) ((double) num8 - (double) num2 * (double) num8 + (double) num1 * (double) x);
            matrix44.m31 = 0.0f;
            matrix44.m02 = (float) ((double) num7 - (double) num2 * (double) num7 + (double) num1 * (double) y);
            matrix44.m12 = (float) ((double) num8 - (double) num2 * (double) num8 - (double) num1 * (double) x);
            matrix44.m22 = num5 + num2 * (1f - num5);
            matrix44.m32 = 0.0f;
            matrix44.m03 = 0.0f;
            matrix44.m13 = 0.0f;
            matrix44.m23 = 0.0f;
            matrix44.m33 = 1f;
            return matrix44;
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix4x4 result)
        {
            float x = axis.x;
            float y = axis.y;
            float z = axis.z;
            float num1 = (float) Math.Sin((double) angle);
            float num2 = (float) Math.Cos((double) angle);
            float num3 = x * x;
            float num4 = y * y;
            float num5 = z * z;
            float num6 = x * y;
            float num7 = x * z;
            float num8 = y * z;
            result.m00 = num3 + num2 * (1f - num3);
            result.m10 = (float) ((double) num6 - (double) num2 * (double) num6 + (double) num1 * (double) z);
            result.m20 = (float) ((double) num7 - (double) num2 * (double) num7 - (double) num1 * (double) y);
            result.m30 = 0.0f;
            result.m01 = (float) ((double) num6 - (double) num2 * (double) num6 - (double) num1 * (double) z);
            result.m11 = num4 + num2 * (1f - num4);
            result.m21 = (float) ((double) num8 - (double) num2 * (double) num8 + (double) num1 * (double) x);
            result.m31 = 0.0f;
            result.m02 = (float) ((double) num7 - (double) num2 * (double) num7 + (double) num1 * (double) y);
            result.m12 = (float) ((double) num8 - (double) num2 * (double) num8 - (double) num1 * (double) x);
            result.m22 = num5 + num2 * (1f - num5);
            result.m32 = 0.0f;
            result.m03 = 0.0f;
            result.m13 = 0.0f;
            result.m23 = 0.0f;
            result.m33 = 1f;
        }

        public void Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            Matrix4x4 identity = Matrix4x4.identity;
            float num1 = 1f / (float) Math.Sqrt((double) this[0, 0] * (double) this[0, 0] + (double) this[1, 0] * (double) this[1, 0] +
                                                (double) this[2, 0] * (double) this[2, 0]);
            identity[0, 0] = this[0, 0] * num1;
            identity[1, 0] = this[1, 0] * num1;
            identity[2, 0] = this[2, 0] * num1;
            float num2 = (float) ((double) identity[0, 0] * (double) this[0, 1] + (double) identity[1, 0] * (double) this[1, 1] +
                (double) identity[2, 0] * (double) this[2, 1]);
            identity[0, 1] = this[0, 1] - num2 * identity[0, 0];
            identity[1, 1] = this[1, 1] - num2 * identity[1, 0];
            identity[2, 1] = this[2, 1] - num2 * identity[2, 0];
            float num3 = 1f / (float) Math.Sqrt((double) identity[0, 1] * (double) identity[0, 1] +
                                                (double) identity[1, 1] * (double) identity[1, 1] +
                                                (double) identity[2, 1] * (double) identity[2, 1]);
            identity[0, 1] *= num3;
            identity[1, 1] *= num3;
            identity[2, 1] *= num3;
            float num4 = (float) ((double) identity[0, 0] * (double) this[0, 2] + (double) identity[1, 0] * (double) this[1, 2] +
                (double) identity[2, 0] * (double) this[2, 2]);
            identity[0, 2] = this[0, 2] - num4 * identity[0, 0];
            identity[1, 2] = this[1, 2] - num4 * identity[1, 0];
            identity[2, 2] = this[2, 2] - num4 * identity[2, 0];
            float num5 = (float) ((double) identity[0, 1] * (double) this[0, 2] + (double) identity[1, 1] * (double) this[1, 2] +
                (double) identity[2, 1] * (double) this[2, 2]);
            identity[0, 2] -= num5 * identity[0, 1];
            identity[1, 2] -= num5 * identity[1, 1];
            identity[2, 2] -= num5 * identity[2, 1];
            float num6 = 1f / (float) Math.Sqrt((double) identity[0, 2] * (double) identity[0, 2] +
                                                (double) identity[1, 2] * (double) identity[1, 2] +
                                                (double) identity[2, 2] * (double) identity[2, 2]);
            identity[0, 2] *= num6;
            identity[1, 2] *= num6;
            identity[2, 2] *= num6;
            if ((double) identity[0, 0] * (double) identity[1, 1] * (double) identity[2, 2] +
                (double) identity[0, 1] * (double) identity[1, 2] * (double) identity[2, 0] +
                (double) identity[0, 2] * (double) identity[1, 0] * (double) identity[2, 1] -
                (double) identity[0, 2] * (double) identity[1, 1] * (double) identity[2, 0] -
                (double) identity[0, 1] * (double) identity[1, 0] * (double) identity[2, 2] -
                (double) identity[0, 0] * (double) identity[1, 2] * (double) identity[2, 1] < 0.0)
            {
                for (int index1 = 0; index1 < 3; ++index1)
                {
                    for (int index2 = 0; index2 < 3; ++index2)
                        identity[index1, index2] = -identity[index1, index2];
                }
            }

            scale =
                    new
                            Vector3((float) ((double) identity[0, 0] * (double) this[0, 0] + (double) identity[1, 0] * (double) this[1, 0] + (double) identity[2, 0] * (double) this[2, 0]),
                                    (float) ((double) identity[0, 1] * (double) this[0, 1] + (double) identity[1, 1] * (double) this[1, 1] +
                                        (double) identity[2, 1] * (double) this[2, 1]),
                                    (float) ((double) identity[0, 2] * (double) this[0, 2] + (double) identity[1, 2] * (double) this[1, 2] +
                                        (double) identity[2, 2] * (double) this[2, 2]));
            rotation = Quaternion.CreateFromRotationMatrix(identity);
            translation = new Vector3(this[0, 3], this[1, 3], this[2, 3]);
        }

        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return
                    string.Format((IFormatProvider) currentCulture, "{0}, {1}, {2}, {3}; ",
                                  (object) this.m00.ToString((IFormatProvider) currentCulture),
                                  (object) this.m01.ToString((IFormatProvider) currentCulture),
                                  (object) this.m02.ToString((IFormatProvider) currentCulture),
                                  (object) this.m03.ToString((IFormatProvider) currentCulture)) +
                    string.Format((IFormatProvider) currentCulture, "{0}, {1}, {2}, {3}; ",
                                  (object) this.m10.ToString((IFormatProvider) currentCulture),
                                  (object) this.m11.ToString((IFormatProvider) currentCulture),
                                  (object) this.m12.ToString((IFormatProvider) currentCulture),
                                  (object) this.m13.ToString((IFormatProvider) currentCulture)) +
                    string.Format((IFormatProvider) currentCulture, "{0}, {1}, {2}, {3}; ",
                                  (object) this.m20.ToString((IFormatProvider) currentCulture),
                                  (object) this.m21.ToString((IFormatProvider) currentCulture),
                                  (object) this.m22.ToString((IFormatProvider) currentCulture),
                                  (object) this.m23.ToString((IFormatProvider) currentCulture)) + string.Format((IFormatProvider) currentCulture,
                                                                                                                "{0}, {1}, {2}, {3}",
                                                                                                                (object) this.m30
                                                                                                                        .ToString((IFormatProvider)
                                                                                                                                  currentCulture),
                                                                                                                (object) this.m31
                                                                                                                        .ToString((IFormatProvider)
                                                                                                                                  currentCulture),
                                                                                                                (object) this.m32
                                                                                                                        .ToString((IFormatProvider)
                                                                                                                                  currentCulture),
                                                                                                                (object) this.m33
                                                                                                                        .ToString((IFormatProvider)
                                                                                                                                  currentCulture));
        }

        public bool Equals(Matrix4x4 other)
        {
            if ((double) this.m00 == (double) other.m00 && (double) this.m11 == (double) other.m11 &&
                ((double) this.m22 == (double) other.m22 && (double) this.m33 == (double) other.m33) &&
                ((double) this.m01 == (double) other.m01 && (double) this.m02 == (double) other.m02 &&
                    ((double) this.m03 == (double) other.m03 && (double) this.m10 == (double) other.m10)) &&
                ((double) this.m12 == (double) other.m12 && (double) this.m13 == (double) other.m13 &&
                    ((double) this.m20 == (double) other.m20 && (double) this.m21 == (double) other.m21) &&
                    ((double) this.m23 == (double) other.m23 && (double) this.m30 == (double) other.m30 && (double) this.m31 == (double) other.m31)))
                return (double) this.m32 == (double) other.m32;
            return false;
        }

        public override bool Equals(object obj)
        {
            bool flag = false;
            if (obj is Matrix4x4)
                flag = this.Equals((Matrix4x4) obj);
            return flag;
        }

        public override int GetHashCode()
        {
            return this.m00.GetHashCode() + this.m01.GetHashCode() + this.m02.GetHashCode() + this.m03.GetHashCode() + this.m10.GetHashCode() +
                    this.m11.GetHashCode() + this.m12.GetHashCode() + this.m13.GetHashCode() + this.m20.GetHashCode() + this.m21.GetHashCode() +
                    this.m22.GetHashCode() + this.m23.GetHashCode() + this.m30.GetHashCode() + this.m31.GetHashCode() + this.m32.GetHashCode() +
                    this.m33.GetHashCode();
        }

        public static Matrix4x4 Transpose(Matrix4x4 matrix)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = matrix.m00;
            matrix44.m01 = matrix.m10;
            matrix44.m02 = matrix.m20;
            matrix44.m03 = matrix.m30;
            matrix44.m10 = matrix.m01;
            matrix44.m11 = matrix.m11;
            matrix44.m12 = matrix.m21;
            matrix44.m13 = matrix.m31;
            matrix44.m20 = matrix.m02;
            matrix44.m21 = matrix.m12;
            matrix44.m22 = matrix.m22;
            matrix44.m23 = matrix.m32;
            matrix44.m30 = matrix.m03;
            matrix44.m31 = matrix.m13;
            matrix44.m32 = matrix.m23;
            matrix44.m33 = matrix.m33;
            return matrix44;
        }

        public static void Transpose(ref Matrix4x4 matrix, out Matrix4x4 result)
        {
            result.m00 = matrix.m00;
            result.m01 = matrix.m10;
            result.m02 = matrix.m20;
            result.m03 = matrix.m30;
            result.m10 = matrix.m01;
            result.m11 = matrix.m11;
            result.m12 = matrix.m21;
            result.m13 = matrix.m31;
            result.m20 = matrix.m02;
            result.m21 = matrix.m12;
            result.m22 = matrix.m22;
            result.m23 = matrix.m32;
            result.m30 = matrix.m03;
            result.m31 = matrix.m13;
            result.m32 = matrix.m23;
            result.m33 = matrix.m33;
        }

        public float Determinant()
        {
            float m00 = this.m00;
            float m10 = this.m10;
            float m20 = this.m20;
            float m30 = this.m30;
            float m01 = this.m01;
            float m11 = this.m11;
            float m21 = this.m21;
            float m31 = this.m31;
            float m02 = this.m02;
            float m12 = this.m12;
            float m22 = this.m22;
            float m32 = this.m32;
            float m03 = this.m03;
            float m13 = this.m13;
            float m23 = this.m23;
            float m33 = this.m33;
            float num1 = (float) ((double) m22 * (double) m33 - (double) m32 * (double) m23);
            float num2 = (float) ((double) m12 * (double) m33 - (double) m32 * (double) m13);
            float num3 = (float) ((double) m12 * (double) m23 - (double) m22 * (double) m13);
            float num4 = (float) ((double) m02 * (double) m33 - (double) m32 * (double) m03);
            float num5 = (float) ((double) m02 * (double) m23 - (double) m22 * (double) m03);
            float num6 = (float) ((double) m02 * (double) m13 - (double) m12 * (double) m03);
            return (float) ((double) m00 * ((double) m11 * (double) num1 - (double) m21 * (double) num2 + (double) m31 * (double) num3) -
                (double) m10 * ((double) m01 * (double) num1 - (double) m21 * (double) num4 + (double) m31 * (double) num5) +
                (double) m20 * ((double) m01 * (double) num2 - (double) m11 * (double) num4 + (double) m31 * (double) num6) -
                (double) m30 * ((double) m01 * (double) num3 - (double) m11 * (double) num5 + (double) m21 * (double) num6));
        }

        public static Matrix4x4 Invert(Matrix4x4 matrix)
        {
            float m00 = matrix.m00;
            float m10 = matrix.m10;
            float m20 = matrix.m20;
            float m30 = matrix.m30;
            float m01 = matrix.m01;
            float m11 = matrix.m11;
            float m21 = matrix.m21;
            float m31 = matrix.m31;
            float m02 = matrix.m02;
            float m12 = matrix.m12;
            float m22 = matrix.m22;
            float m32 = matrix.m32;
            float m03 = matrix.m03;
            float m13 = matrix.m13;
            float m23 = matrix.m23;
            float m33 = matrix.m33;
            float num1 = (float) ((double) m22 * (double) m33 - (double) m32 * (double) m23);
            float num2 = (float) ((double) m12 * (double) m33 - (double) m32 * (double) m13);
            float num3 = (float) ((double) m12 * (double) m23 - (double) m22 * (double) m13);
            float num4 = (float) ((double) m02 * (double) m33 - (double) m32 * (double) m03);
            float num5 = (float) ((double) m02 * (double) m23 - (double) m22 * (double) m03);
            float num6 = (float) ((double) m02 * (double) m13 - (double) m12 * (double) m03);
            float num7 = (float) ((double) m11 * (double) num1 - (double) m21 * (double) num2 + (double) m31 * (double) num3);
            float num8 = (float) -((double) m01 * (double) num1 - (double) m21 * (double) num4 + (double) m31 * (double) num5);
            float num9 = (float) ((double) m01 * (double) num2 - (double) m11 * (double) num4 + (double) m31 * (double) num6);
            float num10 = (float) -((double) m01 * (double) num3 - (double) m11 * (double) num5 + (double) m21 * (double) num6);
            float num11 = (float) (1.0 / ((double) m00 * (double) num7 + (double) m10 * (double) num8 + (double) m20 * (double) num9 +
                (double) m30 * (double) num10));
            Matrix4x4 matrix44;
            matrix44.m00 = num7 * num11;
            matrix44.m01 = num8 * num11;
            matrix44.m02 = num9 * num11;
            matrix44.m03 = num10 * num11;
            matrix44.m10 = (float) -((double) m10 * (double) num1 - (double) m20 * (double) num2 + (double) m30 * (double) num3) * num11;
            matrix44.m11 = (float) ((double) m00 * (double) num1 - (double) m20 * (double) num4 + (double) m30 * (double) num5) * num11;
            matrix44.m12 = (float) -((double) m00 * (double) num2 - (double) m10 * (double) num4 + (double) m30 * (double) num6) * num11;
            matrix44.m13 = (float) ((double) m00 * (double) num3 - (double) m10 * (double) num5 + (double) m20 * (double) num6) * num11;
            float num12 = (float) ((double) m21 * (double) m33 - (double) m31 * (double) m23);
            float num13 = (float) ((double) m11 * (double) m33 - (double) m31 * (double) m13);
            float num14 = (float) ((double) m11 * (double) m23 - (double) m21 * (double) m13);
            float num15 = (float) ((double) m01 * (double) m33 - (double) m31 * (double) m03);
            float num16 = (float) ((double) m01 * (double) m23 - (double) m21 * (double) m03);
            float num17 = (float) ((double) m01 * (double) m13 - (double) m11 * (double) m03);
            matrix44.m20 = (float) ((double) m10 * (double) num12 - (double) m20 * (double) num13 + (double) m30 * (double) num14) * num11;
            matrix44.m21 = (float) -((double) m00 * (double) num12 - (double) m20 * (double) num15 + (double) m30 * (double) num16) * num11;
            matrix44.m22 = (float) ((double) m00 * (double) num13 - (double) m10 * (double) num15 + (double) m30 * (double) num17) * num11;
            matrix44.m23 = (float) -((double) m00 * (double) num14 - (double) m10 * (double) num16 + (double) m20 * (double) num17) * num11;
            float num18 = (float) ((double) m21 * (double) m32 - (double) m31 * (double) m22);
            float num19 = (float) ((double) m11 * (double) m32 - (double) m31 * (double) m12);
            float num20 = (float) ((double) m11 * (double) m22 - (double) m21 * (double) m12);
            float num21 = (float) ((double) m01 * (double) m32 - (double) m31 * (double) m02);
            float num22 = (float) ((double) m01 * (double) m22 - (double) m21 * (double) m02);
            float num23 = (float) ((double) m01 * (double) m12 - (double) m11 * (double) m02);
            matrix44.m30 = (float) -((double) m10 * (double) num18 - (double) m20 * (double) num19 + (double) m30 * (double) num20) * num11;
            matrix44.m31 = (float) ((double) m00 * (double) num18 - (double) m20 * (double) num21 + (double) m30 * (double) num22) * num11;
            matrix44.m32 = (float) -((double) m00 * (double) num19 - (double) m10 * (double) num21 + (double) m30 * (double) num23) * num11;
            matrix44.m33 = (float) ((double) m00 * (double) num20 - (double) m10 * (double) num22 + (double) m20 * (double) num23) * num11;
            return matrix44;
        }

        public static void Invert(ref Matrix4x4 matrix, out Matrix4x4 result)
        {
            float m00 = matrix.m00;
            float m10 = matrix.m10;
            float m20 = matrix.m20;
            float m30 = matrix.m30;
            float m01 = matrix.m01;
            float m11 = matrix.m11;
            float m21 = matrix.m21;
            float m31 = matrix.m31;
            float m02 = matrix.m02;
            float m12 = matrix.m12;
            float m22 = matrix.m22;
            float m32 = matrix.m32;
            float m03 = matrix.m03;
            float m13 = matrix.m13;
            float m23 = matrix.m23;
            float m33 = matrix.m33;
            float num1 = (float) ((double) m22 * (double) m33 - (double) m32 * (double) m23);
            float num2 = (float) ((double) m12 * (double) m33 - (double) m32 * (double) m13);
            float num3 = (float) ((double) m12 * (double) m23 - (double) m22 * (double) m13);
            float num4 = (float) ((double) m02 * (double) m33 - (double) m32 * (double) m03);
            float num5 = (float) ((double) m02 * (double) m23 - (double) m22 * (double) m03);
            float num6 = (float) ((double) m02 * (double) m13 - (double) m12 * (double) m03);
            float num7 = (float) ((double) m11 * (double) num1 - (double) m21 * (double) num2 + (double) m31 * (double) num3);
            float num8 = (float) -((double) m01 * (double) num1 - (double) m21 * (double) num4 + (double) m31 * (double) num5);
            float num9 = (float) ((double) m01 * (double) num2 - (double) m11 * (double) num4 + (double) m31 * (double) num6);
            float num10 = (float) -((double) m01 * (double) num3 - (double) m11 * (double) num5 + (double) m21 * (double) num6);
            float num11 = (float) (1.0 / ((double) m00 * (double) num7 + (double) m10 * (double) num8 + (double) m20 * (double) num9 +
                (double) m30 * (double) num10));
            result.m00 = num7 * num11;
            result.m01 = num8 * num11;
            result.m02 = num9 * num11;
            result.m03 = num10 * num11;
            result.m10 = (float) -((double) m10 * (double) num1 - (double) m20 * (double) num2 + (double) m30 * (double) num3) * num11;
            result.m11 = (float) ((double) m00 * (double) num1 - (double) m20 * (double) num4 + (double) m30 * (double) num5) * num11;
            result.m12 = (float) -((double) m00 * (double) num2 - (double) m10 * (double) num4 + (double) m30 * (double) num6) * num11;
            result.m13 = (float) ((double) m00 * (double) num3 - (double) m10 * (double) num5 + (double) m20 * (double) num6) * num11;
            float num12 = (float) ((double) m21 * (double) m33 - (double) m31 * (double) m23);
            float num13 = (float) ((double) m11 * (double) m33 - (double) m31 * (double) m13);
            float num14 = (float) ((double) m11 * (double) m23 - (double) m21 * (double) m13);
            float num15 = (float) ((double) m01 * (double) m33 - (double) m31 * (double) m03);
            float num16 = (float) ((double) m01 * (double) m23 - (double) m21 * (double) m03);
            float num17 = (float) ((double) m01 * (double) m13 - (double) m11 * (double) m03);
            result.m20 = (float) ((double) m10 * (double) num12 - (double) m20 * (double) num13 + (double) m30 * (double) num14) * num11;
            result.m21 = (float) -((double) m00 * (double) num12 - (double) m20 * (double) num15 + (double) m30 * (double) num16) * num11;
            result.m22 = (float) ((double) m00 * (double) num13 - (double) m10 * (double) num15 + (double) m30 * (double) num17) * num11;
            result.m23 = (float) -((double) m00 * (double) num14 - (double) m10 * (double) num16 + (double) m20 * (double) num17) * num11;
            float num18 = (float) ((double) m21 * (double) m32 - (double) m31 * (double) m22);
            float num19 = (float) ((double) m11 * (double) m32 - (double) m31 * (double) m12);
            float num20 = (float) ((double) m11 * (double) m22 - (double) m21 * (double) m12);
            float num21 = (float) ((double) m01 * (double) m32 - (double) m31 * (double) m02);
            float num22 = (float) ((double) m01 * (double) m22 - (double) m21 * (double) m02);
            float num23 = (float) ((double) m01 * (double) m12 - (double) m11 * (double) m02);
            result.m30 = (float) -((double) m10 * (double) num18 - (double) m20 * (double) num19 + (double) m30 * (double) num20) * num11;
            result.m31 = (float) ((double) m00 * (double) num18 - (double) m20 * (double) num21 + (double) m30 * (double) num22) * num11;
            result.m32 = (float) -((double) m00 * (double) num19 - (double) m10 * (double) num21 + (double) m30 * (double) num23) * num11;
            result.m33 = (float) ((double) m00 * (double) num20 - (double) m10 * (double) num22 + (double) m20 * (double) num23) * num11;
        }

        public static Matrix4x4 Add(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = matrix1.m00 + matrix2.m00;
            matrix44.m01 = matrix1.m01 + matrix2.m01;
            matrix44.m02 = matrix1.m02 + matrix2.m02;
            matrix44.m03 = matrix1.m03 + matrix2.m03;
            matrix44.m10 = matrix1.m10 + matrix2.m10;
            matrix44.m11 = matrix1.m11 + matrix2.m11;
            matrix44.m12 = matrix1.m12 + matrix2.m12;
            matrix44.m13 = matrix1.m13 + matrix2.m13;
            matrix44.m20 = matrix1.m20 + matrix2.m20;
            matrix44.m21 = matrix1.m21 + matrix2.m21;
            matrix44.m22 = matrix1.m22 + matrix2.m22;
            matrix44.m23 = matrix1.m23 + matrix2.m23;
            matrix44.m30 = matrix1.m30 + matrix2.m30;
            matrix44.m31 = matrix1.m31 + matrix2.m31;
            matrix44.m32 = matrix1.m32 + matrix2.m32;
            matrix44.m33 = matrix1.m33 + matrix2.m33;
            return matrix44;
        }

        public static void Add(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
        {
            result.m00 = matrix1.m00 + matrix2.m00;
            result.m01 = matrix1.m01 + matrix2.m01;
            result.m02 = matrix1.m02 + matrix2.m02;
            result.m03 = matrix1.m03 + matrix2.m03;
            result.m10 = matrix1.m10 + matrix2.m10;
            result.m11 = matrix1.m11 + matrix2.m11;
            result.m12 = matrix1.m12 + matrix2.m12;
            result.m13 = matrix1.m13 + matrix2.m13;
            result.m20 = matrix1.m20 + matrix2.m20;
            result.m21 = matrix1.m21 + matrix2.m21;
            result.m22 = matrix1.m22 + matrix2.m22;
            result.m23 = matrix1.m23 + matrix2.m23;
            result.m30 = matrix1.m30 + matrix2.m30;
            result.m31 = matrix1.m31 + matrix2.m31;
            result.m32 = matrix1.m32 + matrix2.m32;
            result.m33 = matrix1.m33 + matrix2.m33;
        }

        public static Matrix4x4 Sub(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = matrix1.m00 - matrix2.m00;
            matrix44.m01 = matrix1.m01 - matrix2.m01;
            matrix44.m02 = matrix1.m02 - matrix2.m02;
            matrix44.m03 = matrix1.m03 - matrix2.m03;
            matrix44.m10 = matrix1.m10 - matrix2.m10;
            matrix44.m11 = matrix1.m11 - matrix2.m11;
            matrix44.m12 = matrix1.m12 - matrix2.m12;
            matrix44.m13 = matrix1.m13 - matrix2.m13;
            matrix44.m20 = matrix1.m20 - matrix2.m20;
            matrix44.m21 = matrix1.m21 - matrix2.m21;
            matrix44.m22 = matrix1.m22 - matrix2.m22;
            matrix44.m23 = matrix1.m23 - matrix2.m23;
            matrix44.m30 = matrix1.m30 - matrix2.m30;
            matrix44.m31 = matrix1.m31 - matrix2.m31;
            matrix44.m32 = matrix1.m32 - matrix2.m32;
            matrix44.m33 = matrix1.m33 - matrix2.m33;
            return matrix44;
        }

        public static void Sub(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
        {
            result.m00 = matrix1.m00 - matrix2.m00;
            result.m01 = matrix1.m01 - matrix2.m01;
            result.m02 = matrix1.m02 - matrix2.m02;
            result.m03 = matrix1.m03 - matrix2.m03;
            result.m10 = matrix1.m10 - matrix2.m10;
            result.m11 = matrix1.m11 - matrix2.m11;
            result.m12 = matrix1.m12 - matrix2.m12;
            result.m13 = matrix1.m13 - matrix2.m13;
            result.m20 = matrix1.m20 - matrix2.m20;
            result.m21 = matrix1.m21 - matrix2.m21;
            result.m22 = matrix1.m22 - matrix2.m22;
            result.m23 = matrix1.m23 - matrix2.m23;
            result.m30 = matrix1.m30 - matrix2.m30;
            result.m31 = matrix1.m31 - matrix2.m31;
            result.m32 = matrix1.m32 - matrix2.m32;
            result.m33 = matrix1.m33 - matrix2.m33;
        }

        public static Matrix4x4 Multiply(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = (float) ((double) matrix1.m00 * (double) matrix2.m00 + (double) matrix1.m01 * (double) matrix2.m10 +
                (double) matrix1.m02 * (double) matrix2.m20 + (double) matrix1.m03 * (double) matrix2.m30);
            matrix44.m01 = (float) ((double) matrix1.m00 * (double) matrix2.m01 + (double) matrix1.m01 * (double) matrix2.m11 +
                (double) matrix1.m02 * (double) matrix2.m21 + (double) matrix1.m03 * (double) matrix2.m31);
            matrix44.m02 = (float) ((double) matrix1.m00 * (double) matrix2.m02 + (double) matrix1.m01 * (double) matrix2.m12 +
                (double) matrix1.m02 * (double) matrix2.m22 + (double) matrix1.m03 * (double) matrix2.m32);
            matrix44.m03 = (float) ((double) matrix1.m00 * (double) matrix2.m03 + (double) matrix1.m01 * (double) matrix2.m13 +
                (double) matrix1.m02 * (double) matrix2.m23 + (double) matrix1.m03 * (double) matrix2.m33);
            matrix44.m10 = (float) ((double) matrix1.m10 * (double) matrix2.m00 + (double) matrix1.m11 * (double) matrix2.m10 +
                (double) matrix1.m12 * (double) matrix2.m20 + (double) matrix1.m13 * (double) matrix2.m30);
            matrix44.m11 = (float) ((double) matrix1.m10 * (double) matrix2.m01 + (double) matrix1.m11 * (double) matrix2.m11 +
                (double) matrix1.m12 * (double) matrix2.m21 + (double) matrix1.m13 * (double) matrix2.m31);
            matrix44.m12 = (float) ((double) matrix1.m10 * (double) matrix2.m02 + (double) matrix1.m11 * (double) matrix2.m12 +
                (double) matrix1.m12 * (double) matrix2.m22 + (double) matrix1.m13 * (double) matrix2.m32);
            matrix44.m13 = (float) ((double) matrix1.m10 * (double) matrix2.m03 + (double) matrix1.m11 * (double) matrix2.m13 +
                (double) matrix1.m12 * (double) matrix2.m23 + (double) matrix1.m13 * (double) matrix2.m33);
            matrix44.m20 = (float) ((double) matrix1.m20 * (double) matrix2.m00 + (double) matrix1.m21 * (double) matrix2.m10 +
                (double) matrix1.m22 * (double) matrix2.m20 + (double) matrix1.m23 * (double) matrix2.m30);
            matrix44.m21 = (float) ((double) matrix1.m20 * (double) matrix2.m01 + (double) matrix1.m21 * (double) matrix2.m11 +
                (double) matrix1.m22 * (double) matrix2.m21 + (double) matrix1.m23 * (double) matrix2.m31);
            matrix44.m22 = (float) ((double) matrix1.m20 * (double) matrix2.m02 + (double) matrix1.m21 * (double) matrix2.m12 +
                (double) matrix1.m22 * (double) matrix2.m22 + (double) matrix1.m23 * (double) matrix2.m32);
            matrix44.m23 = (float) ((double) matrix1.m20 * (double) matrix2.m03 + (double) matrix1.m21 * (double) matrix2.m13 +
                (double) matrix1.m22 * (double) matrix2.m23 + (double) matrix1.m23 * (double) matrix2.m33);
            matrix44.m30 = (float) ((double) matrix1.m30 * (double) matrix2.m00 + (double) matrix1.m31 * (double) matrix2.m10 +
                (double) matrix1.m32 * (double) matrix2.m20 + (double) matrix1.m33 * (double) matrix2.m30);
            matrix44.m31 = (float) ((double) matrix1.m30 * (double) matrix2.m01 + (double) matrix1.m31 * (double) matrix2.m11 +
                (double) matrix1.m32 * (double) matrix2.m21 + (double) matrix1.m33 * (double) matrix2.m31);
            matrix44.m32 = (float) ((double) matrix1.m30 * (double) matrix2.m02 + (double) matrix1.m31 * (double) matrix2.m12 +
                (double) matrix1.m32 * (double) matrix2.m22 + (double) matrix1.m33 * (double) matrix2.m32);
            matrix44.m33 = (float) ((double) matrix1.m30 * (double) matrix2.m03 + (double) matrix1.m31 * (double) matrix2.m13 +
                (double) matrix1.m32 * (double) matrix2.m23 + (double) matrix1.m33 * (double) matrix2.m33);
            return matrix44;
        }

        public static void Multiply(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
        {
            float num1 = (float) ((double) matrix1.m00 * (double) matrix2.m00 + (double) matrix1.m01 * (double) matrix2.m10 +
                (double) matrix1.m02 * (double) matrix2.m20 + (double) matrix1.m03 * (double) matrix2.m30);
            float num2 = (float) ((double) matrix1.m00 * (double) matrix2.m01 + (double) matrix1.m01 * (double) matrix2.m11 +
                (double) matrix1.m02 * (double) matrix2.m21 + (double) matrix1.m03 * (double) matrix2.m31);
            float num3 = (float) ((double) matrix1.m00 * (double) matrix2.m02 + (double) matrix1.m01 * (double) matrix2.m12 +
                (double) matrix1.m02 * (double) matrix2.m22 + (double) matrix1.m03 * (double) matrix2.m32);
            float num4 = (float) ((double) matrix1.m00 * (double) matrix2.m03 + (double) matrix1.m01 * (double) matrix2.m13 +
                (double) matrix1.m02 * (double) matrix2.m23 + (double) matrix1.m03 * (double) matrix2.m33);
            float num5 = (float) ((double) matrix1.m10 * (double) matrix2.m00 + (double) matrix1.m11 * (double) matrix2.m10 +
                (double) matrix1.m12 * (double) matrix2.m20 + (double) matrix1.m13 * (double) matrix2.m30);
            float num6 = (float) ((double) matrix1.m10 * (double) matrix2.m01 + (double) matrix1.m11 * (double) matrix2.m11 +
                (double) matrix1.m12 * (double) matrix2.m21 + (double) matrix1.m13 * (double) matrix2.m31);
            float num7 = (float) ((double) matrix1.m10 * (double) matrix2.m02 + (double) matrix1.m11 * (double) matrix2.m12 +
                (double) matrix1.m12 * (double) matrix2.m22 + (double) matrix1.m13 * (double) matrix2.m32);
            float num8 = (float) ((double) matrix1.m10 * (double) matrix2.m03 + (double) matrix1.m11 * (double) matrix2.m13 +
                (double) matrix1.m12 * (double) matrix2.m23 + (double) matrix1.m13 * (double) matrix2.m33);
            float num9 = (float) ((double) matrix1.m20 * (double) matrix2.m00 + (double) matrix1.m21 * (double) matrix2.m10 +
                (double) matrix1.m22 * (double) matrix2.m20 + (double) matrix1.m23 * (double) matrix2.m30);
            float num10 = (float) ((double) matrix1.m20 * (double) matrix2.m01 + (double) matrix1.m21 * (double) matrix2.m11 +
                (double) matrix1.m22 * (double) matrix2.m21 + (double) matrix1.m23 * (double) matrix2.m31);
            float num11 = (float) ((double) matrix1.m20 * (double) matrix2.m02 + (double) matrix1.m21 * (double) matrix2.m12 +
                (double) matrix1.m22 * (double) matrix2.m22 + (double) matrix1.m23 * (double) matrix2.m32);
            float num12 = (float) ((double) matrix1.m20 * (double) matrix2.m03 + (double) matrix1.m21 * (double) matrix2.m13 +
                (double) matrix1.m22 * (double) matrix2.m23 + (double) matrix1.m23 * (double) matrix2.m33);
            float num13 = (float) ((double) matrix1.m30 * (double) matrix2.m00 + (double) matrix1.m31 * (double) matrix2.m10 +
                (double) matrix1.m32 * (double) matrix2.m20 + (double) matrix1.m33 * (double) matrix2.m30);
            float num14 = (float) ((double) matrix1.m30 * (double) matrix2.m01 + (double) matrix1.m31 * (double) matrix2.m11 +
                (double) matrix1.m32 * (double) matrix2.m21 + (double) matrix1.m33 * (double) matrix2.m31);
            float num15 = (float) ((double) matrix1.m30 * (double) matrix2.m02 + (double) matrix1.m31 * (double) matrix2.m12 +
                (double) matrix1.m32 * (double) matrix2.m22 + (double) matrix1.m33 * (double) matrix2.m32);
            float num16 = (float) ((double) matrix1.m30 * (double) matrix2.m03 + (double) matrix1.m31 * (double) matrix2.m13 +
                (double) matrix1.m32 * (double) matrix2.m23 + (double) matrix1.m33 * (double) matrix2.m33);
            result.m00 = num1;
            result.m01 = num2;
            result.m02 = num3;
            result.m03 = num4;
            result.m10 = num5;
            result.m11 = num6;
            result.m12 = num7;
            result.m13 = num8;
            result.m20 = num9;
            result.m21 = num10;
            result.m22 = num11;
            result.m23 = num12;
            result.m30 = num13;
            result.m31 = num14;
            result.m32 = num15;
            result.m33 = num16;
        }

        public static Vector4 TransformVector4(Matrix4x4 matrix, Vector4 vector)
        {
            float num1 = (float) ((double) vector.x * (double) matrix.m00 + (double) vector.y * (double) matrix.m01 +
                (double) vector.z * (double) matrix.m02 + (double) vector.w * (double) matrix.m03);
            float num2 = (float) ((double) vector.x * (double) matrix.m10 + (double) vector.y * (double) matrix.m11 +
                (double) vector.z * (double) matrix.m12 + (double) vector.w * (double) matrix.m13);
            float num3 = (float) ((double) vector.x * (double) matrix.m20 + (double) vector.y * (double) matrix.m21 +
                (double) vector.z * (double) matrix.m22 + (double) vector.w * (double) matrix.m23);
            float num4 = (float) ((double) vector.x * (double) matrix.m30 + (double) vector.y * (double) matrix.m31 +
                (double) vector.z * (double) matrix.m32 + (double) vector.w * (double) matrix.m33);
            Vector4 vector4;
            vector4.x = num1;
            vector4.y = num2;
            vector4.z = num3;
            vector4.w = num4;
            return vector4;
        }

        public static void TransformVector4(ref Matrix4x4 matrix, ref Vector4 vector, out Vector4 result)
        {
            float num1 = (float) ((double) vector.x * (double) matrix.m00 + (double) vector.y * (double) matrix.m01 +
                (double) vector.z * (double) matrix.m02 + (double) vector.w * (double) matrix.m03);
            float num2 = (float) ((double) vector.x * (double) matrix.m10 + (double) vector.y * (double) matrix.m11 +
                (double) vector.z * (double) matrix.m12 + (double) vector.w * (double) matrix.m13);
            float num3 = (float) ((double) vector.x * (double) matrix.m20 + (double) vector.y * (double) matrix.m21 +
                (double) vector.z * (double) matrix.m22 + (double) vector.w * (double) matrix.m23);
            float num4 = (float) ((double) vector.x * (double) matrix.m30 + (double) vector.y * (double) matrix.m31 +
                (double) vector.z * (double) matrix.m32 + (double) vector.w * (double) matrix.m33);
            result.x = num1;
            result.y = num2;
            result.z = num3;
            result.w = num4;
        }

        public static Vector3 TransformPosition(Matrix4x4 matrix, Vector3 position)
        {
            float num1 = (float) ((double) position.x * (double) matrix.m00 + (double) position.y * (double) matrix.m01 +
                (double) position.z * (double) matrix.m02) + matrix.m03;
            float num2 = (float) ((double) position.x * (double) matrix.m10 + (double) position.y * (double) matrix.m11 +
                (double) position.z * (double) matrix.m12) + matrix.m13;
            float num3 = (float) ((double) position.x * (double) matrix.m20 + (double) position.y * (double) matrix.m21 +
                (double) position.z * (double) matrix.m22) + matrix.m23;
            Vector3 vector3;
            vector3.x = num1;
            vector3.y = num2;
            vector3.z = num3;
            return vector3;
        }

        public static void TransformPosition(ref Matrix4x4 matrix, ref Vector3 position, out Vector3 result)
        {
            float num1 = (float) ((double) position.x * (double) matrix.m00 + (double) position.y * (double) matrix.m01 +
                (double) position.z * (double) matrix.m02) + matrix.m03;
            float num2 = (float) ((double) position.x * (double) matrix.m10 + (double) position.y * (double) matrix.m11 +
                (double) position.z * (double) matrix.m12) + matrix.m13;
            float num3 = (float) ((double) position.x * (double) matrix.m20 + (double) position.y * (double) matrix.m21 +
                (double) position.z * (double) matrix.m22) + matrix.m23;
            result.x = num1;
            result.y = num2;
            result.z = num3;
        }
        
        public Vector3 MultiplyPoint3x4(Vector3 point)
        {
            return TransformPosition(this, point);
        }

        public Vector3 MultiplyVector(Vector3 vector)
        {
            return TransformDirection(this, vector);
        }

        public static Vector3 TransformDirection(Matrix4x4 matrix, Vector3 direction)
        {
            float num1 = (float) ((double) direction.x * (double) matrix.m00 + (double) direction.y * (double) matrix.m01 +
                (double) direction.z * (double) matrix.m02);
            float num2 = (float) ((double) direction.x * (double) matrix.m10 + (double) direction.y * (double) matrix.m11 +
                (double) direction.z * (double) matrix.m12);
            float num3 = (float) ((double) direction.x * (double) matrix.m20 + (double) direction.y * (double) matrix.m21 +
                (double) direction.z * (double) matrix.m22);
            Vector3 vector3;
            vector3.x = num1;
            vector3.y = num2;
            vector3.z = num3;
            return vector3;
        }

        public static void TransformDirection(ref Matrix4x4 matrix, ref Vector3 direction, out Vector3 result)
        {
            float num1 = (float) ((double) direction.x * (double) matrix.m00 + (double) direction.y * (double) matrix.m01 +
                (double) direction.z * (double) matrix.m02);
            float num2 = (float) ((double) direction.x * (double) matrix.m10 + (double) direction.y * (double) matrix.m11 +
                (double) direction.z * (double) matrix.m12);
            float num3 = (float) ((double) direction.x * (double) matrix.m20 + (double) direction.y * (double) matrix.m21 +
                (double) direction.z * (double) matrix.m22);
            result.x = num1;
            result.y = num2;
            result.z = num3;
        }

        public static Matrix4x4 operator -(Matrix4x4 matrix1)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = -matrix1.m00;
            matrix44.m01 = -matrix1.m01;
            matrix44.m02 = -matrix1.m02;
            matrix44.m03 = -matrix1.m03;
            matrix44.m10 = -matrix1.m10;
            matrix44.m11 = -matrix1.m11;
            matrix44.m12 = -matrix1.m12;
            matrix44.m13 = -matrix1.m13;
            matrix44.m20 = -matrix1.m20;
            matrix44.m21 = -matrix1.m21;
            matrix44.m22 = -matrix1.m22;
            matrix44.m23 = -matrix1.m23;
            matrix44.m30 = -matrix1.m30;
            matrix44.m31 = -matrix1.m31;
            matrix44.m32 = -matrix1.m32;
            matrix44.m33 = -matrix1.m33;
            return matrix44;
        }

        public static bool operator ==(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            if ((double) matrix1.m00 == (double) matrix2.m00 && (double) matrix1.m11 == (double) matrix2.m11 &&
                ((double) matrix1.m22 == (double) matrix2.m22 && (double) matrix1.m33 == (double) matrix2.m33) &&
                ((double) matrix1.m01 == (double) matrix2.m01 && (double) matrix1.m02 == (double) matrix2.m02 &&
                    ((double) matrix1.m03 == (double) matrix2.m03 && (double) matrix1.m10 == (double) matrix2.m10)) &&
                ((double) matrix1.m12 == (double) matrix2.m12 && (double) matrix1.m13 == (double) matrix2.m13 &&
                    ((double) matrix1.m20 == (double) matrix2.m20 && (double) matrix1.m21 == (double) matrix2.m21) &&
                    ((double) matrix1.m23 == (double) matrix2.m23 && (double) matrix1.m30 == (double) matrix2.m30 &&
                        (double) matrix1.m31 == (double) matrix2.m31)))
                return (double) matrix1.m32 == (double) matrix2.m32;
            return false;
        }

        public static bool operator !=(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            if ((double) matrix1.m00 == (double) matrix2.m00 && (double) matrix1.m01 == (double) matrix2.m01 &&
                ((double) matrix1.m02 == (double) matrix2.m02 && (double) matrix1.m03 == (double) matrix2.m03) &&
                ((double) matrix1.m10 == (double) matrix2.m10 && (double) matrix1.m11 == (double) matrix2.m11 &&
                    ((double) matrix1.m12 == (double) matrix2.m12 && (double) matrix1.m13 == (double) matrix2.m13)) &&
                ((double) matrix1.m20 == (double) matrix2.m20 && (double) matrix1.m21 == (double) matrix2.m21 &&
                    ((double) matrix1.m22 == (double) matrix2.m22 && (double) matrix1.m23 == (double) matrix2.m23) &&
                    ((double) matrix1.m30 == (double) matrix2.m30 && (double) matrix1.m31 == (double) matrix2.m31 &&
                        (double) matrix1.m32 == (double) matrix2.m32)))
                return (double) matrix1.m33 != (double) matrix2.m33;
            return true;
        }

        public static Matrix4x4 operator +(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = matrix1.m00 + matrix2.m00;
            matrix44.m01 = matrix1.m01 + matrix2.m01;
            matrix44.m02 = matrix1.m02 + matrix2.m02;
            matrix44.m03 = matrix1.m03 + matrix2.m03;
            matrix44.m10 = matrix1.m10 + matrix2.m10;
            matrix44.m11 = matrix1.m11 + matrix2.m11;
            matrix44.m12 = matrix1.m12 + matrix2.m12;
            matrix44.m13 = matrix1.m13 + matrix2.m13;
            matrix44.m20 = matrix1.m20 + matrix2.m20;
            matrix44.m21 = matrix1.m21 + matrix2.m21;
            matrix44.m22 = matrix1.m22 + matrix2.m22;
            matrix44.m23 = matrix1.m23 + matrix2.m23;
            matrix44.m30 = matrix1.m30 + matrix2.m30;
            matrix44.m31 = matrix1.m31 + matrix2.m31;
            matrix44.m32 = matrix1.m32 + matrix2.m32;
            matrix44.m33 = matrix1.m33 + matrix2.m33;
            return matrix44;
        }

        public static Matrix4x4 operator -(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = matrix1.m00 - matrix2.m00;
            matrix44.m01 = matrix1.m01 - matrix2.m01;
            matrix44.m02 = matrix1.m02 - matrix2.m02;
            matrix44.m03 = matrix1.m03 - matrix2.m03;
            matrix44.m10 = matrix1.m10 - matrix2.m10;
            matrix44.m11 = matrix1.m11 - matrix2.m11;
            matrix44.m12 = matrix1.m12 - matrix2.m12;
            matrix44.m13 = matrix1.m13 - matrix2.m13;
            matrix44.m20 = matrix1.m20 - matrix2.m20;
            matrix44.m21 = matrix1.m21 - matrix2.m21;
            matrix44.m22 = matrix1.m22 - matrix2.m22;
            matrix44.m23 = matrix1.m23 - matrix2.m23;
            matrix44.m30 = matrix1.m30 - matrix2.m30;
            matrix44.m31 = matrix1.m31 - matrix2.m31;
            matrix44.m32 = matrix1.m32 - matrix2.m32;
            matrix44.m33 = matrix1.m33 - matrix2.m33;
            return matrix44;
        }

        public static Matrix4x4 operator *(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Matrix4x4 matrix44;
            matrix44.m00 = (float) ((double) matrix1.m00 * (double) matrix2.m00 + (double) matrix1.m01 * (double) matrix2.m10 +
                (double) matrix1.m02 * (double) matrix2.m20 + (double) matrix1.m03 * (double) matrix2.m30);
            matrix44.m01 = (float) ((double) matrix1.m00 * (double) matrix2.m01 + (double) matrix1.m01 * (double) matrix2.m11 +
                (double) matrix1.m02 * (double) matrix2.m21 + (double) matrix1.m03 * (double) matrix2.m31);
            matrix44.m02 = (float) ((double) matrix1.m00 * (double) matrix2.m02 + (double) matrix1.m01 * (double) matrix2.m12 +
                (double) matrix1.m02 * (double) matrix2.m22 + (double) matrix1.m03 * (double) matrix2.m32);
            matrix44.m03 = (float) ((double) matrix1.m00 * (double) matrix2.m03 + (double) matrix1.m01 * (double) matrix2.m13 +
                (double) matrix1.m02 * (double) matrix2.m23 + (double) matrix1.m03 * (double) matrix2.m33);
            matrix44.m10 = (float) ((double) matrix1.m10 * (double) matrix2.m00 + (double) matrix1.m11 * (double) matrix2.m10 +
                (double) matrix1.m12 * (double) matrix2.m20 + (double) matrix1.m13 * (double) matrix2.m30);
            matrix44.m11 = (float) ((double) matrix1.m10 * (double) matrix2.m01 + (double) matrix1.m11 * (double) matrix2.m11 +
                (double) matrix1.m12 * (double) matrix2.m21 + (double) matrix1.m13 * (double) matrix2.m31);
            matrix44.m12 = (float) ((double) matrix1.m10 * (double) matrix2.m02 + (double) matrix1.m11 * (double) matrix2.m12 +
                (double) matrix1.m12 * (double) matrix2.m22 + (double) matrix1.m13 * (double) matrix2.m32);
            matrix44.m13 = (float) ((double) matrix1.m10 * (double) matrix2.m03 + (double) matrix1.m11 * (double) matrix2.m13 +
                (double) matrix1.m12 * (double) matrix2.m23 + (double) matrix1.m13 * (double) matrix2.m33);
            matrix44.m20 = (float) ((double) matrix1.m20 * (double) matrix2.m00 + (double) matrix1.m21 * (double) matrix2.m10 +
                (double) matrix1.m22 * (double) matrix2.m20 + (double) matrix1.m23 * (double) matrix2.m30);
            matrix44.m21 = (float) ((double) matrix1.m20 * (double) matrix2.m01 + (double) matrix1.m21 * (double) matrix2.m11 +
                (double) matrix1.m22 * (double) matrix2.m21 + (double) matrix1.m23 * (double) matrix2.m31);
            matrix44.m22 = (float) ((double) matrix1.m20 * (double) matrix2.m02 + (double) matrix1.m21 * (double) matrix2.m12 +
                (double) matrix1.m22 * (double) matrix2.m22 + (double) matrix1.m23 * (double) matrix2.m32);
            matrix44.m23 = (float) ((double) matrix1.m20 * (double) matrix2.m03 + (double) matrix1.m21 * (double) matrix2.m13 +
                (double) matrix1.m22 * (double) matrix2.m23 + (double) matrix1.m23 * (double) matrix2.m33);
            matrix44.m30 = (float) ((double) matrix1.m30 * (double) matrix2.m00 + (double) matrix1.m31 * (double) matrix2.m10 +
                (double) matrix1.m32 * (double) matrix2.m20 + (double) matrix1.m33 * (double) matrix2.m30);
            matrix44.m31 = (float) ((double) matrix1.m30 * (double) matrix2.m01 + (double) matrix1.m31 * (double) matrix2.m11 +
                (double) matrix1.m32 * (double) matrix2.m21 + (double) matrix1.m33 * (double) matrix2.m31);
            matrix44.m32 = (float) ((double) matrix1.m30 * (double) matrix2.m02 + (double) matrix1.m31 * (double) matrix2.m12 +
                (double) matrix1.m32 * (double) matrix2.m22 + (double) matrix1.m33 * (double) matrix2.m32);
            matrix44.m33 = (float) ((double) matrix1.m30 * (double) matrix2.m03 + (double) matrix1.m31 * (double) matrix2.m13 +
                (double) matrix1.m32 * (double) matrix2.m23 + (double) matrix1.m33 * (double) matrix2.m33);
            return matrix44;
        }
    }
}