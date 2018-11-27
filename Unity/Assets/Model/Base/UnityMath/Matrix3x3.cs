//**************************************************
// Copyright©2018 何冠峰
// Licensed under the MIT license
//**************************************************

namespace PF
{
	public struct Matrix3x3
	{
		public float[] Data;

		public Matrix3x3(float m0, float m1, float m2, float m3, float m4, float m5, float m6, float m7, float m8)
		{
			Data = new float[9];
			Data[0] = m0; Data[3] = m3; Data[6] = m6;
			Data[1] = m1; Data[4] = m4; Data[7] = m7;
			Data[2] = m2; Data[5] = m5; Data[8] = m8;
		}

		public static Matrix3x3 identity
		{
			get
			{
				return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
			}
		}
		public void SetZero()
		{
			Data[0] = 0f; Data[3] = 0f; Data[6] = 0f;
			Data[1] = 0f; Data[4] = 0f; Data[7] = 0f;
			Data[2] = 0f; Data[5] = 0f; Data[8] = 0f;

			//The floats are laid out
			// m0   m3   m6	
			// m1   m4   m7	
			// m2   m5   m8	

			//Data[0]=m00;  Data[3]=m01;  Data[6]=m02;
			//Data[1]=m10;  Data[4]=m11;  Data[7]=m12;
			//Data[2]=m20;  Data[5]=m21;  Data[8]=m22;

			//Get(0, 0)   Get(0, 1)   Get(0, 2)
			//Get(1, 0)   Get(1, 1)   Get(1, 2)
			//Get(2, 0)   Get(2, 1)   Get(2, 2)
		}

		public float Get(int row, int column)
		{
			return Data[row + (column * 3)];
		}
		public void Set(int row, int column, float value)
		{
			Data[row + (column * 3)] = value;
		}

		public void SetOrthoNormalBasis(Vector3 inX, Vector3 inY, Vector3 inZ)
		{
			this.Set(0, 0, inX.x); this.Set(0, 1, inY.x); this.Set(0, 2, inZ.x);
			this.Set(1, 0, inX.y); this.Set(1, 1, inY.y); this.Set(1, 2, inZ.y);
			this.Set(2, 0, inX.z); this.Set(2, 1, inY.z); this.Set(2, 2, inZ.z);
		}
		public float GetDeterminant()
		{
			float fCofactor0 = Get(0, 0) * Get(1, 1) * Get(2, 2);
			float fCofactor1 = Get(0, 1) * Get(1, 2) * Get(2, 0);
			float fCofactor2 = Get(0, 2) * Get(1, 0) * Get(2, 1);

			float fCofactor3 = Get(0, 2) * Get(1, 1) * Get(2, 0);
			float fCofactor4 = Get(0, 1) * Get(1, 0) * Get(2, 2);
			float fCofactor5 = Get(0, 0) * Get(1, 2) * Get(2, 1);

			return fCofactor0 + fCofactor1 + fCofactor2 - fCofactor3 - fCofactor4 - fCofactor5;
		}

		// Right handed
		public static bool LookRotationToMatrix(Vector3 viewVec, Vector3 upVec, out Matrix3x3 m)
		{
			m = Matrix3x3.identity;

			Vector3 z = viewVec;
			// compute u0
			float mag = z.Length();
			if (mag < Mathf.Epsilon)
			{
				return false;
			}
			z /= mag;

			Vector3 x = Vector3.Cross(upVec, z);
			mag = x.Length();
			if (mag < Mathf.Epsilon)
			{
				return false;
			}
			x /= mag;

			Vector3 y = Vector3.Cross(z, x);
			if (!Mathf.CompareApproximate(y.Length(), 1.0F))
				return false;

			m.SetOrthoNormalBasis(x, y, z);
			return true;
		}
	}
}