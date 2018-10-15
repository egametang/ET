using System;
using System.Globalization;

namespace PF
{
    [Serializable]
    public struct Color32: IEquatable<Color32>
    {
        private uint packedValue;

        internal Color32(uint packedValue)
        {
            this.packedValue = packedValue;
        }

        public Color32(int r, int g, int b, int a)
        {
            if (((r | g | b | a) & -256) != 0)
            {
                r = Color32.ClampToByte32(r);
                g = Color32.ClampToByte32(g);
                b = Color32.ClampToByte32(b);
                a = Color32.ClampToByte32(a);
            }

            g <<= 8;
            b <<= 16;
            a <<= 24;
            this.packedValue = (uint) (r | g | b | a);
        }

        public byte R
        {
            get
            {
                return (byte) this.packedValue;
            }
            set
            {
                this.packedValue = this.packedValue & 4294967040U | (uint) value;
            }
        }

        public byte G
        {
            get
            {
                return (byte) (this.packedValue >> 8);
            }
            set
            {
                this.packedValue = (uint) ((int) this.packedValue & -65281 | (int) value << 8);
            }
        }

        public byte B
        {
            get
            {
                return (byte) (this.packedValue >> 16);
            }
            set
            {
                this.packedValue = (uint) ((int) this.packedValue & -16711681 | (int) value << 16);
            }
        }

        public byte A
        {
            get
            {
                return (byte) (this.packedValue >> 24);
            }
            set
            {
                this.packedValue = (uint) ((int) this.packedValue & 16777215 | (int) value << 24);
            }
        }

        public uint PackedValue
        {
            get
            {
                return this.packedValue;
            }
            set
            {
                this.packedValue = value;
            }
        }

        public static Color32 Lerp(Color32 value1, Color32 value2, float amount)
        {
            uint packedValue1 = value1.packedValue;
            uint packedValue2 = value2.packedValue;
            int num1 = (int) (byte) packedValue1;
            int num2 = (int) (byte) (packedValue1 >> 8);
            int num3 = (int) (byte) (packedValue1 >> 16);
            int num4 = (int) (byte) (packedValue1 >> 24);
            int num5 = (int) (byte) packedValue2;
            int num6 = (int) (byte) (packedValue2 >> 8);
            int num7 = (int) (byte) (packedValue2 >> 16);
            int num8 = (int) (byte) (packedValue2 >> 24);
            int num9 = (int) PackUtils.PackUNorm(65536f, amount);
            int num10 = num1 + ((num5 - num1) * num9 >> 16);
            int num11 = num2 + ((num6 - num2) * num9 >> 16);
            int num12 = num3 + ((num7 - num3) * num9 >> 16);
            int num13 = num4 + ((num8 - num4) * num9 >> 16);
            Color32 color32;
            color32.packedValue = (uint) (num10 | num11 << 8 | num12 << 16 | num13 << 24);
            return color32;
        }

        public static Color32 Multiply(Color32 value, float scale)
        {
            uint packedValue = value.packedValue;
            uint num1 = (uint) (byte) packedValue;
            uint num2 = (uint) (byte) (packedValue >> 8);
            uint num3 = (uint) (byte) (packedValue >> 16);
            uint num4 = (uint) (byte) (packedValue >> 24);
            scale *= 65536f;
            uint num5 = (double) scale >= 0.0? ((double) scale <= 16777220.0? (uint) scale : 16777215U) : 0U;
            uint num6 = num1 * num5 >> 16;
            uint num7 = num2 * num5 >> 16;
            uint num8 = num3 * num5 >> 16;
            uint num9 = num4 * num5 >> 16;
            if (num6 > (uint) byte.MaxValue)
                num6 = (uint) byte.MaxValue;
            if (num7 > (uint) byte.MaxValue)
                num7 = (uint) byte.MaxValue;
            if (num8 > (uint) byte.MaxValue)
                num8 = (uint) byte.MaxValue;
            if (num9 > (uint) byte.MaxValue)
                num9 = (uint) byte.MaxValue;
            Color32 color32;
            color32.packedValue = (uint) ((int) num6 | (int) num7 << 8 | (int) num8 << 16 | (int) num9 << 24);
            return color32;
        }

        public static Color32 operator *(Color32 value, float scale)
        {
            uint packedValue = value.packedValue;
            uint num1 = (uint) (byte) packedValue;
            uint num2 = (uint) (byte) (packedValue >> 8);
            uint num3 = (uint) (byte) (packedValue >> 16);
            uint num4 = (uint) (byte) (packedValue >> 24);
            scale *= 65536f;
            uint num5 = (double) scale >= 0.0? ((double) scale <= 16777220.0? (uint) scale : 16777215U) : 0U;
            uint num6 = num1 * num5 >> 16;
            uint num7 = num2 * num5 >> 16;
            uint num8 = num3 * num5 >> 16;
            uint num9 = num4 * num5 >> 16;
            if (num6 > (uint) byte.MaxValue)
                num6 = (uint) byte.MaxValue;
            if (num7 > (uint) byte.MaxValue)
                num7 = (uint) byte.MaxValue;
            if (num8 > (uint) byte.MaxValue)
                num8 = (uint) byte.MaxValue;
            if (num9 > (uint) byte.MaxValue)
                num9 = (uint) byte.MaxValue;
            Color32 color32;
            color32.packedValue = (uint) ((int) num6 | (int) num7 << 8 | (int) num8 << 16 | (int) num9 << 24);
            return color32;
        }

        public override string ToString()
        {
            return string.Format((IFormatProvider) CultureInfo.CurrentCulture, "{R:{0} G:{1} B:{2} A:{3}}", (object) this.R, (object) this.G,
                                 (object) this.B, (object) this.A);
        }

        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Color32)
                return this.Equals((Color32) obj);
            return false;
        }

        public bool Equals(Color32 other)
        {
            return this.packedValue.Equals(other.packedValue);
        }

        public static bool operator ==(Color32 a, Color32 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Color32 a, Color32 b)
        {
            return !a.Equals(b);
        }

        public static Color32 Black
        {
            get
            {
                return new Color32(4278190080U);
            }
        }

        public static Color32 Blue
        {
            get
            {
                return new Color32(4294901760U);
            }
        }

        public static Color32 Green
        {
            get
            {
                return new Color32(4278222848U);
            }
        }

        public static Color32 Red
        {
            get
            {
                return new Color32(4278190335U);
            }
        }

        public static Color32 White
        {
            get
            {
                return new Color32(uint.MaxValue);
            }
        }

        private static int ClampToByte32(int value)
        {
            if (value < 0)
                return 0;
            if (value > (int) byte.MaxValue)
                return (int) byte.MaxValue;
            return value;
        }
    }
}