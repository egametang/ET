using UnityEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class ToolSet
	{
		public static Color ConvertFromHtmlColor(string str)
		{
			if (str.Length < 7 || str[0] != '#')
				return Color.black;

			if (str.Length == 9)
			{
				//optimize:avoid using Convert.ToByte and Substring
				//return new Color32(Convert.ToByte(str.Substring(3, 2), 16), Convert.ToByte(str.Substring(5, 2), 16),
				//  Convert.ToByte(str.Substring(7, 2), 16), Convert.ToByte(str.Substring(1, 2), 16));

				return new Color32((byte)(CharToHex(str[3]) * 16 + CharToHex(str[4])),
					(byte)(CharToHex(str[5]) * 16 + CharToHex(str[6])),
					(byte)(CharToHex(str[7]) * 16 + CharToHex(str[8])),
					(byte)(CharToHex(str[1]) * 16 + CharToHex(str[2])));
			}
			else
			{
				//return new Color32(Convert.ToByte(str.Substring(1, 2), 16), Convert.ToByte(str.Substring(3, 2), 16),
				//Convert.ToByte(str.Substring(5, 2), 16), 255);

				return new Color32((byte)(CharToHex(str[1]) * 16 + CharToHex(str[2])),
					(byte)(CharToHex(str[3]) * 16 + CharToHex(str[4])),
					(byte)(CharToHex(str[5]) * 16 + CharToHex(str[6])),
					255);
			}
		}

		public static Color ColorFromRGB(int value)
		{
			return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f, 1);
		}

		public static Color ColorFromRGBA(int value)
		{
			return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f, ((value >> 24) & 0xFF) / 255f);
		}

		public static int CharToHex(char c)
		{
			if (c >= '0' && c <= '9')
				return (int)c - 48;
			if (c >= 'A' && c <= 'F')
				return 10 + (int)c - 65;
			else if (c >= 'a' && c <= 'f')
				return 10 + (int)c - 97;
			else
				return 0;
		}

		public static Rect Intersection(ref Rect rect1, ref Rect rect2)
		{
			if (rect1.width == 0 || rect1.height == 0 || rect2.width == 0 || rect2.height == 0)
				return new Rect(0, 0, 0, 0);

			float left = rect1.xMin > rect2.xMin ? rect1.xMin : rect2.xMin;
			float right = rect1.xMax < rect2.xMax ? rect1.xMax : rect2.xMax;
			float top = rect1.yMin > rect2.yMin ? rect1.yMin : rect2.yMin;
			float bottom = rect1.yMax < rect2.yMax ? rect1.yMax : rect2.yMax;

			if (left > right || top > bottom)
				return new Rect(0, 0, 0, 0);
			else
				return Rect.MinMaxRect(left, top, right, bottom);
		}

		public static Rect Union(ref Rect rect1, ref Rect rect2)
		{
			if (rect2.width == 0 || rect2.height == 0)
				return rect1;

			if (rect1.width == 0 || rect1.height == 0)
				return rect2;

			float x = Mathf.Min(rect1.x, rect2.x);
			float y = Mathf.Min(rect1.y, rect2.y);
			return new Rect(x, y, Mathf.Max(rect1.xMax, rect2.xMax) - x, Mathf.Max(rect1.yMax, rect2.yMax) - y);
		}

		public static void FlipRect(ref Rect rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				float tmp = rect.xMin;
				rect.xMin = rect.xMax;
				rect.xMax = tmp;
			}
			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				float tmp = rect.yMin;
				rect.yMin = rect.yMax;
				rect.yMax = tmp;
			}
		}

		public static void FlipInnerRect(float sourceWidth, float sourceHeight, ref Rect rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				rect.x = sourceWidth - rect.xMax;
				rect.xMax = rect.x + rect.width;
			}

			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				rect.y = sourceHeight - rect.yMax;
				rect.yMax = rect.y + rect.height;
			}
		}

		public static void uvLerp(Vector2[] uvSrc, Vector2[] uvDest, float min, float max)
		{
			float uMin = float.MaxValue;
			float uMax = float.MinValue;
			float vMin = float.MaxValue;
			float vMax = float.MinValue;
			int len = uvSrc.Length;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				if (v.x < uMin)
					uMin = v.x;
				if (v.x > uMax)
					uMax = v.x;
				if (v.y < vMin)
					vMin = v.y;
				if (v.y > vMax)
					vMax = v.y;
			}
			float uLen = uMax - uMin;
			float vLen = vMax - vMin;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				v.x = (v.x - uMin) / uLen;
				v.y = (v.y - vMin) / vLen;
				uvDest[i] = v;
			}
		}

		public static void SetParent(Transform t, Transform parent)
		{
			if ((object)t.parent == (object)parent)
				return;

#if (UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER)
			t.SetParent(parent, false);
#else
			Vector3 p = t.localPosition;
			Vector3 s = t.localScale;
			Quaternion q = t.localRotation;
			t.parent = parent;
			t.localPosition = p;
			t.localScale = s;
			t.localRotation = q;
#endif
		}

		public static void SkewMatrix(ref Matrix4x4 matrix, float skewX, float skewY)
		{
			skewX = -skewX * Mathf.Deg2Rad;
			skewY = -skewY * Mathf.Deg2Rad;
			float sinX = Mathf.Sin(skewX);
			float cosX = Mathf.Cos(skewX);
			float sinY = Mathf.Sin(skewY);
			float cosY = Mathf.Cos(skewY);

			float m00 = matrix.m00 * cosY - matrix.m10 * sinX;
			float m10 = matrix.m00 * sinY + matrix.m10 * cosX;
			float m01 = matrix.m01 * cosY - matrix.m11 * sinX;
			float m11 = matrix.m01 * sinY + matrix.m11 * cosX;
			float m02 = matrix.m02 * cosY - matrix.m12 * sinX;
			float m12 = matrix.m02 * sinY + matrix.m12 * cosX;

			matrix.m00 = m00;
			matrix.m10 = m10;
			matrix.m01 = m01;
			matrix.m11 = m11;
			matrix.m02 = m02;
			matrix.m12 = m12;
		}

		//From Starling
		public static bool IsPointInTriangle(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			// This algorithm is described well in this article:
			// http://www.blackpawn.com/texts/pointinpoly/default.html

			float v0x = c.x - a.x;
			float v0y = c.y - a.y;
			float v1x = b.x - a.x;
			float v1y = b.y - a.y;
			float v2x = p.x - a.x;
			float v2y = p.y - a.y;

			float dot00 = v0x * v0x + v0y * v0y;
			float dot01 = v0x * v1x + v0y * v1y;
			float dot02 = v0x * v2x + v0y * v2y;
			float dot11 = v1x * v1x + v1y * v1y;
			float dot12 = v1x * v2x + v1y * v2y;

			float invDen = 1.0f / (dot00 * dot11 - dot01 * dot01);
			float u = (dot11 * dot02 - dot01 * dot12) * invDen;
			float v = (dot00 * dot12 - dot01 * dot02) * invDen;

			return (u >= 0) && (v >= 0) && (u + v < 1);
		}
	}
}
