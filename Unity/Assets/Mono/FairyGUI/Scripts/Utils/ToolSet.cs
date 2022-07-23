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

        public static Color ColorFromRGBA(uint value)
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

        public static void RotateUV(Vector2[] uv, ref Rect baseUVRect)
        {
            int vertCount = uv.Length;
            float xMin = Mathf.Min(baseUVRect.xMin, baseUVRect.xMax);
            float yMin = baseUVRect.yMin;
            float yMax = baseUVRect.yMax;
            if (yMin > yMax)
            {
                yMin = yMax;
                yMax = baseUVRect.yMin;
            }

            float tmp;
            for (int i = 0; i < vertCount; i++)
            {
                Vector2 m = uv[i];
                tmp = m.y;
                m.y = yMin + m.x - xMin;
                m.x = xMin + yMax - tmp;
                uv[i] = m;
            }
        }
    }
}
